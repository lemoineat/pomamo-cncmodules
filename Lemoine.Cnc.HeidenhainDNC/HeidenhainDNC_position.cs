// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;

namespace Lemoine.Cnc
{
  /// <summary>
  /// HeidenhainDNC input module
  /// </summary>
  public partial class HeidenhainDNC
  {
    #region Get methods
    /// <summary>
    /// Get the position
    /// Channel 0
    /// </summary>
    public Position Position
    {
      get
      {
        Position position;

        try {
          IDictionary<int, double> positions = null;
          positions = m_interfaceManager.InterfacePosition.GetPositions ();

          // Find the position and return it
          var interfaceConfiguration = m_interfaceManager.InterfaceConfiguration;

          int index_X = interfaceConfiguration.GetAxisId ("X", 0);
          int index_Y = interfaceConfiguration.GetAxisId ("Y", 0);
          int index_Z = interfaceConfiguration.GetAxisId ("Z", 0);

          if (index_X == -1 || index_Y == -1 || index_Z == -1 ||
              !positions.ContainsKey (index_X) || !positions.ContainsKey (index_Y) ||
              !positions.ContainsKey (index_Z)) {
            throw new Exception ("Heidenhein::Position - position not available for axis 0, 1 and 2");
          }

          position = new Position (positions[index_X], positions[index_Y], positions[index_Z]);

          // A, B, C
          int index_A = interfaceConfiguration.GetAxisId ("A", 0);
          if (index_A != -1 && positions.ContainsKey (index_A)) {
            position.A = positions[index_A];
          }

          int index_B = interfaceConfiguration.GetAxisId ("B", 0);
          if (index_B != -1 && positions.ContainsKey (index_B)) {
            position.B = positions[index_B];
          }

          int index_C = interfaceConfiguration.GetAxisId ("C", 0);
          if (index_C != -1 && positions.ContainsKey (index_C)) {
            position.C = positions[index_C];
          }

          // U, V, W
          int index_U = interfaceConfiguration.GetAxisId ("U", 0);
          if (index_U != -1 && positions.ContainsKey (index_U)) {
            position.U = positions[index_U];
          }

          int index_V = interfaceConfiguration.GetAxisId ("V", 0);
          if (index_V != -1 && positions.ContainsKey (index_V)) {
            position.V = positions[index_V];
          }

          int index_W = interfaceConfiguration.GetAxisId ("W", 0);
          if (index_W != -1 && positions.ContainsKey (index_W)) {
            position.W = positions[index_W];
          }
        }
        catch (Exception ex) {
          ProcessException (ex, "HeidenhainDNC", "Position", true);
          throw;
        }

        return position;
      }
    }
    #endregion // Get methods
  }
}