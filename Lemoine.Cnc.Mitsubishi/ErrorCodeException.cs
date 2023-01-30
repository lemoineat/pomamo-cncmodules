// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of ErrorCodeException.
  /// </summary>
  public class ErrorCodeException : Exception
  {
    static ErrorCodes s_errorCodes = new ErrorCodes ();

    #region Getters / Setters
    /// <summary>
    /// Error number
    /// </summary>
    public UInt32 ErrorNumber { get; private set; }

    /// <summary>
    /// Function that triggered the exception
    /// </summary>
    public string FunctionName { get; private set; }

    /// <summary>
    /// Formatted message
    /// </summary>
    public override string Message
    {
      get
      {
        return String.Format ("Error code '0x{0}' received in function '{1}': {2} ({3})",
          ErrorNumber.ToString ("X"), FunctionName,
          s_errorCodes.GetCode (ErrorNumber),
          s_errorCodes.GetDescription (ErrorNumber));
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="errorNumber"></param>
    /// <param name="functionName"></param>
    public ErrorCodeException (UInt32 errorNumber, string functionName)
    {
      ErrorNumber = errorNumber;
      FunctionName = functionName;
    }

    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="errorNumber"></param>
    /// <param name="functionName"></param>
    public ErrorCodeException (int errorNumber, string functionName)
    {
      ErrorNumber = (UInt32)errorNumber;
      FunctionName = functionName;
    }
    #endregion // Constructors
  }
}
