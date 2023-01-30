// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Lemoine.Core.Log;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of InterfaceManager.
  /// </summary>
  public class InterfaceManager
  {
    #region Members
    readonly IDictionary<HeidenhainDNCLib.DNC_INTERFACE_OBJECT, IInterface> m_interfaces = new Dictionary<HeidenhainDNCLib.DNC_INTERFACE_OBJECT, IInterface>();
    readonly InterfaceParameters m_parameters = new InterfaceParameters();
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Current state of the machine
    /// </summary>
    public HeidenhainDNCLib.DNC_STATE ControlState { get; set; }
    
    /// <summary>
    /// Logger
    /// </summary>
    public ILog Logger {
      get { return log; }
      set {
        log = value;
        foreach (var elt in m_interfaces) {
          elt.Value.Logger = value;
        }
      }
    }
    ILog log = LogManager.GetLogger("Lemoine.Cnc.In.HeidenhainDNC");
    
    /// <summary>
    /// Get the configuration interface
    /// Throw an exception if not available
    /// </summary>
    public Interface_configuration InterfaceConfiguration {
      get {
        return GetInterface(HeidenhainDNCLib.DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHCONFIGURATION) as Interface_configuration;
      }
    }
    
    /// <summary>
    /// Get the data interface
    /// Throw an exception if not available
    /// </summary>
    public Interface_data InterfaceData {
      get {
        return GetInterface(HeidenhainDNCLib.DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHDATAACCESS) as Interface_data;
      }
    }
    
    /// <summary>
    /// Get the error interface
    /// Throw an exception if not available
    /// </summary>
    public Interface_error InterfaceError {
      get {
        return GetInterface(HeidenhainDNCLib.DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHERROR) as Interface_error;
      }
    }
    
    /// <summary>
    /// Get the position interface
    /// Throw an exception if not available
    /// </summary>
    public Interface_position InterfacePosition {
      get {
        return GetInterface(HeidenhainDNCLib.DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHAXESPOSITIONSTREAMING) as Interface_position;
      }
    }
    
    /// <summary>
    /// Get the program interface
    /// Throw an exception if not available
    /// </summary>
    public Interface_program InterfaceProgram {
      get {
        return GetInterface(HeidenhainDNCLib.DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHAUTOMATIC) as Interface_program;
      }
    }

    /// <summary>
    /// Get the filesystem interface
    /// Throw an exception if not available
    /// </summary>
    public Interface_fileSystem InterfaceFileSystem
    {
      get {
        return GetInterface(HeidenhainDNCLib.DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHFILESYSTEM) as Interface_fileSystem;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public InterfaceManager ()
    {
      m_interfaces[HeidenhainDNCLib.DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHDATAACCESS] = new Interface_data();
      m_interfaces[HeidenhainDNCLib.DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHCONFIGURATION] = new Interface_configuration();
      m_interfaces[HeidenhainDNCLib.DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHERROR] = new Interface_error();
      m_interfaces[HeidenhainDNCLib.DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHAXESPOSITIONSTREAMING] = new Interface_position();
      m_interfaces[HeidenhainDNCLib.DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHAUTOMATIC] = new Interface_program ();
      m_interfaces[HeidenhainDNCLib.DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHFILESYSTEM] = new Interface_fileSystem ();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Initialize all interfaces based on a machine
    /// </summary>
    public void Initialize(HeidenhainDNCLib.JHMachineInProcess machine)
    {
      // First, close everything
      CloseInterfaces();
      
      // Initialize each interface
      foreach (var element in m_interfaces) {
        
        var interf = element.Value;
        
        Logger.InfoFormat("HeidenhainDNC - initializing interface '{0}'", element.Key);
        if (interf.Initialize(machine, m_parameters)) {
          Logger.InfoFormat("HeidenhainDNC - successfully initialized interface '{0}'", element.Key);
        } else {
          Logger.WarnFormat("HeidenhainDNC - couldn't initialize interface '{0}'", element.Key);
          
          // Interface not initialized, but maybe ok in the past?
          if (interf.ValidInThePast.HasValue && interf.ValidInThePast.Value) {
            throw new COMException("interface " + element.Key + " not accessible anymore", 1638); // 1638 arbitraire (666 en hexa...)
          }

          // Interface data mandatory
          if (element.Key == HeidenhainDNCLib.DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHDATAACCESS) {
            throw new Exception("interface 'data' is not initialized: aborting");
          }
        }
      }
    }
    
    /// <summary>
    /// Reset data acquired by the interfaces (on start)
    /// </summary>
    public void ResetData()
    {
      foreach (var element in m_interfaces) {
        element.Value.ResetData();
      }
    }
    
    /// <summary>
    /// Close the interface and free the resources
    /// </summary>
    public void CloseInterfaces()
    {
      Logger.InfoFormat("HeidenhainDNC - Close all interfaces");
      foreach (var element in m_interfaces) {
        element.Value.Close();
      }
    }
    
    /// <summary>
    /// Set passwords used to read data
    /// </summary>
    /// <param name="type"></param>
    /// <param name="password"></param>
    public void SetPassword(HeidenhainDNCLib.DNC_ACCESS_MODE type, string password)
    {
      Logger.InfoFormat("HeidenhainDNC - set password '{0}' for access '{1}'", password, type);
      m_parameters.SetPassword(type, password);
    }
    
    /// <summary>
    /// Get a password stored
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public string GetPassword(HeidenhainDNCLib.DNC_ACCESS_MODE type)
    {
      return m_parameters.GetPassword(type);
    }
    
    /// <summary>
    /// Return true if at least one interface is ok
    /// </summary>
    /// <returns></returns>
    public bool HasValidInterface()
    {
      foreach (var element in m_interfaces) {
        if (element.Value.Valid) {
          return true;
        }
      }

      return false;
    }
    
    IInterface GetInterface(HeidenhainDNCLib.DNC_INTERFACE_OBJECT type)
    {
      // Existing interface?
      if (!m_interfaces.ContainsKey(type)) {
        throw new Exception("Unknown interface: " + type);
      }

      // Check if the interface is valid
      var result = m_interfaces[type];
      if (!result.Valid) {
        throw new Exception("Interface not available");
      }

      // Finally check if the machine state is ok
      if (ControlState != HeidenhainDNCLib.DNC_STATE.DNC_STATE_MACHINE_IS_AVAILABLE) {
        throw new Exception("Wrong control state: " + ControlState);
      }

      return result;
    }
    #endregion // Methods
  }
}
