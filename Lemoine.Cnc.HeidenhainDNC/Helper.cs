// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Class for help methods
  /// </summary>
  public partial class HeidenhainDNC
  {
    #region Methods
    /// <summary>
    /// Check the DNC version
    /// </summary>
    /// <param name="mayor"></param>
    /// <param name="minor"></param>
    /// <param name="revision"></param>
    /// <param name="build"></param>
    /// <returns>true if ok, false is the DNC version is wrong or null if it cannot be read</returns>
    public bool? CheckDncVersion (int mayor, int minor, int revision, int build)
    {
      var requiredVersion = new int[] { mayor, minor, revision, build };
      int[] installedVersion = null;
      string strVersionComInterface = null;
      HeidenhainDNCLib.JHMachineInProcess machine = null;

      try {
        machine = new HeidenhainDNCLib.JHMachineInProcess ();
        strVersionComInterface = machine.GetVersionComInterface ();
        Marshal.ReleaseComObject (machine);
        machine = null;

        installedVersion = strVersionComInterface.Split (strVersionComInterface.Contains (".") ? '.' : ',')
          .Select (n => Convert.ToInt32 (n)).ToArray ();

        string strRequiredVersionComInterface = requiredVersion[0] + "." +
          requiredVersion[1] + "." +
          requiredVersion[2] + "." +
          requiredVersion[3];

        string strInstalledVersionComInterface = installedVersion[0] + "." +
          installedVersion[1] + "." +
          installedVersion[2] + "." +
          installedVersion[3];

        // Check if installed COM component meets the minimum application requirement
        if (!CheckVersion (requiredVersion, installedVersion)) {
          log.ErrorFormat ("HeidenhainDNC.CheckDncVersion - The version doesn't meet the minimum requirement (current is {0}, required is {1})",
                          strInstalledVersionComInterface, strRequiredVersionComInterface);
          return false;
        }
        else {
          log.InfoFormat ("HeidenhainDNC.CheckDncVersion - Using version {0} (required is {1})",
                         strInstalledVersionComInterface, strRequiredVersionComInterface);
        }
      }
      catch (Exception ex) {
        if (machine != null) {
          Marshal.ReleaseComObject (machine);
        }

        ProcessException (ex, "HeidenhainCNC", "CheckDncVersion", false);
        throw;
      }

      return true;
    }

    /// <summary>
    /// Format and display an exception in the log
    /// If this is a Heidenhain error, a disconnection may occur
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="className"></param>
    /// <param name="methodName"></param>
    /// <param name="allowDisconnect"></param>
    public void ProcessException (Exception ex, string className, string methodName, bool allowDisconnect)
    {
      var comException = ex as COMException;

      // Not a heidenhain exception?
      if (comException == null) {
        if (ex.ToString ().Contains ("REGDB_E_CLASSNOTREG")) {
          log.ErrorFormat ("HeidenhainDNC ({3}) exception in {0}.{1}: {2}", className, methodName,
            "REGDB_E_CLASSNOTREG: HeidenhainDNC not installed on the computer", ConnectionName);
        }
        else {
          // Simple log
          log.ErrorFormat ("HeidenhainDNC ({3}) exception in {0}.{1}: {2}", className, methodName, ex, ConnectionName);
        }

        return;
      }

      // Log Heidenhain exception
      log.ErrorFormat (
        "HeidenhainDNC ({5}) COM exception in {0}.{1}: {2} {3} ({4}) - {6}",
        className,
        methodName,
        Convert.ToString (comException.ErrorCode, 16),
        Enum.GetName (typeof (HeidenhainDNCLib.DNC_HRESULT), (HeidenhainDNCLib.DNC_HRESULT)comException.ErrorCode),
        comException.ErrorCode,
        ConnectionName,
        comException.Message
       );

      // Possibly disconnect
      if (allowDisconnect &&
          (comException.ErrorCode == (int)HeidenhainDNCLib.DNC_HRESULT.DNC32_E_CONNECT_LOST || // Connection to the CNC is lost
           comException.ErrorCode == (int)HeidenhainDNCLib.DNC_HRESULT.DNC_E_DNC32FAILED || // General communication failure
           comException.ErrorCode == (int)HeidenhainDNCLib.DNC_HRESULT.DNC32_E_FAIL_CONNECT || // Fail to connect
           comException.ErrorCode == (int)HeidenhainDNCLib.DNC_HRESULT.DNC_E_NOT_POS_NOW || // Attempted DNC call while DncServer not online
           comException.ErrorCode == (int)HeidenhainDNCLib.DNC_HRESULT.DNC32_E_NOT_CONN || // Requested action not possible when not connected
           comException.ErrorCode == (int)HeidenhainDNCLib.DNC_HRESULT.DNC_E_DNC_PROHIBITED || // Communication with the server is (currently) not allowed
           comException.ErrorCode == -2147467238))  // Configured identity is incorrect
      {
        m_connectionManager.DisconnectLater ();
      }

      // Possibly sleep forever so that the thread is restarted
      if (comException.ErrorCode == (int)HeidenhainDNCLib.DNC_HRESULT.DNC_E_FAIL) { // General failure (DNC_E_FAIL) => very bad
        log.Fatal ("HeidenhainDNC COM Exception: general failure -> sleep forever");
        Thread.Sleep (Timeout.Infinite);
      }
      else if (comException.ErrorCode == 1638) {
        log.Error ("HeidenhainDNC COM Exception: interface not accessible anymore -> sleep forever");
        Thread.Sleep (Timeout.Infinite);
      }
    }
    #endregion // Methods

    #region Private methods
    static bool CheckVersion (int[] requiredVersion, int[] installedVersion)
    {
      if (requiredVersion.Length != 4 || installedVersion.Length != 4) {
        throw (new Exception ());
      }

      for (int i = 0; i < installedVersion.Length; i++) {
        if (installedVersion[i] > requiredVersion[i]) {
          break;
        }

        if (installedVersion[i] == requiredVersion[i]) {
          continue;
        }

        return false;
      }

      return true;
    }

    static HeidenhainDNCLib.DNC_PROTOCOL GetProtocol (HeidenhainDNCLib.DNC_CNC_TYPE cncType)
    {
      HeidenhainDNCLib.DNC_PROTOCOL protocol = HeidenhainDNCLib.DNC_PROTOCOL.DNC_PROT_TCPIP;

      switch (cncType) {
      case HeidenhainDNCLib.DNC_CNC_TYPE.DNC_CNC_TYPE_ATEKM:
        protocol = HeidenhainDNCLib.DNC_PROTOCOL.DNC_PROT_RPC;
        break;
      case HeidenhainDNCLib.DNC_CNC_TYPE.DNC_CNC_TYPE_ITNC:
        protocol = HeidenhainDNCLib.DNC_PROTOCOL.DNC_PROT_TCPIP;
        break;
      case HeidenhainDNCLib.DNC_CNC_TYPE.DNC_CNC_TYPE_MILLPLUS:
        protocol = HeidenhainDNCLib.DNC_PROTOCOL.DNC_PROT_TCPIP; // can be also lsv2
        break;
      case HeidenhainDNCLib.DNC_CNC_TYPE.DNC_CNC_TYPE_MILLPLUSIT:
        protocol = HeidenhainDNCLib.DNC_PROTOCOL.DNC_PROT_COM;
        break;
      case HeidenhainDNCLib.DNC_CNC_TYPE.DNC_CNC_TYPE_TURNPLUS: // cnc pilot 4290
        protocol = HeidenhainDNCLib.DNC_PROTOCOL.DNC_PROT_COM;
        break;
      case HeidenhainDNCLib.DNC_CNC_TYPE.DNC_CNC_TYPE_MILLPLUSIT_NCK:
        protocol = HeidenhainDNCLib.DNC_PROTOCOL.DNC_PROT_RPC;
        break;
      case HeidenhainDNCLib.DNC_CNC_TYPE.DNC_CNC_TYPE_MANUALPLUS_NCK:
        protocol = HeidenhainDNCLib.DNC_PROTOCOL.DNC_PROT_RPC;
        break;
      case HeidenhainDNCLib.DNC_CNC_TYPE.DNC_CNC_TYPE_ATEKM_NCK: // anilam iSeries?
        protocol = HeidenhainDNCLib.DNC_PROTOCOL.DNC_PROT_RPC;
        break;
      case HeidenhainDNCLib.DNC_CNC_TYPE.DNC_CNC_TYPE_TNC320_NCK:
        protocol = HeidenhainDNCLib.DNC_PROTOCOL.DNC_PROT_RPC;
        break;
      case HeidenhainDNCLib.DNC_CNC_TYPE.DNC_CNC_TYPE_GRINDPLUS_NCK:
        protocol = HeidenhainDNCLib.DNC_PROTOCOL.DNC_PROT_RPC;
        break;
      case HeidenhainDNCLib.DNC_CNC_TYPE.DNC_CNC_TYPE_TNC6xx_NCK:
        protocol = HeidenhainDNCLib.DNC_PROTOCOL.DNC_PROT_RPC;
        break;
      case HeidenhainDNCLib.DNC_CNC_TYPE.DNC_CNC_TYPE_AR6000_NCK:
        protocol = HeidenhainDNCLib.DNC_PROTOCOL.DNC_PROT_RPC;
        break;
      case HeidenhainDNCLib.DNC_CNC_TYPE.DNC_CNC_TYPE_CNCPILOT6xx_NCK:
        protocol = HeidenhainDNCLib.DNC_PROTOCOL.DNC_PROT_RPC;
        break;
      case HeidenhainDNCLib.DNC_CNC_TYPE.DNC_CNC_TYPE_TNC128_NCK:
        protocol = HeidenhainDNCLib.DNC_PROTOCOL.DNC_PROT_RPC;
        break;
      case HeidenhainDNCLib.DNC_CNC_TYPE.DNC_CNC_TYPE_GRINDPLUS640_NCK:
        protocol = HeidenhainDNCLib.DNC_PROTOCOL.DNC_PROT_RPC;
        break;
      }

      return protocol;
    }

    static HeidenhainDNCLib.DNC_STATE ComputeNewState (HeidenhainDNCLib.DNC_STATE currentState, HeidenhainDNCLib.DNC_EVT_STATE evtState)
    {
      // In every case, DNC_EVT_STATE_DNC_STOPPED => DNC_STATE_DNC_IS_STOPPED
      if (evtState == HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_DNC_STOPPED) {
        return HeidenhainDNCLib.DNC_STATE.DNC_STATE_DNC_IS_STOPPED;
      }

      // Internal state machine of the NCK based controls
      // --> See HeidenhainDNC COM component documentation for more information
      HeidenhainDNCLib.DNC_STATE newState = currentState;
      switch (currentState) {
      case HeidenhainDNCLib.DNC_STATE.DNC_STATE_NOT_INITIALIZED:
        switch (evtState) {
        case HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_HOST_NOT_AVAILABLE:
          newState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_HOST_IS_NOT_AVAILABLE;
          break;
        }
        break;

      case HeidenhainDNCLib.DNC_STATE.DNC_STATE_HOST_IS_NOT_AVAILABLE:
        switch (evtState) {
        case HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_HOST_AVAILABLE:
          newState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_HOST_IS_AVAILABLE;
          break;
        }
        break;

      case HeidenhainDNCLib.DNC_STATE.DNC_STATE_HOST_IS_AVAILABLE:
        switch (evtState) {
        case HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_HOST_NOT_AVAILABLE:
          newState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_HOST_IS_NOT_AVAILABLE;
          break;
        case HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_WAIT_PERMISSION:
          newState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_WAITING_PERMISSION;
          break;
        }
        break;

      case HeidenhainDNCLib.DNC_STATE.DNC_STATE_WAITING_PERMISSION:
        switch (evtState) {
        case HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_DNC_AVAILABLE:
          newState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_DNC_IS_AVAILABLE;
          break;
        case HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_DNC_STOPPED:
          newState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_DNC_IS_STOPPED;
          break;
        case HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_PERMISSION_DENIED:
          newState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_NO_PERMISSION;
          break;
        }
        break;

      case HeidenhainDNCLib.DNC_STATE.DNC_STATE_DNC_IS_AVAILABLE:
        switch (evtState) {
        case HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_MACHINE_BOOTED:
          newState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_MACHINE_IS_BOOTED;
          break;
        case HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_DNC_STOPPED:
          newState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_DNC_IS_STOPPED;
          break;
        }
        break;

      case HeidenhainDNCLib.DNC_STATE.DNC_STATE_MACHINE_IS_BOOTED:
        switch (evtState) {
        case HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_MACHINE_INITIALIZING:
          newState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_MACHINE_IS_INITIALIZING;
          break;
        case HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_DNC_STOPPED:
          newState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_DNC_IS_STOPPED;
          break;
        }
        break;

      case HeidenhainDNCLib.DNC_STATE.DNC_STATE_MACHINE_IS_INITIALIZING:
        switch (evtState) {
        case HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_MACHINE_AVAILABLE:
          newState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_MACHINE_IS_AVAILABLE;
          break;
        case HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_DNC_STOPPED:
          newState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_DNC_IS_STOPPED;
          break;
        }
        break;

      case HeidenhainDNCLib.DNC_STATE.DNC_STATE_MACHINE_IS_AVAILABLE:
        switch (evtState) {
        case HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_MACHINE_SHUTTING_DOWN:
          newState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_MACHINE_IS_SHUTTING_DOWN;
          break;
        case HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_DNC_STOPPED:
          newState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_DNC_IS_STOPPED;
          break;
        }
        break;

      case HeidenhainDNCLib.DNC_STATE.DNC_STATE_MACHINE_IS_SHUTTING_DOWN:
        switch (evtState) {
        case HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_DNC_STOPPED:
          newState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_DNC_IS_STOPPED;
          break;
        }
        break;

      case HeidenhainDNCLib.DNC_STATE.DNC_STATE_DNC_IS_STOPPED:
        switch (evtState) {
        case HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_CONNECTION_LOST:
          newState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_HOST_IS_STOPPED;
          break;
        }
        break;

      case HeidenhainDNCLib.DNC_STATE.DNC_STATE_HOST_IS_STOPPED:
        switch (evtState) {
        case HeidenhainDNCLib.DNC_EVT_STATE.DNC_EVT_STATE_DNC_STOPPED:
          newState = HeidenhainDNCLib.DNC_STATE.DNC_STATE_DNC_IS_STOPPED;
          break;
        }
        break;

      case HeidenhainDNCLib.DNC_STATE.DNC_STATE_NO_PERMISSION:
        break;
      }

      return newState;
    }
    #endregion // Private methods
  }
}
