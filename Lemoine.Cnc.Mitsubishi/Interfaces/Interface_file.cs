// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Interface_file.
  /// </summary>
  public class Interface_file : GenericMitsubishiInterface
  {
    #region Protected methods
    /// <summary>
    /// Work to do on "start", before new data will be read
    /// Already within a try catch
    /// </summary>
    protected override void ResetDataInstance ()
    {
      // Nothing for now
    }
    #endregion // Protected methods
  }

  /// <summary>
  /// Description of Interface_file_4.
  /// </summary>
  public class Interface_file_4 : Interface_file
  {

  }

  /// <summary>
  /// Description of Interface_file_6.
  /// </summary>
  public class Interface_file_6 : Interface_file
  {

  }
}
