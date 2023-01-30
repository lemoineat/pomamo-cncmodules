// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// HeidenhainDNC input module
  /// </summary>
  public partial class HeidenhainDNC
  {
    #region Get methods
    /// <summary>
    /// Get the program state among:
    /// * IDLE
    /// * RUNNING
    /// * STOPPED
    /// * INTERRUPTED
    /// * FINISHED
    /// * ERROR
    /// * NOT_SELECTED
    /// </summary>
    public string ProgramStatus
    {
      get
      {
        string result;

        try {
          result = m_interfaceManager.InterfaceProgram.ProgramStatus;
        }
        catch (Exception ex) {
          ProcessException (ex, "HeidenhainDNC", "ProgramStatus", true);
          throw;
        }

        // Remove the prefix "DNC_PRG_STS_"
        return result.Remove (0, 12);
      }
    }

    /// <summary>
    /// Execution mode among:
    /// * MANUAL
    /// * MDI
    /// * RPF
    /// * SINGLESTEP
    /// * AUTOMATIC
    /// * OTHER
    /// * SIMULO_TURBO_DEPRECATED
    /// * HANDWHEEL
    /// </summary>
    public string ExecutionMode
    {
      get
      {
        string result;

        try {
          result = m_interfaceManager.InterfaceProgram.ExecutionMode;
        }
        catch (Exception ex) {
          ProcessException (ex, "HeidenhainDNC", "ExecutionMode", true);
          throw;
        }

        // Remove the prefix "DNC_PRG_STS_"
        return result.Remove (0, 9);
      }
    }

    /// <summary>
    /// Feedrate override
    /// </summary>
    public Int32 FeedrateOverride
    {
      get
      {
        int result;

        try {
          result = m_interfaceManager.InterfaceProgram.FeedrateOverride;
        }
        catch (Exception ex) {
          ProcessException (ex, "HeidenhainDNC", "FeedrateOverride", true);
          throw;
        }

        return result;
      }
    }

    /// <summary>
    /// Spindle speed override
    /// </summary>
    public Int32 SpindleSpeedOverride
    {
      get
      {
        int result;

        try {
          result = m_interfaceManager.InterfaceProgram.SpindleSpeedOverride;
        }
        catch (Exception ex) {
          ProcessException (ex, "HeidenhainDNC", "SpindleSpeedOverride", true);
          throw;
        }

        return result;
      }
    }

    /// <summary>
    /// Rapid traverse override
    /// </summary>
    public Int32 RapidTraverseOverride
    {
      get
      {
        int result;

        try {
          result = m_interfaceManager.InterfaceProgram.RapidTraverseOverride;
        }
        catch (Exception ex) {
          ProcessException (ex, "HeidenhainDNC", "RapidTraverseOverride", true);
          throw;
        }

        return result;
      }
    }

    /// <summary>
    /// Current program that is read
    /// </summary>
    public string ProgramName
    {
      get
      {
        string result;

        try {
          result = m_interfaceManager.InterfaceProgram.ProgramName;
        }
        catch (Exception ex) {
          ProcessException (ex, "HeidenhainDNC", "ProgramName", true);
          throw;
        }

        return result;
      }
    }

    /// <summary>
    /// Current subprogram that is read
    /// </summary>
    public string SubProgramName
    {
      get
      {
        string result;

        try {
          result = m_interfaceManager.InterfaceProgram.SubProgramName;
        }
        catch (Exception ex) {
          ProcessException (ex, "HeidenhainDNC", "SubProgramName", true);
          throw;
        }

        return result;
      }
    }
    /// <summary>
    /// Current block that is read in a program
    /// </summary>
    public int BlockNumber
    {
      get
      {
        int result;

        try {
          result = m_interfaceManager.InterfaceProgram.BlockNumber;
        }
        catch (Exception ex) {
          ProcessException (ex, "HeidenhainDNC", "BlockNumber", true);
          throw;
        }

        return result;
      }
    }

    /// <summary>
    /// Content of the current line that is executed in a program
    /// </summary>
    public string CurrentLine
    {
      get
      {
        string result;

        try {
          result = m_interfaceManager.InterfaceProgram.CurrentLine;
        }
        catch (Exception ex) {
          ProcessException (ex, "HeidenhainDNC", "CurrentLine", true);
          throw;
        }

        return result;
      }
    }

    /// <summary>
    /// Get the cutter location
    /// Channel 0
    /// </summary>
    public Position CutterLocation
    {
      get
      {
        Position result;

        try {
          result = m_interfaceManager.InterfaceProgram.CutterLocation;
        }
        catch (Exception ex) {
          ProcessException (ex, "HeidenhainDNC", "CutterLocation", true);
          throw;
        }

        return result;
      }
    }
    #endregion // Get methods
  }
}