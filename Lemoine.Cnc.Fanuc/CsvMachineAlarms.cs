// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Lemoine.Core.Log;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Parser for csv file allowing the Fanuc input module to read machine alarms
  /// All addresses must use the same register type
  /// </summary>
  public class CsvMachineAlarms
  {
    class AlarmActivated
    {
      public string Number { get; private set; }
      public string Message { get; private set; }
      public bool Activated { get; set; }
      public IDictionary<string, string> Properties { get; set; }

      public AlarmActivated (string number, string message)
      {
        Number = number;
        Message = message;
        Activated = false;
        Properties = new Dictionary<string, string> ();
      }
    }

    #region Members
    readonly IDictionary<int, IDictionary<int, AlarmActivated>> m_alarms =
      new Dictionary<int, IDictionary<int, AlarmActivated>> ();
    int m_minAddress = 0;
    int m_maxAddress = 0;
    string m_file = "";
    Import.FwLib.Pmc.ADDRESS m_addressType = Import.FwLib.Pmc.ADDRESS.ALL;
    readonly string m_machineAlarmInput = "unknown";
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (CsvMachineAlarms).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CsvMachineAlarms (string file, string machineAlarmInput)
    {
      m_file = file;
      m_machineAlarmInput = machineAlarmInput;
      Initialize ();
    }
    #endregion // Constructors

    #region Methods
    void Initialize ()
    {
      // Read the csv file
      string fileContent = GetFileContent ();

      // Fill the addresses and messages
      var lines = fileContent.Split ('\n');
      foreach (string line in lines) {
        // Extract data
        try {
          var elements = line.Split ('\t');
          if (elements[0][0] != '#') { // Exclusion of the lines starting with '#'

            // Read the address type if it's not already done
            if (m_addressType == Import.FwLib.Pmc.ADDRESS.ALL) {
              FindAddressType (line);
            }

            var address = elements[0].Remove (0, 1).Split ('.');
            int addressNum = int.Parse (address[0]);
            int bit = int.Parse (address[1]);
            string alarmNumber = elements[1];
            string alarmMessage = (elements.Length > 2) ? elements[2] : "";

            if (!m_alarms.ContainsKey (addressNum)) {
              if (m_minAddress == 0) {
                m_minAddress = addressNum;
                m_maxAddress = addressNum;
              }
              else {
                if (addressNum < m_minAddress) {
                  m_minAddress = addressNum;
                }
                else if (addressNum > m_maxAddress) {
                  m_maxAddress = addressNum;
                }
              }
              m_alarms[addressNum] = new Dictionary<int, AlarmActivated> ();
            }
            m_alarms[addressNum][bit] = new AlarmActivated (alarmNumber, alarmMessage);

            // Additional properties?
            for (int i = 2; i < elements.Length; i++) {
              var split = elements[i].Split ('"');
              if (split.Length == 2) {
                m_alarms[addressNum][bit].Properties[split[0]] = split[1];
              }
              else {
                log.WarnFormat ("Cannot parse the additional property '{0}'", elements[i]);
              }
            }
          }
        }
        catch (Exception e) {
          log.ErrorFormat ("Fanuc csv alarm parser '{0}': error while parsing machine alarm {1} ({2})",
                          m_machineAlarmInput, line, e);
        }
      }
      log.InfoFormat ("{3}: successfully parsed {0} machine alarms. Addresses from {1} to {2}",
                     lines.Length, m_minAddress, m_maxAddress, m_machineAlarmInput);
    }

    /// <summary>
    /// Read all machine alarms
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    internal Import.FwLib.EW ReadAlarms (ushort handle)
    {
      const int maxBuffer = 512;
      Import.FwLib.EW result = Import.FwLib.EW.OK;

      // We can read only "maxBuffer" adresses at a time
      for (int startAdress = m_minAddress; startAdress <= m_maxAddress; startAdress = startAdress + maxBuffer) {
        // Get alarm data
        int bufferLength = Math.Min (maxBuffer, m_maxAddress - startAdress + 1);
        Import.FwLib.EW resultTmp;
        var data = Fanuc.ReadPmcData (handle, out resultTmp, m_addressType, startAdress, bufferLength);
        if (resultTmp != Import.FwLib.EW.OK) {
          log.ErrorFormat ("Couldn't read {0}-{1}: {2}", m_addressType, startAdress, resultTmp);
          result = resultTmp;
        }

        // Read alarms
        for (int address = startAdress; address < startAdress + bufferLength; address++) {
          if (m_alarms.ContainsKey (address)) {
            byte value = data[address - startAdress];
            for (int bit = 0; bit < 8; bit++) {
              if (m_alarms[address].ContainsKey (bit)) {
                m_alarms[address][bit].Activated = ((value & (1 << bit)) != 0);
                if (m_alarms[address][bit].Activated) {
                  log.InfoFormat ("Found machine alarm {0}: {1}",
                                 m_alarms[address][bit].Number,
                                 m_alarms[address][bit].Message);
                }
              }
            }
          }
        }
      }

      return result;
    }

    /// <summary>
    /// Get the machine alarms from a csv file
    /// </summary>
    /// <returns></returns>
    public IList<CncAlarm> GetAlarms ()
    {
      var machineAlarms = new List<CncAlarm> ();

      foreach (var bitAlarms in m_alarms.Values) {
        foreach (var bitAlarm in bitAlarms.Values) {
          if (bitAlarm.Activated) {
            var alarm = new CncAlarm ("Fanuc", m_machineAlarmInput, "machine alarm", bitAlarm.Number);
            alarm.Message = bitAlarm.Message;
            foreach (var property in bitAlarm.Properties) {
              alarm.Properties[property.Key] = property.Value;
            }

            machineAlarms.Add (alarm);
          }
        }
      }

      return machineAlarms;
    }

    string GetFileContent ()
    {
      string fileContent = "";

      // File present next to the dll?
      string filePath = Path.Combine (Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location), m_file);
      if (File.Exists (filePath)) {
        fileContent = File.ReadAllText (filePath);
        log.InfoFormat ("GetFileContent: read alarm definitions from file {0}", filePath);
      }
      else {
        log.InfoFormat ("GetFileContent: couldn't find file {0}", filePath);
      }

      // Or an embedded file?
      if (string.IsNullOrEmpty (fileContent)) {
        using (Stream stream = Assembly.GetExecutingAssembly ().GetManifestResourceStream (m_file)) {
          using (var reader = new StreamReader (stream)) {
            fileContent = reader.ReadToEnd ();
            log.InfoFormat ("GetFileContent: read alarm definitions from embedded file {0}", m_file);
          }
        }
      }

      return fileContent;
    }

    void FindAddressType (string line)
    {
      var elements = line.Split ('\t');
      try {
        char c = elements[0][0];
        if (c == '#') {
          c = elements[0][1];
        }

        switch (c) {
        case 'A':
          m_addressType = Import.FwLib.Pmc.ADDRESS.A;
          break;
        case 'C':
          m_addressType = Import.FwLib.Pmc.ADDRESS.C;
          break;
        case 'D':
          m_addressType = Import.FwLib.Pmc.ADDRESS.D;
          break;
        case 'E':
          m_addressType = Import.FwLib.Pmc.ADDRESS.E;
          break;
        case 'F':
          m_addressType = Import.FwLib.Pmc.ADDRESS.F;
          break;
        case 'G':
          m_addressType = Import.FwLib.Pmc.ADDRESS.G;
          break;
        case 'K':
          m_addressType = Import.FwLib.Pmc.ADDRESS.K;
          break;
        case 'R':
          m_addressType = Import.FwLib.Pmc.ADDRESS.R;
          break;
        case 'T':
          m_addressType = Import.FwLib.Pmc.ADDRESS.T;
          break;
        case 'X':
          m_addressType = Import.FwLib.Pmc.ADDRESS.X;
          break;
        case 'Y':
          m_addressType = Import.FwLib.Pmc.ADDRESS.Y;
          break;
        default:
          m_addressType = Import.FwLib.Pmc.ADDRESS.ALL;
          break;
        }
      }
      catch (Exception e) {
        log.ErrorFormat ("Fanuc csv alarm parser: couldn't find the address type in '{0}': {1}", line, e);
        throw;
      }
    }
    #endregion // Methods
  }
}
