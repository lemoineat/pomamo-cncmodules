// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Fanuc_alarms
  /// </summary>
  public partial class Fanuc
  {
    #region Members
    static readonly short NB_MAX_ALARM_MESSAGES = 10; // cf ODBALMMSG2 data structure
    static readonly short NB_MAX_OPERATOR_MESSAGES = 17; // cf OPMSG3 data structure
    IList<CncAlarm> m_alarms;
    IList<CncAlarm> m_operatorMessages;
    int? m_alarmStatus;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Alarms
    /// </summary>
    public IList<CncAlarm> Alarms
    {
      get {
        if (m_alarms != null || GetAlarmsInformation ()) {
          return m_alarms;
        }

        log.ErrorFormat ("Alarms.get: failed because alarms information was unavailable");
        throw new Exception ("Alarms failed");
      }
    }

    /// <summary>
    /// Operator messages
    /// </summary>
    public IList<CncAlarm> OperatorMessages
    {
      get {
        if (m_operatorMessages != null || GetOperatorMessagesInformation ()) {
          return m_operatorMessages;
        }

        log.ErrorFormat ("OperatorMessages.get: failed because operator messages information was unavailable");
        throw new Exception ("Operator messages failed");
      }
    }

    /// <summary>
    /// Status of alarms returned as a bitfield
    /// </summary>
    public int AlarmStatus
    {
      get {
        if (!m_alarmStatus.HasValue) {
          int alarmStatus;
          var result = (Import.FwLib.EW)Import.FwLib.Cnc.alarm2 (m_handle, out alarmStatus);
          if (Import.FwLib.EW.OK != result) {
            log.ErrorFormat ("AlarmStatus.get: " +
                             "cnc_alarm2 failed with error {0}",
                             result);
            ManageError ("AlarmStatus", result);
            throw new Exception ("cnc_alarm2 failed");
          }
          else {
            m_alarmStatus = alarmStatus;
            return m_alarmStatus.Value;
          }
        }
        else {
          return m_alarmStatus.Value;
        }
      }
    }
    #endregion // Getters / Setters

    #region Private methods
    string GetType (int bitIndex)
    {
      string type = "unknown CncKind " + CncKind;

      // 150i series
      if (CncKind.StartsWith ("15", StringComparison.InvariantCulture)) {
        type = GetTypeClass150i (bitIndex);
      }
      // 160i/180i/210i/0i/Power Mate series
      else if (CncKind.StartsWith ("16", StringComparison.InvariantCulture) ||
               CncKind.StartsWith ("18", StringComparison.InvariantCulture) ||
               CncKind.StartsWith ("21", StringComparison.InvariantCulture) ||
               CncKind.StartsWith ("0", StringComparison.InvariantCulture) ||
               CncKind.StartsWith ("PD", StringComparison.InvariantCulture) ||
               CncKind.StartsWith ("PH", StringComparison.InvariantCulture)) {
        type = GetTypeClass160i (bitIndex);
      }
      // 300i series
      else if (CncKind.StartsWith ("3", StringComparison.InvariantCulture)) {
        type = GetTypeClass3XX (bitIndex);
      }

      return type;
    }

    string GetTypeClass150i (int bitIndex)
    {
      string type = "other";
      switch (bitIndex) {
      case 0:
        type = "Background P/S";
        break; // Notice
      case 1:
        type = "Foreground P/S";
        break; // Program abort
      case 2:
        type = "Overheat alarm";
        break; // Machine stop
      case 3:
        type = "Sub-CPU error";
        break;
      case 4:
        type = "Synchronized error";
        break; // Machine stop
      case 5:
        type = "Parameter switch on";
        break; // Notice
      case 6:
        type = "Overtravel, external data";
        break; // Machine stop
      case 7:
        type = "PMC error";
        break; // Machine stop
      case 8:
        type = "External alarm message (1)";
        break;
      // 9: not used
      case 10:
        type = "Serious P/S";
        break;
      // 11: not used
      case 12:
        type = "Servo alarm";
        break; // Machine stop
      case 13:
        type = "I/O error";
        break; // Program abort
      case 14:
        type = "Power off parameter set";
        break; // Program abort
      case 15:
        type = "System alarm";
        break;
      case 16:
        type = "External alarm message (2)";
        break;
      case 17:
        type = "External alarm message (3)";
        break;
      case 18:
        type = "External alarm message (4)";
        break;
      case 19:
        type = "Macro alarm";
        break; // Program abort
      case 20:
        type = "Spindle alarm";
        break; // Machine stop
      }
      return type;
    }

    string GetTypeClass160i (int bitIndex)
    {
      string type = "other";
      switch (bitIndex) {
      case 0:
        type = "P/S 100";
        break;
      case 1:
        type = "P/S 000";
        break;
      case 2:
        type = "P/S 101";
        break;
      case 3:
        type = "P/S other alarm";
        break;
      case 4:
        type = "Overtravel alarm";
        break; // Machine stop
      case 5:
        type = "Overheat alarm";
        break; // Machine stop
      case 6:
        type = "Servo alarm";
        break; // Machine stop
      case 7:
        type = "System alarm";
        break;
      case 8:
        type = "APC alarm";
        break;
      case 9:
        type = "Spindle alarm";
        break; // Machine stop
      case 10:
        type = "P/S alarm (5000 ...), Punchpress alarm";
        break;
      case 11:
        type = "Laser alarm";
        break;
      // 12: not used
      case 13:
        type = "Rigid tap alarm";
        break;
      // 14: not used
      case 15:
        type = "External alarm message";
        break;
      }
      return type;
    }

    string GetTypeClass3XX (int bitIndex)
    {
      string type = "other";
      switch (bitIndex) {
      case 0:
        type = "Parameter switch on";
        break; // Notice
      case 1:
        type = "Power off parameter set";
        break; // Program abort
      case 2:
        type = "I/O error";
        break; // Program abort
      case 3:
        type = "Foreground P/S";
        break; // Program abort
      case 4:
        type = "Overtravel, external data";
        break; // Machine stop
      case 5:
        type = "Overheat alarm";
        break; // Machine stop
      case 6:
        type = "Servo alarm";
        break; // Machine stop
      case 7:
        type = "Data I/O error";
        break; // Machine stop
      case 8:
        type = "Macro alarm";
        break; // Program abort
      case 9:
        type = "Spindle alarm";
        break; // Machine stop
      case 10:
        type = "Other alarm (DS)";
        break;
      case 11:
        type = "Preventive function alarm";
        break; // Notice
      case 12:
        type = "Background P/S";
        break; // Notice
      case 13:
        type = "Synchronized error";
        break; // Machine stop
               // 14: reserved
      case 15:
        type = "External alarm message";
        break; // 1XXX => machine stop, 2XXX => notice
               // 16: reserved
               // 17: reserved
               // 18: reserved
      case 19:
        type = "PMC error";
        break; // Machine stop
      }
      return type;
    }

    bool GetAlarmsInformation ()
    {
      // Alarm status
      m_alarmStatus = null;
      int alarmStatus = AlarmStatus;

      var tmp = new List<CncAlarm> ();
      if (ControlledAxisNumber.HasValue) {
        short[] possibleAttributesTypes = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 15, 19 };
        foreach (short attributeType in possibleAttributesTypes) {
          if ((alarmStatus & (1 << attributeType)) != 0) { // if an alarm has been raised for a specific type
            short nbAlarms = NB_MAX_ALARM_MESSAGES; // Max number is 10 (according to ODBALMMSG2)
            Import.FwLib.ODBALMMSG2 alarms;
            var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdalmmsg2 (m_handle, attributeType, ref nbAlarms, out alarms);
            log.DebugFormat ("Found {0} alarms for bitIndex {1}", nbAlarms, attributeType);
            if (result == Import.FwLib.EW.OK) {
              if (nbAlarms > NB_MAX_ALARM_MESSAGES) {
                nbAlarms = NB_MAX_ALARM_MESSAGES;
                log.WarnFormat ("GetAlarmsInformation: cnc_rdalmmsg2 returns {0} alarms but only {1} are supported ",
                               nbAlarms, NB_MAX_ALARM_MESSAGES);
              }

              for (int alarmIndex = 0; alarmIndex < nbAlarms; alarmIndex++) {
                Import.FwLib.ODBALMMSG2_data alarm = alarms.msgs[alarmIndex];
                var cncAlarm = new CncAlarm ("Fanuc", CncKind, GetType (attributeType), alarm.alm_no.ToString ());
                cncAlarm.Message = alarm.alm_msg;
                cncAlarm.Properties["Axis"] = alarm.axis.ToString ();
                log.InfoFormat ("Got an alarm from Fanuc! {0}, {1}. Alarm status was {2}",
                               cncAlarm, alarm.alm_msg, (alarmStatus & (1 << attributeType)));
                tmp.Add (cncAlarm);
              }
            }
            else {
              if (result == Import.FwLib.EW.ATTRIB_TYPE) {
                log.InfoFormat ("GetAlarmsInformation: bad attribute type {0} in cnc_rdalmmsg2", attributeType);
              }
              else {
                log.WarnFormat ("GetAlarmsInformation: cnc_rdalmmsg2 failed on index {0} with error {1}", attributeType, result);
              }
            }
          }
        }
      }
      else {
        log.Error ("GetAlarmsInformation: alarms information failed because number of axis is unknown");
        throw new Exception ("GetAlarmsInformation failed");
      }

      m_alarms = tmp;
      return true;
    }

    string GetOperatorMessageType (int type)
    {
      string ret;

      if (type == 4) {
        ret = "Operator message - macro";
      }
      else if (type < 17) {
        ret = "Operator message";
      }
      else {
        ret = "Operator message - unknown";
      }

      return ret;
    }

    bool GetOperatorMessagesInformation ()
    {
      var tmp = new List<CncAlarm> ();
      short nbMessages = NB_MAX_OPERATOR_MESSAGES; // Max number is 17 (according to OPMSG3)
      Import.FwLib.OPMSG3 messages;
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdopmsg3 (m_handle, -1, ref nbMessages, out messages);
      log.DebugFormat ("Found {0} operator messages", nbMessages);
      if (result == Import.FwLib.EW.OK) {
        if (nbMessages > NB_MAX_OPERATOR_MESSAGES) {
          nbMessages = NB_MAX_OPERATOR_MESSAGES;
          log.WarnFormat ("GetAlarmsInformation: rdopmsg3 returns {0} messages but only {1} are supported ",
                         nbMessages, NB_MAX_OPERATOR_MESSAGES);
        }

        for (int messageIndex = 0; messageIndex < nbMessages; messageIndex++) {
          var message = messages.msgs[messageIndex];
          if (message.datano != -1) {
            var cncAlarm = new CncAlarm ("Fanuc", CncKind, GetOperatorMessageType (message.type), message.datano.ToString ());
            cncAlarm.Message = message.data;
            log.InfoFormat ("Got a message from Fanuc! {0}, msg length = {1}", cncAlarm, message.char_num);
            tmp.Add (cncAlarm);
          }
        }
      }
      else {
        log.WarnFormat ("GetOperatorMessagesInformation: cnc_rdopmsg3 failed with error {0}", result);
      }

      m_operatorMessages = tmp;
      return true;
    }
    #endregion // Private methods
  }
}
