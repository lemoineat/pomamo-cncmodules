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
    short? m_pMacroType = null;
    #endregion Members

    #region Reading functions
    /// <summary>
    /// Get PMacro from a range (fast but if we are out of range, nothing will be returned)
    /// </summary>
    /// <param name="param">range defined with a dash separator. Example: 1-5</param>
    /// <returns></returns>
    public IDictionary<string, double> GetPMacroRange (string param)
    {
      var split = param.Split ('-');
      if (2 != split.Length) {
        log.ErrorFormat ("GetPMacroRange: invalid format for the parameter {0}, not {min}-{max}, for example '1-9999'", param);
        throw new ArgumentException ("GetPMacroRange: invalid parameter format", "param");
      }

      int min = int.Parse (split[0]);
      int max = int.Parse (split[1]);

      if (max < min) {
        log.WarnFormat ("GetPMacroRange: empty range {0}-{1}", min, max);
      }

      var macroVariableNumbers = Enumerable.Range (min, max - min + 1);
      return GetPMacros (macroVariableNumbers)
        .ToDictionary (keyValue => keyValue.Key.ToString (), keyValue => keyValue.Value);
    }

    /// <summary>
    /// Get PMacro from a range, reading them one by one
    /// This is slow but the function returns everything that is possible
    /// </summary>
    /// <param name="param">range defined with a dash separator. Example: 1-5</param>
    /// <returns></returns>
    public IDictionary<string, double> GetPMacroRangeOneByOne (string param)
    {
      var split = param.Split ('-');
      if (2 != split.Length) {
        log.ErrorFormat ("GetPMacroRangeOneByOne: invalid format for the parameter {0}, not {min}-{max}, for example '1-9999'", param);
        throw new ArgumentException ("GetPMacroRangeOneByOne: invalid parameter format", "param");
      }

      int min = int.Parse (split[0]);
      int max = int.Parse (split[1]);

      if (max < min) {
        log.WarnFormat ("GetCncVariableRange: empty range {0}-{1}", min, max);
      }

      IDictionary<string, double> result = new Dictionary<string, double> ();
      for (int i = min; i <= max; i++) {
        try {
          var tmp = GetPMacros (new List<int> () { i });
          if (tmp.ContainsKey ((uint)i)) {
            result[i.ToString ()] = tmp[(uint)i];
          }
        }
        catch (Exception e) {
          log.ErrorFormat ("GetPMacroRangeOneByOne: couldn't read PMacro {0}: {1}", i, e.Message);
        }
      }
      return result;
    }
    #endregion Reading functions

    #region Writing functions
    /// <summary>
    /// Writes the P code macro variable (variable for the macro-executor)
    /// </summary>
    /// <param name="number">Specify the P code macro variable number</param>
    /// <param name="type">Type of macro variables to be written: 0 is conversation, 1 is auxiliary, 2 is execution</param>
    /// <param name="value">Specify the value to be set in the p macro</param>
    /// <returns>Error code</returns>
    public Int16 SetPMacro (Int32 number, UInt16 type, double value)
    {
      // Write a PMacro
      int valuesToWrite = 1;
      var values = new Import.FwLib.DOUBLE_ARRAY ();
      values.data[0] = value;
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.wrpmacror2 (m_handle, number, ref valuesToWrite, type, values);
      ManageErrorWithException ("wrpmacror2", result);

      return (Int16)result;
    }
    #endregion Writing functions

    #region Private functions
    Int16 GetPMacroType ()
    {
      if (m_pMacroType.HasValue) {
        return m_pMacroType.Value;
      }

      // Use cnc_getmactype
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.getpmactype (m_handle, out Int16 pMacroType);
      if (result != Import.FwLib.EW.OK) {
        log.ErrorFormat ("GetPMacroType: getpmactype failed with {0}", result);
        ManageError ("GetPMacroType", result);
        throw new Exception ("getpmactype failed");
      }
      m_pMacroType = pMacroType;
      return pMacroType;
    }

    IDictionary<uint, double> GetPMacros (IEnumerable<int> numbers)
    {
      var dic = new Dictionary<uint, double> ();
      if ((numbers == null) || !numbers.Any ()) {
        return dic;
      }

      // Min, max
      uint min = 0;
      uint max = 0;
      bool start = true;
      foreach (int number in numbers) {
        if (start) {
          min = max = (uint)number;
          start = false;
        }
        else {
          if (number < min) {
            min = (uint)number;
          }

          if (number > max) {
            max = (uint)number;
          }
        }
      }
      if (start) {
        return dic; // No data
      }

      // Read variables
      uint offset = 0;
      uint maxToRead = 1024; // Linked to the structure DOUBLE_ARRAY: cannot read more
      do {
        uint startIndex = min + offset;
        uint nbToRead = Math.Min (maxToRead, max - startIndex + 1);
        try {
          var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdpmacror2 (m_handle, startIndex, ref nbToRead, 0, out var mcval);

          if (Import.FwLib.EW.OK != result) {
            log.ErrorFormat ("GetPMacros: rdpmacror2 failed with {0} for P macros {1}-{2}", result, startIndex, startIndex + nbToRead - 1);
            ManageError ("GetPMacros", result);
            throw new Exception ("rdpmacror2 failed");
          }
          if (nbToRead != Math.Min (maxToRead, max - startIndex + 1)) {
            log.ErrorFormat ("GetPMacros: rdpmacror2 returned an unexpected num {0} instead of {1}", nbToRead, Math.Min (maxToRead, max - startIndex + 1));
            throw new Exception ("rdpmacror2 returned an unexpected num");
          }

          // Store the result
          foreach (int number in numbers) {
            if (number >= startIndex && number < startIndex + nbToRead) {
              dic.Add ((uint)number, mcval.data[number - startIndex]);
            }
          }

          offset += maxToRead;
        }
        catch (Exception ex) {
          log.Error ($"GetPMacros: rdpmacror2 failed for start={startIndex} and nb={nbToRead}", ex);
        }
      }
      while (offset < max - min + 1);

      return dic;
    }
    #endregion Private functions
  }
}
