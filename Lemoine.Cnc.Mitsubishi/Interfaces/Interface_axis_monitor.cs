// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Interface_axis_monitor.
  /// </summary>
  public class Interface_axis_monitor : GenericMitsubishiInterface
  {
    #region Protected methods
    /// <summary>
    /// Work to do on "start", before new data will be read
    /// Already within a try catch
    /// </summary>
    protected override void ResetDataInstance ()
    {
      // Nothing for now
    }
    #endregion // Protected methods

    #region Get methods
    /// <summary>
    /// Spindle monitoring
    /// </summary>
    /// <param name="type">Type can be:
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
    /// <param name="spindleNumber">Number of the spindle</param>
    /// <returns></returns>
    public int GetSpindleMonitor (int type, int spindleNumber)
    {
      int value1 = 0;
      string value2 = "";
      int errorNumber = 0;
      if ((errorNumber = CommunicationObject.Monitor_GetSpindleMonitor (type, spindleNumber, out value1, out value2)) != 0) {
        throw new ErrorCodeException (errorNumber, "Monitor_GetSpindleMonitor." + type + "." + spindleNumber);
      }

      return Math.Abs (value1);
    }

    /// <summary>
    /// Servo monitoring
    /// </summary>
    /// <param name="type">Type can be:
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
    /// <param name="axisNumber"></param>
    /// <returns></returns>
    public int GetServoMonitor (int type, int axisNumber)
    {
      int value1 = 0;
      string value2 = "";
      int errorNumber = 0;
      if ((errorNumber = CommunicationObject.Monitor_GetServoMonitor (type, axisNumber, out value1, out value2)) != 0) {
        throw new ErrorCodeException (errorNumber, "Monitor_GetServoMonitor." + type + "." + axisNumber);
      }

      return value1;
    }
    #endregion Get methods
  }
}
