// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.Text;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Interface_device.
  /// </summary>
  public class Interface_device : GenericMitsubishiInterface
  {
    /// <summary>
    /// Kind of value
    /// </summary>
    enum ValueType
    {
      /// <summary>
      /// Read a single bit
      /// </summary>
      EZNC_PLC_BIT = 0x11,

      /// <summary>
      /// Read a byte (8 bits)
      /// </summary>
      EZNC_PLC_BYTE = 0x12,

      /// <summary>
      /// Read a word (2 bytes)
      /// </summary>
      EZNC_PLC_WORD = 0x14,

      /// <summary>
      /// Read a double word (4 bytes)
      /// </summary>
      EZNC_PLC_DWORD = 0x18
    }

    #region Members
    readonly IList<ValueType> m_dataSizes = new List<ValueType> ();
    readonly IList<int> m_registerNumbers = new List<int> ();
    readonly IList<string> m_deviceNames = new List<string> ();
    int[] m_results = null;
    CsvMachineAlarms m_csvMachineAlarms = null;
    IList<CncAlarm> m_machineAlarms = null;
    bool m_machineAlarmListed = false;
    #endregion Members

    #region Protected methods
    /// <summary>
    /// Work to do on "start", before new data will be read
    /// Already within a try catch
    /// </summary>
    protected override void ResetDataInstance ()
    {
      m_results = null;
      m_machineAlarms = null;
      int dataNumber = m_dataSizes.Count;
      if (dataNumber > 0) {
        // Remove previous values
        CommunicationObject.Device_DeleteAll ();

        // Prepare the values (doesn't work yet...)
        var vDevice = new String[dataNumber];
        var vDataType = new int[dataNumber];
        var vValue = new int[dataNumber];
        for (int i = 0; i < dataNumber; i++) {
          vDevice[i] = m_deviceNames[i] + m_registerNumbers[i];
          vDataType[i] = (int)m_dataSizes[i];
          vValue[i] = 0; // Useful only if WriteDevice is called, but here we only read
        }
        Logger.Info ("Mitsubishi.ReadDevice - Setting the device with Device_SetDevice");
        var errorCode = 0;
        if ((errorCode = CommunicationObject.Device_SetDevice (vDevice, vDataType, vValue)) != 0) {
          throw new ErrorCodeException (errorCode, "Device_SetDevice");
        }

        Logger.Info ("Mitsubishi.ReadDevice - Device set with Device_SetDevice, now using Device_Read");

        // Compute the results
        object values = null;
        if ((errorCode = CommunicationObject.Device_Read (out values)) != 0) {
          throw new ErrorCodeException (errorCode, "Device_Read");
        }

        // Convert into an array
        if (values == null) {
          throw new Exception ("Mitsubishi.ReadDevice - The result of Device_Read is null");
        }

        if (!values.GetType ().IsArray) {
          throw new Exception ("Mitsubishi.ReadDevice - The result of Device_Read is not an array");
        }

        m_results = values as int[];

        // Check the right number of results
        if (m_results == null) {
          throw new Exception ("Mitsubishi.ReadDevice - The result of Device_Read is not an array of int");
        }

        if (m_results.Length != m_dataSizes.Count) {
          Logger.ErrorFormat ("Mitsubishi.ReadDevice - Device_Read returned {0} results but {1} were expected",
            m_results.Length, m_dataSizes.Count);
        }
        else {
          Logger.InfoFormat ("Mitsubishi.ReadDevice - Device_Read returned {0} results", m_results.Length);
        }

        Logger.Info ("4");
      }
    }
    #endregion // Protected methods

    #region Get methods
    /// <summary>
    /// Read a value in the specified device
    /// </summary>
    /// <param name="deviceName"></param>
    /// <param name="registerNumber"></param>
    /// <param name="dataSize"></param>
    /// <returns></returns>
    public UInt32 ReadDevice (string deviceName, int registerNumber, int dataSize)
    {
      ValueType type;
      switch (dataSize) {
        case 1:
          type = ValueType.EZNC_PLC_BIT;
          break;
        case 8:
          type = ValueType.EZNC_PLC_BYTE;
          break;
        case 16:
          type = ValueType.EZNC_PLC_WORD;
          break;
        case 32:
          type = ValueType.EZNC_PLC_DWORD;
          break;
        default:
          throw new Exception ("Mitsubishi.ReadDevice - Invalid datasize '" + dataSize + "'. Possible values are 1, 8, 16 and 32.");
      }

      // Find the index of the query
      int index = -1;
      for (int i = 0; i < m_registerNumbers.Count; i++) {
        if (m_dataSizes[i] == type && m_registerNumbers[i] == registerNumber && string.Equals (m_deviceNames[i], deviceName)) {
          index = i;
          break;
        }
      }

      if (index == -1) {
        // Create the query
        m_dataSizes.Add (type);
        m_registerNumbers.Add (registerNumber);
        m_deviceNames.Add (deviceName);
        index = m_dataSizes.Count - 1;
      }

      // Return the value if available
      if (m_results == null || index >= m_results.Length) {
        string errorMsg = string.Format ("Mitsubishi.ReadDevice - value {0}{1} is not ready", deviceName, registerNumber);
        throw new NotReadyException (errorMsg);
      }

      return (UInt32)m_results[index];
    }

    /// <summary>
    /// Get the machine alarms based on a file listing the registers / alarm numbers / alarm descriptions
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public IList<CncAlarm> GetMachineAlarms (string file)
    {
      // Possibly create the machine alarm parser
      if (m_csvMachineAlarms == null) {
        Logger.InfoFormat ("GetMachineAlarms: create a csv parser with file '{0}' for reading alarms", file);
        m_csvMachineAlarms = new CsvMachineAlarms (file, Logger);
      }

      // Possibly read machine alarms
      if (m_machineAlarms == null) {
        var alarmRegisters = m_csvMachineAlarms.GetAlarmRegisters ();

        if (m_machineAlarmListed) {
          // Add an alarm for each value that is not 0
          m_machineAlarms = new List<CncAlarm> ();
          foreach (var alarmRegister in alarmRegisters) {
            if (ReadDevice (alarmRegister.DeviceName, alarmRegister.RegisterNumber, 1) != 0) {
              var alarm = new CncAlarm ("Mitsubishi", "Machine alarm", alarmRegister.AlarmNumber);
              alarm.Message = alarmRegister.Message;
              alarm.CncSubInfo = SystemType.ToString ();
              m_machineAlarms.Add (alarm);
              Logger.InfoFormat ("Mitsubishi.GetMachineAlarms - Found alarm '{0}'", alarm);
            }
          }
        }
        else {
          // Just list the different addresses to read
          foreach (var alarmRegister in alarmRegisters) {
            m_dataSizes.Add (ValueType.EZNC_PLC_BIT);
            m_deviceNames.Add (alarmRegister.DeviceName);
            m_registerNumbers.Add (alarmRegister.RegisterNumber);
          }
          m_machineAlarmListed = true;

          // Throw an exception
          throw new NotReadyException ("Mitsubishi.GetMachineAlarms - result not ready yet");
        }
      }

      return m_machineAlarms;
    }
    #endregion Get methods
  }
}
