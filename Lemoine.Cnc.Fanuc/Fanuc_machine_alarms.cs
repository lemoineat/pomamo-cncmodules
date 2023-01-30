// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Fanuc_machine_alarms.
  /// </summary>
  public partial class Fanuc
  {
    #region Members
    IList<CncAlarm> m_machineAlarms;
    CsvMachineAlarms m_csvMachineAlarms = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Defines where the data has to be read to create tool life data
    /// Possible options are:
    ///  * none, by default
    ///  * murata
    ///  * niigata
    ///  * moriseiki
    ///  * or a csv file name that is next to the dll Lemoine.Cnc.Fanuc.dll
    /// </summary>
    public string MachineAlarmInput { get; set; }

    /// <summary>
    /// Machine alarms
    /// </summary>
    public IList<CncAlarm> MachineAlarms
    {
      get {
        if (m_machineAlarms != null || GetMachineAlarmsInformation ()) {
          return m_machineAlarms;
        }

        log.ErrorFormat ("MachineAlarms.get: failed because alarms information was unavailable");
        throw new Exception ("MachineAlarms failed");
      }
    }
    #endregion // Getters / Setters

    #region Methods
    bool GetMachineAlarmsInformation ()
    {
      m_machineAlarms = null;

      log.InfoFormat ("Fanuc: reading alarms for {0}", MachineAlarmInput);
      try {
        switch (MachineAlarmInput) {
        case "murata":
          m_machineAlarms = GetMachineAlarms_csv ("Lemoine.Cnc.Fanuc.murata_machine_errors.csv");
          break;
        case "niigata":
          m_machineAlarms = GetMachineAlarms_csv ("Lemoine.Cnc.Fanuc.niigata_machine_errors.csv");
          break;
        case "moriseiki":
          m_machineAlarms = GetMachineAlarms_csv ("Lemoine.Cnc.Fanuc.mori_seiki_machine_errors.csv");
          break;
        case "none":
        case "": // nothing
          m_machineAlarms = new List<CncAlarm> ();
          break;
        default:
          m_machineAlarms = GetMachineAlarms_csv (MachineAlarmInput);
          break;
        }
      }
      catch (Exception e) {
        log.ErrorFormat ("Fanuc: couldn't initialize machine alarms for {0}: {1}",
                        MachineAlarmInput, e);
        return false;
      }

      return true;
    }

    IList<CncAlarm> GetMachineAlarms_csv (string file)
    {
      if (m_csvMachineAlarms == null) {
        log.InfoFormat ("Fanuc: create a csv parser with '{0}' for reading alarms", file);
        m_csvMachineAlarms = new CsvMachineAlarms (file, MachineAlarmInput);
      }

      if (m_machineAlarms == null) {
        Import.FwLib.EW result = m_csvMachineAlarms.ReadAlarms (m_handle);
        if (result != Import.FwLib.EW.OK) {
          log.ErrorFormat ("GetMachineAlarms_csv: error {0}", result);
          ManageError ("GetMachineAlarms_csv", result);
        }
        else {
          m_machineAlarms = m_csvMachineAlarms.GetAlarms ();
        }
      }

      return m_machineAlarms;
    }
    #endregion // Methods
  }
}
