// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Fanuc input module
  /// </summary>
  public partial class Fanuc : Lemoine.Cnc.BaseCncModule, Lemoine.Cnc.ICncModule, IDisposable
  {
    #region Members
    bool m_disposed = false; // Track whether Dispose has been called.
    ushort m_handle = 0;
    bool m_connected = false;
    readonly IList<string> m_notAvailableMethods = new List<string> ();
    #endregion

    #region Getters / Setters
    /// <summary>
    /// The connection parameter can be one of the following values:
    /// <entry>host</entry>
    /// <entry>host:port</entry>
    /// <entry>host:port/y</entry>
    /// <entry>HSSB</entry>
    /// <entry>HSSBx</entry>
    /// <entry>HSSBx/y</entry>
    /// where:
    /// <entry>host is either an IP address of a host name</entry>
    /// <entry>port is the used port number</entry>
    /// <entry>y is a path number</entry>
    /// <entry>x is a node number between 0 and 7</entry>
    /// 
    /// In case the port number is not given, the default port number 8193 is used.
    /// </summary>
    public string ConnectionParameter { get; set; }

    /// <summary>
    /// Kind of CNC / CNC type
    /// 
    /// <item>'15' : Series 150/150i</item>
    /// <item>'16' : Series 160/160i</item>
    /// <item>'18' : Series 180/180i</item>
    /// <item>'21' : Series 210/210i</item>
    /// <item>'30i': Series 300i</item>
    /// <item>'31i': Series 310i</item>
    /// <item>'32i': Series 320i</item>
    /// <item>'35i': Series 350i</item>
    /// <item>'0i' : Series 0i</item>
    /// <item>'PDi': Power Mate i-D</item>
    /// <item>'PHi': Power Mate i-H</item>
    /// <item>'PMi': Power Motion i</item>
    /// 
    /// This is suffixed by 'i' in case of a i-Series
    /// For example: 16i
    /// 
    /// It may be suffixed by the model information as well (-A, -B, -C, -D)
    /// </summary>
    public string CncKind { get; private set; }

    /// <summary>
    /// Version number of CNC
    /// </summary>
    public string VersionNumber { get; private set; }

    /// <summary>
    /// Series number of CNC
    /// </summary>
    public string SeriesNumber { get; private set; }

    /// <summary>
    /// Kind of M/T (ASCII)
    /// 
    /// <item>' M' : Machining center</item>
    /// <item>' T' : Lathe</item>
    /// <item>'MM' : M series with 2 path control</item>
    /// <item>'TT' : T series with 2/3 path control</item>
    /// <item>'MT' : T series with compound machining function</item>
    /// <item>' P' : Punch press</item>
    /// <item>' L' : Laser</item>
    /// <item>' W' : Wire cut</item>
    /// </summary>
    public string MTKind { get; private set; }

    /// <summary>
    /// Current controlled axes
    /// </summary>
    public short? ControlledAxisNumber { get; private set; }

    /// <summary>
    /// Max Axis number
    /// </summary>
    public short? MaxAxisNumber { get; private set; }

    /// <summary>
    /// Does a connection to the control end in error ?
    /// </summary>
    public bool ConnectionError
    {
      get {
        bool result = IsConnectionValid ();
        log.DebugFormat ("ConnectionError.get: " +
                         "connection result is {0}",
                         result);
        return !result;
      }
    }

    /// <summary>
    /// Is there an acquisition error (because of a connection error for example) ?
    /// </summary>
    public bool AcquisitionError
    {
      get {
        return ConnectionError;
      }
    }

    /// <summary>
    /// Manual status
    /// 
    /// MDI, Jog, Handle or SingleBlock
    /// </summary>
    public bool Manual
    {
      get {
        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("Manual.get: " +
                           "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        return this.MDI || this.Jog || this.Handle || this.Reference;
      }
    }

    /// <summary>
    /// Auto mode (program execution)
    /// </summary>
    public bool Auto
    {
      get {
        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("Auto.get: " +
                           "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        if (CncKind.StartsWith ("15")) {
          log.Debug ("Auto.get: " +
                     "machine type 15");
          Import.FwLib.ODBST_150 statInfo = this.StatInfo_15;
          log.DebugFormat ("Auto.get: " +
                           "statinfo returned auto={0} manual={1}",
                           statInfo.aut, statInfo.manual);
          switch ((Import.FwLib.AUTO15)statInfo.aut) {
          case Import.FwLib.AUTO15.TAPE:
          case Import.FwLib.AUTO15.MEMORY:
            return true;
          case Import.FwLib.AUTO15.MDI:
          case Import.FwLib.AUTO15.NO_SELECT:
          case Import.FwLib.AUTO15.TEACHIN:
            return false;
          case Import.FwLib.AUTO15.EDIT:
            throw new Exception ("Auto status not known (EDIT)");
          default:
            log.ErrorFormat ("Auto.get: " +
                             "unknown AUTO15 mode {0}",
                             statInfo.manual);
            throw new Exception ("unknown MANUAL15 mode");
          }
        }
        else { // Not a machine type 15
          Import.FwLib.ODBST statInfo = this.StatInfo;
          log.DebugFormat ("Auto.get: " +
                           "statinfo returned aut={0}",
                           statInfo.aut);
          switch ((Import.FwLib.AUTO16)statInfo.aut) {
          case Import.FwLib.AUTO16.MEMORY:
          case Import.FwLib.AUTO16.REMOTE:
          case Import.FwLib.AUTO16.TEST:
            return true;
          case Import.FwLib.AUTO16.MDI:
          case Import.FwLib.AUTO16.JOG:
          case Import.FwLib.AUTO16.TEACHIN_JOG:
          case Import.FwLib.AUTO16.HANDLE:
          case Import.FwLib.AUTO16.TEACHIN_HANDLE:
          case Import.FwLib.AUTO16.NO_SELECT:
          case Import.FwLib.AUTO16.INC_FEED:
          case Import.FwLib.AUTO16.REFERENCE:
            return false;
          case Import.FwLib.AUTO16.EDIT:
            throw new Exception ("Auto status not known (EDIT)");
          default:
            log.ErrorFormat ("Auto.get: " +
                             "unknown AUTO16 mode {0}",
                             statInfo.aut);
            throw new Exception ("unknown AUTO16 mode");
          }
        }
      }
    }
    #endregion

    #region Constructors / Destructor / ToString methods
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Fanuc () : base ("Lemoine.Cnc.In.Fanuc")
    {
      SeriesNumber = "";
      VersionNumber = "";
      CncKind = "";
      MTKind = "";
      ConnectionParameter = null;
      ToolLifeDataInput = "fanuc";
      MachineAlarmInput = "none";
    }

    /// <summary>
    /// Destructor: free unmanaged resources in case Dispose has not been called
    /// </summary>
    ~Fanuc ()
    {
      Dispose (false);
    }

    /// <summary>
    /// Dispose method to free Fanuc resources
    /// Do not make this method virtual.
    /// A derived class should not be able to override this method.
    /// <see cref="IDisposable.Dispose" />
    /// </summary>
    public void Dispose ()
    {
      Dispose (true);
      // This object will be cleaned up by the Dispose method.
      // Therefore, you should call GC.SupressFinalize to
      // take this object off the finalization queue
      // and prevent finalization code for this object
      // from executing a second time.
      GC.SuppressFinalize (this);
    }

    /// <summary>
    /// Dispose(bool disposing) executes in two distinct scenarios.
    /// If disposing equals true, the method has been called directly
    /// or indirectly by a user's code. Managed and unmanaged resources
    /// can be disposed.
    /// If disposing equals false, the method has been called by the
    /// runtime from inside the finalizer and you should not reference
    /// other objects. Only unmanaged resources can be disposed.
    /// 
    /// Note all the variables are not really needed here.
    /// But they are nevertheless here because this class could be used an example
    /// for other classes that could need them.
    /// </summary>
    /// <param name="disposing">Dispose also the managed resources</param>
    protected virtual void Dispose (bool disposing)
    {
      // Check to see if Dispose has already been called.
      if (!this.m_disposed) {
        // If disposing equals true, dispose all managed
        // and unmanaged resources.
        if (disposing) {
          // Dispose managed resources
        }

        // Call the appropriate methods to clean up
        // unmanaged resources here.
        // If disposing is false,
        // only the following code is executed.
        Disconnect ();
      }
      m_disposed = true;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Start method
    /// </summary>
    /// <returns></returns>
    public bool Start ()
    {
      // do not reset m_version and m_maxAxis
      m_spindleLoad.Clear ();
      m_spindleSpeed.Clear ();
      ResetCncProgramCache ();
      m_statInfo = null;
      m_statInfo_15 = null;

      // Alarms
      m_alarms = null;
      m_operatorMessages = null;
      m_machineAlarms = null;

      // Tool management
      m_toolLifeDataInitialized = false;
      m_toolOffsetInformationInitialized = false;

      return IsConnectionValid ();
    }

    /// <summary>
    /// Finish method
    /// </summary>
    /// <returns></returns>
    public void Finish ()
    {
      // Nothing
    }

    /// <summary>
    /// Check if the connection with the CNC is up. If not, connect to it
    /// </summary>
    /// <returns>The connection was successful</returns>
    public bool IsConnectionValid ()
    {
      if (!m_connected) {
        log.Info ("CheckConnection: " +
                  "the CNC is not connected: try to connect");

        // 1. Parse the connection parameters
        //    and initialize the connection
        if (null == this.ConnectionParameter) {
          log.Error ("CheckConnection: " +
                     "no connection parameter is given");
          return false;
        }
        Import.FwLib.EW result;
        string[] parameters = this.ConnectionParameter.Split ('/');
        if (this.ConnectionParameter.Equals ("LOCAL")
            || this.ConnectionParameter.Equals ("HSSB")) { // Local
          log.Debug ("CheckConnection: " +
                     "connecting locally...");
          result = (Import.FwLib.EW)Import.FwLib.Cnc.allclibhndl (out m_handle);
        }
        else if (this.ConnectionParameter.StartsWith ("HSSB")) { // HSSBx[/y]
          if (false == int.TryParse (parameters[0].Substring (4), out var node)) {
            log.WarnFormat ("CheckConnection: " +
                            "parsing the connection parameter {0} " +
                            "failed to retrieve the node",
                            this.ConnectionParameter);
          }
          else {
            log.DebugFormat ("CheckConnection: " +
                             "got node {0} " +
                             "in connection parameter {1}",
                             node,
                             this.ConnectionParameter);
          }
          result = (Import.FwLib.EW)Import.FwLib.Cnc.allclibhndl2 (node, out m_handle);
        }
        else { // Ethernet
          ushort port = 8193;
          string[] ethernetParameters = parameters[0].Split (':');
          if (ethernetParameters.Length > 1) {
            if (false == ushort.TryParse (ethernetParameters[1],
                                          out port)) {
              log.WarnFormat ("CheckConnection: " +
                              "parsing the connection parameter {0} " +
                              "to retrieve the port number failed " +
                              "=> use the default port 8193",
                              this.ConnectionParameter);
              port = 8193;
            }
            else {
              log.DebugFormat ("CheckConnection: " +
                               "got port number {0} " +
                               "in connection parameter {1}",
                               port,
                               this.ConnectionParameter);
            }
          }
          if (false == IPAddress.TryParse (ethernetParameters[0], out var ipAddress)) {
            log.DebugFormat ("CheckConnection: " +
                             "{0} is not an IP address " +
                             "=> try to convert it to an IP address",
                             ethernetParameters[0]);
            IPAddress[] ipAddresses;
            try {
              ipAddresses = Dns.GetHostAddresses (ethernetParameters[0]);
            }
            catch (Exception ex) {
              log.ErrorFormat ("CheckConnection: " +
                               "GetHostAddresses failed for {0} with {1}",
                               ethernetParameters[0],
                               ex);
              throw;
            }
            if (0 == ipAddresses.Length) {
              log.ErrorFormat ("CheckConnection: " +
                               "could not determine an IP address for " +
                               "connection parameter {0}",
                               this.ConnectionParameter);
              throw new Exception ("No valid IP Address from connection parameter");
            }
            ipAddress = ipAddresses[0];
          }
          log.DebugFormat ("CheckConnection:" +
                           "consider IP {0} for {1}",
                           ipAddress, ethernetParameters[0]);
          result = (Import.FwLib.EW)Import.FwLib.Cnc.allclibhndl3 (ipAddress.ToString (),
                                                                    port,
                                                                    10, // timeout
                                                                    out m_handle);
        }
        if (result != Import.FwLib.EW.OK) {
          log.ErrorFormat ("CheckConnection: " +
                           "allclibhndl3 failed with error {0} " +
                           "and connection parameter {1}",
                           result,
                           this.ConnectionParameter);
          return false;
        }
        else {
          log.DebugFormat ("CheckConnection: " +
                           "allclibhndlx was successful with " +
                           "connection parameter {0}",
                           this.ConnectionParameter);
        }

        // 2. Set the path (if any)
        if (parameters.Length > 1) { // xxx/y, where y is the path number
          if (!short.TryParse (parameters[1], out var path)) {
            log.ErrorFormat ("CheckConnection: " +
                             "parsing the connection parameter {0} " +
                             "to get the path number failed",
                             this.ConnectionParameter);
          }
          else {
            log.DebugFormat ("CheckConnection: " +
                             "got path {0} from connection parameter {1}",
                             path,
                             this.ConnectionParameter);
            result = (Import.FwLib.EW)Import.FwLib.Cnc.setpath (m_handle, path);
            if (result != Import.FwLib.EW.OK) {
              log.ErrorFormat ("CheckConnection: " +
                               "setpath failed with error {0} " +
                               "and connection parameter {1}",
                               result,
                               this.ConnectionParameter);
            }
            else {
              log.DebugFormat ("CheckConnection: " +
                               "setpath was successful with " +
                               "connection parameter {0}",
                               this.ConnectionParameter);
            }
          }
        } // End parameters.Length > 1 for Path

        // 3. Get the version
        result = (Import.FwLib.EW)Import.FwLib.Cnc.sysinfo (m_handle, out var sysInfo);
        if (result != Import.FwLib.EW.OK) {
          log.ErrorFormat ("CheckConnection: " +
                           "sysinfo failed with error {0} " +
                           "=> unknown version",
                           result);
          CncKind = ""; // Unknown cnc kind
          MTKind = ""; // Unknown M/T kind
          VersionNumber = ""; // Unknown version number
          SeriesNumber = ""; // Unknown series number
          ControlledAxisNumber = null;
        }
        else {
          log.Debug ("CheckConnection: " +
                     "sysinfo was successful");
          CncKind = "";
          if (' ' != sysInfo.cnc_type[0]) {
            CncKind += sysInfo.cnc_type[0];
          }
          CncKind += sysInfo.cnc_type[1];
          if (GetBit (sysInfo.addinfo1, 1)) { // bit 1 = i Series CNC
            CncKind += "i";
          }
          var modelInformation = sysInfo.addinfo2;
          if (0 != modelInformation) {
            switch (modelInformation) {
            case 1:
              CncKind += "-A";
              break;
            case 2:
              CncKind += "-B";
              break;
            case 3:
              CncKind += "-C";
              break;
            case 4:
              CncKind += "-D";
              break;
            }
          }
          MTKind = "";
          if (' ' != sysInfo.mt_type[0]) {
            MTKind += sysInfo.mt_type[0];
          }
          MTKind += sysInfo.mt_type[1];
          VersionNumber = "";
          VersionNumber += sysInfo.version[0];
          VersionNumber += sysInfo.version[1];
          VersionNumber += sysInfo.version[2];
          VersionNumber += sysInfo.version[3];
          SeriesNumber = "";
          SeriesNumber += sysInfo.series[0];
          SeriesNumber += sysInfo.series[1];
          SeriesNumber += sysInfo.series[2];
          SeriesNumber += sysInfo.series[3];
          MaxAxisNumber = sysInfo.max_axis;
          string controlledAxis = "";
          if (' ' != sysInfo.axes[0]) {
            controlledAxis += sysInfo.axes[0];
          }
          controlledAxis += sysInfo.axes[1];
          ControlledAxisNumber = null;
          if (!short.TryParse (controlledAxis, out var controlledAxisNumber)) {
            log.Error ($"CheckConnection: could not parse the number of controlled axis {controlledAxis} => use {MaxAxisNumber}");
            ControlledAxisNumber = MaxAxisNumber;
          }
          else {
            ControlledAxisNumber = controlledAxisNumber;
          }
          if (log.IsInfoEnabled) {
            log.Info ($"CheckConnection: CncKind={CncKind} Version#={VersionNumber} Series#={SeriesNumber} MaxAxisNumber={MaxAxisNumber} ControlledAxisNumber={ControlledAxisNumber}");
          }
        }

        m_connected = true;
      }

      return true;
    }

    /// <summary>
    /// Log an error and raise an exception when the connection is not valid
    /// </summary>
    public void CheckConnection ()
    {
      if (!IsConnectionValid ()) {
        var stackTrace = new StackTrace ();
        var callingMethod = stackTrace.GetFrame (1).GetMethod ().Name;
        log.ErrorFormat ("CheckConnection: the connection is not valid in {0}",
          callingMethod);
        throw new Exception ("No CNC connection");
      }
    }

    /// <summary>
    /// Check the availability of the method
    /// 
    /// An exception is raised if it is not available
    /// </summary>
    /// <param name="method">not null and not empty</param>
    void CheckAvailability (string method)
    {
      Debug.Assert (!string.IsNullOrEmpty (method));

      if (m_notAvailableMethods.Contains (method)) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("CheckAvailability: {0} is not available", method);
        }
        throw new Exception ("Method not available");
      }
    }
    #endregion
  }
}
