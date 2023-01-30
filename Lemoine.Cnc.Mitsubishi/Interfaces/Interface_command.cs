// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Interface_command.
  /// </summary>
  public class Interface_command : GenericMitsubishiInterface
  {
    /// <summary>
    /// Command type
    /// </summary>
    public enum CommandType
    {
      /// <summary>
      /// M programming 1 (Miscellaneous M value)
      /// </summary>
      M,

      /// <summary>
      /// S programming (Spindle speed function S value)
      /// </summary>
      S,

      /// <summary>
      /// T programming (Tool exchange T value)
      /// </summary>
      T,

      /// <summary>
      /// B programming (Second auxiliary function for indexing table position, etc.)
      /// </summary>
      B
    }

    #region Members
    readonly IDictionary<int, double> m_gcodeCommand = new Dictionary<int, double> ();
    #endregion Members

    #region Protected methods
    /// <summary>
    /// Work to do on "start", before new data will be read
    /// Already within a try catch
    /// </summary>
    protected override void ResetDataInstance ()
    {
      m_gcodeCommand.Clear ();
    }
    #endregion Protected methods

    #region Get methods
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
    public double GetGCodeCommand (int type)
    {
      return GetGCode (type);
    }

    /// <summary>
    /// True if the machine is using the rapid traverse movement
    /// </summary>
    public bool RapidTraverse
    {
      get
      {
        return (Math.Abs (GetGCode (1)) < 0.1);
      }
    }

    /// <summary>
    /// List of active commands per command type
    /// </summary>
    /// <param name="commandType">Can be M, S, T or B</param>
    /// <returns></returns>
    public IList<int> GetActiveCommands (CommandType commandType)
    {
      IList<int> values = new List<int> ();
      int numCommand = 0;
      int value = 0;
      while (numCommand < 8) // Upper limit
      {
        // Try to read a value
        int errorNumber = CommunicationObject.Command_GetCommand2 ((int)commandType, numCommand, out value);

        // EZNC_DATA_READ_SUBSECT  0x80040192  Invalid subsection number => we went too far
        if (errorNumber == Int32.Parse ("80040192", System.Globalization.NumberStyles.HexNumber)) {
          break;
        }
        else if (errorNumber != 0) {
          throw new ErrorCodeException (errorNumber, "Command_GetCommand2." + commandType + "." + numCommand);
        }
        else {
          values.Add (value);
        }

        numCommand++;
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
    public double GetFeedCommand (int type)
    {
      double value = 0.0;
      var errorNumber = 0;
      if ((errorNumber = CommunicationObject.Command_GetFeedCommand (type, out value)) != 0) {
        throw new ErrorCodeException (errorNumber, "GetFeedCommand." + type);
      }

      return Math.Abs (value);
    }
    #endregion Get methods

    #region Private methods
    double GetGCode (int type)
    {
      // Load the value if necessary
      if (!m_gcodeCommand.ContainsKey (type)) {
        double value = 0.0;
        var errorNumber = 0;
        if ((errorNumber = CommunicationObject.Command_GetGCodeCommand (type, out value)) != 0) {
          throw new ErrorCodeException (errorNumber, "Command_GetGCodeCommand." + type);
        }

        m_gcodeCommand[type] = value;
      }

      return m_gcodeCommand[type];
    }
    #endregion Private methods
  }

  /// <summary>
  /// Description of Interface_command_2.
  /// </summary>
  public class Interface_command_2 : Interface_command
  {

  }
}
