// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.Text;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Part of the class MML3 dealing with Cnc alarms
  /// </summary>
  public partial class MML3
  {
    static readonly string CNC_ALARM_TYPE = "Cnc";

    #region Getters / Setters
    /// <summary>
    /// Cnc alarms
    /// </summary>
    public IList<CncAlarm> CncAlarms
    {
      get
      {
        var cncAlarms = new List<CncAlarm> ();

        CheckCncConnection ();

        // Number of alarms
        UInt16 alarmNumber;
        var ret = Md3Cnc.ChkCncAlarm (m_cncHandle, out alarmNumber);
        ManageCncResult ("ChkCncAlarm", ret);

        log.InfoFormat ("MML3.InitializeCncAlarms: {0} alarm(s) found", alarmNumber);

        // Retrieve all alarms
        if (alarmNumber > 0) {
          log.InfoFormat ("MML3: InitializeCncAlarms {0} alarms", alarmNumber);
          var almNum = new UInt16[alarmNumber];
          var axisNo = new UInt16[alarmNumber];
          var almMsg = new StringBuilder[alarmNumber];
          for (int i = 0; i < alarmNumber; i++) {
            almMsg[i] = new StringBuilder (128);
          }

          var almType = new UInt16[alarmNumber];
          UInt16 numAlm = alarmNumber;
          ret = Md3Cnc.CncAlarm (m_cncHandle, almNum, axisNo, almMsg, almType, ref numAlm);
          ManageCncResult ("CncAlarm", ret);

          for (UInt16 i = 0; i < numAlm; i++) {
            UInt16 number = almNum[i];
            var cncAlarm = new CncAlarm (CNC_INFO, CNC_ALARM_TYPE, number.ToString ());
            cncAlarm.Message = almMsg[i].ToString ();

            // Axis number
            cncAlarm.Properties["axis"] = axisNo[i].ToString ();

            // machine type 
            cncAlarm.CncSubInfo = "Pro" + ProXVersion.ToString ();

            // Alarm type
            cncAlarm.Properties["type"] = almType[i].ToString ();

            // O and sequence numbers
            try {
              cncAlarm.Properties["Execution block"] = GetExecBlock ("");
            }
            catch (Exception e) {
              log.WarnFormat ("Couldn't add the execution block to a cnc alarm: {0}", e.ToString ());
            }

            // Store the alarm
            string txt = "";
            foreach (var val in cncAlarm.Properties) {
              txt += val.Key + "=" + val.Value + " ; ";
            }

            log.InfoFormat ("MML3: found cnc alarm {0}, code {1}, message {2}",
                           txt, number, cncAlarm.Message);
            cncAlarms.Add (cncAlarm);
          }
        }

        return cncAlarms;
      }
    }
    #endregion // Getters / Setters
  }
}
