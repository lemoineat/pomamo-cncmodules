// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using Lemoine.Core.Log;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of ConnectionManager.
  /// </summary>
  public class ConnectionManager
  {
    #region Event
    /// <summary>
    /// Event emitted when a connection is required
    /// </summary>
    public event Action ConnectionRequired;
    
    /// <summary>
    /// Event emitted when a disconnection is required
    /// </summary>
    public event Action DisconnectionRequired;
    
    /// <summary>
    /// Logger
    /// </summary>
    public ILog Logger { get; set; }
    #endregion // Events
    
    #region Members
    DateTime m_lastDisconnect, m_lastConnect;
    bool m_manualDisconnectionRequired = false;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// When the acquisition is in error, this is the duration after which
    /// we try nevertheless to connect
    /// Default is 5 minutes
    /// </summary>
    public TimeSpan SecureDuration { get; set; }
    
    /// <summary>
    /// Period after a disconnection with no connection and no disconnection
    /// Default is 10 seconds
    /// </summary>
    public TimeSpan DisconnectionInterval { get; set; }
    
    /// <summary>
    /// Period after a connection with no connection and no disconnection
    /// Default is 15 seconds
    /// </summary>
    public TimeSpan ConnectionInterval { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ConnectionManager()
    {
      Logger = LogManager.GetLogger("Lemoine.Cnc.In.HeidenhainDNC");
      
      // Default durations
      SecureDuration = TimeSpan.FromMinutes(5);
      DisconnectionInterval = TimeSpan.FromSeconds(10);
      ConnectionInterval = TimeSpan.FromSeconds(15);
      
      m_lastConnect = new DateTime(1970, 1, 1);
      m_lastDisconnect = new DateTime(1970, 1, 1);
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Method to call on start of the acquisition module
    /// The connection manager takes the decision - or not - to connect / disconnect
    /// </summary>
    /// <param name="acquisitionOk">If false, a Connect or Disconnect can be ordered</param>
    /// <param name="disconnectAllowed">If true, a disconnection will be ordered prior to a connection if the acquisition is not ok</param>
    public void Start(bool acquisitionOk, bool disconnectAllowed)
    {
      // Cancel a manual disconnection if its not allowed
      if (!disconnectAllowed && m_manualDisconnectionRequired) {
        Logger.Info("HeidenhainDNC.ConnectionManager - Cancel a manual disconnection because the module is already disconnected");
        m_manualDisconnectionRequired = false;
      }
      
      // Then we check if a manual disconnection is required
      if (m_manualDisconnectionRequired) {
        AskForDisconnect();
        return;
      }
      
      // Then, if the acquisition is ok => nothing to do
      if (acquisitionOk) {
        return;
      }

      // The acquisition is not ok, we either disconnect first if needed or connect
      if (disconnectAllowed) {
        // The secure duration is taken into account here: we don't disconnect quickly contrary to the manual disconnections
        if (DateTime.Now.Subtract(m_lastConnect) > SecureDuration) {
          AskForDisconnect ();
        }
        else {
          Logger.Warn("HeidenhainDNC.ConnectionManager - Waiting for the secure duration or a manual action before disconnecting");
        }
      } else {
        AskForConnect ();
      }
    }
    
    /// <summary>
    /// Manual ask for a disconnection (occurring during a next Start)
    /// </summary>
    public void DisconnectLater()
    {
      Logger.Warn("HeidenhainDNC.ConnectionManager - A manual disconnection is ordered");
      m_manualDisconnectionRequired = true;
    }
    
    /// <summary>
    /// Manually ask to cancel the disconnection query
    /// </summary>
    public void CancelDisconnectQuery()
    {
      Logger.Warn("HeidenhainDNC.ConnectionManager - any disconnection query is now canceled");
      m_manualDisconnectionRequired = false;
    }
    #endregion // Methods
    
    #region Private methods
    void AskForConnect()
    {
      // Do nothing if the intervals are too short
      if (DateTime.Now.Subtract(m_lastDisconnect) < DisconnectionInterval ||
          DateTime.Now.Subtract(m_lastConnect) < ConnectionInterval) {
        Logger.Warn("HeidenhainDNC.ConnectionManager - Ask for connect => wait");
        return;
      }
      
      m_lastConnect = DateTime.Now;
      if (ConnectionRequired != null) {
        ConnectionRequired ();
      }
    }
    
    void AskForDisconnect()
    {
      // Do nothing if the intervals are too short
      if (DateTime.Now.Subtract(m_lastDisconnect) < DisconnectionInterval ||
          DateTime.Now.Subtract(m_lastConnect) < ConnectionInterval) {
        Logger.Warn("HeidenhainDNC.ConnectionManager - Ask for disconnect => wait");
        return;
      }
      
      m_lastDisconnect = DateTime.Now;
      m_manualDisconnectionRequired = false;
      if (DisconnectionRequired != null) {
        DisconnectionRequired ();
      }
    }
    #endregion // Private methods
  }
}
