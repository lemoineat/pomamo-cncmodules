// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Lemoine.Cnc
{
  /// <summary>
  /// HeidenhainDNC input module
  /// </summary>
  public partial class HeidenhainDNC : BaseCncModule, ICncModule, IDisposable
  {
    readonly InterfaceManager m_interfaceManager = new InterfaceManager ();
    readonly ConnectionManager m_connectionManager = new ConnectionManager ();
    bool? m_dncVersionOk = null; // null if it is not read yet, false if wrong version, true if right version
    HeidenhainDNCLib.JHMachineInProcess m_machine;
    HeidenhainDNCLib.DNC_STATE m_controlState;
    bool m_isDisposed = false;

    #region Getters / Setters
    /// <summary>
    /// Current control state
    /// (linked to the interface manager so that no interface is used)
    /// </summary>
    HeidenhainDNCLib.DNC_STATE ControlState
    {
      get
      {
        return m_controlState;
      }
      set
      {
        if (value != m_controlState) {
          if (value == HeidenhainDNCLib.DNC_STATE.DNC_STATE_NOT_INITIALIZED ||
              value == HeidenhainDNCLib.DNC_STATE.DNC_STATE_DNC_IS_STOPPED) {
            log.Error ("HeidenhainDNC.ControlState: state changed to '" + value + "'");
          }
          else if (value == HeidenhainDNCLib.DNC_STATE.DNC_STATE_MACHINE_IS_AVAILABLE) {
            log.Info ("HeidenhainDNC.ControlState: state changed to '" + value + "'");
            m_connectionManager.CancelDisconnectQuery ();
          }
          else {
            log.Warn ("HeidenhainDNC.ControlState: state changed to '" + value + "'");
          }
        }

        m_interfaceManager.ControlState = value;
        m_controlState = value;

        // Depending on the current state, ask for a disconnection if m_machine is not null
        if ((m_controlState == HeidenhainDNCLib.DNC_STATE.DNC_STATE_NOT_INITIALIZED || m_controlState == HeidenhainDNCLib.DNC_STATE.DNC_STATE_DNC_IS_STOPPED) &&
            m_machine != null) {
          m_connectionManager.DisconnectLater ();
        }
      }
    }

    /// <summary>
    /// Name of the connection, whose configuration is edited with
    /// ConfigureConnection.exe from Heidenhain
    /// </summary>
    public string ConnectionName { get; set; }

    /// <summary>
    /// Cnc type of the control
    /// </summary>
    public string CncType { get; set; }

    /// <summary>
    /// Ip address of the machine
    /// </summary>
    public string IpAddress { get; set; }

    /// <summary>
    /// Port of the machine
    /// </summary>
    public string Port { get; set; }

    /// <summary>
    /// Connection error
    /// </summary>
    public bool ConnectionError
    {
      get
      {
        return (ControlState != HeidenhainDNCLib.DNC_STATE.DNC_STATE_MACHINE_IS_AVAILABLE);
      }
    }
    #endregion // Getters / Setters

    #region Constructors, destructor
    /// <summary>
    /// Default constructor
    /// </summary>
    public HeidenhainDNC () : base ("Lemoine.Cnc.In.HeidenhainDNC")
    {
      // Prepare DNC configuration and commands
      m_machine = null;
      ControlState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_NOT_INITIALIZED;
      Multiplier = 10000;

      // Link the connection manager
      m_connectionManager.ConnectionRequired += Connect;
      m_connectionManager.DisconnectionRequired += Disconnect;
    }

    /// <summary>
    /// <see cref="IDisposable.Dispose" />
    /// </summary>
    public void Dispose ()
    {
      Dispose (true);
      GC.SuppressFinalize (this);
    }

    /// <summary>
    /// Destructor: free unmanaged resources in case Dispose has not been called
    /// </summary>
    ~HeidenhainDNC ()
    {
      Dispose (false);
    }

    void Dispose (bool disposing)
    {
      log.WarnFormat ("HeidenhainDNC.Dispose - disposing is '{0}'", disposing);
      m_isDisposed = true;
      if (!m_isDisposed) {
        if (disposing) {
          // Free here only managed resources
        }

        // Then clean up unmanaged resources
        if (m_machine != null) {
          Disconnect (); // Takes effect immediatly, no connection manager
        }
      }
    }
    #endregion // Constructor, destructor

    #region Methods
    /// <summary>
    /// Start method
    /// </summary>
    public bool Start ()
    {
      if (m_isDisposed) {
        return false;
      }

      log.Info ("HeidenhainDNC.Start");
      Application.DoEvents ();

      // 0. Initialize objects
      m_interfaceManager.Logger = log;
      m_connectionManager.Logger = log;

      // 1. Check the cnc version
      if (!m_dncVersionOk.HasValue) {
        try {
          log.Info ("HeidenhainDNC.Start: try to find the DNC server version");
          m_dncVersionOk = CheckDncVersion (1, 5, 1, 0);
          if (!m_dncVersionOk.HasValue) {
            throw new Exception ("DNC version has no value");
          }
        }
        catch (Exception ex) {
          log.ErrorFormat ("HeidenhainDNC.Start - couldn't find the DNC server version: {0}", ex);
          m_dncVersionOk = null;
          return false;
        }
      }
      if (!m_dncVersionOk.Value) {
        log.Error ("HeidenhainDNC.Start: wrong DNC server version");
        return false; // Don't go further
      }

      // 2. Reset data that could have been previously computed before further readings
      m_interfaceManager.ResetData ();

      // 3. Update the state of the machine
      if (m_machine != null) {
        try {
          ControlState = m_machine.GetState ();
        }
        catch (Exception ex) {
          ProcessException (ex, "HeidenhainDNC", "Start (update machine state)", true);
        }
      }

      // 4. Order disconnect or connect if needed
      // The acquisition is operational if there is no connection error
      // A disconnection may be required before a connection if m_machine is not null
      m_connectionManager.Start (!ConnectionError, m_machine != null);

      // 5. Check the interfaces
      if (!m_interfaceManager.HasValidInterface () && !ConnectionError) {
        log.Warn ("HeidenhainDNC.Start - InitInterfaces");
        try {
          m_interfaceManager.Initialize (m_machine);
        }
        catch (Exception ex) {
          m_interfaceManager.CloseInterfaces ();
          ProcessException (ex, "HeidenhainDNC", "Start (check interfaces)", true);
        }
      }

      return !ConnectionError;
    }

    /// <summary>
    /// Connect the control
    /// Condition: m_machine is null
    /// </summary>
    void Connect ()
    {
      if (m_machine != null) {
        log.Warn ("Connect: the machine already exists, return");
        return;
      }

      log.Debug ("Connect");
      try {
        m_machine = new HeidenhainDNCLib.JHMachineInProcess ();

        if (string.IsNullOrEmpty (ConnectionName)) {
          // Cnc type
          var cncType = (HeidenhainDNCLib.DNC_CNC_TYPE)Enum.Parse (typeof (HeidenhainDNCLib.DNC_CNC_TYPE), CncType);

          // Create and configure an ad-hoc connection
          var newConnection = (HeidenhainDNCLib.IJHConnection2)m_machine;
          newConnection.Configure (cncType, GetProtocol (cncType));
          if (!string.IsNullOrEmpty (IpAddress)) {
            newConnection.set_propertyValue (
              HeidenhainDNCLib.DNC_CONNECTION_PROPERTY.DNC_CP_HOST, (object)IpAddress);
          }

          if (!string.IsNullOrEmpty (Port)) {
            int port = int.Parse (Port);
            newConnection.set_propertyValue (
              HeidenhainDNCLib.DNC_CONNECTION_PROPERTY.DNC_CP_PORT, (object)port);
          }

          // Connect the machine
          m_machine.ConnectRequest3 (newConnection, null, null);
        }
        else {
          // Browse the list of existing connections
          HeidenhainDNCLib.IJHConnectionList connectionList = m_machine.ListConnections ();
          if (connectionList == null) {
            throw new Exception ("Connection list is null!");
          }

          HeidenhainDNCLib.IJHConnection connection = null;
          bool found = false;
          for (int i = 0; i < connectionList.Count; i++) {
            connection = connectionList[i];
            if (connection.name == ConnectionName) {
              CncType = connection.cncType.ToString ();
              found = true;
            }
            Marshal.ReleaseComObject (connection);
          }
          Marshal.ReleaseComObject (connectionList);

          if (!found) {
            throw new Exception ("Connection '" + ConnectionName + "'not found! Please use the HeidenhainDNC wizard to create it.");
          }

          // Connect the machine
          m_machine.ConnectRequest (ConnectionName);
        }

        // Connect the event "OnStateChanged"
        if (CncType.EndsWith ("_NCK", StringComparison.InvariantCultureIgnoreCase)) {
          log.Info ("Connect: NCK type => Add event 'OnStateChanged'");
          m_machine.OnStateChanged += OnStateChanged;
        }

        if (log.IsInfoEnabled) {
          log.Info ($"Connect: tried to connect to {ConnectionName}, the result will be known on the next start");
        }
      }
      catch (Exception ex) {
        ControlState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_NOT_INITIALIZED;
        ProcessException (ex, "HeidenhainDNC", "Connect", true);
      }
      Application.DoEvents ();
    }

    /// <summary>
    /// Disconnect control (free m_machine and close the interfaces)
    /// Condition: m_machine not null
    /// </summary>
    void Disconnect ()
    {
      if (m_machine == null) {
        log.Warn ("HeidenhainDNC.Disconnect - machine is null already");
        return;
      }

      // Cf heidenhain documentation (C# examples)
      GC.Collect ();
      GC.Collect (); // Yes, 2 times

      log.Fatal ("HeidenhainDNC.Disconnect: try to disconnect");
      try {
        // Unsubscribe all events
        try {
          m_machine.OnStateChanged -= OnStateChanged;

          // Will trigger another disconnection because of this state but m_machine will be null when the disconnection is complete
          ControlState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_NOT_INITIALIZED;
        }
        catch (Exception e) {
          log.ErrorFormat ("HeidenhainDNC.Disconnect - couldn't remove OnStateChanged: {0}", e);
        }

        // End interfaces
        try {
          m_interfaceManager.CloseInterfaces ();
        }
        catch (Exception ex) {
          log.ErrorFormat ("HeidenhainDNC.Disconnect - couldn't end interfaces: {0}", ex);
        }

        // Disconnect the machine
        Application.DoEvents ();
        System.Threading.Thread.Sleep (1000); // Sometimes crash if this pause is removed
        try {
          m_machine.Disconnect ();
        }
        catch (COMException cex) {
          if (cex.ErrorCode == Convert.ToInt32 (HeidenhainDNCLib.DNC_HRESULT.DNC_E_NOT_POS_NOW)) {
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!! VERY IMPORTANT !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // If Disconnect() returns HRESULT = 0x80040266 / DNC_HRESULT.DNC_E_NOT_POS_NOW,
            // you probably forgot to release some HeidenhainDNC resources!
            // In most cases of the DNC_E_NOT_POS_NOW HRESULT, the Disconnect() is not possible,
            // because there are still HeidenhainDNC resources like the "interfaces",
            // "helper objects" or "events" in use.
            // --> Please release them before calling the Disconnect() method to avoid memory leaks!
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!! VERY IMPORTANT !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            log.FatalFormat ("HeidenhainDNC.Disconnect - the disconnection resulted in a 'DNC_E_NOT_POS_NOW' error, " +
                            "meaning that all resources have not been released!");
            try {
              // This 2nd Disconnect forces to Disconnect() from control, even if there
              // are some HeidenhainDNC resources in use. This should never happen.
              // Please ensure to release all requested HeidenhainDNC resources!
              m_machine.Disconnect ();
            }
            catch (Exception e) {
              log.ErrorFormat ("HeidenhainDNC.Disconnect - couldn't disconnect a second time: {0}", e);
            }
          }
          else {
            log.ErrorFormat ("HeidenhainDNC.Disconnect - couldn't disconnect: COM Exception {0} - {1}", cex.ErrorCode, cex.Message);
          }
        }
        catch (Exception e) {
          log.ErrorFormat ("HeidenhainDNC.Disconnect - couldn't disconnect: {0}", e);
        }
        finally {
          try {
            Marshal.ReleaseComObject (m_machine);
          }
          catch (Exception e) {
            log.ErrorFormat ("HeidenhainDNC.Disconnect - couldn't release the machine: {0}", e);
          }
          m_machine = null;
        }
        log.Info ("HeidenhainDNC.Disconnect - successfully disconnected");
      }
      catch (Exception ex) {
        ProcessException (ex, "HeidenhainDNC", "Disconnect", false);
      }

      m_machine = null;
      Application.DoEvents ();
    }
    #endregion // Methods

    #region Event reactions
    /// <summary>
    /// Event coming from the machine
    /// Only used for NCK based control (the state is also checked in the method "Start")
    /// </summary>
    /// <param name="eventValue"></param>
    void OnStateChanged (HeidenhainDNCLib.DNC_EVT_STATE eventValue)
    {
      if (m_isDisposed) {
        return;
      }

      log.WarnFormat ("HeidenhainDNC.OnStateChanged - event received: '{0}'", eventValue);

      try {
        // Update the state
        ControlState = ComputeNewState (ControlState, eventValue);
      }
      catch (Exception ex) {
        ProcessException (ex, "HeidenhainDNC", "OnStateChanged", true);
      }
    }
    #endregion // Event reactions
  }
}
