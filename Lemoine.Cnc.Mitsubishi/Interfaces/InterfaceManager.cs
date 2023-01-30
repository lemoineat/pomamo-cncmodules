// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;
using EZNCAUTLib;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of InterfaceManager.
  /// </summary>
  public class InterfaceManager
  {
    /// <summary>
    /// Interfaces provided by the communication object
    /// May depend on the system type:
    /// (1) Magic64, M6x5M, M6x5L, C64, CNC700
    /// (2) C70, M700, M800
    /// </summary>
    public enum InterfaceType
    {
      /// <summary>
      /// COMMUNICATION_2 for (1) and COMMUNICATION_3 for (2)
      /// </summary>
      COMMUNICATION,

      /// <summary>
      /// SYSTEM
      /// </summary>
      SYSTEM,

      /// <summary>
      /// POSITION
      /// </summary>
      POSITION,

      /// <summary>
      /// COMMAND_2
      /// </summary>
      COMMAND,

      /// <summary>
      /// PROGRAM_2
      /// </summary>
      PROGRAM,

      /// <summary>
      /// TIME
      /// </summary>
      TIME,

      /// <summary>
      /// AXIS_MONITOR
      /// </summary>
      AXIS_MONITOR,

      /// <summary>
      /// RUN_STATUS
      /// </summary>
      RUN_STATUS,

      /// <summary>
      /// FILE_4 for (1) and FILE_6 for (2)
      /// </summary>
      FILE,

      /// <summary>
      /// COMMON_VARIABLE_2
      /// </summary>
      COMMON_VARIABLE,

      /// <summary>
      /// LOCAL_VARIABLE_2
      /// </summary>
      LOCAL_VARIABLE,

      /// <summary>
      /// TOOL_3
      /// </summary>
      TOOL,

      /// <summary>
      /// ATC for (1) and ATC_3 for (2)
      /// </summary>
      ATC,

      /// <summary>
      /// PARAMETER_2 for (1) and PARAMETER_3 for (2)
      /// </summary>
      PARAMETER,

      /// <summary>
      /// OPERATION
      /// </summary>
      OPERATION,

      /// <summary>
      /// DEVICE
      /// </summary>
      DEVICE,

      /// <summary>
      /// GENERIC_2, only for (1)
      /// </summary>
      GENERIC,

      /// <summary>
      /// SUB_FUNCTION_2
      /// </summary>
      SUB_FUNCTION
    }

    #region Members
    readonly IDictionary<InterfaceType, IMitsubishiInterface> m_interfaces = new Dictionary<InterfaceType, IMitsubishiInterface> ();
    DispEZNcCommunication m_commObject = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Logger
    /// </summary>
    public ILog Logger { get; set; }

    /// <summary>
    /// True is the connection to the machine is open
    /// </summary>
    public bool ConnectionOpen { get; set; }

    /// <summary>
    /// System type of the connected machine
    /// </summary>
    public Mitsubishi.MitsubishiSystemType SystemType { get; set; }

    /// <summary>
    /// Get the communication interface
    /// </summary>
    public Interface_communication InterfaceCommunication { get { return GetInterface<Interface_communication> (InterfaceType.COMMUNICATION); } }

    /// <summary>
    /// Get the system interface
    /// </summary>
    public Interface_system InterfaceSystem { get { return GetInterface<Interface_system> (InterfaceType.SYSTEM); } }

    /// <summary>
    /// Get the position interface
    /// </summary>
    public Interface_position InterfacePosition { get { return GetInterface<Interface_position> (InterfaceType.POSITION); } }

    /// <summary>
    /// Get the command interface
    /// </summary>
    public Interface_command InterfaceCommand { get { return GetInterface<Interface_command> (InterfaceType.COMMAND); } }

    /// <summary>
    /// Get the program interface
    /// </summary>
    public Interface_program InterfaceProgram { get { return GetInterface<Interface_program> (InterfaceType.PROGRAM); } }

    /// <summary>
    /// Get the time interface
    /// </summary>
    public Interface_time InterfaceTime { get { return GetInterface<Interface_time> (InterfaceType.TIME); } }

    /// <summary>
    /// Get the axis monitor interface
    /// </summary>
    public Interface_axis_monitor InterfaceAxisMonitor { get { return GetInterface<Interface_axis_monitor> (InterfaceType.AXIS_MONITOR); } }

    /// <summary>
    /// Get the run status interface
    /// </summary>
    public Interface_run_status InterfaceRunStatus { get { return GetInterface<Interface_run_status> (InterfaceType.RUN_STATUS); } }

    /// <summary>
    /// Get the file interface
    /// </summary>
    public Interface_file InterfaceFile { get { return GetInterface<Interface_file> (InterfaceType.FILE); } }

    /// <summary>
    /// Get the common variable interface
    /// </summary>
    public Interface_common_variable InterfaceCommonVariable { get { return GetInterface<Interface_common_variable> (InterfaceType.COMMON_VARIABLE); } }

    /// <summary>
    /// Get the local variable interface
    /// </summary>
    public Interface_local_variable InterfaceLocalVariable { get { return GetInterface<Interface_local_variable> (InterfaceType.LOCAL_VARIABLE); } }

    /// <summary>
    /// Get the tool interface
    /// </summary>
    public Interface_tool InterfaceTool { get { return GetInterface<Interface_tool> (InterfaceType.TOOL); } }

    /// <summary>
    /// Get the ATC interface
    /// </summary>
    public Interface_atc InterfaceATC { get { return GetInterface<Interface_atc> (InterfaceType.ATC); } }

    /// <summary>
    /// Get the parameter interface
    /// </summary>
    public Interface_parameter InterfaceParameter { get { return GetInterface<Interface_parameter> (InterfaceType.PARAMETER); } }

    /// <summary>
    /// Get the operation interface
    /// </summary>
    public Interface_operation InterfaceOperation { get { return GetInterface<Interface_operation> (InterfaceType.OPERATION); } }

    /// <summary>
    /// Get the device interface
    /// </summary>
    public Interface_device InterfaceDevice { get { return GetInterface<Interface_device> (InterfaceType.DEVICE); } }

    /// <summary>
    /// Get the generic interface
    /// </summary>
    public Interface_generic InterfaceGeneric { get { return GetInterface<Interface_generic> (InterfaceType.GENERIC); } }

    /// <summary>
    /// Get the sub function interface
    /// </summary>
    public Interface_sub_function InterfaceSubFunction { get { return GetInterface<Interface_sub_function> (InterfaceType.SUB_FUNCTION); } }
    #endregion // Getters / Setters

    #region Constructor
    /// <summary>
    /// Default constructor
    /// </summary>
    public InterfaceManager ()
    {
      ConnectionOpen = false;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Create a communication object if not done yet
    /// Return true if ok
    /// </summary>
    /// <returns></returns>
    public bool Start ()
    {
      // Return false if the system type is unknown
      if (SystemType == Mitsubishi.MitsubishiSystemType.UNKNOWN) {
        Logger.Error ("Mitsubishi - Unknown system type");
        return false;
      }

      // Create the communication object if necessary
      if (m_commObject == null) {
        try {
          Logger.Info ("Mitsubishi - Creating the communication object");
          m_commObject = new DispEZNcCommunication ();
        }
        catch (Exception ex) {
          Logger.ErrorFormat ("Mitsubishi - Couldn't create the communication object: {0}", ex);

          // Release everything
          this.Close ();

          return false;
        }
      }

      // Initialize the interfaces when the system is kwown
      GenericMitsubishiInterface.CommunicationObject = m_commObject;
      if (m_interfaces.Count == 0) {
        InitializeInterfaces ();
      }

      return true;
    }

    void InitializeInterfaces ()
    {
      Logger.Info ("Mitsubishi - Initializing interfaces");

      // Global attributes for the interfaces
      GenericMitsubishiInterface.SystemType = SystemType;
      GenericMitsubishiInterface.Logger = Logger;

      // Common interfaces for all system types
      m_interfaces[InterfaceType.SYSTEM] = new Interface_system ();
      m_interfaces[InterfaceType.POSITION] = new Interface_position ();
      m_interfaces[InterfaceType.COMMAND] = new Interface_command_2 ();
      m_interfaces[InterfaceType.PROGRAM] = new Interface_program_2 ();
      m_interfaces[InterfaceType.TIME] = new Interface_time ();
      m_interfaces[InterfaceType.AXIS_MONITOR] = new Interface_axis_monitor ();
      m_interfaces[InterfaceType.RUN_STATUS] = new Interface_run_status ();
      m_interfaces[InterfaceType.COMMON_VARIABLE] = new Interface_common_variable_2 ();
      m_interfaces[InterfaceType.LOCAL_VARIABLE] = new Interface_local_variable_2 ();
      m_interfaces[InterfaceType.TOOL] = new Interface_tool_3 ();
      m_interfaces[InterfaceType.ATC] = new Interface_atc ();
      m_interfaces[InterfaceType.OPERATION] = new Interface_operation ();
      m_interfaces[InterfaceType.DEVICE] = new Interface_device ();

      // Interfaces depending on the system type
      switch (SystemType) {
        case Mitsubishi.MitsubishiSystemType.MAGIC_CARD_64:
        case Mitsubishi.MitsubishiSystemType.MELDAS_600L_6X5L:
        case Mitsubishi.MitsubishiSystemType.MELDAS_600M_6X5M:
        case Mitsubishi.MitsubishiSystemType.MELDAS_C6_C64:
          // Old way
          m_interfaces[InterfaceType.COMMUNICATION] = new Interface_communication_2 ();
          m_interfaces[InterfaceType.FILE] = new Interface_file_4 ();
          m_interfaces[InterfaceType.PARAMETER] = new Interface_parameter_2 ();
          m_interfaces[InterfaceType.GENERIC] = new Interface_generic_2 (); // Generic is only here
          m_interfaces[InterfaceType.SUB_FUNCTION] = new Interface_sub_function_2 ();
          break;
        case Mitsubishi.MitsubishiSystemType.MELDAS_700L:
        case Mitsubishi.MitsubishiSystemType.MELDAS_700M:
        case Mitsubishi.MitsubishiSystemType.MELDAS_C70:
        case Mitsubishi.MitsubishiSystemType.MELDAS_800L:
        case Mitsubishi.MitsubishiSystemType.MELDAS_800M:
          // New way
          m_interfaces[InterfaceType.COMMUNICATION] = new Interface_communication_3 ();
          m_interfaces[InterfaceType.FILE] = new Interface_file_6 ();
          m_interfaces[InterfaceType.PARAMETER] = new Interface_parameter_3 ();
          m_interfaces[InterfaceType.SUB_FUNCTION] = new Interface_sub_function_3 ();
          break;
        case Mitsubishi.MitsubishiSystemType.UNKNOWN:
          // Nothing
          break;
      }
    }

    /// <summary>
    /// Reinitialize data, preceeding a new acquisition
    /// </summary>
    public void ResetData ()
    {
      foreach (IMitsubishiInterface elt in m_interfaces.Values) {
        elt.ResetData ();
      }
    }

    /// <summary>
    /// Free all resources if necessary
    /// </summary>
    public void Close ()
    {
      // Close the communication object if needed
      if (ConnectionOpen) {
        try {
          int errorCode = m_commObject.Close ();
          if (errorCode != 0) {
            throw new ErrorCodeException (errorCode, "Close");
          }
        }
        catch (Exception ex) {
          Logger.ErrorFormat ("Mitsubishi - Failed to close the communication object: {0}", ex.Message);
        }
        ConnectionOpen = false;
      }

      // Release the COM object
      Logger.Info ("Releasing the COM object");
      System.Runtime.InteropServices.Marshal.ReleaseComObject (m_commObject);
      GenericMitsubishiInterface.CommunicationObject = null;
      Logger.Info ("COM object successfully released");
      m_commObject = null;
    }

    T GetInterface<T> (InterfaceType interfaceType) where T : GenericMitsubishiInterface
    {
      // Check if the interface can be used
      if (!m_interfaces.ContainsKey (interfaceType)) {
        throw new Exception ("Mitsubishi - Interface type '" + interfaceType + "' is not defined");
      }

      if (!ConnectionOpen && interfaceType != InterfaceType.COMMUNICATION) {
        throw new Exception ("Mitsubishi - Connection not open");
      }

      // Cast the interface
      IMitsubishiInterface result = m_interfaces[interfaceType] as T;
      if (result == null) {
        throw new Exception ("Mitsubishi - wrong type for the interface (check the code!)");
      }

      return result as T;
    }
    #endregion // Methods
  }
}
