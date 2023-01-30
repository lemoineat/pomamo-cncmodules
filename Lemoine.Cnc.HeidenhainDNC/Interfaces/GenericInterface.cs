// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Runtime.InteropServices;
using Lemoine.Core.Log;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of GenericInterface.
  /// The class should throw no exceptions
  /// </summary>
  public abstract class GenericInterface<T> :
    IInterface
    where T: class
  {
    #region Members
    readonly HeidenhainDNCLib.DNC_INTERFACE_OBJECT m_interfaceType;
    
    /// <summary>
    /// Interface taken from the machine
    /// </summary>
    protected T m_interface = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// True if it happened that we had this interface valid, False if never, null if we never checked
    /// </summary>
    public bool? ValidInThePast { get; private set; }
    
    /// <summary>
    /// Return true if the interface can be used
    /// </summary>
    public bool Valid {
      get { return m_interface != null; }
    }
    
    /// <summary>
    /// Logger
    /// </summary>
    public ILog Logger { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="interfaceType">Kind of interface</param>
    protected GenericInterface(HeidenhainDNCLib.DNC_INTERFACE_OBJECT interfaceType)
    {
      m_interfaceType = interfaceType;
      ValidInThePast = null;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Initialize the interface
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="parameters"></param>
    /// <returns>True if success</returns>
    public bool Initialize(HeidenhainDNCLib.JHMachineInProcess machine, InterfaceParameters parameters)
    {
      // Close a previous interface if necessary
      if (m_interface != null) {
        Close ();
      }

      // Check conditions
      if (machine == null) {
        throw new Exception("Cannot initialize the interface '" + m_interfaceType + "' with a null machine");
      }

      // Open a new interface
      bool error = false;
      try {
        m_interface = machine.GetInterface(m_interfaceType) as T;
        if (m_interface != null) {
          InitializeInstance (parameters);
        }
      } catch (NotImplementedException ex) {
        Logger.ErrorFormat("HeidenhainDNC - Cannot initialize the interface {0}, " +
                           "probably because you didn't specify the right kind of machine in the connection configuration: {1}", m_interfaceType, ex);
        error = true;
      } catch (InvalidCastException ex) {
        Logger.ErrorFormat("HeidenhainDNC - Cannot initialize the interface {0}, " +
                           "probably because you didn't specify the right kind of machine in the connection configuration: {1}", m_interfaceType, ex);
        error = true;
      } catch (Exception ex) {
        Logger.ErrorFormat("HeidenhainDNC - Cannot initialize the interface {0}: {1}", m_interfaceType, ex);
        error = true;
      }
      error |= (m_interface == null);
      
      // Update ValidInThePast
      if (!error) {
        ValidInThePast = true;
      }

      if (!ValidInThePast.HasValue) {
        ValidInThePast = false;
      }

      return !error;
    }
    
    /// <summary>
    /// Reinitialize data, preceeding a new acquisition
    /// </summary>
    public void ResetData()
    {
      try {
        ResetDataInstance();
      } catch (Exception ex) {
        Logger.ErrorFormat("HeidenhainDNC - Couldn't reset data for interface '{0}': {1}", m_interfaceType, ex);
      }
    }
    
    /// <summary>
    /// Close the interface and free the resources
    /// </summary>
    public void Close()
    {
      // Do nothing if the interface is already null
      if (m_interface == null) {
        return;
      }

      // Free the resources
      try {
        Marshal.ReleaseComObject(m_interface);
        Logger.InfoFormat("HeidenhainDNC - Successfully closed the interface '{0}'", m_interfaceType);
      } catch (Exception ex) {
        Logger.ErrorFormat("HeidenhainDNC - Couldn't close interface '{0}': {1}", m_interfaceType, ex);
      }
      m_interface = null;
    }
    #endregion // Methods
    
    #region Methods to override
    /// <summary>
    /// Extra work during the initialization
    /// Already within a try catch
    /// </summary>
    /// <param name="parameters"></param>
    protected abstract void InitializeInstance(InterfaceParameters parameters);
    
    /// <summary>
    /// Work to do on "start", before new data will be read
    /// Already within a try catch
    /// </summary>
    protected abstract void ResetDataInstance();
    #endregion // Methods to override
  }
}
