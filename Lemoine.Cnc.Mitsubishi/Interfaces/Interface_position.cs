// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Interface_position.
  /// </summary>
  public class Interface_position : GenericMitsubishiInterface
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
    #endregion Protected methods

    #region Get methods
    /// <summary>
    /// Get the current position
    /// </summary>
    /// <returns></returns>
    public Position Position
    {
      get
      {
        var pos = new Position ();
        var errorNumber = 0;
        double value;

        // X
        if ((errorNumber = CommunicationObject.Position_GetCurrentPosition (1, out value)) != 0) {
          throw new ErrorCodeException (errorNumber, "GetPosition.1");
        }

        pos.X = value;

        // Y
        if ((errorNumber = CommunicationObject.Position_GetCurrentPosition (2, out value)) != 0) {
          throw new ErrorCodeException (errorNumber, "GetPosition.2");
        }

        pos.Y = value;

        // Z
        if ((errorNumber = CommunicationObject.Position_GetCurrentPosition (3, out value)) != 0) {
          throw new ErrorCodeException (errorNumber, "GetPosition.3");
        }

        pos.Z = value;

        return pos;
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
    public double GetFeedSpeed (int type)
    {
      double value = 0.0;
      var errorNumber = 0;
      if ((errorNumber = CommunicationObject.Position_GetFeedSpeed (type, out value)) != 0) {
        throw new ErrorCodeException (errorNumber, "GetFeedSpeed." + type);
      }

      return Math.Abs (value);
    }
    #endregion Get methods
  }
}
