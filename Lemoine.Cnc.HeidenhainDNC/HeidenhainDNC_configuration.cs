// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// HeidenhainDNC input module
  /// </summary>
  public partial class HeidenhainDNC
  {
    #region Get methods
    /// <summary>
    /// Return a formatted string to show all axes referenced in the control
    /// </summary>
    public string AllAxes
    {
      get
      {
        string result;
        try {
          result = m_interfaceManager.InterfaceConfiguration.AllAxes;
        }
        catch (Exception ex) {
          ProcessException (ex, "HeidenhainDNC", "GetBoolData", true);
          throw;
        }
        return result;
      }
    }
    #endregion // Get methods
  }
}