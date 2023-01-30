// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Interface_local_variable.
  /// </summary>
  public class Interface_local_variable : GenericMitsubishiInterface
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
    /// Read a local variable
    /// </summary>
    /// <param name="variableNumber">from 1 to 33</param>
    /// <param name="level">macro subprogram execution level from 0 to 4</param>
    /// <returns></returns>
    public double LocalVRead (int variableNumber, int level)
    {
      double value = 0;
      int type = 0;
      int errorCode = 0;
      if ((errorCode = CommunicationObject.LocalVariable_Read2 (variableNumber, level, out value, out type)) != 0) {
        throw new ErrorCodeException (errorCode, "LocalVariable_Read2." + variableNumber + "." + level);
      }

      if (SystemType == Mitsubishi.MitsubishiSystemType.MELDAS_600M_6X5M ||
          SystemType == Mitsubishi.MitsubishiSystemType.MELDAS_700L ||
          SystemType == Mitsubishi.MitsubishiSystemType.MELDAS_700M ||
          SystemType == Mitsubishi.MitsubishiSystemType.MELDAS_800L ||
          SystemType == Mitsubishi.MitsubishiSystemType.MELDAS_800M) {
        // A check is made on the type
        if (type == 0) {
          throw new Exception ("Mitsubishi.LocalVRead - variable '" + variableNumber + "' with level '" + level + "' not defined");
        }
      }

      return value;
    }
    #endregion Get methods
  }

  /// <summary>
  /// Description of Interface_local_variable_2.
  /// </summary>
  public class Interface_local_variable_2 : Interface_local_variable
  {

  }
}
