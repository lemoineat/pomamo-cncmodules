// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Interface_program.
  /// </summary>
  public class Interface_program : GenericMitsubishiInterface
  {
    /// <summary>
    /// Kind of program, according to the file "EZNcErr.bas"
    /// </summary>
    public enum ProgramType
    {
      /// <summary>
      /// Main program
      /// </summary>
      EZNC_MAINPRG = 0,

      /// <summary>
      /// Sub program
      /// </summary>
      EZNC_SUBPRG = 1
    }

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
    /// Current block number
    /// </summary>
    /// <param name="programType"></param>
    /// <returns></returns>
    public int GetBlockNumber (ProgramType programType)
    {
      var errorNumber = 0;
      int value = 0;
      if ((errorNumber = CommunicationObject.Program_GetBlockNumber ((int)programType, out value)) != 0) {
        throw new ErrorCodeException (errorNumber, "Program_GetBlockNumber");
      }

      return value;
    }

    /// <summary>
    /// Current sequence number
    /// </summary>
    /// <param name="programType"></param>
    /// <returns></returns>
    public int GetSequenceNumber (ProgramType programType)
    {
      var errorNumber = 0;
      int value = 0;
      if ((errorNumber = CommunicationObject.Program_GetSequenceNumber ((int)programType, out value)) != 0) {
        throw new ErrorCodeException (errorNumber, "Program_GetSequenceNumber");
      }

      return value;
    }

    /// <summary>
    /// Current program name
    /// </summary>
    /// <param name="programType"></param>
    /// <returns></returns>
    public string GetProgramName (ProgramType programType)
    {
      var errorNumber = 0;
      string value = "";
      if ((errorNumber = CommunicationObject.Program_GetProgramNumber2 ((int)programType, out value)) != 0) {
        throw new ErrorCodeException (errorNumber, "Program_GetProgramNumber2");
      }

      return value;
    }
    #endregion // Get methods
  }

  /// <summary>
  /// Description of Interface_program_2.
  /// </summary>
  public class Interface_program_2 : Interface_program
  {

  }
}
