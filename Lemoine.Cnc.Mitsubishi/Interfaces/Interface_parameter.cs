// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Interface_parameter.
  /// </summary>
  public class Interface_parameter : GenericMitsubishiInterface
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
  /// Description of Interface_parameter_2.
  /// </summary>
  public class Interface_parameter_2 : Interface_parameter
  {

  }

  /// <summary>
  /// Description of Interface_parameter_3.
  /// </summary>
  public class Interface_parameter_3 : Interface_parameter
  {

  }
}
