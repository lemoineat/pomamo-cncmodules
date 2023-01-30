// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Lemoine.Cnc.Import.FwLib;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Fanuc_tool_management_data.
  /// </summary>
  public partial class Fanuc
  {
    #region Members
    bool m_toolOffsetInformationInitialized = false;
    Int16 m_toolOffsetMemoryType = 0; // 0 = A, 1 = B, 2 = C
    Int16 m_toolOffsetNumber = 0;
    bool? m_toolOffsetIsInch = null;
    double? m_toolOffsetMultiplier = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// True if the system is configured to use inches
    /// This value is initialized based on the active G20 or G21, or is null
    /// It can be forced to use the other unit (in that case the multiplier might be adapted)
    /// </summary>
    public bool? ToolOffsetIsInch
    {
      get {
        ReadToolOffsetInformation ();
        return m_toolOffsetIsInch;
      }
      set {
        ReadToolOffsetInformation ();
        m_toolOffsetIsInch = value;
        ComputeToolOffsetMultiplier ();
      }
    }

    /// <summary>
    /// Multiplier used after reading a tool offset value or before writing it
    /// This value is initialized based on the configuration or remains null if it's not possible
    /// A null value means no multiplier
    /// </summary>
    public double? ToolOffsetMultiplier
    {
      get {
        ReadToolOffsetInformation ();
        return m_toolOffsetMultiplier;
      }
      set {
        ReadToolOffsetInformation ();
        m_toolOffsetMultiplier = value;
      }
    }

    /// <summary>
    /// Get the effective tool offset multiplier
    /// </summary>
    /// <returns></returns>
    double GetToolOffsetMultiplier ()
    {
      var toolOffsetMultiplier = this.ToolOffsetMultiplier;
      return toolOffsetMultiplier ?? 1.0;
    }
    #endregion Getters / Setters

    #region Reading functions
    /// <summary>
    /// Type of memory to read for the tool offset
    /// 0 is "A"
    /// 1 is "B"
    /// 2 is "C"
    /// </summary>
    public int ToolOffsetMemoryType
    {
      get {
        ReadToolOffsetInformation ();
        return (int)m_toolOffsetMemoryType;
      }
    }

    /// <summary>
    /// Number of tool offsets that are possible to read
    /// </summary>
    public int ToolOffsetNumber
    {
      get {
        ReadToolOffsetInformation ();
        return (int)m_toolOffsetNumber;
      }
    }

    /// <summary>
    /// Get a set of tool offsets
    /// </summary>
    /// <param name="param">ListString (first character is the separator)</param>
    /// <returns></returns>
    public IDictionary<string, double> ReadToolOffsetSet (string param)
    {
      var result = new Dictionary<string, double> ();

      var toolParameters = Lemoine.Collections.EnumerableString.ParseListString (param);
      foreach (var toolParameter in toolParameters) {
        result[toolParameter] = ReadToolOffsetValue (toolParameter);
      }
      return result;
    }

    /// <summary>
    /// Reads the tool offset value specified by a number and a type.
    /// </summary>
    /// <param name="param">
    ///   2 or 3 parameters separated by ',', ';' or '|'.
    ///   1st parameter: tool offset number.
    ///   2nd parameter: tool offset type (number, depends of the machine).
    ///   3rd parameter: multiplier (default is the value in ToolOffsetMultiplier).
    /// </param>
    /// <returns></returns>
    public double ReadToolOffsetValue (string param)
    {
      string[] parameters = param.Split (new char[] { ',', ';', '|' }, 3);
      if (parameters.Length < 2) {
        log.ErrorFormat ("ReadToolOffsetValue: not enough parameters in {0}", param);
        throw new ArgumentException ("ReadToolOffsetValue: number of parameters");
      }

      if (!short.TryParse (parameters[0], out short toolOffsetNumber)) {
        log.ErrorFormat ("ReadToolOffsetValue: 1st parameter {0} is not a short", parameters[0]);
        throw new ArgumentException ("ReadToolOffsetValue: 1st parameter not a short");
      }

      if (!short.TryParse (parameters[1], out short toolOffsetType)) {
        toolOffsetType = GetOffsetType (parameters[1]);
      }

      double multiplier = ToolOffsetMultiplier ?? 1.0;
      if (parameters.Length >= 3) {
        if (!double.TryParse (parameters[2], out multiplier)) {
          log.ErrorFormat ("ReadToolOffsetValue: 3rd parameter {0} is not a double", parameters[2]);
          throw new ArgumentException ("ReadToolOffsetValue: 3rd parameter not a double");
        }
      }

      log.DebugFormat ("ReadToolOffsetValue: number={0} type={1} multiplicator={2}",
                       toolOffsetNumber, toolOffsetType, multiplier);
      return ReadToolOffsetValue (toolOffsetNumber, toolOffsetType, multiplier);
    }

    /// <summary>
    /// Reads tool offset values specified by a range and a type.
    /// </summary>
    /// <param name="param">
    ///   3 or 4 parameters separated by ',', ';' or '|'.
    ///   1st parameter: start tool offset number.
    ///   2nd parameter: end tool offset number.
    ///   3rd parameter: tool offset type (number, depends of the machine).
    ///   4th parameter: multiplier (default is the value in ToolOffsetMultiplier).
    /// </param>
    /// <returns>an array of values whose length is (end - start + 1)</returns>
    public double[] ReadToolOffsetValues (string param)
    {
      string[] parameters = param.Split (new char[] { ',', ';', '|' }, 4);
      if (parameters.Length < 3) {
        log.ErrorFormat ("ReadToolOffsetValues: not enough parameters in {0}", param);
        throw new ArgumentException ("ReadToolOffsetValues: number of parameters");
      }

      if (!short.TryParse (parameters[0], out short toolOffsetStart)) {
        log.ErrorFormat ("ReadToolOffsetValues: 1st parameter {0} is not a short", parameters[0]);
        throw new ArgumentException ("ReadToolOffsetValues: 1st parameter not a short");
      }

      if (!short.TryParse (parameters[1], out short toolOffsetEnd)) {
        log.ErrorFormat ("ReadToolOffsetValues: 2nd parameter {0} is not a short", parameters[1]);
        throw new ArgumentException ("ReadToolOffsetValues: 2nd parameter not a short");
      }

      if (!short.TryParse (parameters[2], out short toolOffsetType)) {
        toolOffsetType = GetOffsetType (parameters[2]);
      }

      double multiplier = ToolOffsetMultiplier ?? 1.0;
      if (parameters.Length >= 4) {
        if (!double.TryParse (parameters[3], out multiplier)) {
          log.ErrorFormat ("ReadToolOffsetValues: 4th parameter {0} is not a double", parameters[3]);
          throw new ArgumentException ("ReadToolOffsetValues: 4th parameter not a double");
        }
      }

      log.DebugFormat ("ReadToolOffsetValues: start={0} end={1} type={2} multiplicator={3}",
                        toolOffsetStart, toolOffsetEnd, toolOffsetType, multiplier);
      return ReadToolOffsetValues (toolOffsetStart, toolOffsetEnd, toolOffsetType, multiplier);
    }

    /// <summary>
    /// Reads the work zero offset value specified by a number and a type.
    /// </summary>
    /// <param name="param">
    ///   2 or 3 parameters separated by ',', ';' or '|'.
    ///   1st parameter: work zero offset number.
    ///   2nd parameter: work zero offset axis.
    ///   3rd parameter: multiplicator (default: 1.0).
    /// </param>
    /// <returns></returns>
    public double ReadWorkZeroOffsetValue (string param)
    {
      string[] parameters = param.Split (new char[] { ',', ';', '|' }, 3);
      if (parameters.Length < 2) {
        log.ErrorFormat ("ReadWorkZeroOffsetValue: " +
                         "not enough parameters in {0}",
                         param);
        throw new ArgumentException ("ReadWorkZeroOffsetValue: number of parameters");
      }
      Debug.Assert (2 <= parameters.Length);

      if (!short.TryParse (parameters[0], out short workZeroOffsetNumber)) {
        log.ErrorFormat ("ReadWorkZeroOffsetValue: " +
                         "1st parameter {0} is not a short",
                         parameters[0]);
        throw new ArgumentException ("ReadWorkZeroOffsetValue: 1st parameter not a short");
      }

      if (!short.TryParse (parameters[1], out short axisNumber)) {
        log.ErrorFormat ("ReadWorkZeroOffsetValue: " +
                         "2nd parameter {0} is not valid",
                         parameters[2]);
        throw new ArgumentException ("ReadWorkZeroOffsetValue: 2nd parameter not valid");
      }

      double multiplier = ToolOffsetMultiplier ?? 1.0;
      if (3 <= parameters.Length) {
        if (!double.TryParse (parameters[2], out multiplier)) {
          log.ErrorFormat ("ReadWorkZeroOffsetValue: " +
                           "3rd parameter {0} is not a double",
                           parameters[3]);
          throw new ArgumentException ("ReadWorkZeroOffsetValue: 3rd parameter not a double");
        }
      }

      log.DebugFormat ("ReadWorkZeroOffsetValue: " +
                       "number={0} axis={1} multiplicator={2}",
                       workZeroOffsetNumber, axisNumber, multiplier);
      return ReadWorkZeroOffsetValue (workZeroOffsetNumber, axisNumber, multiplier);
    }
    #endregion Reading functions

    #region Writing functions
    /// <summary>
    /// Write a tool offset value at a specified index
    /// </summary>
    /// <param name="type">Kind of property to write (0, 1, 2, 3 or GC, WC, ...)</param>
    /// <param name="index"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public int WriteToolOffsetValue (string type, short index, double value)
    {
      // Find the corresponding offset type number
      if (!short.TryParse (type, out short toolOffsetType)) {
        toolOffsetType = GetOffsetType (type);
      }

      // Apply a multiplier, if specified
      if (ToolOffsetMultiplier.HasValue) {
        value /= ToolOffsetMultiplier.Value;
      }

      // Write a tool offset data
      CheckConnection ();
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.wrtofs (m_handle, index, toolOffsetType, 8, (int)(value + 0.5));
      ManageErrorWithException ("wrtofs", result);

      return (int)result;
    }

    /// <summary>
    /// Write tool offset values starting at a specified index
    /// </summary>
    /// <param name="type">Kind of property to write (0, 1, 2, 3)</param>
    /// <param name="startIndex"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public void WriteToolOffsetValues (string type, short startIndex, double[] values)
    {
      // Find the corresponding offset type number
      if (!short.TryParse (type, out short toolOffsetType)) {
        log.Fatal ($"WriteToolOffsetValues: type={type} is not supported (not implemented)"); // GetOffsetType only works for a single tool
        throw new ArgumentException ("Type is not supported (not implemented)", type);
      }

      WriteToolOffsetValues (toolOffsetType, startIndex, values);
    }

    /// <summary>
    /// Write tool offset values starting at a specified index
    /// </summary>
    /// <param name="type">Property to write</param>
    /// <param name="startIndex"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public void WriteToolOffsetValues (short type, short startIndex, double[] values)
    {
      var blockMax = values.Length / 128;
      for (int block = 0; block <= blockMax; ++block) {
        var readElements = block * 128;
        var blockStart = (short)(startIndex + readElements);
        var subValues = values.Skip (readElements).Take (128).ToArray ();
        if (log.IsDebugEnabled) {
          log.Debug ($"WriteToolOffsetValues: length={values.Length}, process block {block}, start={blockStart}");
        }
        WriteToolOffsetValuesMax128OfsOnly (type, blockStart, subValues);
      }
    }

    /// <summary>
    /// Write tool offset values starting at a specified index
    /// 
    /// This method supports a maximum of 128 elements and only m_ofs or t_ofs inputs
    /// </summary>
    /// <param name="type">Type. &gt;= 0 and not 9</param>
    /// <param name="startIndex"></param>
    /// <param name="values">maximum 128 values (values.Length &lt;= 128)</param>
    /// <returns></returns>
    public void WriteToolOffsetValuesMax128OfsOnly (short type, short startIndex, double[] values)
    {
      Debug.Assert (values.Length <= 128);
      if (128 < values.Length) {
        log.Error ($"WriteToolOffsetValues: more than 128 values, length={values.Length}");
        throw new ArgumentException ("Array has more than 128 elements", "values");
      }

      if (type < 0) {
        log.Error ($"WriteToolOffsetValues: type {type} is not supported here since it must not used Ofs but another value");
        throw new ArgumentException ("A negative type is not supported here since ofs must not be used", "type");
      }
      if (type == 9) {
        log.Error ($"WriteToolOffsetValues: type {type} is not supported here since t_tip must be used instead");
        throw new ArgumentException ("Type 9 is not valid here", "type");
      }

      var toolOffsetMultiplier = this.GetToolOffsetMultiplier ();
      if (log.IsDebugEnabled) {
        log.Debug ($"WriteToolOffsetValues: tool offset multiplier is {toolOffsetMultiplier}");
      }
      var nbElements = values.Length;
      CheckConnection ();
      IODBTO_M_T_OFS_W data = new IODBTO_M_T_OFS_W {
        datano_s = (short)startIndex,
        datano_e = (short)(startIndex + nbElements - 1),
        type = type,
        m_t_ofs = new int[128]
      };
      Debug.Assert (nbElements <= 128);
      for (int i = 0; i < nbElements; ++i) {
        data.m_t_ofs[i] = (int)(values[i] / toolOffsetMultiplier + 0.5);
      }
      var ofsLength = (short)(8 + 4 * nbElements);
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.wrtofsr (m_handle, ofsLength, data);
      ManageErrorWithException ("wrtofsr", result);
    }
    #endregion Writing functions

    #region Private functions
    void ReadToolOffsetInformation ()
    {
      if (m_toolOffsetInformationInitialized) {
        return;
      }

      // Read the information about tool offset
      CheckConnection ();
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdtofsinfo (m_handle, out Import.FwLib.ODBTLINF info);
      ManageErrorWithException ("rdtofsinfo", result);
      m_toolOffsetMemoryType = info.ofs_type;
      m_toolOffsetNumber = info.use_no;

      // Inch or mm?
      try {
        double group5value = GetModal ('G', 5);
        if (group5value == 20) {
          m_toolOffsetIsInch = true;
        }
        else if (group5value == 21) {
          m_toolOffsetIsInch = false;
        }

        log.InfoFormat ("ReadToolOffsetInformation - group 5 is {0} => ToolOffsetIsInch is {1}", group5value, m_toolOffsetIsInch);
      }
      catch (Exception e) {
        log.ErrorFormat ("ReadToolOffsetInformation - couldn't determine ToolOffsetIsInch: {0}", e);
        m_toolOffsetIsInch = null;
      }

      // Compute the multiplier
      ComputeToolOffsetMultiplier ();

      m_toolOffsetInformationInitialized = true;
    }

    void ComputeToolOffsetMultiplier ()
    {
      // First method: using cnc_getfigure
      short validFig = 0;
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.getfigure (m_handle, 1, ref validFig,
        out Import.FwLib.SHORT_ARRAY decFigIn, out Import.FwLib.SHORT_ARRAY decFigOut);
      ManageErrorWithException ("getfigure", result);
      if (validFig > 0) {
        log.InfoFormat ("ComputeToolOffsetMultiplier - the decimal place is {0} (in) or {1} (out)", decFigIn.data[0], decFigOut.data[0]);
        m_toolOffsetMultiplier = 1;
        for (int i = 0; i < decFigIn.data[0]; i++) {
          m_toolOffsetMultiplier = m_toolOffsetMultiplier.Value * 0.1;
        }
      }
      else if (!m_toolOffsetIsInch.HasValue) {
        // Second method: directly reading parameters
        // We must know first if we are using inches or mm
        m_toolOffsetMultiplier = 0.001;
        if (CncKind.Contains ("15")) {
          if (ReadParamAsByte ("6002|-1|0") == 1) {
            m_toolOffsetMultiplier = 0.01;
          }
          else if (ReadParamAsByte ("6002|-1|1") == 1) {
            m_toolOffsetMultiplier = 0.0001;
          }
          else if (ReadParamAsByte ("6004|-1|0") == 1) {
            m_toolOffsetMultiplier = 0.00001;
          }
          else if (ReadParamAsByte ("6007|-1|0") == 1) {
            m_toolOffsetMultiplier = 0.000001;
          }
        }
        else if (CncKind.Equals ("30i")) {
          if (ReadParamAsByte ("5042|-1|0") == 1) {
            m_toolOffsetMultiplier = 0.01;
          }
          else if (ReadParamAsByte ("5042|-1|1") == 1) {
            m_toolOffsetMultiplier = 0.0001;
          }
          else if (ReadParamAsByte ("5042|-1|2") == 1) {
            m_toolOffsetMultiplier = 0.00001;
          }
          else if (ReadParamAsByte ("5042|-1|3") == 1) {
            m_toolOffsetMultiplier = 0.000001;
          }
        }
        else if (CncKind.Equals ("0i-D") || CncKind.Equals ("PMi-A")) {
          if (ReadParamAsByte ("5042|-1|0") == 1) {
            m_toolOffsetMultiplier = 0.01;
          }
          else if (ReadParamAsByte ("5042|-1|1") == 1) {
            m_toolOffsetMultiplier = 0.0001;
          }
        }
        else {
          if (ReadParamAsByte ("1004|-1|0") == 1) {
            m_toolOffsetMultiplier = 0.01;
          }
          else if (ReadParamAsByte ("1004|-1|1") == 1) {
            m_toolOffsetMultiplier = 0.0001;
          }
        }

        if (m_toolOffsetIsInch.Value) {
          m_toolOffsetMultiplier *= 0.1;
        }

        log.InfoFormat ("ComputeToolOffsetMultiplier - found multiplier {0} for system {1}, IsInch is ", m_toolOffsetMultiplier, CncKind, m_toolOffsetIsInch.Value);
      }
    }

    double ReadToolOffsetValue (short toolOffsetNumber, short toolOffsetType, double multiplier)
    {
      CheckConnection ();
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdtofs (m_handle, toolOffsetNumber, toolOffsetType,
                                                             (short)Marshal.SizeOf (typeof (Import.FwLib.ODBTOFS)),
                                                             out Import.FwLib.ODBTOFS data);
      if (Import.FwLib.EW.OK != result) {
        log.ErrorFormat ("ReadToolOffsetValue: rdtofs failed with {0} for toolOffsetNumber={1} toolOffsetType={2}",
                         result, toolOffsetNumber, toolOffsetType);
        ManageError ("ReadToolOffsetValue", result);
        throw new Exception ("rdtofs failed");
      }

      log.DebugFormat ("ReadToolOffsetValue: got {0} for number={1} type={2}", data.data, toolOffsetNumber, toolOffsetType);
      return multiplier * data.data;
    }

    double[] ReadToolOffsetValues (short toolOffsetStart, short toolOffsetEnd, short toolOffsetType, double multiplier)
    {
      CheckConnection ();
      double[] values = new double[toolOffsetEnd - toolOffsetStart + 1];

      // Read 128 values at a same time
      Import.FwLib.EW result = Import.FwLib.EW.OK;
      short count = 0;
      short increment = 128;
      while (count < values.Length) {
        // Prepare new data to read
        short startNumber = (short)(count + toolOffsetStart);
        short endNumber = (short)Math.Min (count + toolOffsetStart + increment - 1, toolOffsetEnd);

        // Read data
        result = (Import.FwLib.EW)Import.FwLib.Cnc.rdtofsr (m_handle, startNumber, toolOffsetType, endNumber,
                                                            (short)(8 + 4 * (endNumber - startNumber + 1)),
                                                            out Import.FwLib.IODBTO_M_T_OFS data);
        for (int i = data.datano_s; i <= data.datano_e; i++) {
          values[count + i - data.datano_s] = data.m_t_ofs[i - data.datano_s];
        }

        // Go on
        count += increment;
      }

      if (Import.FwLib.EW.OK != result) {
        log.ErrorFormat ("ReadToolOffsetValues: rdtofs failed with {0} for toolOffset={1}-{2} toolOffsetType={3}",
                         result, toolOffsetStart, toolOffsetEnd, toolOffsetType);
        ManageError ("ReadToolOffsetValues", result);
        throw new Exception ("rdtofs failed");
      }

      // Apply a multiplier and return the result
      for (int i = 0; i < values.Length; i++) {
        values[i] *= multiplier;
      }

      return values;
    }

    double ReadWorkZeroOffsetValue (short workZeroOffsetNumber, short axisNumber, double multiplier)
    {
      CheckConnection ();
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdzofs (m_handle, workZeroOffsetNumber, axisNumber,
                                                              (short)Marshal.SizeOf (typeof (Import.FwLib.ODBTOFS)),
                                                              out Import.FwLib.IODBZOFS data);
      if (Import.FwLib.EW.OK != result) {
        log.ErrorFormat ("ReadWorkZeroOffsetValue: rdzofs failed with {0} for number={1} axis={2}",
                         result, workZeroOffsetNumber, axisNumber);
        ManageError ("ReadWorkZeroOffsetValue", result);
        throw new Exception ("rdzofs failed");
      }

      log.DebugFormat ("ReadWorkZeroOffsetValue: got {0} for number={1} axis={2}",
                       data.data, workZeroOffsetNumber, axisNumber);
      return multiplier * data.data;
    }

    short GetOffsetType (string offsetTypeDescription)
    {
      short toolOffsetType;
      switch (offsetTypeDescription) {
      case "WC": // Wear Cutter radius on Machining Center Series
      case "WX": // Wear X axis on Lathe Series (T series)
        toolOffsetType = 0;
        break;
      case "GC": // Geometry Cutter radius on Machining Center Series
      case "GX": // Geometry X axis on Lathe Series (T series)
        toolOffsetType = 1;
        break;
      case "WL": // Wear Tool length on Machining Center Series
      case "WZ": // Wear Z axis on Lathe Series (T series)
        toolOffsetType = 2;
        break;
      case "GL": // Geometry Tool length on Machining Center Series
      case "GZ": // Geometry Z axis on Lathe Series (T series)
        toolOffsetType = 3;
        break;
      case "WR": // (column 'R' on the screen)
      case "WN": // Wear Nose R on Lathe Series (T series)
        toolOffsetType = 4;
        break;
      case "GR": // (column 'R' on the screen)
      case "GN": // Geometry Nose R on Lathe Series (T series)
        toolOffsetType = 5;
        break;
      case "WT": // (column 'T' on the screen)
      case "WU":
      case "WI": // Wear Imaginary tool nose
        toolOffsetType = 6;
        break;
      case "GT": // (column 'T' on the screen)
      case "GU":
      case "GI": // Geometry Imaginary tool nose
        toolOffsetType = 7;
        break;
      case "WY": // Wear Y axis on Lathe Series (T series)
      case "WV":
        toolOffsetType = 8;
        break;
      case "GY": // Geometry Y axis on Lathe Series (T series)
      case "GV":
        toolOffsetType = 9;
        break;
      case "X2": // Wear X axis on Lathe Series (T series, second geometry offset, Series 300i)
        toolOffsetType = 100;
        break;
      case "Z2": // Wear Z axis on Lathe Series (T series, second geometry offset, Series 300i)
        toolOffsetType = 101;
        break;
      case "Y2": // Wear Y axis on Lathe Series (T series, second geometry offset, Series 300i)
        toolOffsetType = 102;
        break;
      default:
        log.ErrorFormat ("GetOffsetType: value {0} is not valid", offsetTypeDescription);
        throw new ArgumentException ("GetOffsetType: value " + offsetTypeDescription + " is not valid");
      }
      return toolOffsetType;
    }
    #endregion Private functions
  }
}
