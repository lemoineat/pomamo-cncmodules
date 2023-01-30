// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Interface_sub_function.
  /// </summary>
  public class Interface_sub_function : GenericMitsubishiInterface
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
  /// Description of Interface_sub_function.
  /// </summary>
  public class Interface_sub_function_2 : Interface_sub_function
  {

  }

  /// <summary>
  /// Description of Interface_sub_function.
  /// </summary>
  public class Interface_sub_function_3 : Interface_sub_function
  {

  }
}
