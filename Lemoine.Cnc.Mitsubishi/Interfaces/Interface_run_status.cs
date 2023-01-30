// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Interface_run_status.
  /// </summary>
  public class Interface_run_status : GenericMitsubishiInterface
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

    #region Get methods
    /// <summary>
    /// Get the operation status
    /// </summary>
    /// <param name="operationType">Can be:
    /// 0 - Tool length measurement
    /// 1 - Automatic operation "run". Gets the status indicating that the system is operating automatically
    /// 2 - Automatic operation "start". Gets the status indicating that the system is operating automatically and
    ///     that a movement command or M, S, T, B process is being executed.
    /// 3 - Automatic operation "pause". Gets the status indicating that automatic operation is paused
    ///     while executing a movement command or miscellaneous command with automatic operation. (only for M700/M800 series)
    /// </param>
    /// <returns></returns>
    public int GetRunStatus (int operationType)
    {
      int value = 0;
      int errorCode = 0;
      if ((errorCode = CommunicationObject.Status_GetRunStatus (operationType, out value)) != 0) {
        throw new ErrorCodeException (errorCode, "Status_GetRunStatus." + operationType);
      }

      return value;
    }

    /// <summary>
    /// True if in G01, G02, G03, G31, G33, G34, or G35 mode
    /// False otherwise
    /// </summary>
    public bool CuttingMode
    {
      get
      {
        int value = 0;
        int errorCode = 0;
        if ((errorCode = CommunicationObject.Status_GetCuttingMode (out value)) != 0) {
          throw new ErrorCodeException (errorCode, "Status_GetCuttingMode");
        }

        return (value > 0);
      }
    }
    #endregion Get methods
  }
}
