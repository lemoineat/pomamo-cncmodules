// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of MML3_position.
  /// </summary>
  public class MML3_position
  {
    #region Getters / Setters
    /// <summary>
    /// Magazine number
    /// </summary>
    public UInt32 MagazineNumber { get; private set; }

    /// <summary>
    /// Pot number
    /// </summary>
    public Int32 PotNumber { get; private set; }

    /// <summary>
    /// Cutter number
    /// </summary>
    public UInt32 CutterNumber { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="magazineNumber"></param>
    /// <param name="potNumber"></param>
    /// <param name="cutterNumber"></param>
    public MML3_position (UInt32 magazineNumber, Int32 potNumber, UInt32 cutterNumber)
    {
      MagazineNumber = magazineNumber;
      PotNumber = potNumber;
      CutterNumber = cutterNumber;
    }
    #endregion // Constructors
  }
}
