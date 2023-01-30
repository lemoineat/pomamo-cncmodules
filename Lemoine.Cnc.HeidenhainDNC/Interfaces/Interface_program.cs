// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Interface_program.
  /// </summary>
  public class Interface_program: GenericInterface<HeidenhainDNCLib.IJHAutomatic3>
  {
    #region Members
    bool m_overridesInitialized = false;
    Int32 m_feedrateOverride;
    Int32 m_speedOverride;
    Int32 m_rapidTraverseOverride;
    
    bool m_programInitialized = false;
    string m_currentProgramName;
    string m_currentSubProgramName;
    int m_currentProgramBlock;
    string m_currentProgramContent;
    
    bool m_cutterLocationInitialized = false;
    readonly IDictionary<string, double> m_cutterLocation = new Dictionary<string, double>();
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Interface_program() : base(HeidenhainDNCLib.DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHAUTOMATIC) {}
    #endregion // Constructors

    #region Protected methods
    /// <summary>
    /// Extra work during the initialization
    /// Already within a try catch
    /// </summary>
    protected override void InitializeInstance(InterfaceParameters parameters)
    {
      // Nothing
    }
    
    /// <summary>
    /// Work to do on "start", before new data will be read
    /// Already within a try catch
    /// </summary>
    protected override void ResetDataInstance()
    {
      m_overridesInitialized = false;
      m_programInitialized = false;
      m_cutterLocationInitialized = false;
      m_cutterLocation.Clear();
    }
    #endregion // Protected methods
    
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
    public string ProgramStatus {
      get {
        string str = m_interface.GetProgramStatus().ToString();
        
        // Remove the prefix "DNC_PRG_STS_"
        return str.Remove(0, 12);
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
    public string ExecutionMode {
      get {
        string str = m_interface.GetExecutionMode().ToString();
        
        // Remove the prefix "DNC_EXEC_"
        return str.Remove(0, 9);
      }
    }
    
    /// <summary>
    /// Feedrate override
    /// </summary>
    public Int32 FeedrateOverride {
      get {
        if (!m_overridesInitialized) {
          InitializeOverrides ();
        }

        if (m_overridesInitialized) {
          return m_feedrateOverride;
        }

        throw new Exception("Overrides cannot be initialized");
      }
    }
    
    /// <summary>
    /// Spindle speed override
    /// </summary>
    public Int32 SpindleSpeedOverride {
      get {
        if (!m_overridesInitialized) {
          InitializeOverrides ();
        }

        if (m_overridesInitialized) {
          return m_speedOverride;
        }

        throw new Exception("Overrides cannot be initialized");
      }
    }
    
    /// <summary>
    /// Rapid traverse override
    /// </summary>
    public Int32 RapidTraverseOverride {
      get {
        if (!m_overridesInitialized) {
          InitializeOverrides ();
        }

        if (m_overridesInitialized) {
          return m_rapidTraverseOverride;
        }

        throw new Exception("Overrides cannot be initialized");
      }
    }
    
    /// <summary>
    /// Current program that is read
    /// </summary>
    public string ProgramName {
      get {
        if (!m_programInitialized) {
          InitializeProgram ();
        }

        if (m_programInitialized) {
          return m_currentProgramName;
        }

        throw new Exception("Program information cannot be initialized");
      }
    }

    /// <summary>
    /// Current subprogram that is read
    /// </summary>
    public string SubProgramName
    {
      get
      {
        if (!m_programInitialized) {
          InitializeProgram ();
        }

        if (m_programInitialized) {
          return m_currentSubProgramName;
        }

        throw new Exception ("Program information cannot be initialized");
      }
    }

    /// <summary>
    /// Current block that is read in a program
    /// </summary>
    public int BlockNumber {
      get {
        if (!m_programInitialized) {
          InitializeProgram ();
        }

        if (m_programInitialized) {
          return m_currentProgramBlock;
        }

        throw new Exception("Program information cannot be initialized");
      }
    }
    
    /// <summary>
    /// Content of the current line that is executed in a program
    /// </summary>
    public string CurrentLine {
      get {
        if (!m_programInitialized) {
          InitializeProgram ();
        }

        if (m_programInitialized) {
          return m_currentProgramContent ?? "";
        }

        throw new Exception("Program information cannot be initialized");
      }
    }
    
    /// <summary>
    /// Get the cutter location
    /// Channel 0
    /// </summary>
    public Position CutterLocation {
      get {
        if (!m_cutterLocationInitialized) {
          InitializeCutterLocation (0);
        }

        if (!m_cutterLocationInitialized) {
          throw new Exception("Cutter location cannot be initialized");
        }

        // Find the position and return it
        if (!m_cutterLocation.ContainsKey("X") || !m_cutterLocation.ContainsKey("Y") ||
            !m_cutterLocation.ContainsKey("Z")) {
          throw new Exception("Heidenhein:CutterLocation - position not available for axis X, Y and Z");
        }

        var position = new Position(m_cutterLocation["X"], m_cutterLocation["Y"], m_cutterLocation["Z"]);
        
        // A, B, C
        if (m_cutterLocation.ContainsKey("A")) {
          position.A = m_cutterLocation["A"];
        }

        if (m_cutterLocation.ContainsKey("B")) {
          position.B = m_cutterLocation["B"];
        }

        if (m_cutterLocation.ContainsKey("C")) {
          position.C = m_cutterLocation["C"];
        }

        // U, V, W
        if (m_cutterLocation.ContainsKey("U")) {
          position.U = m_cutterLocation["U"];
        }

        if (m_cutterLocation.ContainsKey("V")) {
          position.V = m_cutterLocation["V"];
        }

        if (m_cutterLocation.ContainsKey("W")) {
          position.W = m_cutterLocation["W"];
        }

        return position;
      }
    }
    #endregion // Get methods
    
    #region Private methods
    void InitializeOverrides()
    {
      object var1 = (Int32)0;
      object var2 = (Int32)0;
      object var3 = (Int32)0;
      
      try {
        m_interface.GetOverrideInfo(ref var1, ref var2, ref var3);
      } catch (COMException ex) {
        if (ex.ErrorCode == -2147220894) {
          // Sometimes this error occurs...
          m_overridesInitialized = true;
          return;
        }
      }
      
      m_feedrateOverride = (Int32)var1;
      m_speedOverride = (Int32)var2;
      m_rapidTraverseOverride = (Int32)var3;
      
      m_overridesInitialized = true;
    }
    
    void InitializeProgram()
    {
      object selectedProgram = "";
      HeidenhainDNCLib.JHProgramPositionList positionList = m_interface.GetExecutionPoint(ref selectedProgram);
      
      m_currentProgramName = (string)selectedProgram;
      Logger.DebugFormat ("HeidenhainDNC.InitializeProgram selectedProgram={0} ------------------", m_currentProgramName);
      foreach (HeidenhainDNCLib.JHProgramPosition position in positionList) {
        Logger.DebugFormat ("HeidenhainDNC.InitializeProgram selectedProgram={0}, programName={1}", m_currentProgramName, position.programName);
        if (position.programName == m_currentProgramName) {
          m_currentProgramContent = position.blockContent;
          m_currentProgramBlock = position.blockNumber;
        }
        else {
          // subprogram
          m_currentSubProgramName = position.programName;
        }
        Marshal.ReleaseComObject(position);
      }
      Marshal.ReleaseComObject(positionList);
      
      m_programInitialized = true;
    }
    
    void InitializeCutterLocation(int channel)
    {
      HeidenhainDNCLib.IJHCutterLocationList locationList = m_interface.GetCutterLocation(channel);
      foreach (HeidenhainDNCLib.IJHCutterLocation location in locationList) {
        m_cutterLocation[location.bstrCoordinateName] = location.dPosition;
        Marshal.ReleaseComObject(location);
      }
      Marshal.ReleaseComObject(locationList);
      
      m_cutterLocationInitialized = true;
    }
    #endregion // Private methods
  }
}
