// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Fanuc_controlled_axis_spindle.
  /// </summary>
  public partial class Fanuc
  {
    readonly Hashtable m_spindleSpeed = new Hashtable (); // Spindle # -> Spindle speed
    readonly Hashtable m_spindleLoad = new Hashtable (); // Spindle # -> Spindle load

    /// <summary>
    /// Rapid traverse rate
    /// </summary>
    public double RapidTraverseRate
    {
      get {
        // The rapid traverse rate to use is determined by:
        // - If bit 0 of parameter 8002 is 1: EIF0g to EIF15g
        // - Else the value of parameter 1420

        // I am not sure how to get the effective rapid traverse rate yet.
        // Let's suppose it is also returned by actf
        // So let's use this for the moment
        if (RapidTraverse) {
          double feedRate = this.Feedrate;
          log.DebugFormat ("RapidTraverseRate: " +
                           "RapidTraverse is on " +
                           "=> return feed rate {0}",
                           feedRate);
          return feedRate;
        }
        else {
          log.Debug ("RapidTraverseRate: " +
                     "RapidTraverse is off " +
                     "=> return an exception");
          throw new InvalidOperationException ("RapidTraverse off");
        }
      }
    }

    /// <summary>
    /// Get the axis name
    /// </summary>
    /// <param name="param">axis number: from 0 to 9</param>
    /// <returns></returns>
    public char GetAxisName (string param)
    {
      var axisNumber = short.Parse (param);
      if ((axisNumber > 9) || (axisNumber < 0)) {
        log.Error ($"GetAxisName: invalid axis number {axisNumber}, should be between 0 and 9");
        throw new ArgumentException ("Invalid axis number not between 0 and 9", "param");
      }
      short nbAxis = 10;
      Import.FwLib.EW result = (Import.FwLib.EW)Import.FwLib.Cnc.rdposition (
        m_handle, 0, ref nbAxis, out Import.FwLib.ODBPOS odbPositions);
      if (Import.FwLib.EW.OK != result) {
        log.Error ($"GetAxisName: rdposition failed with error {result}");
        ManageError ("Position", result);
        throw new Exception ("rdposition failed");
      }
      else {
        var axisName = Char.ToUpperInvariant (odbPositions.position[axisNumber].abs.name);
        if (log.IsDebugEnabled) {
          log.Debug ($"GetAxisName: {axisNumber} => {axisName}");
        }
        return axisName;
      }
    }

    /// <summary>
    /// Get the axis number
    /// </summary>
    /// <param name="param">axis name</param>
    /// <returns></returns>
    public short GetAxisNumber (string param)
    {
      if (param.Length != 1) {
        log.Error ($"GetAxisNumber: invalid axis name {param}, should be a unique character");
        throw new ArgumentException ("Invalid axis name, not a unique character", "param");
      }
      return GetAxisNumber (param[0]);
    }

    short GetAxisNumber (char axisName)
    {
      short nbAxis = 10;
      Import.FwLib.EW result = (Import.FwLib.EW)Import.FwLib.Cnc.rdposition (
        m_handle, 0, ref nbAxis, out Import.FwLib.ODBPOS odbPositions);
      if (Import.FwLib.EW.OK != result) {
        log.Error ($"GetAxisNumber: rdposition failed with error {result}");
        ManageError ("Position", result);
        throw new Exception ("rdposition failed");
      }
      else {
        for (short i = 0; i < nbAxis; i++) {
          var name = odbPositions.position[i].abs.name;
          if (char.ToUpperInvariant (axisName) == char.ToUpperInvariant (name)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetAxisNumber: {axisName} => {i}");
            }
            return i;
          }
        }
        log.Error ($"GetAxisNumber: {axisName} not found");
        throw new Exception ($"GetAxisNumber: axis name {axisName} not found");
      }
    }

    /// <summary>
    /// Position
    /// 
    /// The position is in metric units.
    /// The conversion is automatically made if needed.
    /// </summary>
    public Lemoine.Cnc.Position Position
    {
      get {
        if (false == IsConnectionValid ()) {
          log.Error ("Position.get: " +
                     "connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        Position position = new Position ();
        short nbAxis = 10;
        Import.FwLib.EW result = (Import.FwLib.EW)Import.FwLib.Cnc.rdposition (
          m_handle, 0, ref nbAxis, out Import.FwLib.ODBPOS odbPositions);
        if (Import.FwLib.EW.OK != result) {
          log.ErrorFormat ("Position.get: " +
                           "rdposition failed with error {0}",
                           result);
          ManageError ("Position", result);
          throw new Exception ("rdposition failed");
        }
        else {
          for (int i = 0; i < nbAxis; i++) {
            Import.FwLib.POSELM absPosition = odbPositions.position[i].abs;
            double v = absPosition.data * Math.Pow (10.0, -absPosition.dec);
            bool inches = (1 == absPosition.unit);
            switch (Char.ToUpperInvariant (absPosition.name)) {
            case 'X':
              position.X = Lemoine.Conversion.Converter.ConvertToMetric (v, !inches);
              break;
            case 'Y':
              position.Y = Lemoine.Conversion.Converter.ConvertToMetric (v, !inches);
              break;
            case 'Z':
              position.Z = Lemoine.Conversion.Converter.ConvertToMetric (v, !inches);
              break;
            case 'U':
              position.U = Lemoine.Conversion.Converter.ConvertToMetric (v, !inches);
              break;
            case 'V':
              position.V = Lemoine.Conversion.Converter.ConvertToMetric (v, !inches);
              break;
            case 'W':
              position.W = Lemoine.Conversion.Converter.ConvertToMetric (v, !inches);
              break;
            case 'A':
              position.A = v;
              break;
            case 'B':
              position.B = v;
              break;
            case 'C':
              position.C = v;
              break;
            }
          }
          position.Time = DateTime.UtcNow;
          log.DebugFormat ("Position.get: got {0}", position);
          return position;
        }
      }
    }

    /// <summary>
    /// Feedrate
    /// </summary>
    public double Feedrate
    {
      get {
        if (false == IsConnectionValid ()) {
          log.ErrorFormat ("Feedrate.get: connection to the CNC failed");
          throw new Exception ("No CNC connection");
        }

        Import.FwLib.EW result = (Import.FwLib.EW)Import.FwLib.Cnc.actf (m_handle, out Import.FwLib.ODBACT actualFeed);
        if (Import.FwLib.EW.OK != result) {
          log.ErrorFormat ("Feedrate.get: actf failed with error {0}",
                           result);
          ManageError ("Feedrate", result);
          throw new Exception ("actf failed");
        }
        else {
          log.DebugFormat ("Feedrate.get: actf was successful and returned {0}", actualFeed.data);
          return actualFeed.data;
        }
      }
    }

    /// <summary>
    /// Get the spindle speed and spindle load of a given spindle
    /// </summary>
    /// <param name="spindleNumber">Spindle number</param>
    void GetSpindleInformation (short spindleNumber)
    {
      if (m_spindleSpeed.ContainsKey (spindleNumber)
          && m_spindleLoad.ContainsKey (spindleNumber)) {
        log.InfoFormat ("GetSpindleInformation: " +
                        "the spindle information is already known for spindle {0}, " +
                        "SpindleLoad={1} SpindleSpeed={2}",
                        spindleNumber, m_spindleLoad, m_spindleSpeed);
        return;
      }

      if (false == IsConnectionValid ()) {
        log.ErrorFormat ("GetSpindleInformation: " +
                         "connection to the CNC failed " +
                         "for spindle {0}",
                         spindleNumber);
        throw new Exception ("No CNC connection");
      }

      Import.FwLib.ODBSPLOAD spindleInfo;
      short dataNum = spindleNumber;
      Import.FwLib.EW result = (Import.FwLib.EW)Import.FwLib.Cnc.rdspmeter (m_handle, -1, ref dataNum,
                                                                             out spindleInfo);
      if (Import.FwLib.EW.OK != result) {
        log.ErrorFormat ("GetSpindleInformation: rdspmeter failed with {0} for spindle {1}",
                         result, spindleNumber);
        ManageError ("GetSpindleInformation", result);
        throw new Exception ("rdspmeter failed");
      }
      double spindleLoadValue = spindleInfo.spload.data * Math.Pow (10.0, -spindleInfo.spload.dec);
      m_spindleLoad[spindleNumber] = spindleLoadValue;
      double spindleSpeedValue = spindleInfo.spspeed.data * Math.Pow (10.0, -spindleInfo.spspeed.dec);
      m_spindleSpeed[spindleNumber] = spindleSpeedValue;
      log.DebugFormat ("GetSpindleInformation: " +
                       "got speed={0} and load={1} for spindle {2}",
                       spindleSpeedValue, spindleLoadValue, spindleNumber);
    }

    /// <summary>
    /// Get the spindle load for a given spindle
    /// </summary>
    /// <param name="param">Spindle number. If empty 1st spindle is taken</param>
    public double GetSpindleLoad (string param)
    {
      short spindleNumber = 1;
      if ((null != param) && (param.Length > 0)) {
        if (!short.TryParse (param, out spindleNumber)) {
          spindleNumber = 1;
          log.WarnFormat ("GetSpindleLoad: " +
                          "param {0} could not be parsed to get the spindle number " +
                          "=> the first spindle is taken",
                          param);
        }
        else {
          log.DebugFormat ("GetSpindleLoad: " +
                           "try to get the spindle load for spindle {0}",
                           spindleNumber);
        }
      }
      GetSpindleInformation (spindleNumber);
      double spindleLoadValue = (double)m_spindleLoad[spindleNumber];
      log.DebugFormat ("GetSpindleLoad: " +
                       "got spindle load {0} for spindle number {1}",
                       spindleLoadValue,
                       spindleNumber);
      return spindleLoadValue;
    }

    /// <summary>
    /// Get the spindle speed for a given spindle
    /// </summary>
    /// <param name="param">Spindle number. If empty 1st spindle is taken</param>
    public double GetSpindleSpeed (string param)
    {
      short spindleNumber = 1;
      if ((null != param) && (param.Length > 0)) {
        if (!short.TryParse (param, out spindleNumber)) {
          spindleNumber = 1;
          log.WarnFormat ("GetSpindleSpeed: " +
                          "param {0} could not be parsed to get the spindle number " +
                          "=> the first spindle is taken",
                          param);
        }
        else {
          log.DebugFormat ("GetSpindleSpeed: try to get the spindle speed for spindle {0}",
                           spindleNumber);
        }
      }
      GetSpindleInformation (spindleNumber);
      double spindleSpeedValue = (double)m_spindleSpeed[spindleNumber];
      log.DebugFormat ("GetSpindleSpeed: got spindle speed {0} for spindle number {1}",
                       spindleSpeedValue, spindleNumber);
      return spindleSpeedValue;
    }
  }
}
