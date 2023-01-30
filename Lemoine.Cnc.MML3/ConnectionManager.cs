// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.IO;
using System.Reflection;
using Lemoine.Info;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Connection manager that allows or not a connection
  /// </summary>
  public class ConnectionManager
  {
    #region Members
    DateTime m_proXconnectionDate;
    DateTime m_cncConnectionDate;
    readonly int CONNECTION_DELAY = 60; // In seconds
    #endregion Members

    #region Constructor, destructor
    /// <summary>
    /// Default constructor
    /// </summary>
    public ConnectionManager ()
    {
      // Initialize the dates so that a connection is already possible
      m_cncConnectionDate = m_proXconnectionDate = DateTime.UtcNow.AddSeconds (-CONNECTION_DELAY-1);
    }
    #endregion Constructor, destructor

    #region Methods
    /// <summary>
    /// Return true if we can try a connection for ProX
    /// </summary>
    /// <returns></returns>
    public bool CanConnectProX ()
    {
      DateTime currentTime = DateTime.UtcNow;
      if (currentTime > m_proXconnectionDate.AddSeconds (CONNECTION_DELAY)) {
        m_proXconnectionDate = currentTime;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Return true if we can try a connection for Cnc
    /// </summary>
    /// <returns></returns>
    public bool CanConnectCnc ()
    {
      DateTime currentTime = DateTime.UtcNow;
      if (currentTime > m_cncConnectionDate.AddSeconds (CONNECTION_DELAY)) {
        m_cncConnectionDate = currentTime;
        return true;
      }
      return false;
    }
    #endregion Methods
  }
}
