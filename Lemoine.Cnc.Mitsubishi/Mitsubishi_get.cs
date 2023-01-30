// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Mitsubishi_position.
  /// </summary>
  public partial class Mitsubishi
  {
    #region System
    /// <summary>
    /// NC system S/W number, name, and PLC version
    /// </summary>
    public string Version
    {
      get
      {
        string version = "";
        try {
          version = m_interfaceManager.InterfaceSystem.Version;
        }
        catch (Exception ex) {
          ProcessException (ex);
          throw;
        }
        return version;
      }
    }

    /// <summary>
    /// True if the system is enabled, false otherwise
    /// </summary>
    /// <returns></returns>
    public bool SystemEnabled
    {
      get
      {
        bool value = false;
        try {
          value = m_interfaceManager.InterfaceSystem.SystemEnabled;
        }
        catch (Exception ex) {
          ProcessException (ex);
          throw;
        }
        return value;
      }
    }

    /// <summary>
    /// Number of axes of the system
    /// </summary>
    /// <returns></returns>
    public int AxisNumber
    {
      get
      {
        int value = 0;
        try {
          value = m_interfaceManager.InterfaceSystem.AxisNumber;
        }
        catch (Exception ex) {
          ProcessException (ex);
          throw;
        }
        return value;
      }
    }

    /// <summary>
    /// Current list of alarms
    /// </summary>
    public IList<CncAlarm> Alarms
    {
      get
      {
        IList<CncAlarm> alarms = null;
        try {
          alarms = m_interfaceManager.InterfaceSystem.Alarms;
        }
        catch (Exception ex) {
          ProcessException (ex);
          throw;
        }
        return alarms;
      }
    }
    #endregion System

    #region Position
    /// <summary>
    /// Current position
    /// </summary>
    public Position Position
    {
      get
      {
        var position = new Position ();
        try {
          position = m_interfaceManager.InterfacePosition.Position;
        }
        catch (Exception ex) {
          ProcessException (ex);
          throw;
        }
        return position;
      }
    }

    /// <summary>
    /// Get the feed speed
    /// </summary>
    /// <param name="type">Possible values:
    /// 0 - F programming feedrate (FA), mm / min
    /// 1 - Effective feedrate in manual feed (FM), mm / min
    /// 2 - Synchronous feedrate (FS), from 0.0 to 100.0 mm / rev (or up to 1000.0?)
    /// 3 - Effective feedrate in automatic operation (Fc), mm / min
    /// 4 - Screw lead feedrate (FE), from 0.0 to 1000.0 mm
    /// </param>
    /// <returns></returns>
    public double GetFeedSpeed (string type)
    {
      double value = 0.0;
      try {
        value = m_interfaceManager.InterfacePosition.GetFeedSpeed (int.Parse (type));
      }
      catch (Exception ex) {
        ProcessException (ex);
        throw;
      }
      return value;
    }
    #endregion Position

    #region Command
    /// <summary>
    /// Get the G code command according to groups
    /// </summary>
    /// <param name="type">Possible values:
    /// 1 - Interpolation mode - G00, G01, G02, G03, G33, G02.1, G03.1, G02.3, G03.3, G02.4, G03.4, G062
    /// 2 - Plane selection - G17, G18, G19
    /// 3 - Absolute - G90, (incremental) G91
    /// 4 - Chuck barrier - G22, G23
    /// 5 - Feed mode - G93, G94, G95
    /// 6 - Inch - G20, (millimeter) G21
    /// 7 - Radial compensation mode - G40, G41, G42, G41.2, G42.2
    /// 8 - Length compensation mode - G43, G44, G43.1, G43.4, G43.5, G49
    /// 9 - Fixed cycle mode - G70, G71, G72, G73, G74, G75, G76, G77, G78, G79, G80, G81, G82, G83, G84, G85, G86, G87, G88, G89
    /// 10 - Initial point return - G98, (R point return) G99
    /// 11 - G50, G51
    /// 12 - Workpiece coordinate system modal
    /// 13 - Cutting mode - G61, G61.1, G61.2, G62, G63, G63.1, G63.2, G64
    /// 14 - Modal call - G66, G66.1, G67
    /// 15 - Normal control - G40.1, G41.1, G42.1 (only for M700/M800 series M system)
    /// 16 - Coordinate rotation - G68, G68.2, G68.3, G69 (only for M700/M800 series M system)
    /// 17 - Constant surface speed control - G96, G97
    /// 18 - Polar coordinate command - G15, G16
    /// 19 - G command mirror image - G50.1, G51.1
    /// 20 - Spindle selection - G43.1, G44.1, G47.1
    /// 21 - Cylindrical interpolation / polar coordinate interpolation - G07.1, G107, G12.1, G112, G13.1, G113 (only for M700/M800 series M system)
    /// </param>
    /// <returns></returns>
    public double GetGCodeCommand (string type)
    {
      double value = 0.0;
      try {
        value = m_interfaceManager.InterfaceCommand.GetGCodeCommand (int.Parse (type));
      }
      catch (Exception ex) {
        ProcessException (ex);
        throw;
      }
      return value;
    }

    /// <summary>
    /// True if the machine is using the rapid traverse movement
    /// </summary>
    public bool RapidTraverse
    {
      get
      {
        bool value = false;
        try {
          value = m_interfaceManager.InterfaceCommand.RapidTraverse;
        }
        catch (Exception ex) {
          ProcessException (ex);
          throw;
        }
        return value;
      }
    }

    /// <summary>
    /// Get active commands (M, S, T, B)
    /// </summary>
    /// <param name="command">Command type: M, S, T or B</param>
    /// <returns></returns>
    public IList<int> GetActiveCommands (string command)
    {
      IList<int> values = null;
      try {
        var commandType = (Interface_command.CommandType)Enum.Parse (typeof (Interface_command.CommandType), command);
        values = m_interfaceManager.InterfaceCommand.GetActiveCommands (commandType);
      }
      catch (Exception ex) {
        ProcessException (ex);
        throw;
      }
      return values;
    }

    /// <summary>
    /// Get the feed speed command
    /// </summary>
    /// <param name="type">Possible values:
    /// 0 - F programming feedrate (FA), mm / min
    /// 1 - Effective feedrate in manual feed (FM), mm / min
    /// 2 - Synchronous feedrate (FS), from 0.0 to 100.0 mm / rev (or up to 1000.0?)
    /// 3 - Effective feedrate in automatic operation (Fc), mm / min
    /// 4 - Screw lead feedrate (FE), from 0.0 to 1000.0 mm
    /// </param>
    /// <returns></returns>
    public double GetFeedCommand (string type)
    {
      double value = 0.0;
      try {
        value = m_interfaceManager.InterfaceCommand.GetFeedCommand (int.Parse (type));
      }
      catch (Exception ex) {
        ProcessException (ex);
        throw;
      }
      return value;
    }
    #endregion Command

    #region Program
    /// <summary>
    /// Current block number
    /// </summary>
    /// <param name="isMain">True for main program, subprogram otherwise</param>
    /// <returns></returns>
    public int GetBlockNumber (string isMain)
    {
      bool isMainB = (isMain.ToLower () == "true" || isMain == "1");
      int value = 0;
      try {
        value = m_interfaceManager.InterfaceProgram.GetBlockNumber (
          isMainB ? Interface_program.ProgramType.EZNC_MAINPRG : Interface_program.ProgramType.EZNC_SUBPRG);
      }
      catch (Exception ex) {
        ProcessException (ex);
        throw;
      }
      return value;
    }

    /// <summary>
    /// Current sequence number
    /// </summary>
    /// <param name="isMain">True for main program, subprogram otherwise</param>
    /// <returns></returns>
    public int GetSequenceNumber (string isMain)
    {
      bool isMainB = (isMain.ToLower () == "true" || isMain == "1");
      int value = 0;
      try {
        value = m_interfaceManager.InterfaceProgram.GetSequenceNumber (
          isMainB ? Interface_program.ProgramType.EZNC_MAINPRG : Interface_program.ProgramType.EZNC_SUBPRG);
      }
      catch (Exception ex) {
        ProcessException (ex);
        throw;
      }
      return value;
    }

    /// <summary>
    /// Current program name
    /// </summary>
    /// <param name="isMain">True for main program, subprogram otherwise</param>
    /// <returns></returns>
    public string GetProgramName (string isMain)
    {
      bool isMainB = (isMain.ToLower () == "true" || isMain == "1");
      string value = "";
      try {
        value = m_interfaceManager.InterfaceProgram.GetProgramName (
          isMainB ? Interface_program.ProgramType.EZNC_MAINPRG : Interface_program.ProgramType.EZNC_SUBPRG);
      }
      catch (Exception ex) {
        ProcessException (ex);
        throw;
      }
      return value;
    }
    #endregion Program

    #region Axis monitor
    /// <summary>
    /// Spindle monitoring
    /// </summary>
    /// <param name="param">Format {type}:{spindle number}, where type can be:
    /// 0 - Gain. Spindle position loop gain (unit 1/s)
    /// 1 - Droop. Position deviation amount (unit I)
    /// 2 - Spindle (SR, SF) rotation speed. Actual spindle motor speed. Including override. (unit rpm)
    /// 3 - Load. Spindle motor load. (unit from 0 [%])
    /// 4 - LED display. 7-segment LED display on a driver. Outputs a 3-digit character string from "00\0" to "FF\0".
    /// 5 - Alarm 1. Up to 3 alphanumeric characters.
    /// 6 - Alarm 2. Up to 3 alphanumeric characters.
    /// 7 - Alarm 3. Up to 3 alphanumeric characters.
    /// 8 - Alarm 4. Up to 3 alphanumeric characters.
    /// 10 - Cycle counter
    /// 11 - Control input 1
    /// 12 - Control input 2
    /// 13 - Control input 3
    /// 14 - Control input 4 (M700/M800 series only)
    /// 15 - Control output 1
    /// 16 - Control output 2
    /// 17 - Control output 3
    /// 18 - Control output 4
    /// </param>
    /// <returns></returns>
    public int GetSpindleMonitor (string param)
    {
      int value = 0;
      try {
        var split = param.Split (':');
        if (split.Length != 2) {
          throw new Exception ("Mitsubishi - GetSpindleMonitor received a badly formated argument '" + param + "'");
        }

        value = m_interfaceManager.InterfaceAxisMonitor.GetSpindleMonitor (int.Parse (split[0]), int.Parse (split[1]));
      }
      catch (Exception ex) {
        ProcessException (ex);
        throw;
      }
      return value;
    }

    /// <summary>
    /// Servo monitoring
    /// </summary>
    /// <param name="param">Format {type}:{axis number}, where type can be:
    /// 0 - Gain. Position loop gain status display (unit 1/s)
    /// 1 - Droop. Tracking delay (unit i)
    /// 2 - Speed. Actual motor speed (unit rpm)
    /// 3 - Current. Load current. Motor current, displayed by converting to continuous current when stalled. (unit %)
    /// 4 - MAXCUR1. Maximum current I (unit %)
    /// 5 - MAXCUR1. Maximum current II (unit %)
    /// 6 - Overload (unit %)
    /// 7 - Regenerative load (unit %)
    /// 10 - Cycle counter
    /// 11 - Grid interval (Unit: Command unit)
    /// 12 - Grid amount (Unit: Command unit)
    /// 13 - MACPOS. Machine position (Unit: Command unit)
    /// 14 - MOT POS. Motor end FB (Unit: Command unit)
    /// 15 - SCA POS. Machine end FB (Unit: Command unit)
    /// 16 - FB ERROR. FB error (Unit: i)
    /// 17 - DFB COMP. DFB compensation amount
    /// 18 - Remain command (Unit: Command unit)
    /// 19 - Currnt posn (Unit: Command unit)
    /// 20 - Manual interrupt amount (Unit: Command unit)
    /// 100 - AMP DISP. Amplifier display. 7-segment LED display on a drive unit. Outputs a 3-digit character string from "00\0" to "FF\0".
    /// 101 - Alarm 1. Outputs a 3-digit character string
    /// 102 - Alarm 2. Outputs a 3-digit character string
    /// 103 - Alarm 3. Outputs a 3-digit character string
    /// 104 - Alarm 4. Outputs a 3-digit character string
    /// </param>
    /// <returns></returns>
    public int GetServoMonitor (string param)
    {
      int value = 0;
      try {
        var split = param.Split (':');
        if (split.Length != 2) {
          throw new Exception ("Mitsubishi - GetServoMonitor received a badly formated argument '" + param + "'");
        }

        value = m_interfaceManager.InterfaceAxisMonitor.GetServoMonitor (int.Parse (split[0]), int.Parse (split[1]));
      }
      catch (Exception ex) {
        ProcessException (ex);
        throw;
      }
      return value;
    }
    #endregion Axis monitor

    #region Run status
    /// <summary>
    /// Get the operation status
    /// </summary>
    /// <param name="operationType">Can be:
    /// 0 - Tool length measurement
    /// 1 - Automatic operation "run". Gets the status indicating that the system is operating automatically
    /// 2 - Automatic operation "start". Gets the status indicating that the system is operating automatically and
    ///     that a movement command or M, S, T, B process is being executed.
    /// 3 - Automatic operation "pause". Gets the status indicating that automatic operation is paused
    ///     while executing a movement command or miscellaneous command with automatic operation. (only for M700/M800 series)
    /// </param>
    /// <returns></returns>
    public bool GetRunStatus (string operationType)
    {
      bool value = false;
      try {
        value = (m_interfaceManager.InterfaceRunStatus.GetRunStatus (int.Parse (operationType)) > 0);
      }
      catch (Exception ex) {
        ProcessException (ex);
        throw;
      }
      return value;
    }

    /// <summary>
    /// True if in G01, G02, G03, G31, G33, G34, or G35 mode
    /// False otherwise
    /// </summary>
    public bool CuttingMode
    {
      get
      {
        bool value = false;
        try {
          value = m_interfaceManager.InterfaceRunStatus.CuttingMode;
        }
        catch (Exception ex) {
          ProcessException (ex);
          throw;
        }
        return value;
      }
    }
    #endregion Run status

    #region Common variable
    /// <summary>
    /// Read a common variable
    /// </summary>
    /// <param name="variableNumber">100 to 199, 500 to 999 and additionally for M700/M800 series:
    /// 400 to 999, 100100 to 100199, 200100 to 200199, 300100 to 300199, 400100 to 400199, 500100 to 500199,
    /// 600100 to 600199, 700100 to 700199, 800100 to 800199, 900000 to 907399</param>
    /// <returns></returns>
    public double CommonVRead (string variableNumber)
    {
      return CommonVRead (int.Parse (variableNumber));
    }

    /// <summary>
    /// Read a common variable
    /// </summary>
    /// <param name="variableNumber">100 to 199, 500 to 999 and additionally for M700/M800 series:
    /// 400 to 999, 100100 to 100199, 200100 to 200199, 300100 to 300199, 400100 to 400199, 500100 to 500199,
    /// 600100 to 600199, 700100 to 700199, 800100 to 800199, 900000 to 907399</param>
    /// <returns></returns>
    double CommonVRead (int variableNumber)
    {
      try {
        return m_interfaceManager.InterfaceCommonVariable.CommonVRead (variableNumber);
      }
      catch (Exception ex) {
        ProcessException (ex);
        throw;
      }
    }

    /// <summary>
    /// Read and log a range of common variables
    /// </summary>
    /// <param name="param">Format {min}-{max}</param>
    /// <returns></returns>
    public bool LogCommonVRead (string param)
    {
      int min = 0;
      int max = 0;
      try {
        var parts = param.Split ('-');
        min = int.Parse (parts[0]);
        max = int.Parse (parts[1]);
      }
      catch (Exception e) {
        log.ErrorFormat ("Mitsubishi.LogCommonVRead - Wrong parameters {0} (should be 'min-max'): {1}", param, e.Message);
      }

      for (int i = min; i <= max; i++) {
        try {
          double value = m_interfaceManager.InterfaceCommonVariable.CommonVRead (i);
          log.FatalFormat ("Common variable {0} = {1}", i, value);
        }
        catch (Exception e) {
          log.ErrorFormat ("Couldn't read variable {0}: {1}", i, e.Message);
        }
      }

      return true;
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

    IDictionary<int, double> GetCustomMacros (IEnumerable<int> macroVariableNumbers)
    {
      IDictionary<int, double> result = new Dictionary<int, double> ();

      if (!macroVariableNumbers.Any ()) {
        return result;
      }

      { // Between 1 and 33 (Local variables)
        var localVariables = macroVariableNumbers.Where (k => (1 <= k) && (k <= 33));
        foreach (var localVariable in localVariables) {
          try {
            result[localVariable] = LocalVRead (localVariable, 0);
          }
          catch (Exception ex) {
            log.ErrorFormat ("GetCustomMacros: CommonVRead failed with macroNumber {0} (not available ?), {1}",
              localVariable, ex);
          }
        }
      }
      { // From 100 (Common variables)
        var commonVariables = macroVariableNumbers.Where (k => (100 <= k));
        foreach (var commonVariable in commonVariables) {
          try {
            result[commonVariable] = CommonVRead (commonVariable);
          }
          catch (Exception ex) {
            log.ErrorFormat ("GetCustomMacros: CommonVRead failed with macroNumber {0} (not available ?), {1}",
              commonVariable, ex);
          }
        }
      }

      return result;
    }
    #endregion Common variable

    #region Local variable
    /// <summary>
    /// Read a local variable
    /// </summary>
    /// <param name="param">Format {variable number}:{level}, where
    /// {variable number} is from 1 to 33 and
    /// {level} is from 0 to 4 (macro subprogram execution level)</param>
    /// <returns></returns>
    public double LocalVRead (string param)
    {
      var split = param.Split (':');
      if (split.Length != 2) {
        log.ErrorFormat ("LocalVRead: invalid argument");
        throw new ArgumentException ("Mtsubishi - CommonVRead received a badly formated argument '" + param + "'");
      }
      return LocalVRead (int.Parse (split[0]), int.Parse (split[1]));
    }

    /// <summary>
    /// Read a local variable
    /// </summary>
    /// <param name="variableNumber"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    double LocalVRead (int variableNumber, int level)
    {
      try {
        return m_interfaceManager.InterfaceLocalVariable.LocalVRead (variableNumber, level);
      }
      catch (Exception ex) {
        ProcessException (ex);
        throw;
      }
    }

    /// <summary>
    /// Read and log a range of common variables
    /// </summary>
    /// <param name="param">Format {min}-{max}</param>
    /// <returns></returns>
    public bool LogLocalVRead (string param)
    {
      int min = 0;
      int max = 0;
      try {
        var parts = param.Split ('-');
        min = int.Parse (parts[0]);
        max = int.Parse (parts[1]);
      }
      catch (Exception e) {
        log.ErrorFormat ("Mitsubishi.LogLocalVRead - Wrong parameters {0} (should be 'min-max'): {1}", param, e.Message);
      }

      // Scan for all levels
      for (int level = 0; level <= 4; level++) {
        for (int i = min; i <= max; i++) {
          try {
            double value = m_interfaceManager.InterfaceLocalVariable.LocalVRead (i, level);
            log.FatalFormat ("Local variable {0} level {1} = {2}", i, level, value);
          }
          catch (Exception e) {
            log.ErrorFormat ("Couldn't read variable {0} with level {1}: {2}", i, level, e.Message);
          }
        }
      }

      return true;
    }
    #endregion Local variable

    #region Tool
    /// <summary>
    /// Current tool life data
    /// </summary>
    public ToolLifeData ToolLifeData
    {
      get
      {
        ToolLifeData value = null;
        try {
          value = m_interfaceManager.InterfaceTool.ToolLifeData;
        }
        catch (Exception ex) {
          ProcessException (ex);
          throw;
        }
        return value;
      }
    }

    /// <summary>
    /// gettool offset
    /// <param name="param">list: toolSetNumber;kind</param>
    /// with kind = 
    /// O X
    /// 1 Z
    /// 3 X wear
    /// 4 Z wear/// 
    /// </summary>
    public double GetOffset (string param)
    {
      if (string.IsNullOrEmpty (param)) {
        log.Error ("GetOffset: empty param ");
        return 0;
      }
      else {
        var paramItems = Lemoine.Collections.EnumerableString.ParseListString (param);
        if (paramItems.Length == 2) {
          string toolSetNumberString = paramItems[0];
          string kindString = paramItems[1];
          int toolSetNumber = -1;
          int kind = -1;

          if (!int.TryParse (toolSetNumberString, out toolSetNumber) || !int.TryParse (kindString, out kind)) {
            log.Error ($"GetOffset: bad param format: {param} ");
            return 0;
          }
          double value = -1;
          try {
            value = m_interfaceManager.InterfaceTool.GetOffset (toolSetNumber, kind);
          }
          catch (Exception ex) {
            ProcessException (ex);
            throw;
          }
          return value;
        }
        else {
          log.Error ($"GetOffset: bad param format: {param} ");
          return 0;
        }
      }
    }
    #endregion Tool

    #region Device
    /// <summary>
    /// Read a value in the specified device
    /// /!\ Only one kind of data can be read from a specific device. It's for example forbidden to read bits and words from "B" /!\
    /// </summary>
    /// <param name="param">Format is {device type}:{register number}:{bits}, {bits} being 1, 8, 16 or 32</param>
    /// <returns></returns>
    public UInt32 ReadDevice (string param)
    {
      UInt32 value = 0;
      try {
        var split = param.Split (':');
        if (split.Length != 3) {
          throw new Exception ("Mitsubishi - ReadDevice received a badly formated argument '" + param + "'");
        }

        value = m_interfaceManager.InterfaceDevice.ReadDevice (split[0], int.Parse (split[1]), int.Parse (split[2]));
      }
      catch (Exception ex) {
        ProcessException (ex);
        throw;
      }
      return value;
    }

    /// <summary>
    /// Read a value in the specified device
    /// /!\ Only one kind of data can be read from a specific device. It's for example forbidden to read bits and words from "B" /!\
    /// </summary>
    /// <param name="param">Format is {device type}:{register number}:{bits}:{bitnumber}
    /// {bits} being 1, 8, 16 or 32
    /// {bitnumber} 0 is the less significant bit</param>
    /// <returns></returns>
    public bool ReadBoolDevice (string param)
    {
      bool value = false;
      try {
        var split = param.Split (':');
        if (split.Length != 4) {
          throw new Exception ("Mitsubishi - ReadDevice received a badly formated argument '" + param + "'");
        }

        uint valueTmp = m_interfaceManager.InterfaceDevice.ReadDevice (split[0], int.Parse (split[1]), int.Parse (split[2]));

        // Select the right bit
        value = (valueTmp & (0x1 << int.Parse (split[3]))) != 0;
      }
      catch (Exception ex) {
        ProcessException (ex);
        throw;
      }

      return value;
    }

    /// <summary>
    /// Get the machine alarms based on a file listing the registers / alarm numbers / alarm descriptions
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public IList<CncAlarm> GetMachineAlarms (string file)
    {
      IList<CncAlarm> value = null;

      try {
        value = m_interfaceManager.InterfaceDevice.GetMachineAlarms (file);
      }
      catch (Exception ex) {
        ProcessException (ex);
        throw;
      }

      return value;
    }
    #endregion Device
  }
}
