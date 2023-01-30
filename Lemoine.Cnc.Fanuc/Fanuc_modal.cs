// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Fanuc_others.
  /// </summary>
  public partial class Fanuc
  {
    /// <summary>
    /// Get the current status of a modal group
    /// 
    /// The modal group is made of:
    /// <item>the address / the letter</item>
    /// <item>optionally the group type number in case there are several group types for the address, prefixed by /</item>
    /// For example: M/12, X
    /// </summary>
    /// <param name="param">Modal group</param>
    /// <returns></returns>
    public double GetModal (string param)
    {
      // - Parse the parameter
      if (string.IsNullOrEmpty (param)) {
        log.ErrorFormat ("GetModal: " +
                         "invalid parameter {0} (empty or null)",
                         param);
        throw new ArgumentException ("Invalid parameter");
      }
      Debug.Assert (0 < param.Length);
      char address = param[0];
      short? groupTypeNumber = null; // Undefined
      if (1 < param.Length) { // Not only the address
        if ('/' != param[1]) {
          log.WarnFormat ("GetModal: " +
                          "the second character is not / " +
                          "although it should be");
        }
        else if (param.Length < 3) {
          log.WarnFormat ("GetModal: " +
                          "the parameter is not long enough for a group type");
        }
        else {
          string groupTypeString = param.Substring (2);
          short n;
          if (!short.TryParse (groupTypeString, out n)) {
            log.ErrorFormat ("GetModal: " +
                             "the group type in parameter {0} is not an integer",
                             param);
          }
          else {
            log.DebugFormat ("GetModal: " +
                             "the group type is {0}",
                             n);
            groupTypeNumber = n;
          }
        }
      }
      if (log.IsDebugEnabled) {
        log.DebugFormat ("GetModal: address={0} group type={1}",
                         address, groupTypeNumber);
      }
      return GetModal (address, groupTypeNumber);
    }

    double GetModal (char address, short? groupTypeNumber)
    {
      if (this.CncKind.Equals ("15i")) { // Use cnc_rdcommand or cnc_rdgcode
        if ('G' == address) { // Use cnc_rdgcode
          if (!groupTypeNumber.HasValue) {
            throw new Exception ("GetModal, address G, CncKind 15i: groupTypeNumber must be defined");
          }

          return GetModalWithGCode (groupTypeNumber.Value);
        }
        else { // Use cnc_rdcommand
          return GetModalWithRdcommand (address, groupTypeNumber);
        }
      }
      else { // Not 150i control, use cnc_modal
        return GetModalWithModal (address, groupTypeNumber);
      }
    }

    double GetModalWithRdcommand (char address, short? groupTypeNumber)
    {
      CheckAvailability ("rdcommand");
      CheckConnection ();

      if (!groupTypeNumber.HasValue) { // For commanded only addresses
        switch (address) {
        case 'I':
          groupTypeNumber = 108;
          break;
        case 'J':
          groupTypeNumber = 109;
          break;
        case 'K':
          groupTypeNumber = 110;
          break;
        case 'R':
          groupTypeNumber = 117;
          break;
        case 'X':
          groupTypeNumber = 123;
          break;
        case 'Y':
          groupTypeNumber = 124;
          break;
        case 'Z':
          groupTypeNumber = 125;
          break;
        default:
          log.ErrorFormat ("GetModalWithRdcommand: " +
                           "the group type number is unknown, " +
                           "although it is required for address {0}",
                           address);
          throw new ArgumentException ("Missing required group type number");
        }
      }
      Debug.Assert (groupTypeNumber.HasValue);
      short num_cmd = 1;
      Import.FwLib.ODBCMD command;
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdcommand (m_handle,
                                                                 groupTypeNumber.Value,
                                                                 1, // Active block
                                                                 ref num_cmd,
                                                                 out command);
      if (Import.FwLib.EW.OK != result) {
        log.ErrorFormat ("GetModalWithRdcommand: " +
                         "cnc_rdcommand failed with error {0}",
                         result);
        switch (result) {
        case Lemoine.Cnc.Import.FwLib.EW.LENGTH:
          log.ErrorFormat ("GetModalWithRdcommand: " +
                           "Data block length error. " +
                           "The number of G code data to be read (num_cmd) is wrong");
          break;
        case Lemoine.Cnc.Import.FwLib.EW.NUMBER_RANGE:
          log.ErrorFormat ("GetModalWithRdcommand: " +
                           "Data number error. " +
                           "The specification of commanded data (type) is wrong");
          break;
        case Lemoine.Cnc.Import.FwLib.EW.ATTRIB_TYPE:
          log.ErrorFormat ("GetModalWithRdcommand: " +
                           "Data attribute error. " +
                           "The specification of block (block) is wrong");
          break;
        default: // Generic error, do not write a specific error message
          break;
        }
        ManageErrorWithException ("rdcommand", result);
      }
      if (0 == num_cmd) {
        log.WarnFormat ("GetModalWithRdcommand: " +
                        "no value for {0}{1}",
                        address, groupTypeNumber);
        throw new Exception ("No value");
      }
      if (address != command.cmd[0].adrs) {
        log.ErrorFormat ("GetModalWithRdcommand: " +
                         "address {0} was returned although the requested address was {1}, " +
                         "probably the group type {2} was wrong",
                         command.cmd[0].adrs, address,
                         groupTypeNumber);
        throw new ArgumentException ("Bad group type number");
      }
      System.Diagnostics.Debug.Assert (1 == num_cmd);
      double v = command.cmd[0].cmd_val;
      short flag = command.cmd[0].flag;
      if (GetBit (flag, 4)) { // Flag #4: There is a command of decimal point
        v = v * Math.Pow (10.0, -command.cmd[0].dec_val);
      }
      if (GetBit (flag, 5)) { // Flag #5: Negative
        v = -v;
      }
      log.DebugFormat ("GetModalWithRdcommand: " +
                       "got value {0}",
                       v);
      return v;
    }

    double GetModalWithGCode (short groupTypeNumber)
    {
      CheckAvailability ("rdgcode");
      CheckConnection ();

      // Read the GCode number
      short num_cmd = 1;
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdgcode (m_handle, groupTypeNumber,
                                                              1, // Active block
                                                              ref num_cmd,
                                                              out Import.FwLib.ODBGCD gcode);
      if (Import.FwLib.EW.OK != result) {
        log.ErrorFormat ("GetModalWithGCode: cnc_rdgcode failed with error {0}", result);
        ManageErrorWithException ("rdgcode", result);
      }
      if (0 == num_cmd) {
        log.WarnFormat ("GetModalWithGCode: no value for group {0}", groupTypeNumber);
        throw new Exception ("No value");
      }

      if (groupTypeNumber != gcode.gcode[0].group) {
        log.ErrorFormat ("GetModalWithGCode: groupTypeNumber {0} was returned although the requested groupTypeNumber was {1}",
                         gcode.gcode[0].group, groupTypeNumber);
        throw new ArgumentException ("Bad group type number");
      }
      System.Diagnostics.Debug.Assert (1 == num_cmd);
      string code = new string (gcode.gcode[0].code);

      // Extract the number
      var regex = new Regex ("([0-9\\.]+)");
      var match = regex.Match (code);
      double v = 0;
      if (match.Success) {
        try {
          v = double.Parse (match.Groups[1].Value);
        }
        catch (Exception e) {
          log.ErrorFormat ("GetModalWithGCode: couldn't find a number in {0}, exception is {1}", code, e);
          throw new Exception ("Cannot read code number");
        }
      }
      else {
        log.ErrorFormat ("GetModalWithGCode: couldn't find a number in {0}", code);
        throw new Exception ("Cannot read code number");
      }

      log.DebugFormat ("GetModalWithGCode: got value {0}", v);
      return v;
    }

    double GetModalWithModal (char address, short? groupTypeNumber)
    {
      CheckAvailability ("modal");
      CheckConnection ();

      if (!groupTypeNumber.HasValue) { // For commanded only addresses
        switch (address) {
        case 'B':
          groupTypeNumber = 100;
          break;
        case 'D':
          groupTypeNumber = 101;
          break;
        case 'E':
          groupTypeNumber = 102;
          break;
        case 'F':
          groupTypeNumber = 103;
          break;
        case 'H':
          groupTypeNumber = 104;
          break;
        case 'L':
          groupTypeNumber = 105;
          break;
        case 'M':
          groupTypeNumber = 106;
          break;
        case 'S':
          groupTypeNumber = 107;
          break;
        case 'T':
          groupTypeNumber = 108;
          break;
        case 'R':
          groupTypeNumber = 109;
          break;
        case 'P':
          groupTypeNumber = 110;
          break;
        case 'Q':
          groupTypeNumber = 111;
          break;
        case 'A':
          groupTypeNumber = 112;
          break;
        case 'C':
          groupTypeNumber = 113;
          break;
        case 'I':
          groupTypeNumber = 114;
          break;
        case 'J':
          groupTypeNumber = 115;
          break;
        case 'K':
          groupTypeNumber = 116;
          break;
        case 'N':
          groupTypeNumber = 117;
          break;
        case 'O':
          groupTypeNumber = 118;
          break;
        case 'U':
          groupTypeNumber = 119;
          break;
        case 'V':
          groupTypeNumber = 120;
          break;
        case 'W':
          groupTypeNumber = 121;
          break;
        case 'X':
          groupTypeNumber = 122;
          break;
        case 'Y':
          groupTypeNumber = 123;
          break;
        case 'Z':
          groupTypeNumber = 124;
          break;
        default:
          log.ErrorFormat ("GetModalWithModal: " +
                           "the group type number is unknown, " +
                           "although it is required for address {0}",
                           address);
          throw new ArgumentException ("Missing required group type number");
        }
      }
      Debug.Assert (groupTypeNumber.HasValue);
      short activeBlock;
      if (CncKind.StartsWith ("15")) {
        activeBlock = 1;
      }
      else {
        activeBlock = 0;
      }
      if ('G' == address) { // G code
        Import.FwLib.EW result = (Import.FwLib.EW)Import.FwLib.Cnc.modal (
          m_handle, groupTypeNumber.Value, activeBlock, out Import.FwLib.ODBMDL_1 modal);
        switch (result) {
        case Lemoine.Cnc.Import.FwLib.EW.NUMBER_RANGE:
          log.ErrorFormat ("GetModalWithModal: " +
                           "Data number error. " +
                           "The specification of modal data (type) is wrong");
          break;
        case Lemoine.Cnc.Import.FwLib.EW.ATTRIB_TYPE:
          log.ErrorFormat ("GetModalWithModal: " +
                           "Data attribute error. " +
                           "The specification of block (block) is wrong");
          break;
        default: // Generic error, do not write a specific error message
          break;
        }
        ManageErrorWithException ("modal", result);
        if (log.IsDebugEnabled) {
          log.DebugFormat ("GetModalWithModal: " +
                           "got {0} for {1}{2}",
                           modal.g_data, address, groupTypeNumber);
        }

        // Convert g_data to value
        switch (groupTypeNumber.Value) {
        case 5:
          switch (modal.g_data) {
          case 0:
            return 20; // G20
          case 1:
            return 21; // G21
          default:
            throw new Exception ("Unknown g_data " + modal.g_data + " returned for group 5");
          }
        default:
          throw new NotImplementedException ("Conversion from g_data to value for group != 5");
        }
      }
      else { // Other than G code
        Import.FwLib.ODBMDL_3 modal;
        Import.FwLib.EW result = (Import.FwLib.EW)Import.FwLib.Cnc.modal (m_handle,
                                                                           groupTypeNumber.Value,
                                                                           activeBlock,
                                                                           out modal);
        switch (result) {
        case Lemoine.Cnc.Import.FwLib.EW.NUMBER_RANGE:
          log.ErrorFormat ("GetModalWithModal: " +
                           "Data number error. " +
                           "The specification of modal data (type) is wrong");
          break;
        case Lemoine.Cnc.Import.FwLib.EW.ATTRIB_TYPE:
          log.ErrorFormat ("GetModalWithModal: " +
                           "Data attribute error. " +
                           "The specification of block (block) is wrong");
          break;
        default: // Generic error, do not write a specific error message
          break;
        }
        ManageErrorWithException ("modal", result);
        double v = modal.aux.aux_data;
        bool presentBlock;
        if (GetBit (modal.aux.flag1, 5)) { // Positive / Negative
          v = -v;
        }
        if (CncKind.StartsWith ("15")) { // Series 150
          if (GetBit (modal.aux.flag1, 4) && GetBit (modal.aux.flag1, 6)) {
            v = v / 10.0;
          }
          presentBlock = GetBit (modal.aux.flag2, 7);
        }
        else if (CncKind.StartsWith ("3")) { // Series 30i/31i/32i/35i
          if (GetBit (modal.aux.flag1, 6)) { // Decimal point
            v = v * Math.Pow (10.0, -modal.aux.flag2);
          }
          presentBlock = GetBit (modal.aux.flag1, 7);
        }
        else { // Series 160/180/210, 160i/180i/210i, 0i, Power Mate i
          short decimals = 0;
          if (GetBit (modal.aux.flag2, 0)) {
            decimals += 1;
          }
          if (GetBit (modal.aux.flag2, 1)) {
            decimals += 2;
          }
          if (GetBit (modal.aux.flag2, 2)) {
            decimals += 4;
          }
          bool decimalPoint;
          if (MTKind.Contains ("W")
              && (CncKind.Equals ("16i") || CncKind.Equals ("18i"))) { // Series 16i/18i-W
            decimalPoint = GetBit (modal.aux.flag1, 4);
          }
          else {
            decimalPoint = GetBit (modal.aux.flag1, 6);
          }
          if (decimalPoint) {
            v = v * Math.Pow (10.0, -decimals);
          }
          presentBlock = GetBit (modal.aux.flag1, 7);
        }
        if (log.IsDebugEnabled) {
          log.DebugFormat ("GetModalWithModal: " +
                           "got {0} for {1}{2}",
                           v, address, groupTypeNumber);
        }
        if (Equals (0, v) && !presentBlock) {
          log.InfoFormat ("GetModalWithModal: " +
                          "discard value {0} because it may correspond to the default value",
                          v);
          throw new Exception ("Modal value 0 discarded");
        }
        return v;
      } // End other than G code
    }

    /// <summary>
    /// Return a list of active M codes
    /// </summary>
    /// <param name="param">Comma separated list of m-codes to filter. If the first character is '-', the m-codes will be excluded (for example: -0,1), else only the lsited ones will be included (for example: 6,8)</param>
    /// <returns></returns>
    public System.Collections.IList GetActiveMCodes (string param)
    {
      var result = new System.Collections.Generic.List<int> ();
      try {
        result.Add ((int)GetModal ("M/106"));
      }
      catch (Exception) {
        log.DebugFormat ("GetActiveMCodes: " +
                         "GetModal M/106 failed");
      }

      if (this.CncKind.Equals ("15i")) {
        for (int i = 126; i <= 129; ++i) { // M126 to M129
          try {
            var m = (int)GetModal ("M/" + i.ToString ());
            if (0 != m) {
              result.Add (m);
            }
          }
          catch (Exception) {
            log.DebugFormat ("GetActiveMCodes: " +
                             "GetModal M/{0} failed on 15i", i);
          }
        }
      }
      else if (!MTKind.Contains ("W")) { // 16i/18i-W: 106 only, else add 125 and 126
        try {
          var m125 = (int)GetModal ("M/125");
          if (0 != m125) {
            result.Add (m125);
          }
        }
        catch (Exception) {
          log.DebugFormat ("GetActiveMCodes: " +
                           "GetModal M/125 failed");
        }
        try {
          var m126 = (int)GetModal ("M/126");
          if (0 != m126) {
            result.Add (m126);
          }
        }
        catch (Exception) {
          log.DebugFormat ("GetActiveMCodes: " +
                           "GetModal M/126 failed");
        }
      }

      // Filter
      if (!string.IsNullOrEmpty (param)) {
        if (param[0] == '-') { // exclusion
          var s = param.Substring (1);
          var l = s.Split (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .ToList<string> ();
          var filtered = result
            .Where (x => !l.Contains (x.ToString ()));
          result = filtered.ToList ();
        }
        else { // inclusion
          var s = param;
          var l = s.Split (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .ToList<string> ();
          var filtered = result
            .Where (x => l.Contains (x.ToString ()));
          result = filtered.ToList ();
        }
      }

      return result;
    }
  }
}
