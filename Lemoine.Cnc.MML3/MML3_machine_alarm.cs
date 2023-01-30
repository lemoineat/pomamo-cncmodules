// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.IO;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Part of the class MML3 dealing with Machine alarms
  /// </summary>
  public partial class MML3
  {
    #region Members
    bool m_alarmTranslatorInitialized = false;
    internal AlarmTranslator_Default m_alarmTranslator = new AlarmTranslator_Default ();
    #endregion // Members

    static readonly string CNC_INFO = "MML3";
    static readonly string MACHINE_ALARM_TYPE = "Machine";

    #region Getters / Setters
    /// <summary>
    /// Machine alarms
    /// </summary>
    public IList<CncAlarm> MachineAlarms
    {
      get
      {
        CheckProXConnection ();
        CheckAlarmTranslations ();

        var machineAlarms = new List<CncAlarm> ();

        // Number of alarms and warnings
        UInt32 alarmNumber, warningNumber;
        var ret = m_md3ProX.ChkMcAlarm (m_proXhandle, out alarmNumber, out warningNumber);
        ManageProXResult ("ChkMcAlarm", ret);

        log.InfoFormat ("MML3.InitializeMachineAlarms: {0} alarm(s) found", alarmNumber + warningNumber);

        // Retrieve all alarms
        if (alarmNumber + warningNumber > 0) {
          log.InfoFormat ("MML3: initializeMachineAlarms {0} warnings and {1} alarms", warningNumber, alarmNumber);
          var alarmNo = new UInt32[alarmNumber + warningNumber];
          var alarmType = new byte[alarmNumber + warningNumber];
          var seriousLevel = new byte[alarmNumber + warningNumber];
          var powerOutDisable = new byte[alarmNumber + warningNumber];
          var cycleStartDisable = new byte[alarmNumber + warningNumber];
          var retryEnable = new byte[alarmNumber + warningNumber];
          var failedNcReset = new bool[alarmNumber + warningNumber];
          var occuredTime = new SYSTEMTIME[alarmNumber + warningNumber];
          UInt32 sumArray = alarmNumber + warningNumber;

          ret = m_md3ProX.McAlarm (m_proXhandle, ref alarmNo, ref alarmType, ref seriousLevel, ref powerOutDisable,
                                  ref cycleStartDisable, ref retryEnable, ref failedNcReset, ref occuredTime,
                                  ref sumArray);
          ManageProXResult ("McAlarm", ret);

          for (int i = 0; i < sumArray; i++) {
            uint number = alarmNo[i];
            if (number >= 135000 && number < 136000) {
              // We skip the element: this is a NC alarm
              continue;
            }
            var machineAlarm = new CncAlarm (CNC_INFO, MACHINE_ALARM_TYPE, number.ToString ());
            m_alarmTranslator.ProcessAlarm (machineAlarm);

            // machine type 
            machineAlarm.CncSubInfo = "Pro" + ProXVersion.ToString ();

            // Alarm type
            switch (alarmType[i]) {
              case 1:
                machineAlarm.Properties["type"] = "alarm";
                break;
              case 2:
                machineAlarm.Properties["type"] = "warning";
                break;
              default:
                // Shouldn't happen, we skip the alarm
                log.ErrorFormat ("InitializeMachineAlarms: bad type {0}", alarmType[i]);
                continue;
            }

            // Serious level
            switch (seriousLevel[i]) {
              case 0:
                machineAlarm.Properties["serious level"] = "normal";
                break;
              case 1:
                machineAlarm.Properties["serious level"] = "damage";
                break;
              default:
                machineAlarm.Properties["serious level"] = "unknown (" + seriousLevel[i] + ")";
                break;
            }

            // Power out disable
            switch (powerOutDisable[i]) {
              case 0:
                machineAlarm.Properties["power off"] = "required";
                break;
              case 1:
                machineAlarm.Properties["power off"] = "not required";
                break;
              default:
                machineAlarm.Properties["power off"] = "unknown (" + powerOutDisable[i] + ")";
                break;
            }

            // Cycle start disable
            switch (cycleStartDisable[i]) {
              case 0:
                machineAlarm.Properties["cycle start"] = "possible";
                break;
              case 1:
                machineAlarm.Properties["cycle start"] = "impossible";
                break;
              default:
                machineAlarm.Properties["cycle start"] = "unknown (" + cycleStartDisable[i] + ")";
                break;
            }

            // Retry enable
            switch (retryEnable[i]) {
              case 0:
                machineAlarm.Properties["retry"] = "not possible";
                break;
              case 1:
                machineAlarm.Properties["retry"] = "possible";
                break;
              default:
                machineAlarm.Properties["retry"] = "unknown (" + retryEnable[i] + ")";
                break;
            }

            // Failed nc reset
            if (failedNcReset[i]) {
              machineAlarm.Properties["reset"] = "executed";
            }
            else {
              machineAlarm.Properties["reset"] = "not executed";
            }

            // Date
            if (occuredTime[i].wYear != 0) {
              try {
                machineAlarm.Properties["date"] = TranslateDateTime (occuredTime[i])
                  .ToString ("yyyy-MM-dd HH:mm:ss.zzz");
              }
              catch {
                log.WarnFormat ("Couldn't translate date {0}/{1}/{2} {3}:{4}:{5}.{6}",
                               occuredTime[i].wYear, occuredTime[i].wMonth, occuredTime[i].wDay,
                               occuredTime[i].wHour, occuredTime[i].wMinute, occuredTime[i].wSecond,
                               occuredTime[i].wMilliseconds);
              }
            }

            // O and sequence numbers
            try {
              machineAlarm.Properties["Execution block"] = GetExecBlock ("");
            } catch (Exception e) {
              log.WarnFormat ("Couldn't add the execution block to a machine alarm: {0}", e.ToString ());
            }

            // Store the alarm
            string txt = "";
            foreach (var val in machineAlarm.Properties) {
              txt += val.Key + "=" + val.Value + " ; ";
            }

            log.InfoFormat ("MML3: found machine alarm {0}, code {1}, message {2}",
                           txt, number, machineAlarm.Message);
            machineAlarms.Add (machineAlarm);
          }
        }

        return machineAlarms;
      }
    }
    #endregion // Getters / Setters

    #region Private methods
    void CheckAlarmTranslations ()
    {
      if (!m_alarmTranslatorInitialized) {
        m_alarmTranslator.ClearTranslations ();
        if (Md3ProVersion == 3) {
          // NC alarms are added
          string productVersion, machineModel, machineSeries, platform, servicePack, serialNumber;
          m_md3ProX.SoftwareInfo (m_proXhandle, out productVersion, out machineModel, out machineSeries,
                                 out platform, out servicePack, out serialNumber);
          log.InfoFormat ("MML3: added Pro3 alarms for " + productVersion);
          AddAlarmTranslations ("pro3_common");
          AddAlarmTranslations (productVersion);
        }
        else if (Md3ProVersion == 5) {
          AddAlarmTranslations ("pro5_common");
        }
        else {
          AddAlarmTranslations ("makino_machine_alarms");
        }
        m_alarmTranslatorInitialized = true;
      }
    }

    internal void AddAlarmTranslations (string fileName)
    {
      string fileContent = "";
      try {
        using (Stream stream = GetType().Assembly.GetManifestResourceStream (fileName + ".csv")) {
          using (var reader = new StreamReader (stream)) {
            fileContent = reader.ReadToEnd ();
          }
        }
        if (m_alarmTranslator.InitializeWithContent (fileContent)) {
          log.InfoFormat ("MML3: successfully read alarms from file {0}", fileName);
        }
        else {
          log.ErrorFormat ("MML3: error while reading machine alarms from file {0}", fileName);
        }
      }
      catch (Exception e) {
        log.ErrorFormat ("MML3: error while reading machine alarms from file {0}: {1}", fileName, e);
      }
    }

    DateTime TranslateDateTime (SYSTEMTIME systemTime)
    {
      return new DateTime (systemTime.wYear, systemTime.wMonth, systemTime.wDay,
                          systemTime.wHour, systemTime.wMinute, systemTime.wSecond,
                          systemTime.wMilliseconds, DateTimeKind.Local);
    }
    #endregion Private methods
  }
}
