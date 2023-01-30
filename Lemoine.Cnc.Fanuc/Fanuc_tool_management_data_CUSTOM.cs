// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using Lemoine.Core.SharedData;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Fanuc_tool_management_data.
  /// </summary>
  public partial class Fanuc
  {
    CustomToolLife m_customToolLife = null;

    ToolLifeData GetToolLife_CUSTOM (string confFile)
    {
      if (string.IsNullOrEmpty (confFile)) {
        return new ToolLifeData ();
      }

      if (m_customToolLife == null) {
        log.InfoFormat ("Fanuc.GetToolLife_CUSTOM: create a csv parser with '{0}' for reading toollife", confFile);
        m_customToolLife = new CustomToolLife (confFile, log);
      }

      var tld = new ToolLifeData ();
      var tvds = m_customToolLife.ToolVariablesByToolNumber;
      foreach (var tvd in tvds) {
        int index = tld.AddTool ();
        tld[index].ToolId = tvd.ToolNumber.ToString ();
        tld[index].ToolNumber = tvd.ToolNumber.ToString ();
        int index2 = tld[index].AddLifeDescription ();

        // Tool life
        try {
          // Current
          tld[index][index2].LifeValue = this.GetMacro (tvd.Current) * m_customToolLife.Multiplier;
          tld[index][index2].LifeType = ToolUnit.TimeSeconds;

          // Direction & unit
          tld[index][index2].LifeDirection = m_customToolLife.ToolLifeDirection;
          tld[index][index2].LifeType = m_customToolLife.ToolUnit;

          // Limit
          tld[index][index2].LifeLimit = String.IsNullOrEmpty (tvd.Max) ? (double?)null : (this.GetMacro (tvd.Max) * m_customToolLife.Multiplier);

          // Warning
          if (!String.IsNullOrEmpty (tvd.Warning)) {
            if (m_customToolLife.IsWarningRelative || m_customToolLife.ToolLifeDirection == ToolLifeDirection.Down) {
              tld[index][index2].LifeWarningOffset = this.GetMacro (tvd.Warning) * m_customToolLife.Multiplier;
            }
            else if (tld[index][index2].LifeLimit != null) {
              tld[index][index2].LifeWarningOffset = tld[index][index2].LifeLimit - this.GetMacro (tvd.Warning) * m_customToolLife.Multiplier;
            }
          }
        }
        catch (Exception e) {
          log.ErrorFormat ("Fanuc.ToolLifeCustom - error when reading a custom tool life with {0}: {1}", tvd, e.Message);
        }
      }

      return tld;
    }
  }
}
