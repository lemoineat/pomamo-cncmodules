// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Fanuc_PMC.
  /// </summary>
  public partial class Fanuc
  {
    /// <summary>
    /// Read a single PMC address
    /// </summary>
    /// <param name="param">For example R19.7 or F004#3</param>
    /// <returns></returns>
    public bool ReadSinglePmcAddress (string param)
    {
      if (string.IsNullOrEmpty (param)) {
        log.Debug ("ReadSinglePmcAddress: empty param, skip it");
        throw new ArgumentException ("empty param", "param");
      }

      if (param.Length < 4) {
        log.ErrorFormat ("ReadSinglePmcAddress: " +
                         "{0} is invalid (length < 4)",
                         param);
        throw new ArgumentException ("invalid param", "param");
      }

      Import.FwLib.Pmc.ADDRESS addressType = GetAddressType (param[0]);
      string[] addresses = param.Substring (1).Split (new char[] { '.', '#' }, 2);
      if (2 != addresses.Length) {
        log.ErrorFormat ("ReadSinglePmcAddress: " +
                         "{0} is invalid (no separator)",
                         param);
        throw new ArgumentException ("invalid param", "param");
      }
      int address = int.Parse (addresses[0]);
      int bitNumber = int.Parse (addresses[1]);

      return ReadSinglePmcAddress (addressType, address, bitNumber);
    }

    /// <summary>
    /// Read a word in PMC address, made of the eight bits
    /// </summary>
    /// <param name="param">For example R19 or D0</param>
    /// <returns></returns>
    public int ReadWordPmcAddress (string param)
    {
      if (param.Length < 2) {
        log.ErrorFormat ("ReadWordPmcAddress: {0} is invalid", param);
        throw new ArgumentException ("param");
      }

      Import.FwLib.EW result;
      byte value = ReadPmcData (m_handle, out result,
                               GetAddressType (param[0]),
                               int.Parse (param.Substring (1)), 1)[0];
      if (Import.FwLib.EW.OK != result) {
        log.ErrorFormat ("ReadWordPmcAddress: ReadPmcData {0}", result);
        ManageError ("ReadWordPmcAddress", result);
        throw new Exception ("ReadPmcData failed");
      }

      return value;
    }

    bool ReadSinglePmcAddress (Import.FwLib.Pmc.ADDRESS addressType, int address, int bitNumber)
    {
      if (false == IsConnectionValid ()) {
        log.ErrorFormat ("ReadSinglePmcAddress: " +
                         "connection to the CNC failed");
        throw new Exception ("No CNC connection");
      }

      Import.FwLib.EW result;
      var data = ReadPmcData (m_handle, out result, addressType, address, 1);
      if (Import.FwLib.EW.OK != result) {
        log.ErrorFormat ("ReadSinglePmcAddress: " +
                         "rdpmcrng failed with error {0}",
                         result);
        ManageError ("ReadSinglePmcAddress", result);
        throw new Exception ("rdpmcrnc_cdata failed");
      }
      int mask = 1 << bitNumber; // bit 0 = 1 = 00001, bit 1 = 00010, bit 2 = 00100, bit 3 = 01000
      int v = (int)(data[0] & mask);
      log.DebugFormat ("ReadSinglePmcAddress: " +
                       "rdpmcrng was successful and returned " +
                       "raw={0} v={1}",
                       data[0], v);
      return (0 < v);
    }

    /// <summary>
    /// SingleBlock
    /// </summary>
    public bool SingleBlock
    {
      get {
        // MSBK <F004#3>
        return ReadSinglePmcAddress (Import.FwLib.Pmc.ADDRESS.F, 4, 3);
      }
    }

    /// <summary>
    /// RapidTraverse
    /// </summary>
    public bool RapidTraverse
    {
      get {
        // RPDO <F002#1>
        return ReadSinglePmcAddress (Import.FwLib.Pmc.ADDRESS.F, 2, 1);
      }
    }

    /// <summary>
    /// CuttingFeed
    /// </summary>
    public bool CuttingFeed
    {
      get {
        // CUT <F002#6>
        return ReadSinglePmcAddress (Import.FwLib.Pmc.ADDRESS.F, 2, 6);
      }
    }

    /// <summary>
    /// RapidTraverseOverride
    /// </summary>
    public int RapidTraverseOverride
    {
      get {
        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("RapidTraverseOverride.get: " +
                           "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        // 1. Use the rapid traverse override signals or the 1% step traverse override signals ?
        //    G096#7
        Import.FwLib.EW result;
        var data = ReadPmcData (m_handle, out result, Import.FwLib.Pmc.ADDRESS.G, 96, 1);
        if (Import.FwLib.EW.OK != result) {
          log.ErrorFormat ("RapidTraverseOverride.get: " +
                           "1st rdpmcrng failed with error {0}",
                           result);
          ManageError ("RapidTraverseOverride", result);
          throw new Exception ("rdpmcrnc_cdata failed");
        }
        int v = (int)(data[0] & 128); // bit 7 = 128 = 2^7 = 01000000 = 0x80
        log.DebugFormat ("RapidTraverseOverride.get: " +
                         "1st rdpmcrng was successful and returned " +
                         "raw={0} cancel flag={1}",
                         data[0], v);
        if (v > 0) { // 1% step: HROV0 to HROV6 <G096 #0 to #6>
          int r = data[0];
          r = ~r; // make the complement
          r &= 0x7f; // keep only the latest 7 bits
          if (r > 100) { // clamped at 100% if greater than 100%
            r = 100;
          }
          log.DebugFormat ("RapidTraverseOverride.get: " +
                           "1% step traverse override, " +
                           "got raw={0} rapidTraverseOverride={1}",
                           data[0], r);
          return r;
        }
        else { // ROV1,ROV2 <G014#0,#1>
          log.Debug ("RapidTraverseOverride.get: " +
                     "Not the 1% step traverse override signals, " +
                     "ROV1/ROV2 signals");
          data = ReadPmcData (m_handle, out result, Import.FwLib.Pmc.ADDRESS.G, 14, 1);
          if (Import.FwLib.EW.OK != result) {
            log.ErrorFormat ("RapidTraverseOverride.get: " +
                             "rdpmcrng on ROV1/ROV2 failed with error {0}",
                             result);
            ManageError ("RapidTraverseOverride", result);
            throw new Exception ("rdpmcrng failed");
          }
          int rov1 = (int)(data[0] & 1); // bit 1 = 1 = 2^0
          int rov2 = (int)(data[0] & 2); // bit 2 = 2 = 2^1 = 010
          if (0 == rov2) {
            if (0 == rov1) { // 100%
              log.Info ("RapidTraverseOverride.get: " +
                        "rov1=0, rov2=0 => 100%");
              return 100;
            }
            else { // 1 == rov1 => 50%
              log.Info ("RapidTraverseOverride.get: " +
                        "rov1=1, rov2=0 => 50%");
              return 50;
            }
          }
          else { // 1 == rov2
            if (0 == rov1) { // 25%
              log.Info ("RapidTraverseOverride.get: " +
                        "rov1=0, rov2=1 => 25%");
              return 25;
            }
            else { // 1 == rov1 => F0 %
              log.Info ("RapidTraverseOverride.get: " +
                        "rov1=1, rov2=1 => F0%");
              // TODO: get F0 from parameter No. 1421
              log.ErrorFormat ("RapidTraverseOverride.get: " +
                               "retrieving parameter 1421 for F0 is not implemented");
              throw new NotImplementedException ();
            }
          }
        }
      }
    }

    /// <summary>
    /// Feedrate override
    /// </summary>
    public int FeedrateOverride
    {
      get {
        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("FeedrateOverride.get: " +
                           "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        // 1. Test the OVC (override cancelled) flag: bit 4 of address G006
        Import.FwLib.EW result;
        var data = ReadPmcData (m_handle, out result, Import.FwLib.Pmc.ADDRESS.G, 6, 1);
        if (Import.FwLib.EW.OK != result) {
          log.ErrorFormat ("FeedrateOverride.get: " +
                           "1st rdpmcrng failed with error {0}",
                           result);
          ManageError ("FeedrateOverride", result);
          throw new Exception ("rdpmcrnc_cdata failed");
        }
        int v = (int)(data[0] & 16); // 16 = 2^4 = 0x10
        log.DebugFormat ("FeedrateOverride.get: " +
                         "1st rdpmcrng was successful and returned " +
                         "raw={0} cancel flag={1}",
                         data[0], v);
        if (v > 0) {
          log.Info ("FeedrateOverride.get: " +
                    "feedrate override cancel flag is active " +
                    "=> return 100");
          return 100;
        }

        // 2. Get the feedrate override
        data = ReadPmcData (m_handle, out result, Import.FwLib.Pmc.ADDRESS.G, 12, 1);
        if (Import.FwLib.EW.OK != result) {
          log.ErrorFormat ("FeedrateOverride.get: " +
                           "2nd rdpmcrng failed with error {0}",
                           result);
          ManageError ("FeedrateOverride", result);
          throw new Exception ("rdpmcrng failed");
        }
        int r = data[0];
        r = ~r; // make the complement
        r &= 0xff; // keep only the latest byte
        int feedrateOverride = r;
        log.DebugFormat ("FeedrateOverride.get: " +
                         "2nd rdpmcrng was successful and returned " +
                         "raw={0} feedrateOverride={1}",
                         data[0], feedrateOverride);
        return feedrateOverride;
      }
    }

    /// <summary>
    /// Spindle speed override
    /// </summary>
    public int SpindleSpeedOverride
    {
      get {
        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("SpindleSpeedOverride.get: " +
                           "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        Import.FwLib.EW result;
        var data = ReadPmcData (m_handle, out result, Import.FwLib.Pmc.ADDRESS.G, 30, 1);
        if (Import.FwLib.EW.OK != result) {
          log.ErrorFormat ("SpindleSpeedOverride.get: " +
                           "rdpmcrng failed with error {0}",
                           result);
          ManageError ("SpindleSpeedOverride", result);
          throw new Exception ("rdpmcrng failed");
        }
        int spindleSpeedOverride = data[0];
        log.DebugFormat ("SpindleSpeedOverride.get: " +
                         "rdpmcrng was successful and returned " +
                         "raw={0} spindleSpeedOverride={1}",
                         data[0], spindleSpeedOverride);
        return spindleSpeedOverride;
      }
    }

    /// <summary>
    /// Read a range of PMC variables: fast but it can fail if we are out of range
    /// </summary>
    /// <param name="param">{AdressType}-{min}-{max}</param>
    /// <returns></returns>
    public IDictionary<string, double> GetPmcDataRange (string param)
    {
      // Parse the parameter that should be in the form {AdressType}-{min}-{max}
      var split = param.Split ('-');
      if (split.Length != 3 || split[0].Length != 1) {
        log.ErrorFormat ("GetPmcDataRange: invalid format for the parameter {0}, not {AdressType}-{min}-{max}, for example 'A-1-100'");
        throw new ArgumentException ("GetPmcDataRange: invalid parameter format", "param");
      }
      Import.FwLib.Pmc.ADDRESS addressType = GetAddressType (split[0][0]);
      int min = int.Parse (split[1]);
      int max = int.Parse (split[2]);

      // Read data
      Import.FwLib.EW result;
      var data = ReadPmcData (m_handle, out result, addressType, min, max - min + 1);
      if (result != Import.FwLib.EW.OK) {
        log.ErrorFormat ("GetPmcDataRange: rdpmcrng failed with error {0}", result);
        ManageError ("GetPmcDataRange", result);
        throw new Exception ("rdpmcrng failed");
      }

      // Store the values
      IDictionary<string, double> dic = new Dictionary<string, double> ();
      for (int i = min; i <= max; i++) {
        dic[split[0] + i.ToString ()] = data[i - min];
      }

      return dic;
    }

    /// <summary>
    /// Read a range of PMC variables, one by one: slow but we have everything
    /// </summary>
    /// <param name="param">{AdressType}-{min}-{max}</param>
    /// <returns></returns>
    public IDictionary<string, double> GetPmcDataRangeOneByOne (string param)
    {
      // Parse the parameter that should be in the form {AdressType}-{min}-{max}
      var split = param.Split ('-');
      if (split.Length != 3 || split[0].Length != 1) {
        log.ErrorFormat ("GetPmcDataRangeOneByOne: invalid format for the parameter {0}, not {AdressType}-{min}-{max}, for example 'A-1-100'");
        throw new ArgumentException ("GetPmcDataRangeOneByOne: invalid parameter format", "param");
      }
      Import.FwLib.Pmc.ADDRESS addressType = GetAddressType (split[0][0]);
      int min = int.Parse (split[1]);
      int max = int.Parse (split[2]);

      // Read data one by one
      IDictionary<string, double> dic = new Dictionary<string, double> ();
      for (int i = min; i <= max; i++) {
        try {
          Import.FwLib.EW result;
          var data = ReadPmcData (m_handle, out result, addressType, i, 1);
          if (Import.FwLib.EW.OK != result) {
            log.ErrorFormat ("GetPmcDataRangeOneByOne: rdpmcrng failed with error {0}", result);
            ManageError ("GetPmcDataRangeOneByOne", result);
          }

          // Store the value
          dic[split[0] + i.ToString ()] = data[0];
        }
        catch (Exception) {
          // We continue
        }
      }

      return dic;
    }

    /// <summary>
    /// Read a PMC variable
    /// </summary>
    /// <param name="param">{AdressType}-{variable number}</param>
    /// <returns></returns>
    public double GetPmcData (string param)
    {
      // Parse the parameter that should be in the form {AdressType}-{variable number}
      var split = param.Split ('-');
      if (split.Length != 2 || split[0].Length != 1) {
        log.ErrorFormat ("GetPmcData: invalid format for the parameter {0}, not {AdressType}-{variable number}, for example 'A-100'");
        throw new ArgumentException ("GetPmcData: invalid parameter format", "param");
      }
      Import.FwLib.Pmc.ADDRESS addressType = GetAddressType (split[0][0]);
      int variableNumber = int.Parse (split[1]);

      // Read the value
      Import.FwLib.EW result;
      var data = ReadPmcData (m_handle, out result, addressType, variableNumber, 1);
      if (Import.FwLib.EW.OK != result) {
        log.ErrorFormat ("GetPmcData: rdpmcrng failed with error {0}", result);
        ManageError ("GetPmcData", result);
      }
      return (double)data[0];
    }

    /// <summary>
    /// Write a byte in a PMC variable
    /// </summary>
    /// <param name="param">{AdressType}-{variable number}</param>
    /// <param name="value">value as a byte</param>
    /// <returns>Error code</returns>
    public void SetPmcData (string param, byte value)
    {
      // Parse the parameter that should be in the form {AdressType}-{variable number}
      var split = param.Split ('-');
      if (split.Length != 2 || split[0].Length != 1) {
        log.ErrorFormat ("SetPmcData: invalid format for the parameter {0}, not {AdressType}-{variable number}, for example 'A-100'");
        throw new ArgumentException ("SetPmcData: invalid parameter format", "param");
      }
      Import.FwLib.Pmc.ADDRESS addressType = GetAddressType (split[0][0]);
      int variableNumber = int.Parse (split[1]);

      // Prepare the buffer
      Import.FwLib.IODBPMC_CDATA buffer = new Import.FwLib.IODBPMC_CDATA ();
      buffer.type_a = (short)addressType;
      buffer.type_d = 0; // byte
      buffer.datano_e = (short)variableNumber;
      buffer.datano_s = (short)variableNumber;
      buffer.cdata[0] = value;

      // Write data
      var result = (Import.FwLib.EW)Import.FwLib.Pmc.wrpmcrng (m_handle, (short)(8 + 1), buffer);
      if (result != Import.FwLib.EW.OK) {
        log.ErrorFormat ("SetPmcData: wrpmcrng failed with {0}", result);
        ManageError ("SetPmcData", result);
        throw new Exception ("SetPmcData failed");
      }
    }

    /// <summary>
    /// Convenient function to read PMC data
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="result"></param>
    /// <param name="addressType"></param>
    /// <param name="startAddress"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    static internal byte[] ReadPmcData (ushort handle, out Import.FwLib.EW result,
                                       Import.FwLib.Pmc.ADDRESS addressType,
                                       int startAddress, int length)
    {
      if (length > 512) {
        throw new Exception ("PmcData structure has not been configured for data of more than 512 bytes");
      }

      Import.FwLib.IODBPMC_CDATA buffer;
      result = (Import.FwLib.EW)Import.FwLib.Pmc.rdpmcrng (
        handle,
        (short)addressType,
        (short)Import.FwLib.Pmc.TYPE.BYTE,
        (ushort)startAddress, (ushort)(startAddress + length - 1),
        (ushort)(8 + length),
        out buffer);

      // Copy of the data
      var ret = new byte[length];
      for (int i = 0; i < length; i++) {
        ret[i] = buffer.cdata[i];
      }

      return ret;
    }

    Import.FwLib.Pmc.ADDRESS GetAddressType (char addressTypeLetter)
    {
      Import.FwLib.Pmc.ADDRESS addressType;
      switch (addressTypeLetter) {
      case 'A':
        addressType = Import.FwLib.Pmc.ADDRESS.A;
        break;
      case 'C':
        addressType = Import.FwLib.Pmc.ADDRESS.C;
        break;
      case 'D':
        addressType = Import.FwLib.Pmc.ADDRESS.D;
        break;
      case 'E':
        addressType = Import.FwLib.Pmc.ADDRESS.E;
        break;
      case 'F':
        addressType = Import.FwLib.Pmc.ADDRESS.F;
        break;
      case 'G':
        addressType = Import.FwLib.Pmc.ADDRESS.G;
        break;
      case 'K':
        addressType = Import.FwLib.Pmc.ADDRESS.K;
        break;
      case 'R':
        addressType = Import.FwLib.Pmc.ADDRESS.R;
        break;
      case 'T':
        addressType = Import.FwLib.Pmc.ADDRESS.T;
        break;
      case 'X':
        addressType = Import.FwLib.Pmc.ADDRESS.X;
        break;
      case 'Y':
        addressType = Import.FwLib.Pmc.ADDRESS.Y;
        break;
      default:
        log.ErrorFormat ("GetAddressType: " +
                         "invalid address type {0}",
                         addressTypeLetter);
        throw new ArgumentException ("param");
      }
      return addressType;
    }
  }
}
