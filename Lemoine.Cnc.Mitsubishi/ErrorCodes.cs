// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using Lemoine.Core.Log;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of ErrorCodes.
  /// </summary>
  public class ErrorCodes
  {
    #region Members
    readonly IDictionary<UInt32, string> m_descriptions = new Dictionary<UInt32, string> ();
    readonly IDictionary<UInt32, string> m_codes = new Dictionary<UInt32, string> ();
    ILog log = LogManager.GetLogger (typeof (ErrorCodes).FullName);
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ErrorCodes ()
    {
      // Load csv
      string content = "";
      using (Stream stream = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("Lemoine.Cnc.Mitsubishi.error_codes.csv")) {
        using (var reader = new StreamReader (stream)) {
          content = reader.ReadToEnd ();
        }
      }

      ParseContent (content);
    }
    #endregion // Constructors

    #region Methods
    void ParseContent (string content)
    {
      // Fill the dictionary
      var lines = content.Split (new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
      foreach (string line in lines) {
        // Parse the line
        var elements = line.Split (new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (elements.Length != 3) {
          log.WarnFormat ("Mitsubishi - bad entry for an error code: '{0}'", line);
          continue;
        }

        try {
          // Add the definition
          UInt32 errorNumber = UInt32.Parse (elements[1].Substring (2), System.Globalization.NumberStyles.HexNumber);
          m_codes[errorNumber] = elements[0];
          m_descriptions[errorNumber] = elements[2];
        }
        catch (Exception ex) {
          log.ErrorFormat ("Mitsubishi - couldn't parse the error code '{0}': {1}", line, ex);
        }
      }
    }

    /// <summary>
    /// Get the code associated to an error number
    /// </summary>
    /// <param name="errorNumber"></param>
    /// <returns></returns>
    public string GetCode (UInt32 errorNumber)
    {
      return m_codes.ContainsKey (errorNumber) ? m_codes[errorNumber] : "no code";
    }

    /// <summary>
    /// Get the description associated to an error number
    /// </summary>
    /// <param name="errorNumber"></param>
    /// <returns></returns>
    public string GetDescription (UInt32 errorNumber)
    {
      return m_descriptions.ContainsKey (errorNumber) ? m_descriptions[errorNumber] : "no description";
    }
    #endregion // Methods
  }
}
