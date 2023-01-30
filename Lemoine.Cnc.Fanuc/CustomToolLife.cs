// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.Cnc
{
  /// <summary>
  /// 
  /// </summary>
  public class CustomToolLife
  {
    /// <summary>
    /// Class gathering a series of variables to read
    /// </summary>
    public class ToolVariablesData
    {
      /// <summary>
      /// Number of the tool whose variables are specified
      /// </summary>
      public int ToolNumber { get; set; }

      /// <summary>
      /// Variable to read for the current value of a tool life
      /// </summary>
      public string Current { get; set; }

      /// <summary>
      /// Variable to read for the warning
      /// </summary>
      public string Warning { get; set; }

      /// <summary>
      /// Variable to read for the maximum value of a tool life
      /// </summary>
      public string Max { get; set; }

      /// <summary>
      /// ToString override
      /// </summary>
      /// <returns></returns>
      public override string ToString ()
      {
        return "[ToolNumber: " + ToolNumber + ", Current: " + Current + ", Warning: " + Warning + ", Max: " + Max + "]";
      }
    }

    #region Members
    readonly IList<ToolVariablesData> m_toolVariables = new List<ToolVariablesData> ();
    readonly ILog m_log;
    #endregion Members

    #region Getters / Setters
    /// <summary>
    /// Get all tool variables by tool number
    /// </summary>
    public IList<ToolVariablesData> ToolVariablesByToolNumber
    {
      get { return m_toolVariables; }
    }

    /// <summary>
    /// True if the warning is relative to the maximum
    /// False if this is an absolute value
    /// Default is false
    /// </summary>
    public bool IsWarningRelative { get; private set; }

    /// <summary>
    /// Tool life direction
    /// Default is unknown
    /// </summary>
    public Lemoine.Core.SharedData.ToolLifeDirection ToolLifeDirection { get; private set; }

    /// <summary>
    /// Tool unit
    /// Default is Unknown
    /// </summary>
    public Lemoine.Core.SharedData.ToolUnit ToolUnit { get; private set; }

    /// <summary>
    /// Multiplier used to translate data (for example from minutes to seconds)
    /// Default is 1
    /// </summary>
    public double Multiplier { get; private set; }
    #endregion Getters / Setters

    #region Constructor / Destructor
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="confFile">Configuration file for reading toollife</param>
    /// <param name="logger">Logger</param>
    public CustomToolLife (string confFile, ILog logger)
    {
      m_log = logger;

      // Default configuration
      IsWarningRelative = false;
      ToolLifeDirection = Lemoine.Core.SharedData.ToolLifeDirection.Unknown;
      ToolUnit = Lemoine.Core.SharedData.ToolUnit.Unknown;
      Multiplier = 1.0;

      // Get the content of the file
      var content = GetFileContent (confFile);

      // Parse the configuration file
      Parse (content);
    }
    #endregion Constructor / Destructor

    #region Methods
    void Parse (string fileContent)
    {
      var lines = fileContent.Split (new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
      int toolNumber;
      foreach (var line in lines) {
        // Skip empty lines and comments
        if (line.Length == 0 || line[0] == '#') {
          continue;
        }

        // Parameter?
        var parts = line.Split ('=');
        if (parts.Length == 2) {
          try {
            switch (parts[0]) {
            case "Unit":
              ToolUnit = (Lemoine.Core.SharedData.ToolUnit)Enum.Parse (typeof (Lemoine.Core.SharedData.ToolUnit), parts[1]);
              break;
            case "Multiplier":
              Multiplier = double.Parse (parts[1]);
              break;
            case "Direction":
              ToolLifeDirection = (Lemoine.Core.SharedData.ToolLifeDirection)Enum.Parse (typeof (Lemoine.Core.SharedData.ToolLifeDirection), parts[1]);
              break;
            case "WarningType":
              switch (parts[1]) {
              case "Absolute":
                IsWarningRelative = false;
                break;
              case "Relative":
                IsWarningRelative = true;
                break;
              default:
                throw new Exception ("Unknown value");
              }
              break;
            default:
              throw new Exception ("Unknown parameter");
            }
            m_log.InfoFormat ("Fanuc.CustomToolLife - Successfully parsed argument '{0}' for parameter '{1}'", parts[1], parts[0]);
          }
          catch (Exception e) {
            m_log.ErrorFormat ("Fanuc.CustomToolLife - Couldn't parse argument '{0}' for parameter '{1}': {2}", parts[1], parts[0], e.Message);
          }

          continue;
        }

        // Skip if the number of parts is not ok
        parts = line.Split (',');
        if (parts.Length == 4) {
          try {
            // New ToolVariablesData
            var tvd = new ToolVariablesData ();

            // Tool number must be an integer
            if (!int.TryParse (parts[0], out toolNumber)) {
              throw new Exception ("Tool number is not an integer");
            }

            tvd.ToolNumber = toolNumber;

            // Current value must not be empty
            if (string.IsNullOrEmpty (parts[1])) {
              throw new Exception ("Variable for the current value is not specified");
            }

            tvd.Current = parts[1];

            // Warning and maximum can be empty
            tvd.Warning = parts[2];
            tvd.Max = parts[3];

            m_log.InfoFormat ("Fanuc.CustomToolLife - Successfully parsed line '{0}'", line);
            m_toolVariables.Add (tvd);
          }
          catch (Exception e) {
            m_log.ErrorFormat ("Fanuc.CustomToolLife - Couldn't parse line '{0}': {1}", line, e.Message);
          }

          continue;
        }

        m_log.ErrorFormat ("Fanuc.CustomToolLife - Couldn't parse line '{0}'", line);
      }
    }

    string GetFileContent (string confFile)
    {
      string fileContent = "";

      // File present next to the dll?
      string filePath = Path.Combine (Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location), confFile);
      if (File.Exists (filePath)) {
        fileContent = File.ReadAllText (filePath);
      }
      else {
        throw new Exception ("Lemoine.Cnc.Fanuc - CsvCustomToolLife: couldn't find file '" + confFile + "'");
      }

      return fileContent;
    }
    #endregion Methods
  }
}
