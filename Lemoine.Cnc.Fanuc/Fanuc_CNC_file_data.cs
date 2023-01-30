// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Fanuc_cnc_file_data.
  /// </summary>
  public partial class Fanuc
  {
    #region Members
    short? m_macroType = null;
    #endregion Members

    #region Reading functions
    /// <summary>
    /// Get the value of a macro
    /// </summary>
    /// <param name="param">Macro number</param>
    /// <returns></returns>
    public double GetMacro (string param)
    {
      int macroNumber = int.Parse (param);

      if (false == IsConnectionValid ()) {
        log.Error ($"GetMacro: connection to the CNC failed for macroNumber {macroNumber}");
        throw new Exception ("No CNC connection");
      }

      // One variable to read
      IList<int> variables = new List<int> () { macroNumber };
      var result = GetCustomMacros (variables);
      return result[macroNumber];
    }

    /// <summary>
    /// Get the value of a macro and return an int
    /// </summary>
    /// <param name="param">Macro number</param>
    /// <returns></returns>
    public int GetMacroInt (string param)
    {
      return (int)GetMacro (param);
    }

    /// <summary>
    /// Get the value of a macro and return a long
    /// </summary>
    /// <param name="param">Macro number</param>
    /// <returns></returns>
    public long GetMacroLong (string param)
    {
      return (long)GetMacro (param);
    }

    /// <summary>
    /// Get a set of cnc variables.
    /// </summary>
    /// <param name="param">ListString (first character is the separator)</param>
    /// <returns></returns>
    public IDictionary<string, double> GetCncVariableSet (string param)
    {
      var macroVariableNumbers = Lemoine.Collections.EnumerableString.ParseListString (param)
        .Select (k => int.Parse (k))
        .Distinct ();
      return GetCustomMacros (macroVariableNumbers)
        .ToDictionary (keyValue => keyValue.Key.ToString (), keyValue => keyValue.Value);
    }

    /// <summary>
    /// Get variables from a range
    /// </summary>
    /// <param name="param">range defined with a dash separator. Example: 1-5</param>
    /// <returns></returns>
    public IDictionary<string, double> GetCncVariableRange (string param)
    {
      var split = param.Split ('-');
      if (2 != split.Length) {
        log.ErrorFormat ("GetCncVariableRange: invalid format for the parameter {0}, not {min}-{max}, for example '1-9999'", param);
        throw new ArgumentException ("GetCncVariableRange: invalid parameter format", "param");
      }

      int min = int.Parse (split[0]);
      int max = int.Parse (split[1]);

      if (max < min) {
        log.WarnFormat ("GetCncVariableRange: empty range {0}-{1}", min, max);
      }

      var macroVariableNumbers = Enumerable.Range (min, max - min + 1);
      return GetCustomMacros (macroVariableNumbers)
        .ToDictionary (keyValue => keyValue.Key.ToString (), keyValue => keyValue.Value);
    }

    /// <summary>
    /// Get a range of variables from X to Y included having a gap of Z between each variable,
    /// param being in the format X-Y-Z or X-Y (default gap=1)
    /// 
    /// For example: 1-9-2 will read 1, 3, 5, 7, 9
    /// </summary>
    /// <param name="param"></param>
    /// <returns>dictionary of doubles</returns>
    public IDictionary<int, double> GetVariables (string param)
    {
      var splitParam = param.Split ('-');

      if ((3 != splitParam.Length) && (2 != splitParam.Length)) {
        string txt = $"GetVariables: parameter {param} is not in the format X-Y or X-Y-Z";
        log.Error (txt);
        throw new ArgumentException (txt);
      }

      int valInf, valSup;
      try {
        valInf = int.Parse (splitParam[0]);
      }
      catch (Exception ex) {
        string txt = $"GetVariables: couldn't parse X as an int in parameter {param}";
        log.Error (txt, ex);
        throw new ArgumentException (txt);
      }
      try {
        valSup = int.Parse (splitParam[1]);
      }
      catch (Exception ex) {
        string txt = $"GetVariables: couldn't parse Y as an int in parameter {param}";
        log.Error (txt, ex);
        throw new ArgumentException (txt);
      }

      int gap = 1;
      if (3 == splitParam.Length) {
        try {
          gap = int.Parse (splitParam[2]);
        }
        catch (Exception ex) {
          string txt = $"GetVariables: couldn't parse Z as an int in parameter {param}";
          log.Error (txt, ex);
          throw new ArgumentException (txt);
        }
      }

      return GetVariables (valInf, valSup, gap);
    }

    /// <summary>
    /// Read the information on a parameter
    /// It is possible to extract a specific bit
    /// </summary>
    /// <param name="param">syntax is {Param number} or {Param number}|{bit}</param>
    /// <returns>return the value as a byte</returns>
    public short ReadParaInfo (String param)
    {
      // Check the param
      var splitParam = param.Split ('|');
      Int16 number;
      int bit = -1;
      try {
        if (splitParam.Length == 1) {
          number = Int16.Parse (splitParam[0]);
        }
        else if (splitParam.Length == 2) {
          number = Int16.Parse (splitParam[0]);
          bit = int.Parse (splitParam[1]);
        }
        else {
          throw new ArgumentException ("wrong syntax for the input parameter", "param");
        }
      }
      catch (Exception ex) {
        log.Error ($"ReadParaInfo: couldn't parse parameter: {param}", ex);
        throw;
      }

      // Get the param value
      var paraInfo = ReadParaInfo (number);

      // Possibly extract a bit
      if (bit >= 0) {
        return GetBit (paraInfo, bit) ? (short)1 : (short)0;
      }

      return paraInfo;
    }

    /// <summary>
    /// Read a param as a byte
    /// It is possible to extract a bit
    /// The axis number can be 0 if this information is not necessary
    /// 
    /// For the axis number, only 0 and 1 (or -1 in case there is no associated axis) is supported here
    /// </summary>
    /// <param name="param">syntax is {Param number} or {Param number}|{axis number} or {Param number}|{axis number}|{bit}</param>
    /// <returns>return the value as a byte</returns>
    public byte ReadParamAsByte (string param)
    {
      // Check the param
      var splitParam = param.Split ('|');
      Int16 number;
      Int16 axis = 0;
      int bit = -1;
      try {
        if (splitParam.Length == 1) {
          number = Int16.Parse (splitParam[0]);
        }
        else if (splitParam.Length == 2) {
          number = Int16.Parse (splitParam[0]);
          axis = Int16.Parse (splitParam[1]);
        }
        else if (splitParam.Length == 3) {
          number = Int16.Parse (splitParam[0]);
          axis = Int16.Parse (splitParam[1]);
          bit = int.Parse (splitParam[2]);
        }
        else {
          throw new ArgumentException ("wrong syntax for the input parameter", "param");
        }
      }
      catch (Exception ex) {
        log.Error ($"ReadParamAsByte: couldn't parse parameter: {param}", ex);
        throw;
      }

      // Get the param value
      byte paramValue = ReadParamAsByte (number, axis);

      // Possibly extract a bit
      if (bit >= 0 && bit < 8) {
        paramValue = GetBit (paramValue, bit) ? (byte)1 : (byte)0;
      }

      return paramValue;
    }

    /// <summary>
    /// Type of a parameter
    /// </summary>
    enum ParamType
    {
      /// <summary>
      /// bit type
      /// </summary>
      Bit = 0,
      /// <summary>
      /// byte type
      /// </summary>
      Byte = 1,
      /// <summary>
      /// word type
      /// </summary>
      Word = 2,
      /// <summary>
      /// 2-word type
      /// </summary>
      Word2 = 3,
      /// <summary>
      /// real type
      /// </summary>
      Real = 4,
    }

    ParamType GetParamType (short number)
    {
      var paraInfo = ReadParaInfo (number);
      if (CncKind.StartsWith ("15i")) {
        var bit9 = GetBit (paraInfo, 9);
        var bit10 = GetBit (paraInfo, 10);
        var bit11 = GetBit (paraInfo, 11);
        var a = 0;
        if (bit9) {
          a += 1;
        }
        if (bit10) {
          a += 2;
        }
        if (bit11) {
          a += 4;
        }
        switch (a) {
        case 0:
          return ParamType.Bit;
        case 1:
          return ParamType.Byte;
        case 2:
          return ParamType.Word;
        case 3:
          return ParamType.Word2;
        case 4:
          return ParamType.Real;
        }
      }
      if (CncKind.StartsWith ("30i") || CncKind.StartsWith ("31i") || CncKind.StartsWith ("PMi") || CncKind.StartsWith ("0i")) {
        if (GetBit (paraInfo, 12)) {
          return ParamType.Real;
        }
      }
      var bit0 = GetBit (paraInfo, 0);
      var bit1 = GetBit (paraInfo, 1);
      var n = 0;
      if (bit0) {
        n += 1;
      }
      if (bit1) {
        n += 2;
      }
      switch (n) {
      case 0:
        return ParamType.Bit;
      case 1:
        return ParamType.Byte;
      case 2:
        return ParamType.Word;
      case 3:
        return ParamType.Word2;
      default:
        throw new InvalidOperationException ();
      }
    }

    (Int16, Int16, Int16) GetParamNumberAxisItem (string param)
    {
      var splitParam = param.Split ('|');
      Int16 number;
      Int16 axis = 0;
      Int16 item = 0;
      try {
        if ((splitParam.Length < 1) || (3 < splitParam.Length)) {
          throw new ArgumentException ("wrong syntax for the input parameter", "param");
        }
        number = Int16.Parse (splitParam[0]);
        if (splitParam.Length >= 2) {
          axis = Int16.Parse (splitParam[1]);
        }
        if (splitParam.Length >= 3) {
          if (!Int16.TryParse (splitParam[2], out item)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetParamNumberAxisItem: try to consider {splitParam[2]} as an axis name");
            }
            try {
              item = GetAxisNumber (splitParam[2]);
            }
            catch (Exception ex) {
              log.Error ($"GetParamNumberAxisItem: {splitParam[2]} is not a valid axis name", ex);
              throw;
            }
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"GetParamNumberAxisItem: couldn't parse parameter: {param}", ex);
        throw;
      }

      return (number, axis, item);
    }

    /// <summary>
    /// Read a parameter
    /// 
    /// axis number is 0 (no axis), the number of axis, or -1 (all axis)
    /// </summary>
    /// <param name="param">syntax is {Param number} or {Param number}|{axis number} or {Param number}|{axis number}|{item}</param>
    /// <returns>return the value as a word</returns>
    public object ReadParam (string param)
    {
      var (number, axis, item) = GetParamNumberAxisItem (param);
      var paramType = GetParamType (number);
      switch (paramType) {
      case ParamType.Bit:
        return GetBit (ReadParamAsBytes (number, axis, item), 0);
      case ParamType.Byte:
        return ReadParamAsBytes (number, axis, item);
      case ParamType.Word:
        return ReadParamAsWords (number, axis, item);
      case ParamType.Word2:
        return ReadParamAs2Words (number, axis, item);
      case ParamType.Real:
        return ReadParamAsReals (number, axis, item);
      default:
        log.Error ($"ReadParam: not supported param type {paramType}");
        throw new InvalidOperationException ();
      }
    }

    /// <summary>
    /// Read a param as bytes
    /// 
    /// axis number is 0 (no axis), the number of axis, or -1 (all axis)
    /// </summary>
    /// <param name="param">syntax is {Param number} or {Param number}|{axis number} or {Param number}|{axis number}|{item}</param>
    /// <returns>return the value as a word</returns>
    public Int16 ReadParamAsBytes (string param)
    {
      var (number, axis, item) = GetParamNumberAxisItem (param);
      return ReadParamAsBytes (number, axis, item);
    }

    /// <summary>
    /// Read a param as a word
    /// 
    /// axis number is 0 (no axis), the number of axis, or -1 (all axis)
    /// </summary>
    /// <param name="param">syntax is {Param number} or {Param number}|{axis number} or {Param number}|{axis number}|{item}</param>
    /// <returns>return the value as a word</returns>
    public Int16 ReadParamAsWords (string param)
    {
      var (number, axis, item) = GetParamNumberAxisItem (param);
      return ReadParamAsWords (number, axis, item);
    }

    /// <summary>
    /// Read a param as a 2-word
    /// 
    /// axis number is 0 (no axis), the number of axis, or -1 (all axis)
    /// </summary>
    /// <param name="param">syntax is {Param number} or {Param number}|{axis number} or {Param number}|{axis number}|{item}</param>
    /// <returns>return the value as a 2-word</returns>
    public Int32 ReadParamAs2Words (string param)
    {
      var (number, axis, item) = GetParamNumberAxisItem (param);
      // Get the param value
      try {
        return ReadParamAs2Words (number, axis, item);
      }
      catch (Exception ex) {
        log.Error ($"ReadParamAs2Words: exception for number={number} axis={axis} item={item}", ex);
        throw;
      }
    }

    /// <summary>
    /// Read a param as real
    /// 
    /// axis number is 0 (no axis), the number of axis, or -1 (all axis)
    /// </summary>
    /// <param name="param">syntax is {Param number} or {Param number}|{axis number} or {Param number}|{axis number}|{item}</param>
    /// <returns>return the value as a real</returns>
    public double ReadParamAsReals (string param)
    {
      var (number, axis, item) = GetParamNumberAxisItem (param);
      // Get the param value
      try {
        return ReadParamAsReals (number, axis, item);
      }
      catch (Exception ex) {
        log.Error ($"ReadParamAsReals: exception for number={number} axis={axis} item={item}", ex);
        throw;
      }
    }
    #endregion Reading functions

    #region Writing functions
    /// <summary>
    /// Set a macro with a value
    /// Possible ranges are
    /// * [100 - 999] common variables
    /// * [1000 - 9999] system variables
    /// * [98000 - 98499] common variables
    /// </summary>
    /// <param name="macroNumber">Macro to update</param>
    /// <param name="macroValue">Value to specify</param>
    /// <returns>Error code</returns>
    public Int16 SetMacro (Int32 macroNumber, double macroValue)
    {
      CheckConnection ();

      Import.FwLib.EW result;
      if (macroNumber >= 100 && macroNumber <= 999 ||
        macroNumber >= 98000 && macroNumber <= 98499) {
        // Use wrmacror2
        result = SetMacroWithWrmacror2 ((UInt32)macroNumber, macroValue);
      }
      else if (macroNumber >= 1000 && macroNumber <= 9999) {
        // Use wrmacro
        result = SetMacroWithWrmacro ((Int16)macroNumber, macroValue);
      }
      else {
        throw new Exception ("Cannot write to macro " + macroNumber);
      }

      return (Int16)result;
    }
    #endregion Writing functions

    #region Private functions
    double ConvertMacroValue (int mcr, short dec)
    {
      if (0 == GetMacroType ()) { // decimal form floating-point type
        return mcr * Math.Pow (10.0, -dec);
      }
      else { // binary form floating-point type
        if (dec < -20 || dec > 20) { // Just raise a log in case the value does not look correct
          log.WarnFormat ("ConvertMacroValue: weird dec value {0}", dec);
        }
        return mcr * Math.Pow (2.0, -dec);
      }
    }

    short GetMacroType ()
    {
      if (m_macroType.HasValue) {
        return m_macroType.Value;
      }

      if (false == IsConnectionValid ()) {
        log.ErrorFormat ("GetMacroType: connection to the CNC failed");
        throw new Exception ("No CNC connection");
      }

      // Use cnc_getmactype
      short macroType;
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.getmactype (m_handle, out macroType);
      if (Import.FwLib.EW.OK != result) {
        log.Error ($"GetMacroType: getmactype failed with {result}");
        ManageError ("GetMacroType", result);
        throw new Exception ("getmactype failed");
      }
      m_macroType = macroType;
      return macroType;
    }

    IDictionary<int, double> GetCustomMacros (IEnumerable<int> macroVariableNumbers)
    {
      IDictionary<int, double> result = new Dictionary<int, double> ();

      if (!macroVariableNumbers.Any ()) {
        return result;
      }

      { // 1 to 999
        var macroNumbersTo999 = macroVariableNumbers.Where (k => k <= 999);
        if (macroNumbersTo999.Any ()) {
          var count = macroNumbersTo999.Count ();
          if (1 == count) {
            var macroNumber = macroNumbersTo999.First ();
            try {
              result[macroNumber] = GetCustomMacro1to9999 (macroNumber);
            }
            catch (Exception ex) {
              log.Error ($"GetCustomMacros: GetCustomMacro1to999 failed with macroNumber {macroNumber} (not available ?)", ex);
            }
          }
          else { // Use a range
            var min = macroNumbersTo999.Min ();
            var max = macroNumbersTo999.Max ();
            var number = max - min + 1;
            var result1to999 = GetCustomMacros1to999 (min, number);
            result = result1to999
              .Where (keyValue => macroVariableNumbers.Contains (keyValue.Key))
              .ToDictionary (keyValue => keyValue.Key, keyValue => keyValue.Value);
          }
        }
      }

      { // System variables 1000 to 9999
        // (or can it work up to 32767 which is the maximum value for a short type? to be tested
        // cnc_rdmacror2 or cnc_rdmacror3 may be required instead)
        var systemVariables = macroVariableNumbers.Where (k => (1000 <= k) && (k <= 9999));
        foreach (var systemVariable in systemVariables) {
          try {
            result[systemVariable] = GetCustomMacro1to9999 (systemVariable);
          }
          catch (Exception ex) {
            log.Error ($"GetCustomMacros: GetCustomMacro1to9999 failed with macroNumber {systemVariable} (not available ?)", ex);
          }
        }
      }

      { // System variables 10000 to 97999, using cnc_rdmacror2
        var systemVariables = macroVariableNumbers.Where (k => (10000 <= k) && (k <= 97999));
        foreach (var systemVariable in systemVariables) {
          try {
            result[systemVariable] = GetCustomMacro10000to97999 (systemVariable);
          }
          catch (Exception ex) {
            log.Error ($"GetCustomMacros: GetCustomMacro10000to97999 failed with macroNumber {systemVariable} (not available ?)", ex);
          }
        }
      }

      { // 98000 to 98499 => use rdmacror3
        // TODO: get in one step different variables
        var macroNumbersFrom98000 = macroVariableNumbers.Where (k => (98000 <= k) && (k <= 98499));
        foreach (var macroNumber in macroNumbersFrom98000) {
          try {
            result[macroNumber] = GetCustomMacro98000to98499 (macroNumber);
          }
          catch (Exception ex) {
            log.Error ($"GetCustomMacros: GetCustomMacro98000to98499 failed with macroNumber {macroNumber} (not available ?)", ex);
          }
        }
      }

      return result;
    }

    /// <summary>
    /// Note: according to the cnc_rdmacro documentation,
    /// cnc_rdmacro may read the following custom macros:
    /// <item>Local variable (#1,..,#33)</item>
    /// <item>Common variable (#100,..,#999)</item>
    /// <item>System variable (#1000,..,#9999)</item>
    /// 
    /// Because cnc_rdmacro takes in parameter a short number, cnc_rdmacro
    /// can't work with a number greater than 32767 for sure
    /// 
    /// For system variables with number &gt; 9999 cnc_rdmacror2 or cncrdmacror3 should be used instead
    /// </summary>
    /// <param name="macroVariableNumber"></param>
    /// <returns></returns>
    double GetCustomMacro1to9999 (int macroVariableNumber)
    {
      Debug.Assert (1 <= macroVariableNumber);
      Debug.Assert (macroVariableNumber <= 9999);

      short shortMacroNumber = (short)macroVariableNumber;
      Import.FwLib.ODBM macro;
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdmacro (m_handle, shortMacroNumber,
                                                               (short)Marshal.SizeOf (typeof (Import.FwLib.ODBM)),
                                                               out macro);
      if (Import.FwLib.EW.OK != result) {
        log.ErrorFormat ("GetCustomMacro1to9999: rdmacro failed with {0} for macro {1}", result, macroVariableNumber);
        ManageError ("GetCustomMacro1to9999", result);
        throw new Exception ("rdmacro failed");
      }
      double macroValue = ConvertMacroValue (macro.mcr_val, macro.dec_val);
      log.DebugFormat ("GetCustomMacro1to9999: got macro #{0} = {1}", macroVariableNumber, macroValue);
      return macroValue;
    }

    /// <summary>
    /// Note: according to the cnc_rdmacro documentation,
    /// cnc_rdmacro may read the following custom macros:
    /// <item>Local variable (#1,..,#33)</item>
    /// <item>Common variable (#100,..,#999)</item>
    /// <item>System variable (#1000,..,~)</item>
    /// FWLIBAPI short WINAPI cnc_rdmacror2(unsigned short FlibHndl, unsigned long s_no, unsigned long *num, double *data);
    /// </summary>
    /// <param name="macroVariableNumber"></param>
    /// <returns></returns>
    double GetCustomMacro10000to97999 (int macroVariableNumber)
    {
      int num = 1;
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdmacror2 (m_handle, macroVariableNumber, ref num, out Import.FwLib.DOUBLE_ARRAY data);
      if (Import.FwLib.EW.OK != result) {
        log.ErrorFormat ("GetCustomMacro10000to97999: rdmacror2 failed with {0} for macro {1}", result, macroVariableNumber);
        ManageError ("GetCustomMacro10000to97999", result);
        throw new Exception ("rdmacror2 failed");
      }
      if (num != 1) {
        log.ErrorFormat ("GetCustomMacro10000to97999: couldn't read macro {0}, number of data is {1}", macroVariableNumber, num);
        throw new Exception ("rdmacror2 failed");
      }

      double macroValue = data.data[0];
      log.DebugFormat ("GetCustomMacro10000to97999: got macro #{0} = {1}", macroVariableNumber, macroValue);
      return macroValue;
    }

    IDictionary<int, double> GetCustomMacros1to999 (int lower, int number)
    {
      Debug.Assert (1 <= lower);
      Debug.Assert (lower <= 999);

      IDictionary<int, double> result = new Dictionary<int, double> ();
      GetCustomMacros1to999 (lower, number, ref result);
      return result;
    }

    void GetCustomMacros1to999 (int lower, int number, ref IDictionary<int, double> r)
    {
      if (number < 1) { // Empty range: end of recursion
        return;
      }

      const int maxNumberOfValues = 40;
      if (number > maxNumberOfValues) {
        GetCustomMacros1to999 (lower, maxNumberOfValues, ref r);
        GetCustomMacros1to999 (lower + maxNumberOfValues, number - maxNumberOfValues, ref r);
        return;
      }
      else { // number <= maxNumberOfValues
        try {
          GetCustomMacros1to999LimitedNumberOfValues (lower, number, ref r);
        }
        catch (Exception ex) { // Try with individual requests instead
          log.ErrorFormat ("GetCustomMacros1to999: GetCustomMacros1to999NoRecursion lower={0} number={1} failed => try individual requests, {2}",
            lower, number, ex);
          for (int i = 0; i < number; ++i) {
            int macroNumber = lower + i;
            try {
              r[macroNumber] = GetCustomMacro1to9999 (macroNumber);
            }
            catch (Exception ex1) {
              log.ErrorFormat ("GetCustomMacros1to999: GetCustomMacro1to999 failed with macroNumber {0} (not available ?) => skip it, {1}",
                macroNumber, ex1);
            }
          }
        }
      }
    }

    void GetCustomMacros1to999LimitedNumberOfValues (int lower, int number, ref IDictionary<int, double> r)
    {
      const int maxNumberOfValues = 40;
      Debug.Assert (number <= maxNumberOfValues);

      int upper = lower + number - 1;

      Debug.Assert (1 <= lower);
      Debug.Assert (1 <= upper);
      Debug.Assert (lower <= 999);
      Debug.Assert (upper <= 999);

      var macror = new Lemoine.Cnc.Import.FwLib.IODBMR ();
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdmacror (m_handle, (short)lower, (short)upper,
                                                               (short)(8 + 8 * number),
                                                               out macror);
      if (Import.FwLib.EW.OK != result) {
        log.ErrorFormat ("GetCustomMacros1to999LimitedNumberOfValues: rdmacror failed with {0} for macro {1} to {2}", result, lower, upper);
        ManageError ("GetCustomMacros1to999LimitedNumberOfValues", result);
        throw new Exception ("rdmacror failed");
      }
      if (macror.datano_s != (short)lower) {
        log.FatalFormat ("GetCustomMacros1to999LimitedNumberOfValues: invalid datano_s={0} VS start={1}",
          macror.datano_s, lower);
        Debug.Assert (false);
        throw new Exception ("GetCustomMacros1to999LimitedNumberOfValues: invalid datano_s");
      }
      if (macror.datano_e != (short)upper) {
        log.WarnFormat ("GetCustomMacros1to999LimitedNumberOfValues: datano_e={0} VS end={1}",
          macror.datano_e, upper);
        if ((short)upper < macror.datano_e) {
          log.FatalFormat ("GetCustomMacros1to999LimitedNumberOfValues: returned last number {0} > {1}",
            macror.datano_e, upper);
          Debug.Assert (false);
          throw new Exception ("GetCustomMacros1to999LimitedNumberOfValues: invalid datano_e");
        }
      }
      for (int i = 0; i < number; ++i) {
        var macro = lower + i;
        var mcr = macror.iodbmr[i].mcr_val;
        var dec = macror.iodbmr[i].dec_val;
        r[macro] = ConvertMacroValue (mcr, dec);
      }
    }

    double GetCustomMacro98000to98499 (int macroVariableNumber)
    {
      Debug.Assert (98000 <= macroVariableNumber);
      Debug.Assert (macroVariableNumber <= 98499);

      var mcr = new Import.FwLib.IODBMRN3 ();
      int num = 1;
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdmacror3 (m_handle, macroVariableNumber, ref num, out mcr);
      if (Import.FwLib.EW.OK != result) {
        log.ErrorFormat ("GetMacro: " +
                         "rdmacror3 failed with {0} " +
                         "for macro {1}",
                         result, macroVariableNumber);
        ManageError ("GetCustomMacro98000to98499", result);
        throw new Exception ("rdmacror3 failed");
      }
      if (1 != num) {
        log.ErrorFormat ("GetMacro: " +
                         "rdmacror3 returned an unexpected num {0}",
                         num);
        throw new Exception ("rdmacror3 returned an unexpected num");
      }
      log.DebugFormat ("GetMacro: " +
                       "got macro # {0} = {1} with rdmacror3",
                       macroVariableNumber, mcr.mcr_val);
      return mcr.mcr_val;
    }

    IDictionary<int, double> GetVariables (int valInf, int valSup, int gap)
    {
      var number = valSup - valInf + 1;
      IEnumerable<int> macroVariableNumbers = Enumerable.Range (valInf, number)
        .Where (i => 0 == (i - valInf) % gap);
      return GetCustomMacros (macroVariableNumbers);
    }

    Import.FwLib.EW SetMacroWithWrmacro (Int16 macroNumber, double macroValue)
    {
      // Convert a double number into a form M*10^(-E) or M*2^(-E)
      Int32 m = 0;
      Int16 e = 0;
      Int32 power = (GetMacroType () == 0) ? 10 : 2; // decimal or binary form floating-point type

      // If macroValue too big, use the exponant to keep it in the possible range
      while ((macroValue > 999999999 || macroValue < -999999999) && e >= -21) {
        macroValue /= power;
        e--;
      }
      if (macroValue > 999999999 || macroValue < -999999999) {
        // Weird
        throw new Exception ("Number " + macroValue + " is too big");
      }

      // Increase the exponant if decimals are found
      while (macroValue % 1 != 0 && macroValue * power < 999999999 && macroValue * power > -999999999 && e < 20) {
        macroValue *= power;
        e++;
      }

      // Modified value of macroValue is now in "m"
      m = (Int32)Math.Round (macroValue);

      // Store the value
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.wrmacro (m_handle, macroNumber, 10, m, e);
      ManageErrorWithException ("wrmacro", result);

      return result;
    }

    Import.FwLib.EW SetMacroWithWrmacror2 (UInt32 macroNumber, double macroValue)
    {
      // Write in a macro
      UInt32 valuesToWrite = 1;
      var values = new Import.FwLib.DOUBLE_ARRAY_W ();
      values.data[0] = macroValue;
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.wrmacror2 (m_handle, macroNumber, ref valuesToWrite, values);
      ManageErrorWithException ("wrmacror2", result);
      if (1 != valuesToWrite) {
        log.Error ($"SetMacroWithWrmacror2: valuesToWrite={valuesToWrite}, expected was 1");
        throw new Exception ("SetMacroWithMwmacror2: write error");
      }

      return result;
    }

    byte ReadParamAsByte (Int16 number, Int16 axis)
    {
      // Read a parameter
      CheckConnection ();
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdparam (m_handle, number, axis, 4 + 1, out Import.FwLib.IODBPSD_C param);
      ManageErrorWithException ("rdparam", result);
      return param.cdata;
    }

    byte ReadParamAsBytes (Int16 number, Int16 axis, int item = 0)
    {
      // Read a parameter
      CheckConnection ();
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdparam_byte (m_handle, number, axis, 4 + 1 * 32, out Import.FwLib.IODBPSD_CS param);
      ManageErrorWithException ("rdparam", result);
      return param.cdatas[item];
    }

    Int16 ReadParamAsWords (Int16 number, Int16 axis, int item = 0)
    {
      // Read a parameter
      CheckConnection ();
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdparam_word (m_handle, number, axis, 4 + 2 * 32, out Import.FwLib.IODBPSD_IS param);
      ManageErrorWithException ("rdparam", result);
      return param.idatas[item];
    }

    Int32 ReadParamAs2Words (Int16 number, Int16 axis, int item = 0)
    {
      // Read a parameter
      CheckConnection ();
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdparam_2word (m_handle, number, axis, 4 + 4 * 32, out Import.FwLib.IODBPSD_LS param);
      ManageErrorWithException ("rdparam", result);
      return param.ldatas[item];
    }

    double ReadParamAsReals (Int16 number, Int16 axis, int item = 0)
    {
      // Read a parameter
      CheckConnection ();
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdparam_real (m_handle, number, axis, 4 + 8 * 32, out Import.FwLib.IODBPSD_RS param);
      ManageErrorWithException ("rdparam", result);
      var r = param.rdatas[item];
      if (log.IsDebugEnabled) {
        log.Debug ($"ReadParamAsReal: number={number}, axis={axis}, item={item} => v={r.prm_val} dec={r.dec_val}");
      }
      return r.prm_val * Math.Pow (10.0, -r.dec_val);
    }

    short ReadParaInfo (Int16 number)
    {
      // Read a parameter
      CheckConnection ();
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdparainfo (m_handle, number, 1, out Import.FwLib.ODBPARAIF paraif);
      ManageErrorWithException ("rdparainfo", result);
      if (log.IsDebugEnabled) {
        log.Debug ($"ReadParaInfo: parameter number={paraif.prev_no}, type={paraif.prm_type}");
      }
      return paraif.prm_type;
    }

    #endregion Private functions
  }
}
