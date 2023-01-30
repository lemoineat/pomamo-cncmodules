// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Class containing all necessary parameters to initialize the interfaces
  /// </summary>
  public class InterfaceParameters
  {
    #region Members
    readonly IDictionary<HeidenhainDNCLib.DNC_ACCESS_MODE, string> m_passwords = new Dictionary<HeidenhainDNCLib.DNC_ACCESS_MODE, string>();
    #endregion // Members

    #region Methods
    /// <summary>
    /// Set passwords used to read data
    /// </summary>
    /// <param name="type"></param>
    /// <param name="password"></param>
    public void SetPassword(HeidenhainDNCLib.DNC_ACCESS_MODE type, string password)
    {
      m_passwords[type] = password;
    }
    
    /// <summary>
    /// Get a password stored
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public string GetPassword(HeidenhainDNCLib.DNC_ACCESS_MODE type)
    {
      return m_passwords.ContainsKey(type) ? m_passwords[type] : "";
    }
    #endregion // Methods
  }
}
