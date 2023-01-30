// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Exception thrown when values are not ready
  /// It doesn't generate an error log but an info
  /// </summary>
  class NotReadyException : Exception
  {
    #region Members
    readonly String m_message;
    #endregion Members

    #region Getters / Setters
    /// <summary>
    /// Message associated
    /// </summary>
    public override string Message
    {
      get
      {
        return m_message;
      }
    }
    #endregion Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="message"></param>
    public NotReadyException (string message)
    {
      m_message = message;
    }
    #endregion Constructors
  }
}
