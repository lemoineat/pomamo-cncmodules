// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;

namespace Lemoine.Cnc
{
  /// <summary>
  /// HeidenhainDNC input module
  /// </summary>
  public partial class HeidenhainDNC
  {
    #region Get methods
    /// <summary>
    /// Get all alarms
    /// </summary>
    /// <param name="param">channel number</param>
    /// <returns></returns>
    public IList<CncAlarm> GetAlarms (string param)
    {
      IList<CncAlarm> result;
      try {
        result = m_interfaceManager.InterfaceError.GetAlarms (param);
      }
      catch (Exception ex) {
        ProcessException (ex, "HeidenhainDNC", "GetAlarms", true);
        throw;
      }

      return result;
    }
    #endregion // Get methods
  }
}