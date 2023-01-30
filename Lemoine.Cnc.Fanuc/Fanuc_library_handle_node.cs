// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Diagnostics;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Fanuc_library_handle_node.
  /// </summary>
  public partial class Fanuc
  {
    /// <summary>
    /// Disconnect from the control, free the handle
    /// </summary>
    void Disconnect ()
    {
      m_connected = false;
      if (0 != m_handle) {
        Import.FwLib.Cnc.freelibhndl (m_handle);
        m_handle = 0;
      }
    }

    /// <summary>
    /// Manage an error
    /// Some errors may require a disconnection.
    /// This method takes care of this
    /// </summary>
    /// <param name="method">method name</param>
    /// <param name="error">Error to manage</param>
    void ManageError (string method, Import.FwLib.EW error)
    {
      if (log.IsDebugEnabled) {
        log.DebugFormat ("ManageError error={0} in method {1} /B", error, method);
      }
      switch (error) {
      case Import.FwLib.EW.HANDLE:
      case Import.FwLib.EW.SOCKET:
        log.InfoFormat ("ManageError error={0} in method {1}: HANDLE and SOCKET errors require a disconnection", error, method);
        Disconnect ();
        break;
      case Import.FwLib.EW.VERSION:
        log.InfoFormat ("ManageError error={0}: {1} is not available",
          error, method);
        if (!string.IsNullOrEmpty (method) && !m_notAvailableMethods.Contains (method)) {
          m_notAvailableMethods.Add (method);
        }
        break;
      default:
        break;
      }
    }

    /// <summary>
    /// Full manage an error
    /// 
    /// If the result is not OK, log it and raise an exception
    /// </summary>
    /// <param name="method"></param>
    /// <param name="result"></param>
    void ManageErrorWithException (string method, Import.FwLib.EW result)
    {
      if (Import.FwLib.EW.OK != result) {
        var stackTrace = new StackTrace ();
        var callingMethod = stackTrace.GetFrame (1).GetMethod ().Name;
        log.Error ($"ManageErrorWithException: {callingMethod} failed with {result} in Focas method {method}");
        ManageError (method, result);
        throw new Exception (callingMethod + " failed");
      }
    }
  }
}
