// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Fanuc_others.
  /// </summary>
  public partial class Fanuc
  {
    /// <summary>
    /// Possible automatic / manual modes for Fanuc controls
    /// </summary>
    enum FanucAutoManualMode
    {
      NoSelect,
      Mdi,
      Tape,
      Memory,
      Edit,
      Teachin,
      Handle,
      Jog,
      AngularJog,
      JogHandle,
      TeachinJog,
      TeachinHandle,
      IncFeed,
      IncHandle,
      Reference,
      Remote,
      Test
    };

    /// <summary>
    /// Possible running status for Fanuc controls
    /// </summary>
    enum FanucRunningStatus
    {
      Stop,
      Hold,
      Start,
      Mstr,
      Restart,
      Prsr,
      Mnsrc,
      Restart2,
      Reset,
      Hpcc,
      NotReady,
      Ready
    };

    /// <summary>
    /// Possible motion modes for Fanuc controls
    /// </summary>
    enum FanucMotionStatus
    {
      Nothing,
      Motion,
      Dwell,
      Wait
    };

    Import.FwLib.ODBST? m_statInfo;        // status of Cnc
    Import.FwLib.ODBST_150? m_statInfo_15; // status of Cnc (if machine type 15)

    /// <summary>
    /// Emergency status
    /// </summary>
    public bool Emergency
    {
      get {
        int emergencyStatus = (CncKind.StartsWith ("15")) ? StatInfo_15.emergency : StatInfo.emergency;
        log.DebugFormat ("Emergency: " +
                         "got {0} for the emergency status",
                         emergencyStatus);
        return (1 == emergencyStatus);
      }
    }

    /// <summary>
    /// Automatic or Manual mode
    /// 
    /// One of the following strings: NoSelect, Mdi, Tape, Memory, Edit, Teachin,
    /// Handle, Jog, AngularJog, JogHandle, TeachinJog, TeachinHandle, IncFeed, IncHandle, Reference,
    /// Remote, Test
    /// </summary>
    public string AutoManualMode
    {
      get {
        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("AutoManualMode.get: " +
                           "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        if (CncKind.StartsWith ("15")) {
          log.Debug ("AutoManualMode.get: " +
                     "machine type 15");
          Import.FwLib.ODBST_150 statInfo = this.StatInfo_15;
          log.DebugFormat ("AutoManualMode.get: " +
                           "statinfo returned aut={0} manual={1}",
                           statInfo.aut, statInfo.manual);
          switch ((Import.FwLib.AUTO15)statInfo.aut) {
          case Import.FwLib.AUTO15.MDI:
            return FanucAutoManualMode.Mdi.ToString ();
          case Import.FwLib.AUTO15.TAPE:
            return FanucAutoManualMode.Tape.ToString ();
          case Import.FwLib.AUTO15.MEMORY:
            return FanucAutoManualMode.Memory.ToString ();
          case Import.FwLib.AUTO15.EDIT:
            return FanucAutoManualMode.Edit.ToString ();
          case Import.FwLib.AUTO15.TEACHIN:
            return FanucAutoManualMode.Teachin.ToString ();
          case Import.FwLib.AUTO15.NO_SELECT:
            break;
          }
          switch ((Import.FwLib.MANUAL15)statInfo.manual) {
          case Import.FwLib.MANUAL15.HANDLE:
            return FanucAutoManualMode.Handle.ToString ();
          case Import.FwLib.MANUAL15.JOG:
            return FanucAutoManualMode.Jog.ToString ();
          case Import.FwLib.MANUAL15.ANGULAR_JOG:
            return FanucAutoManualMode.AngularJog.ToString ();
          case Import.FwLib.MANUAL15.JOG_HANDLE:
            return FanucAutoManualMode.JogHandle.ToString ();
          case Import.FwLib.MANUAL15.INC_FEED:
            return FanucAutoManualMode.IncFeed.ToString ();
          case Import.FwLib.MANUAL15.INC_HANDLE:
            return FanucAutoManualMode.IncHandle.ToString ();
          case Import.FwLib.MANUAL15.REFERENCE:
            return FanucAutoManualMode.Reference.ToString ();
          case Import.FwLib.MANUAL15.NO_SELECT:
            break;
          }
          log.WarnFormat ("AutoManualMode.get: " +
                          "no automatic or manual mode aut={0} manual={1}",
                          statInfo.aut, statInfo.manual);
          return FanucAutoManualMode.NoSelect.ToString ();
        }
        else { // Not a machine type 15
          Import.FwLib.ODBST statInfo = this.StatInfo;
          log.DebugFormat ("AutoManualMode.get: " +
                           "statinfo returned aut={0}",
                           statInfo.aut);
          switch ((Import.FwLib.AUTO16)statInfo.aut) {
          case Import.FwLib.AUTO16.MDI:
            return FanucAutoManualMode.Mdi.ToString ();
          case Import.FwLib.AUTO16.MEMORY:
            return FanucAutoManualMode.Memory.ToString ();
          case Import.FwLib.AUTO16.NO_SELECT:
            return FanucAutoManualMode.NoSelect.ToString ();
          case Import.FwLib.AUTO16.EDIT:
            return FanucAutoManualMode.Edit.ToString ();
          case Import.FwLib.AUTO16.HANDLE:
            return FanucAutoManualMode.Handle.ToString ();
          case Import.FwLib.AUTO16.JOG:
            return FanucAutoManualMode.Jog.ToString ();
          case Import.FwLib.AUTO16.TEACHIN_JOG:
            return FanucAutoManualMode.TeachinJog.ToString ();
          case Import.FwLib.AUTO16.TEACHIN_HANDLE:
            return FanucAutoManualMode.TeachinHandle.ToString ();
          case Import.FwLib.AUTO16.INC_FEED:
            return FanucAutoManualMode.IncFeed.ToString ();
          case Import.FwLib.AUTO16.REFERENCE:
            return FanucAutoManualMode.Reference.ToString ();
          case Import.FwLib.AUTO16.REMOTE:
            return FanucAutoManualMode.Remote.ToString ();
          case Import.FwLib.AUTO16.TEST:
            return FanucAutoManualMode.Test.ToString ();
          default:
            log.ErrorFormat ("AutoManualMode.get: " +
                             "unknown AUTO16 mode {0}",
                             statInfo.aut);
            throw new Exception ("unknown AUTO16 mode");
          }
        }
      }
    }

    /// <summary>
    /// Running status
    /// 
    /// One of the following strings: Stop, Hold, Start, Mstr, Restart, Prsr, Mnsrc, Restart2, Reset, Hpcc, NotReady, Ready
    /// </summary>
    public string RunningStatus
    {
      get {
        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("RunningStatus.get: " +
                           "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        if (CncKind.StartsWith ("15")) {
          log.Debug ("RunningStatus.get: " +
                     "machine type 15");
          Import.FwLib.ODBST_150 statInfo = this.StatInfo_15;
          log.DebugFormat ("RunningStatus.get: " +
                           "statinfo returned run={0}",
                           statInfo.run);
          switch ((Import.FwLib.RUN15)statInfo.run) {
          case Import.FwLib.RUN15.STOP:
            return FanucRunningStatus.Stop.ToString ();
          case Import.FwLib.RUN15.HOLD:
            return FanucRunningStatus.Hold.ToString ();
          case Import.FwLib.RUN15.START:
            return FanucRunningStatus.Start.ToString ();
          case Import.FwLib.RUN15.MSTR:
            return FanucRunningStatus.Mstr.ToString ();
          case Import.FwLib.RUN15.RESTART:
            return FanucRunningStatus.Restart.ToString ();
          case Import.FwLib.RUN15.RESTART2:
            return FanucRunningStatus.Restart2.ToString ();
          case Import.FwLib.RUN15.RESET:
            return FanucRunningStatus.Reset.ToString ();
          case Import.FwLib.RUN15.HPCC:
            return FanucRunningStatus.Hpcc.ToString ();
          default:
            log.ErrorFormat ("RunningStatus.get: " +
                             "unknown RUN15 mode {0}",
                             statInfo.run);
            throw new Exception ("unknown RUN15 mode");
          }
        }
        else { // Not a machine type 15
          Import.FwLib.ODBST statInfo = this.StatInfo;
          log.DebugFormat ("RunningStatus.get: " +
                           "statinfo returned run={0}",
                           statInfo.run);
          if (CncKind.StartsWith ("16") && MTKind.Contains ("W")) { // Machine type 16W
            switch ((Import.FwLib.RUN16W)statInfo.run) {
            case Import.FwLib.RUN16W.NOT_READY:
              return FanucRunningStatus.NotReady.ToString ();
            case Import.FwLib.RUN16W.M_READY:
              return FanucRunningStatus.Ready.ToString ();
            case Import.FwLib.RUN16W.C_START:
              return FanucRunningStatus.Start.ToString ();
            case Import.FwLib.RUN16W.F_HOLD:
              return FanucRunningStatus.Hold.ToString ();
            case Import.FwLib.RUN16W.B_STOP:
              return FanucRunningStatus.Stop.ToString ();
            default:
              log.ErrorFormat ("RunningStatus.get: " +
                               "unknown RUN16W mode {0}",
                               statInfo.run);
              throw new Exception ("unknown RUN16W mode");
            }
          }
          else { // Machine type 16
            switch ((Import.FwLib.RUN16)statInfo.run) {
            case Import.FwLib.RUN16.RESET:
              return FanucRunningStatus.Reset.ToString ();
            case Import.FwLib.RUN16.STOP:
              return FanucRunningStatus.Stop.ToString ();
            case Import.FwLib.RUN16.HOLD:
              return FanucRunningStatus.Hold.ToString ();
            case Import.FwLib.RUN16.START:
              return FanucRunningStatus.Start.ToString ();
            case Import.FwLib.RUN16.MSTR:
              return FanucRunningStatus.Mstr.ToString ();
            default:
              log.ErrorFormat ("RunningStatus.get: " +
                               "unknown RUN16 mode {0}",
                               statInfo.run);
              throw new Exception ("unknown RUN16 mode");
            }
          }
        }
      }
    }

    /// <summary>
    /// Motion mode
    /// 
    /// One of the following strings: Nothing, Motion, Dwell, Wait
    /// </summary>
    public string MotionStatus
    {
      get {
        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("MotionStatus.get: " +
                           "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        if (CncKind.StartsWith ("15")) {
          log.Debug ("MotionStatus.get: " +
                     "machine type 15");
          Import.FwLib.ODBST_150 statInfo = this.StatInfo_15;
          log.DebugFormat ("MotionStatus.get: " +
                           "statinfo returned motion={0}",
                           statInfo.motion);
          switch ((Import.FwLib.MOTION15)statInfo.motion) {
          case Import.FwLib.MOTION15.NOTHING:
            return FanucMotionStatus.Nothing.ToString ();
          case Import.FwLib.MOTION15.MOTION:
            return FanucMotionStatus.Motion.ToString ();
          case Import.FwLib.MOTION15.DWELL:
            return FanucMotionStatus.Dwell.ToString ();
          case Import.FwLib.MOTION15.WAIT:
            return FanucMotionStatus.Wait.ToString ();
          default:
            log.ErrorFormat ("MotionStatus.get: " +
                             "unknown MOTION15 mode {0}",
                             statInfo.motion);
            throw new Exception ("unknown MOTION15 mode");
          }
        }
        else { // Not a machine type 15
          Import.FwLib.ODBST statInfo = this.StatInfo;
          log.DebugFormat ("MotionStatus.get: " +
                           "statinfo returned motion={0}",
                           statInfo.motion);
          switch ((Import.FwLib.MOTION16)statInfo.motion) {
          case Import.FwLib.MOTION16.NOTHING:
            return FanucMotionStatus.Nothing.ToString ();
          case Import.FwLib.MOTION16.MOTION:
            return FanucMotionStatus.Motion.ToString ();
          case Import.FwLib.MOTION16.DWELL:
            return FanucMotionStatus.Dwell.ToString ();
          default:
            log.ErrorFormat ("MotionStatus.get: " +
                             "unknown MOTION16 mode {0}",
                             statInfo.motion);
            throw new Exception ("unknown MOTION16 mode");
          }
        }
      }
    }

    #region Properties that are determined by the modes
    /// <summary>
    /// Hold status
    /// </summary>
    public bool Hold
    {
      get {
        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("Hold.get: " +
                           "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        if (CncKind.StartsWith ("15")) {
          log.Debug ("Hold.get: " +
                     "machine type 15");
          Import.FwLib.ODBST_150 statInfo = this.StatInfo_15;
          log.DebugFormat ("Hold.get: " +
                           "statinfo returned run={0}",
                           statInfo.run);
          switch ((Import.FwLib.RUN15)statInfo.run) {
          case Import.FwLib.RUN15.HOLD:
            return true;
          case Import.FwLib.RUN15.STOP:
          case Import.FwLib.RUN15.START:
          case Import.FwLib.RUN15.HPCC: // High precision contour control
          case Import.FwLib.RUN15.MSTR: // The tool is returning or being repositioned when the tool retract and return function is executed
          case Import.FwLib.RUN15.RESET:
          case Import.FwLib.RUN15.RESTART:
          case Import.FwLib.RUN15.RESTART2:
            return false;
          default:
            log.ErrorFormat ("Hold.get: " +
                             "unknown RUN15 mode {0}",
                             statInfo.run);
            throw new Exception ("unknown RUN15 mode");
          }
        }
        else { // Not a machine type 15
          Import.FwLib.ODBST statInfo = StatInfo;
          log.DebugFormat ("Hold.get: " +
                           "statinfo returned run={0}",
                           statInfo.run);
          if (CncKind.StartsWith ("16") && MTKind.Contains ("W")) { // Machine type 16W
            switch ((Import.FwLib.RUN16W)statInfo.run) {
            case Import.FwLib.RUN16W.F_HOLD:
              return true;
            case Import.FwLib.RUN16W.NOT_READY:
            case Import.FwLib.RUN16W.M_READY:
            case Import.FwLib.RUN16W.C_START:
            case Import.FwLib.RUN16W.B_STOP:
              return false;
            default:
              log.ErrorFormat ("Hold.get: " +
                               "unknown RUN16W mode {0}",
                               statInfo.run);
              throw new Exception ("unknown RUN16W mode");
            }
          }
          else { // Machine type 16
            switch ((Import.FwLib.RUN16)statInfo.run) {
            case Import.FwLib.RUN16.HOLD:
              return true;
            case Import.FwLib.RUN16.RESET:
            case Import.FwLib.RUN16.STOP:
            case Import.FwLib.RUN16.START:
            case Import.FwLib.RUN16.MSTR: // The tool is returning or being repositioned when the tool retract and return function is executed
              return false;
            default:
              log.ErrorFormat ("Hold.get: " +
                               "unknown RUN16 mode {0}",
                               statInfo.run);
              throw new Exception ("unknown RUN16 mode");
            }
          }
        }
      }
    }

    /// <summary>
    /// Idle axis: return true if the axis are idle
    /// </summary>
    public bool IdleAxis
    {
      get {
        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("IdleAxis.get: " +
                           "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        if (CncKind.StartsWith ("15")) {
          log.Debug ("IdleAxis.get: " +
                     "machine type 15");
          Import.FwLib.ODBST_150 statInfo = this.StatInfo_15;
          log.DebugFormat ("IdleAxis.get: " +
                           "statinfo returned motion={0}",
                           statInfo.motion);
          switch ((Import.FwLib.MOTION15)statInfo.motion) {
          case Import.FwLib.MOTION15.NOTHING:
          case Import.FwLib.MOTION15.DWELL:
          case Import.FwLib.MOTION15.WAIT:
            return true;
          case Import.FwLib.MOTION15.MOTION:
            return false;
          default:
            log.ErrorFormat ("IdleAxis.get: " +
                             "unknown MOTION15 mode {0}",
                             statInfo.motion);
            throw new Exception ("unknown MOTION15 mode");
          }
        }
        else { // Not a machine type 15
          Import.FwLib.ODBST statInfo = this.StatInfo;
          log.DebugFormat ("IdleAxis.get: " +
                           "statinfo returned motion={0}",
                           statInfo.motion);
          switch ((Import.FwLib.MOTION16)statInfo.motion) {
          case Import.FwLib.MOTION16.MOTION:
            return false;
          case Import.FwLib.MOTION16.NOTHING:
          case Import.FwLib.MOTION16.DWELL:
            return true;
          default:
            log.ErrorFormat ("IdleAxis.get: " +
                             "unknown MOTION16 mode {0}",
                             statInfo.motion);
            throw new Exception ("unknown MOTION16 mode");
          }
        }
      }
    }

    /// <summary>
    /// Machine lock (test mode) property (not for machine 15)
    /// </summary>
    public bool MachineLock
    {
      get {
        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("MachineLock.get: " +
                           "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        if (CncKind.StartsWith ("15")) {
          log.Debug ("MachineLock.get: " +
                     "machine type 15, Lock status is not available");
          throw new Exception ("Lock not available for machine type 15");
        }
        else { // Not a machine type 15
          Import.FwLib.ODBST statInfo = this.StatInfo;
          log.DebugFormat ("MachineLock.get: " +
                           "statinfo returned aut={0}",
                           statInfo.aut);
          switch ((Import.FwLib.AUTO16)statInfo.aut) {
          case Import.FwLib.AUTO16.TEST:
            return true;
          case Import.FwLib.AUTO16.MDI:
          case Import.FwLib.AUTO16.JOG:
          case Import.FwLib.AUTO16.TEACHIN_JOG:
          case Import.FwLib.AUTO16.HANDLE:
          case Import.FwLib.AUTO16.TEACHIN_HANDLE:
          case Import.FwLib.AUTO16.MEMORY:
          case Import.FwLib.AUTO16.NO_SELECT:
          case Import.FwLib.AUTO16.EDIT:
          case Import.FwLib.AUTO16.INC_FEED:
          case Import.FwLib.AUTO16.REFERENCE:
          case Import.FwLib.AUTO16.REMOTE:
            return false;
          default:
            log.ErrorFormat ("MachineLock.get: " +
                             "unknown AUTO16 mode {0}",
                             statInfo.aut);
            throw new Exception ("unknown AUTO16 mode");
          }
        }
      }
    }

    /// <summary>
    /// MDI property
    /// </summary>
    public bool MDI
    {
      get {
        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("MDI.get: " +
                           "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        if (CncKind.StartsWith ("15")) {
          log.Debug ("MDI.get: " +
                     "machine type 15");
          Import.FwLib.ODBST_150 statInfo = this.StatInfo_15;
          log.DebugFormat ("MDI.get: " +
                           "statinfo returned aut={0} manual={1}",
                           statInfo.aut, statInfo.manual);
          switch ((Import.FwLib.AUTO15)statInfo.aut) {
          case Import.FwLib.AUTO15.MDI:
            return true;
          case Import.FwLib.AUTO15.TAPE:
          case Import.FwLib.AUTO15.MEMORY:
          case Import.FwLib.AUTO15.EDIT:
          case Import.FwLib.AUTO15.TEACHIN:
          case Import.FwLib.AUTO15.NO_SELECT:
            return false;
          default:
            log.ErrorFormat ("MDI.get: " +
                             "unknown AUTO15 mode {0}",
                             statInfo.manual);
            throw new Exception ("unknown MANUAL15 mode");
          }
        }
        else { // Not a machine type 15
          Import.FwLib.ODBST statInfo = this.StatInfo;
          log.DebugFormat ("MDI.get: " +
                           "statinfo returned aut={0}",
                           statInfo.aut);
          switch ((Import.FwLib.AUTO16)statInfo.aut) {
          case Import.FwLib.AUTO16.MDI:
            return true;
          case Import.FwLib.AUTO16.JOG:
          case Import.FwLib.AUTO16.TEACHIN_JOG:
          case Import.FwLib.AUTO16.HANDLE:
          case Import.FwLib.AUTO16.TEACHIN_HANDLE:
          case Import.FwLib.AUTO16.MEMORY:
          case Import.FwLib.AUTO16.NO_SELECT:
          case Import.FwLib.AUTO16.EDIT:
          case Import.FwLib.AUTO16.INC_FEED:
          case Import.FwLib.AUTO16.REFERENCE:
          case Import.FwLib.AUTO16.REMOTE:
          case Import.FwLib.AUTO16.TEST:
            return false;
          default:
            log.ErrorFormat ("MDI.get: " +
                             "unknown AUTO16 mode {0}",
                             statInfo.aut);
            throw new Exception ("unknown AUTO16 mode");
          }
        }
      }
    }

    /// <summary>
    /// Jog property
    /// </summary>
    public bool Jog
    {
      get {
        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("Jog.get: " +
                           "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        if (CncKind.StartsWith ("15")) {
          log.Debug ("Handle.get: " +
                     "machine type 15");
          Import.FwLib.ODBST_150 statInfo = this.StatInfo_15;
          log.DebugFormat ("Jog.get: " +
                           "statinfo returned aut={0} manual={1}",
                           statInfo.aut, statInfo.manual);
          switch ((Import.FwLib.MANUAL15)statInfo.manual) {
          case Import.FwLib.MANUAL15.JOG:
          case Import.FwLib.MANUAL15.ANGULAR_JOG:
          case Import.FwLib.MANUAL15.JOG_HANDLE:
            return true;
          case Import.FwLib.MANUAL15.HANDLE:
          case Import.FwLib.MANUAL15.INC_HANDLE:
          case Import.FwLib.MANUAL15.INC_FEED:
          case Import.FwLib.MANUAL15.REFERENCE:
          case Import.FwLib.MANUAL15.NO_SELECT:
            return false;
          default:
            log.ErrorFormat ("Jog.get: " +
                             "unknown MANUAL15 mode {0}",
                             statInfo.manual);
            throw new Exception ("unknown MANUAL15 mode");
          }
        }
        else { // Not a machine type 15
          Import.FwLib.ODBST statInfo = this.StatInfo;
          log.DebugFormat ("Jog.get: " +
                           "statinfo returned aut={0}",
                           statInfo.aut);
          switch ((Import.FwLib.AUTO16)statInfo.aut) {
          case Import.FwLib.AUTO16.JOG:
          case Import.FwLib.AUTO16.TEACHIN_JOG:
            return true;
          case Import.FwLib.AUTO16.HANDLE:
          case Import.FwLib.AUTO16.TEACHIN_HANDLE:
          case Import.FwLib.AUTO16.MDI:
          case Import.FwLib.AUTO16.MEMORY:
          case Import.FwLib.AUTO16.NO_SELECT:
          case Import.FwLib.AUTO16.EDIT:
          case Import.FwLib.AUTO16.INC_FEED:
          case Import.FwLib.AUTO16.REFERENCE:
          case Import.FwLib.AUTO16.REMOTE:
          case Import.FwLib.AUTO16.TEST:
            return false;
          default:
            log.ErrorFormat ("Jog.get: " +
                             "unknown AUTO16 mode {0}",
                             statInfo.aut);
            throw new Exception ("unknown AUTO16 mode");
          }
        }
      }
    }

    /// <summary>
    /// Handle/Handwheel property
    /// </summary>
    public bool Handle
    {
      get {
        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("Handle.get: " +
                           "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        if (CncKind.StartsWith ("15")) {
          log.Debug ("Handle.get: " +
                     "machine type 15");
          Import.FwLib.ODBST_150 statInfo = this.StatInfo_15;
          log.DebugFormat ("Handle.get: " +
                           "statinfo returned aut={0} manual={1}",
                           statInfo.aut, statInfo.manual);
          switch ((Import.FwLib.MANUAL15)statInfo.manual) {
          case Import.FwLib.MANUAL15.HANDLE:
          case Import.FwLib.MANUAL15.INC_HANDLE:
          case Import.FwLib.MANUAL15.JOG_HANDLE:
            return true;
          case Import.FwLib.MANUAL15.JOG:
          case Import.FwLib.MANUAL15.ANGULAR_JOG:
          case Import.FwLib.MANUAL15.INC_FEED:
          case Import.FwLib.MANUAL15.REFERENCE:
          case Import.FwLib.MANUAL15.NO_SELECT:
            return false;
          default:
            log.ErrorFormat ("Handle.get: " +
                             "unknown MANUAL15 mode {0}",
                             statInfo.manual);
            throw new Exception ("unknown MANUAL15 mode");
          }
        }
        else { // Not a machine type 15
          Import.FwLib.ODBST statInfo = this.StatInfo;
          log.DebugFormat ("Handle.get: " +
                           "statinfo returned aut={0}",
                           statInfo.aut);
          switch ((Import.FwLib.AUTO16)statInfo.aut) {
          case Import.FwLib.AUTO16.HANDLE:
          case Import.FwLib.AUTO16.TEACHIN_HANDLE:
            return true;
          case Import.FwLib.AUTO16.MDI:
          case Import.FwLib.AUTO16.MEMORY:
          case Import.FwLib.AUTO16.NO_SELECT:
          case Import.FwLib.AUTO16.EDIT:
          case Import.FwLib.AUTO16.JOG:
          case Import.FwLib.AUTO16.TEACHIN_JOG:
          case Import.FwLib.AUTO16.INC_FEED:
          case Import.FwLib.AUTO16.REFERENCE:
          case Import.FwLib.AUTO16.REMOTE:
          case Import.FwLib.AUTO16.TEST:
            return false;
          default:
            log.ErrorFormat ("Handle.get: " +
                             "unknown AUTO16 mode {0}",
                             statInfo.aut);
            throw new Exception ("unknown AUTO16 mode");
          }
        }
      }
    }

    /// <summary>
    /// Refence (return to reference) property
    /// </summary>
    public bool Reference
    {
      get {
        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("Reference.get: " +
                           "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        if (CncKind.StartsWith ("15")) {
          log.Debug ("Reference.get: " +
                     "machine type 15");
          Import.FwLib.ODBST_150 statInfo = this.StatInfo_15;
          log.DebugFormat ("Reference.get: " +
                           "statinfo returned aut={0} manual={1}",
                           statInfo.aut, statInfo.manual);
          switch ((Import.FwLib.MANUAL15)statInfo.manual) {
          case Import.FwLib.MANUAL15.REFERENCE:
            return true;
          case Import.FwLib.MANUAL15.HANDLE:
          case Import.FwLib.MANUAL15.INC_HANDLE:
          case Import.FwLib.MANUAL15.JOG_HANDLE:
          case Import.FwLib.MANUAL15.JOG:
          case Import.FwLib.MANUAL15.ANGULAR_JOG:
          case Import.FwLib.MANUAL15.INC_FEED:
          case Import.FwLib.MANUAL15.NO_SELECT:
            return false;
          default:
            log.ErrorFormat ("Reference.get: " +
                             "unknown MANUAL15 mode {0}",
                             statInfo.manual);
            throw new Exception ("unknown MANUAL15 mode");
          }
        }
        else { // Not a machine type 15
          Import.FwLib.ODBST statInfo = this.StatInfo;
          log.DebugFormat ("Reference.get: " +
                           "statinfo returned aut={0}",
                           statInfo.aut);
          switch ((Import.FwLib.AUTO16)statInfo.aut) {
          case Import.FwLib.AUTO16.REFERENCE:
            return true;
          case Import.FwLib.AUTO16.HANDLE:
          case Import.FwLib.AUTO16.TEACHIN_HANDLE:
          case Import.FwLib.AUTO16.MDI:
          case Import.FwLib.AUTO16.MEMORY:
          case Import.FwLib.AUTO16.NO_SELECT:
          case Import.FwLib.AUTO16.EDIT:
          case Import.FwLib.AUTO16.JOG:
          case Import.FwLib.AUTO16.TEACHIN_JOG:
          case Import.FwLib.AUTO16.INC_FEED:
          case Import.FwLib.AUTO16.REMOTE:
          case Import.FwLib.AUTO16.TEST:
            return false;
          default:
            log.ErrorFormat ("Reference.get: " +
                             "unknown AUTO16 mode {0}",
                             statInfo.aut);
            throw new Exception ("unknown AUTO16 mode");
          }
        }
      }
    }

    /// <summary>
    /// Ready status
    /// </summary>
    public bool Ready
    {
      get {
        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("Ready.get: " +
                           "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        if (CncKind.StartsWith ("16") && MTKind.Contains ("W")) { // Machine type 16W
          Import.FwLib.ODBST statInfo = this.StatInfo;
          log.DebugFormat ("Ready.get: " +
                           "statinfo returned run={0}",
                           statInfo.run);
          switch ((Import.FwLib.RUN16W)statInfo.run) {
          case Import.FwLib.RUN16W.M_READY:
            return true;
          case Import.FwLib.RUN16W.NOT_READY:
          case Import.FwLib.RUN16W.C_START:
          case Import.FwLib.RUN16W.F_HOLD:
          case Import.FwLib.RUN16W.B_STOP:
            return false;
          default:
            log.ErrorFormat ("Ready.get: " +
                             "unknown RUN16W mode {0}",
                             statInfo.run);
            throw new Exception ("unknown RUN16W mode");
          }
        }
        else { // Not machine type 16W
          log.DebugFormat ("Ready.get: " +
                           "ready status is only available for machine 16W");
          throw new Exception ("Ready is only available for machine 16W");
        }
      }
    }

    /// <summary>
    /// NotReady status (only for 16w)
    /// </summary>
    public bool NotReady
    {
      get {
        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("NotReady.get: " +
                           "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        if (CncKind.StartsWith ("16") && MTKind.Contains ("W")) { // Machine type 16W
          Import.FwLib.ODBST statInfo = this.StatInfo;
          log.DebugFormat ("NotReady.get: " +
                           "statinfo returned run={0}",
                           statInfo.run);
          switch ((Import.FwLib.RUN16W)statInfo.run) {
          case Import.FwLib.RUN16W.NOT_READY:
            return true;
          case Import.FwLib.RUN16W.M_READY:
          case Import.FwLib.RUN16W.C_START:
          case Import.FwLib.RUN16W.F_HOLD:
          case Import.FwLib.RUN16W.B_STOP:
            return false;
          default:
            log.ErrorFormat ("NotReady.get: " +
                             "unknown RUN16W mode {0}",
                             statInfo.run);
            throw new Exception ("unknown RUN16W mode");
          }
        }
        else { // Not machine type 16W
          log.DebugFormat ("NotReady.get: " +
                           "ready status is only available for machine 16W");
          throw new Exception ("NotReady is only available for machine 16W");
        }
      }
    }

    /// <summary>
    /// The program execution was stopped (not for 16W)
    /// </summary>
    public bool Stopped
    {
      get {
        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("Stopped.get: " +
                           "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        if (CncKind.StartsWith ("15")) {
          log.Debug ("Stopped.get: " +
                     "machine type 15");
          Import.FwLib.ODBST_150 statInfo = this.StatInfo_15;
          log.DebugFormat ("RunningStatus.get: " +
                           "statinfo returned run={0}",
                           statInfo.run);
          switch ((Import.FwLib.RUN15)statInfo.run) {
          case Import.FwLib.RUN15.STOP:
            return true;
          case Import.FwLib.RUN15.HOLD:
          case Import.FwLib.RUN15.START:
          case Import.FwLib.RUN15.MSTR:
          case Import.FwLib.RUN15.RESTART:
          case Import.FwLib.RUN15.RESTART2:
          case Import.FwLib.RUN15.RESET:
          case Import.FwLib.RUN15.HPCC:
            return false;
          default:
            log.ErrorFormat ("Stopped.get: " +
                             "unknown RUN15 mode {0}",
                             statInfo.run);
            throw new Exception ("unknown RUN15 mode");
          }
        }
        else { // Not a machine type 15
          Import.FwLib.ODBST statInfo = this.StatInfo;
          log.DebugFormat ("Stopped.get: " +
                           "statinfo returned run={0}",
                           statInfo.run);
          if (CncKind.StartsWith ("16") && MTKind.Contains ("W")) { // Machine type 16W
            log.DebugFormat ("Stopped.get: " +
                             "machine type 16W not supported",
                             statInfo.run);
            throw new Exception ("machine 16W not supported");
          }
          else { // Machine type 16
            switch ((Import.FwLib.RUN16)statInfo.run) {
            case Import.FwLib.RUN16.STOP:
              return true;
            case Import.FwLib.RUN16.RESET:
            case Import.FwLib.RUN16.HOLD:
            case Import.FwLib.RUN16.START:
            case Import.FwLib.RUN16.MSTR:
              return false;
            default:
              log.ErrorFormat ("Stopped.get: " +
                               "unknown RUN16 mode {0}",
                               statInfo.run);
              throw new Exception ("unknown RUN16 mode");
            }
          }
        }
      }
    }

    /// <summary>
    /// The program execution was reset (not for 16W)
    /// </summary>
    public bool Reset
    {
      get {
        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("Reset.get: " +
                           "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        if (CncKind.StartsWith ("15")) {
          log.Debug ("Reset.get: " +
                     "machine type 15");
          Import.FwLib.ODBST_150 statInfo = this.StatInfo_15;
          log.DebugFormat ("Reset.get: " +
                           "statinfo returned run={0}",
                           statInfo.run);
          switch ((Import.FwLib.RUN15)statInfo.run) {
          case Import.FwLib.RUN15.RESET:
            return true;
          case Import.FwLib.RUN15.STOP:
          case Import.FwLib.RUN15.HOLD:
          case Import.FwLib.RUN15.START:
          case Import.FwLib.RUN15.MSTR:
          case Import.FwLib.RUN15.RESTART:
          case Import.FwLib.RUN15.RESTART2:
          case Import.FwLib.RUN15.HPCC:
            return false;
          default:
            log.ErrorFormat ("Reset.get: " +
                             "unknown RUN15 mode {0}",
                             statInfo.run);
            throw new Exception ("unknown RUN15 mode");
          }
        }
        else { // Not a machine type 15
          Import.FwLib.ODBST statInfo = this.StatInfo;
          log.DebugFormat ("Reset.get: " +
                           "statinfo returned run={0}",
                           statInfo.run);
          if (CncKind.StartsWith ("16") && MTKind.Contains ("W")) { // Machine type 16W
            log.DebugFormat ("Reset.get: " +
                             "machine type 16W not supported",
                             statInfo.run);
            throw new Exception ("machine 16W not supported");
          }
          else { // Machine type 16
            switch ((Import.FwLib.RUN16)statInfo.run) {
            case Import.FwLib.RUN16.RESET:
              return true;
            case Import.FwLib.RUN16.STOP:
            case Import.FwLib.RUN16.HOLD:
            case Import.FwLib.RUN16.START:
            case Import.FwLib.RUN16.MSTR:
              return false;
            default:
              log.ErrorFormat ("Reset.get: " +
                               "unknown RUN16 mode {0}",
                               statInfo.run);
              throw new Exception ("unknown RUN16 mode");
            }
          }
        }
      }
    }
    #endregion // Properties that are determined by the modes

    /// <summary>
    /// Status information (if machine type is not 15)
    /// </summary>
    private Import.FwLib.ODBST StatInfo
    {
      get {
        Import.FwLib.EW result;

        if (m_statInfo.HasValue) {
          return m_statInfo.Value;
        }

        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("StatInfo.get: " +
                           "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        result = (Import.FwLib.EW)Import.FwLib.Cnc.statinfo (m_handle, out var statInfo);
        if (Import.FwLib.EW.OK != result) {
          log.ErrorFormat ("StatInfo.get: " +
                           "statinfo failed with error {0}",
                           result);
          ManageError ("StatInfo", result);
          throw new Exception ("statinfo failed");
        }
        else {
          m_statInfo = statInfo;
          return m_statInfo.Value;
        }
      }
    }

    /// <summary>
    /// Status information (if machine type 15)
    /// </summary>
    private Import.FwLib.ODBST_150 StatInfo_15
    {
      get {
        Import.FwLib.EW result;

        if (m_statInfo_15.HasValue) {
          return m_statInfo_15.Value;
        }

        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("StatInfo_15.get: " +
                           "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        result = (Import.FwLib.EW)Import.FwLib.Cnc.statinfo_150 (m_handle, out var statInfo);
        if (Import.FwLib.EW.OK != result) {
          log.ErrorFormat ("AutoManualMode.get: " +
                           "statinfo failed with error {0}",
                           result);
          ManageError ("StatInfo_15", result);
          throw new Exception ("statinfo failed");
        }
        else {
          m_statInfo_15 = statInfo;
          return m_statInfo_15.Value;
        }
      }
    }
  }
}
