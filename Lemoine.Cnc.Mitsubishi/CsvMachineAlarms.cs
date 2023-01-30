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
  class CsvMachineAlarms
  {
    /// <summary>
    /// Convenient class to store the definition of an alarm and its register
    /// </summary>
    public class AlarmRegister
    {
      /// <summary>
      /// Name of the device
      /// </summary>
      public string DeviceName { get; private set; }

      /// <summary>
      /// Register number
      /// </summary>
      public int RegisterNumber { get; private set; }

      /// <summary>
      /// Message
      /// </summary>
      public string Message { get; private set; }

      /// <summary>
      /// Alarm number
      /// </summary>
      public string AlarmNumber { get; private set; }

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="deviceName"></param>
      /// <param name="registerNumber"></param>
      /// <param name="message"></param>
      /// <param name="alarmNumber"></param>
      public AlarmRegister (string deviceName, int registerNumber, string message, string alarmNumber)
      {
        DeviceName = deviceName;
        RegisterNumber = registerNumber;
        Message = message;
        AlarmNumber = alarmNumber;
      }
    }

    #region Members
    readonly IList<AlarmRegister> m_alarms = new List<AlarmRegister> ();
    string m_file = "";
    readonly ILog m_logger = null;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="file"></param>
    /// <param name="logger"></param>
    public CsvMachineAlarms (string file, ILog logger)
    {
      m_logger = logger;
      m_file = file;
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
            var address = elements[0].Split ('.');
            string addressRegister = address[0];
            int addressNum = int.Parse (address[1]);
            string alarmNumber = elements[1];
            string alarmMessage = (elements.Length > 2) ? elements[2] : "";
            m_alarms.Add (new AlarmRegister (addressRegister, addressNum, alarmMessage, alarmNumber));
          }
        }
        catch (Exception ex) {
          m_logger.Error ($"Initialize: error while parsing machine alarm {line}", ex);
        }
      }
      m_logger.InfoFormat ("Mitsubishi.CsvMachineAlarms - successfully added {0} machine alarm definitions", m_alarms.Count);
    }

    /// <summary>
    /// Read all machine alarms
    /// </summary>
    /// <returns></returns>
    public IList<AlarmRegister> GetAlarmRegisters ()
    {
      return m_alarms;
    }

    string GetFileContent ()
    {
      string fileContent = "";

      // File present next to the dll?
      string filePath = Path.Combine (Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location), m_file);
      if (File.Exists (filePath)) {
        fileContent = File.ReadAllText (filePath);
        m_logger.InfoFormat ("GetFileContent: read alarm definitions from file {0}", filePath);
      }
      else {
        m_logger.InfoFormat ("GetFileContent: couldn't find file {0}", filePath);
      }

      // Or an embedded file?
      if (string.IsNullOrEmpty (fileContent)) {
        using (Stream stream = Assembly.GetExecutingAssembly ().GetManifestResourceStream (m_file)) {
          using (var reader = new StreamReader (stream)) {
            fileContent = reader.ReadToEnd ();
            m_logger.InfoFormat ("GetFileContent: read alarm definitions from embedded file {0}", m_file);
          }
        }
      }

      return fileContent;
    }
    #endregion // Methods
  }
}
