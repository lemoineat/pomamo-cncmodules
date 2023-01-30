// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of IInterface.
  /// </summary>
  public interface IMitsubishiInterface
  {
    /// <summary>
    /// Reinitialize data, preceeding a new acquisition
    /// </summary>
    void ResetData ();
  }
}
