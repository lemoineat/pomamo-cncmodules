// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Interface_common_variable.
  /// </summary>
  public class Interface_common_variable : GenericMitsubishiInterface
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
    /// Read a common variable
    /// </summary>
    /// <param name="variableNumber">100 to 199, 500 to 999 and additionally for M700/M800 series:
    /// 400 to 999, 100100 to 100199, 200100 to 200199, 300100 to 300199, 400100 to 400199, 500100 to 500199,
    /// 600100 to 600199, 700100 to 700199, 800100 to 800199, 900000 to 907399</param>
    /// <returns></returns>
    public double CommonVRead (int variableNumber)
    {
      double v = 0.0;
      int type = 0;
      int errorNumber;
      errorNumber = CommunicationObject.SetHead (1); // Default part system number
      if (0 != errorNumber) {
        throw new ErrorCodeException (errorNumber, "SetHead");
      }
      errorNumber = CommunicationObject.CommonVariable_Read2 (variableNumber, out v, out type);
      if (0 != errorNumber) {
        throw new ErrorCodeException (errorNumber, "CommonVariable_Read2." + variableNumber);
      }
      if (SystemType == Mitsubishi.MitsubishiSystemType.MELDAS_600M_6X5M ||
          SystemType == Mitsubishi.MitsubishiSystemType.MELDAS_700L ||
          SystemType == Mitsubishi.MitsubishiSystemType.MELDAS_700M ||
          SystemType == Mitsubishi.MitsubishiSystemType.MELDAS_800L ||
          SystemType == Mitsubishi.MitsubishiSystemType.MELDAS_800M) {
        // A check is made on the type
        switch (type) {
          case 0: // Not set
            Logger.ErrorFormat ("CommonVRead: variable {0} is not set", variableNumber);
            throw new ArgumentException ("not set", "variableNumber");
          case 1: // Numerical value
            Logger.DebugFormat ("CommonVRead:got #{0}={1}", variableNumber, v);
            break;
          default: // Unexpected value
            Logger.FatalFormat ("CommonVRead: unexpected type {0}", type);
            throw new Exception ("Unexpected returned type in CommonVariable_Read2");
        }
      }

      return v;
    }
    #endregion Get methods
  }

  /// <summary>
  /// Description of Interface_common_variable_2.
  /// </summary>
  public class Interface_common_variable_2 : Interface_common_variable
  {

  }
}
