// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

namespace Lemoine.Cnc
{
  /// <summary>
  /// Okuma input module, tool life read / write
  /// </summary>
  public partial class OkumaThincApi
  {
    #region Getters
    /// <summary>
    /// Read tool life data
    /// </summary>
    public ToolLifeData GetToolLifeData (string param)
    {
      // Number of tools
      int toolNumber = Call<int> ("CTools", "GetMaxTools");

      var toolLifeData = new ToolLifeData ();
      for (int i = 0; i < toolNumber; i++) {
        int managementMode;
        try {
          managementMode = Call<int> ("CTools", "GetMode", i);
        }
        catch (System.Reflection.TargetInvocationException ex) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetToolLifeData: tool #{i}, GetMode returned an exception => skip it", ex.InnerException);
          }
          continue;
        }
        catch (System.Exception ex) {
          log.Error ($"GetToolLifeData: tool #{i} returned an exception, skip it (or throw ?)", ex);
          continue;
        }

        int toolIndex = toolLifeData.AddTool ();

        // Tool number and id
        toolLifeData[toolIndex].ToolNumber = toolLifeData[toolIndex].ToolId = i.ToString ();

        // Tool name
        toolLifeData[toolIndex].SetProperty ("name", Call<string> ("CTools", "GetToolName", i));

        // Group number
        toolLifeData[toolIndex].SetProperty ("group", Call<int> ("CTools", "GetGroupNo", i));

        // Pot number
        toolLifeData[toolIndex].PotNumber = Call<int> ("CTools", "GetPotNo", i);

        // Tool life
        int toolLifeIndex = toolLifeData[toolIndex].AddLifeDescription ();
        toolLifeData[toolIndex][toolLifeIndex].LifeDirection = Core.SharedData.ToolLifeDirection.Down;

        // Unit
        int multiplier = 1;
        switch (managementMode) {
        case 0: // CountMode
        case 1: // CountSpareMode
          toolLifeData[toolIndex][toolLifeIndex].LifeType = Core.SharedData.ToolUnit.Parts;
          break;
        case 2: // TimeMode
        case 3: // TimeSpareMode
          multiplier = 60; // Conversion minutes => seconds
          toolLifeData[toolIndex][toolLifeIndex].LifeType = Core.SharedData.ToolUnit.TimeSeconds;
          break;
        case 4: // NoMode
        default:
          toolLifeData[toolIndex][toolLifeIndex].LifeType = Core.SharedData.ToolUnit.Unknown;
          break;
        }

        // Maximum
        toolLifeData[toolIndex][toolLifeIndex].LifeLimit = multiplier * Call<int> ("CTools", "GetToolLife", i);

        // Remaining tool life
        toolLifeData[toolIndex][toolLifeIndex].LifeValue = multiplier * Call<int> ("CTools", "GetToolLifeRemaining", i);
      }

      return toolLifeData;
    }
    #endregion Other methods
  }
}
