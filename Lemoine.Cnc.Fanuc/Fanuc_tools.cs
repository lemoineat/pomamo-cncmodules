// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Fanuc_others.
  /// </summary>
  public partial class Fanuc
  {
    #region Utils
    /// <summary>
    /// Extract a specific bit from a byte
    /// </summary>
    /// <param name="b"></param>
    /// <param name="bitNumber">From 0</param>
    /// <returns></returns>
    static bool GetBit (byte b, int bitNumber)
    {
      return (b & (1 << bitNumber)) != 0;
    }

    /// <summary>
    /// Extract a specific bit from a short
    /// </summary>
    /// <param name="b"></param>
    /// <param name="bitNumber">From 0</param>
    /// <returns></returns>
    static bool GetBit (short b, int bitNumber)
    {
      return (b & (1 << bitNumber)) != 0;
    }
    #endregion // Utils    
  }
}
