// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Okuma input module, tool compensation read / write
  /// </summary>
  public partial class OkumaThincApi
  {
    /// <summary>
    /// All kinds of offset that can be read / written
    /// The choice between them might depend on the machine
    /// </summary>
    internal enum ToolCompensationType
    {
      // The parameter associated to the following enum values is TOOL OFFSET NUMBER

      CutterRCompOffset = 1,
      CutterRCompWearOffset = 2,
      ToolLengthOffset = 3,
      ToolLengthWearOffset = 4,

      // The parameter associated to the following enum values is TOOL NUMBER

      CutterROffset_1 = 5,
      CutterROffset_2 = 6,
      CutterROffset_3 = 7,

      CutterRWearOffset_1 = 8,
      CutterRWearOffset_2 = 9,
      CutterRWearOffset_3 = 10,

      ToolOffset_1 = 11,
      ToolOffset_2 = 12,
      ToolOffset_3 = 13,

      ToolWearOffset_1 = 14,
      ToolWearOffset_2 = 15,
      ToolWearOffset_3 = 16,

      // The following enum values can be used if, mentioning the documentation, "8 digits tool ID specification" is valid
      // Is it a parameter? What is a valid specification? Is Okuma writing invalid specification? It's like a bit of a mystery
      // If "8 digits tool ID specification" is valid, we shouldn't probably use the previous enum values
      //
      // The parameter associated to the following enum values is TOOL POT NUMBER

      _8DigitsToolID_CutterRCompOffset_1 = 101,
      _8DigitsToolID_CutterRCompOffset_2 = 102,
      _8DigitsToolID_CutterRCompOffset_3 = 103,

      _8DigitsToolID_CutterRCompWearOffset = 104,

      _8DigitsToolID_ToolLengthOffset_1 = 105,
      _8DigitsToolID_ToolLengthOffset_2 = 106,
      _8DigitsToolID_ToolLengthOffset_3 = 107,

      _8DigitsToolID_ToolLengthWearOffset = 108,
    }

    #region Read methods
    /// <summary>
    /// Read "Tool wear option spec"
    /// </summary>
    public bool GetOptionSpec_ToolWearOffset
    {
      get
      {
        return Call<bool> ("CSpec", "GetOptionSpecCode", 1 /*OptionSpecEnum.ToolWearOffset*/);
      }
    }

    /// <summary>
    /// Read "MOP Tool option spec"
    /// </summary>
    public bool GetOptionSpec_MOPTool
    {
      get
      {
        return Call<bool> ("CSpec", "GetOptionSpecCode", 2 /*OptionSpecEnum.MOPTool*/);
      }
    }

    /// <summary>
    /// Read "0.1 Micron option spec"
    /// </summary>
    public bool GetOptionSpec_OneTenthMicron
    {
      get
      {
        return Call<bool> ("CSpec", "GetOptionSpecCode", 3 /*OptionSpecEnum.OneTenthMicron*/);
      }
    }

    /// <summary>
    /// Read "8 Digits Tool ID spec"
    /// </summary>
    public bool GetOptionSpec_ToolID8Digits
    {
      get
      {
        return Call<bool> ("CSpec", "GetOptionSpecCode", 4 /*OptionSpecEnum.ToolID8Digits*/);
      }
    }

    /// <summary>
    /// Read a tool offset value
    /// The parameter is made of:
    /// * a kind of offset, as a text or number
    /// * the character '|'
    /// * the tool offset number, tool number or tool pot number depending on the kind of offset (see the enum ToolCompensationType)
    /// For example:
    /// * CutterROffset_1|2
    /// * _8DigitsToolID_CutterRCompWearOffset|12
    /// * 101|4
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public double ReadToolOffset (string param)
    {
      if (string.IsNullOrEmpty (param) || param.Split ('|').Length != 2) {
        log.ErrorFormat ("ReadToolOffset: param must be in the form {offset kind}|{number}");
        throw new ArgumentException ();
      }

      // Parse the compensation type
      var split = param.Split ('|');
      ToolCompensationType compensationType;
      try {
        compensationType =
        int.TryParse (split[0], out int compensationTypeInt) ?
        (ToolCompensationType)compensationTypeInt :
        (ToolCompensationType)Enum.Parse (typeof (ToolCompensationType), split[0]);
      } catch (Exception e) {
        log.ErrorFormat ("ReadToolOffset: Couldn't parse {0} as a ToolCompensationType: {1}", split[0], e);
        throw;
      }

      // Parse the index
      if (!int.TryParse(split[1], out int index)) {
        log.ErrorFormat ("ReadToolOffset: Couldn't parse {0} as an integer", split[1]);
        throw new ArgumentException ("ReadToolOffset: Couldn't parse the index in " + param, "param");
      }

      double result = 0;
      switch (compensationType) {
      case ToolCompensationType.CutterRCompOffset:
        result = Call<double> ("CTools", "GetCutterRCompOffset", index);
        break;
      case ToolCompensationType.CutterRCompWearOffset:
        result = Call<double> ("CTools", "GetCutterRCompWearOffset", index);
        break;
      case ToolCompensationType.ToolLengthOffset:
        result = Call<double> ("CTools", "GetToolLengthOffset", index);
        break;
      case ToolCompensationType.ToolLengthWearOffset:
        result = Call<double> ("CTools", "GetToolLengthWearOffset", index);
        break;
      case ToolCompensationType.CutterROffset_1:
        result = Call<double> ("CTools", "GetCutterROffset", index, 0 /*ToolCompensationEnum.HADA*/);
        break;
      case ToolCompensationType.CutterROffset_2:
        result = Call<double> ("CTools", "GetCutterROffset", index, 1 /*ToolCompensationEnum.HBDB*/);
        break;
      case ToolCompensationType.CutterROffset_3:
        result = Call<double> ("CTools", "GetCutterROffset", index, 2 /*ToolCompensationEnum.HCDC*/);
        break;
      case ToolCompensationType.CutterRWearOffset_1:
        result = Call<double> ("CTools", "GetCutterRWearOffset", index, 0 /*ToolCompensationEnum.HADA*/);
        break;
      case ToolCompensationType.CutterRWearOffset_2:
        result = Call<double> ("CTools", "GetCutterRWearOffset", index, 1 /*ToolCompensationEnum.HBDB*/);
        break;
      case ToolCompensationType.CutterRWearOffset_3:
        result = Call<double> ("CTools", "GetCutterRWearOffset", index, 2 /*ToolCompensationEnum.HCDC*/);
        break;
      case ToolCompensationType.ToolOffset_1:
        result = Call<double> ("CTools", "GetToolOffset", index, 0 /*ToolCompensationEnum.HADA*/);
        break;
      case ToolCompensationType.ToolOffset_2:
        result = Call<double> ("CTools", "GetToolOffset", index, 1 /*ToolCompensationEnum.HBDB*/);
        break;
      case ToolCompensationType.ToolOffset_3:
        result = Call<double> ("CTools", "GetToolOffset", index, 2 /*ToolCompensationEnum.HCDC*/);
        break;
      case ToolCompensationType.ToolWearOffset_1:
        result = Call<double> ("CTools", "GetToolWearOffset", index, 0 /*ToolCompensationEnum.HADA*/);
        break;
      case ToolCompensationType.ToolWearOffset_2:
        result = Call<double> ("CTools", "GetToolWearOffset", index, 1 /*ToolCompensationEnum.HBDB*/);
        break;
      case ToolCompensationType.ToolWearOffset_3:
        result = Call<double> ("CTools", "GetToolWearOffset", index, 2 /*ToolCompensationEnum.HCDC*/);
        break;
      case ToolCompensationType._8DigitsToolID_CutterRCompOffset_1:
        result = Call<double> ("CTools2", "GetCutterRCompOffset1", index);
        break;
      case ToolCompensationType._8DigitsToolID_CutterRCompOffset_2:
        result = Call<double> ("CTools2", "GetCutterRCompOffset2", index);
        break;
      case ToolCompensationType._8DigitsToolID_CutterRCompOffset_3:
        result = Call<double> ("CTools2", "GetCutterRCompOffset3", index);
        break;
      case ToolCompensationType._8DigitsToolID_CutterRCompWearOffset:
        result = Call<double> ("CTools2", "GetCutterRCompWearOffset", index);
        break;
      case ToolCompensationType._8DigitsToolID_ToolLengthOffset_1:
        result = Call<double> ("CTools2", "GetToolLengthOffset1", index);
        break;
      case ToolCompensationType._8DigitsToolID_ToolLengthOffset_2:
        result = Call<double> ("CTools2", "GetToolLengthOffset2", index);
        break;
      case ToolCompensationType._8DigitsToolID_ToolLengthOffset_3:
        result = Call<double> ("CTools2", "GetToolLengthOffset3", index);
        break;
      case ToolCompensationType._8DigitsToolID_ToolLengthWearOffset:
        result = Call<double> ("CTools2", "GetToolLengthWearOffset", index);
        break;
      }

      return result;
    }
    #endregion Read methods

    #region Write methods
    /// <summary>
    /// Write a tool offset value
    /// The parameter is made of:
    /// * a kind of offset, as a text or number
    /// * the character '|'
    /// * the tool offset number, tool number or tool pot number depending on the kind of offset (see the enum ToolCompensationType)
    /// For example:
    /// * CutterROffset_1|2
    /// * _8DigitsToolID_CutterRCompWearOffset|12
    /// * 101|4
    /// </summary>
    /// <param name="param"></param>
    /// <param name="value">value to set</param>
    /// <returns></returns>
    public void WriteToolOffset (string param, double value)
    {
      if (string.IsNullOrEmpty (param) || param.Split ('|').Length != 2) {
        log.ErrorFormat ("WriteToolOffset: param must be in the form {offset kind}|{number}");
        throw new ArgumentException ();
      }

      // Parse the compensation type
      var split = param.Split ('|');
      ToolCompensationType compensationType;
      try {
        compensationType =
        int.TryParse (split[0], out int compensationTypeInt) ?
        (ToolCompensationType)compensationTypeInt :
        (ToolCompensationType)Enum.Parse (typeof (ToolCompensationType), split[0]);
      }
      catch (Exception e) {
        log.ErrorFormat ("WriteToolOffset: Couldn't parse {0} as a ToolCompensationType: {1}", split[0], e);
        throw;
      }

      // Parse the index
      if (!int.TryParse (split[1], out int index)) {
        log.ErrorFormat ("WriteToolOffset: Couldn't parse {0} as an integer", split[1]);
        throw new ArgumentException ("WriteToolOffset: Couldn't parse the index in " + param, "param");
      }

      switch (compensationType) {
      case ToolCompensationType.CutterRCompOffset:
        Call ("CTools", "SetCutterRCompOffset", index, value);
        break;
      case ToolCompensationType.CutterRCompWearOffset:
        Call ("CTools", "SetCutterRCompWearOffset", index, value);
        break;
      case ToolCompensationType.ToolLengthOffset:
        Call ("CTools", "SetToolLengthOffset", index, value);
        break;
      case ToolCompensationType.ToolLengthWearOffset:
        Call ("CTools", "SetToolLengthWearOffset", index, value);
        break;
      case ToolCompensationType.CutterROffset_1:
        Call ("CTools", "SetCutterROffset", index, 0 /*ToolCompensationEnum.HADA*/, value);
        break;
      case ToolCompensationType.CutterROffset_2:
        Call ("CTools", "SetCutterROffset", index, 1 /*ToolCompensationEnum.HBDB*/, value);
        break;
      case ToolCompensationType.CutterROffset_3:
        Call ("CTools", "SetCutterROffset", index, 2 /*ToolCompensationEnum.HCDC*/, value);
        break;
      case ToolCompensationType.CutterRWearOffset_1:
        Call ("CTools", "SetCutterRWearOffset", index, 0 /*ToolCompensationEnum.HADA*/, value);
        break;
      case ToolCompensationType.CutterRWearOffset_2:
        Call ("CTools", "SetCutterRWearOffset", index, 1 /*ToolCompensationEnum.HBDB*/, value);
        break;
      case ToolCompensationType.CutterRWearOffset_3:
        Call ("CTools", "SetCutterRWearOffset", index, 2 /*ToolCompensationEnum.HCDC*/, value);
        break;
      case ToolCompensationType.ToolOffset_1:
        Call ("CTools", "SetToolOffset", index, 0 /*ToolCompensationEnum.HADA*/, value);
        break;
      case ToolCompensationType.ToolOffset_2:
        Call ("CTools", "SetToolOffset", index, 1 /*ToolCompensationEnum.HBDB*/, value);
        break;
      case ToolCompensationType.ToolOffset_3:
        Call ("CTools", "SetToolOffset", index, 2 /*ToolCompensationEnum.HCDC*/, value);
        break;
      case ToolCompensationType.ToolWearOffset_1:
        Call ("CTools", "SetToolWearOffset", index, 0 /*ToolCompensationEnum.HADA*/, value);
        break;
      case ToolCompensationType.ToolWearOffset_2:
        Call ("CTools", "SetToolWearOffset", index, 1 /*ToolCompensationEnum.HBDB*/, value);
        break;
      case ToolCompensationType.ToolWearOffset_3:
        Call ("CTools", "SetToolWearOffset", index, 2 /*ToolCompensationEnum.HCDC*/, value);
        break;
      case ToolCompensationType._8DigitsToolID_CutterRCompOffset_1:
        Call ("CTools2", "SetCutterRCompOffset1", index, value);
        break;
      case ToolCompensationType._8DigitsToolID_CutterRCompOffset_2:
        Call ("CTools2", "SetCutterRCompOffset2", index, value);
        break;
      case ToolCompensationType._8DigitsToolID_CutterRCompOffset_3:
        Call ("CTools2", "SetCutterRCompOffset3", index, value);
        break;
      case ToolCompensationType._8DigitsToolID_CutterRCompWearOffset:
        Call ("CTools2", "SetCutterRCompWearOffset", index, value);
        break;
      case ToolCompensationType._8DigitsToolID_ToolLengthOffset_1:
        Call ("CTools2", "SetToolLengthOffset1", index, value);
        break;
      case ToolCompensationType._8DigitsToolID_ToolLengthOffset_2:
        Call ("CTools2", "SetToolLengthOffset2", index, value);
        break;
      case ToolCompensationType._8DigitsToolID_ToolLengthOffset_3:
        Call ("CTools2", "SetToolLengthOffset3", index, value);
        break;
      case ToolCompensationType._8DigitsToolID_ToolLengthWearOffset:
        Call ("CTools2", "SetToolLengthWearOffset", index, value);
        break;
      }
    }
    #endregion Write methods
  }
}
