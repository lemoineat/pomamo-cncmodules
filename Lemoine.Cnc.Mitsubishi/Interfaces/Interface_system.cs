// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Interface_system.
  /// </summary>
  public class Interface_system : GenericMitsubishiInterface
  {
    /// <summary>
    /// Alarm types, according to the file "EZNcErr.bas"
    /// </summary>
    enum AlarmType
    {
      M_ALM_ALL_ALARM = 0x0000, // NOT for M6x5M
      M_ALM_NC_ALARM = 0x0100,
      M_ALM_STOP_CODE = 0x0200, // NOT for M6x5M
      M_ALM_PLC_ALARM = 0x0300,
      M_ALM_OPE_MSG = 0x0400,
      M_ALM_WARNING = 0x0500 // Only for M6x5M
    }

    #region Members
    IList<CncAlarm> m_alarms = new List<CncAlarm> ();
    bool m_alarmsInitialized = false;
    #endregion // Members

    #region Protected methods
    /// <summary>
    /// Work to do on "start", before new data will be read
    /// Already within a try catch
    /// </summary>
    protected override void ResetDataInstance ()
    {
      m_alarms = new List<CncAlarm> ();
      m_alarmsInitialized = false;
    }
    #endregion // Protected methods

    #region Get methods
    /// <summary>
    /// NC system S/W number, name, and PLC version
    /// </summary>
    public string Version
    {
      get
      {
        var result = 0;
        string version = "";
        if ((result = CommunicationObject.System_GetVersion (1, 0, out version)) != 0) {
          throw new ErrorCodeException (result, "GetVersion");
        }

        return version;
      }
    }

    /// <summary>
    /// True if the system is enabled, false otherwise
    /// </summary>
    /// <returns></returns>
    public bool SystemEnabled
    {
      get
      {
        var errorNumber = 0;
        int value = 0;
        if ((errorNumber = CommunicationObject.System_GetSystemInformation (0, out value)) != 0) {
          throw new ErrorCodeException (errorNumber, "GetSystemInformation.0");
        }

        return (value > 0);
      }
    }

    /// <summary>
    /// Number of axes of the system
    /// </summary>
    /// <returns></returns>
    public int AxisNumber
    {
      get
      {
        var errorNumber = 0;
        int value = 0;
        if ((errorNumber = CommunicationObject.System_GetSystemInformation (1, out value)) != 0) {
          throw new ErrorCodeException (errorNumber, "GetSystemInformation.1");
        }

        return value;
      }
    }

    /// <summary>
    /// Current list of alarms
    /// </summary>
    public IList<CncAlarm> Alarms
    {
      get
      {
        if (m_alarmsInitialized) {
          return m_alarms;
        }

        // NC alarm
        ReadAlarms (AlarmType.M_ALM_NC_ALARM, "NC alarm");

        // Stop code
        if (SystemType != Mitsubishi.MitsubishiSystemType.MELDAS_600M_6X5M) {
          ReadAlarms (AlarmType.M_ALM_STOP_CODE, "Stop code");
        }

        // PLC alarm
        ReadAlarms (AlarmType.M_ALM_PLC_ALARM, "PLC alarm");

        // Operator message
        ReadAlarms (AlarmType.M_ALM_OPE_MSG, "Operator msg");

        m_alarmsInitialized = true;
        return m_alarms;
      }
    }
    #endregion // Get methods

    #region Private methods
    void ReadAlarms (AlarmType alarmType, string alarmTypeStr)
    {
      string value = "";
      var errorNumber = SystemType < Mitsubishi.MitsubishiSystemType.MELDAS_700L ?
        CommunicationObject.System_GetAlarm (10, (int)alarmType, out value) : // 10 is the number of alarms to read (10 being the maximum)
        CommunicationObject.System_GetAlarm2 (10, (int)alarmType, out value);

      if (errorNumber != 0) {
        throw new ErrorCodeException (errorNumber, "System_GetAlarm (" + alarmTypeStr + ")");
      }

      var messages = value.Split (new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
      foreach (var message in messages) {
        var alarm = new CncAlarm ("Mitsubishi", alarmTypeStr, "-");
        alarm.CncSubInfo = SystemType.ToString ();
        alarm.Message = message;
        Logger.InfoFormat ("Mitsubishi.ReadAlarms - Found alarm type {0} with message {1}", alarmTypeStr, message);
        m_alarms.Add (alarm);
      }
    }
    #endregion // Private methods
  }
}
