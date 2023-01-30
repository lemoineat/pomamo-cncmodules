// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using Lemoine.Core.SharedData;
using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Fanuc_tool_management_data.
  /// </summary>
  public partial class Fanuc
  {
    ToolLifeData GetToolLife_EMAG ()
    {
      var tld = new ToolLifeData ();

      // Read all tool life data
      var allVariables = new System.Collections.Generic.List<int> ();
      for (int i = 0; i < 12; i++) {
        int offset = 20 * i;
        allVariables.Add (11000 + offset); // Status
        allVariables.Add (11002 + offset); // T number
        allVariables.Add (11009 + offset); // Nominal
        allVariables.Add (11010 + offset); // Remainder
        allVariables.Add (11011 + offset); // Type
      }
      var allValues = GetPMacros (allVariables);

      for (int toolIndex = 0; toolIndex < 12; toolIndex++) {
        uint offset = 20 * (uint)toolIndex;
        double status = allValues[11000 + offset];
        int tNumber = Convert.ToInt32 (allValues[11002 + offset]);
        double nominal = allValues[11009 + offset];
        double remainder = allValues[11010 + offset];
        double type = allValues[11011 + offset];

        // Keep only status 1 (active). 2 is disabled, 3 is spare, 4 and 5 are reserved
        if (status < 0.9 || status > 1.1) {
          continue;
        }

        // New tool position
        tld.AddTool ();
        tld[toolIndex].PotNumber = tNumber;
        tld[toolIndex].ToolNumber = tNumber.ToString ();
        tld[toolIndex].ToolId = tNumber.ToString ();

        // Life of the tool depending on the type
        tld[toolIndex].AddLifeDescription ();
        tld[toolIndex][0].LifeDirection = ToolLifeDirection.Down;
        if (type > 0.9 && type < 1.1) {
          // Type 1 is number of times
          tld[toolIndex][0].LifeType = ToolUnit.NumberOfTimes;
          tld[toolIndex][0].LifeLimit = nominal;
          tld[toolIndex][0].LifeValue = remainder;
        }
        else if (type > 1.9 && type < 2.1) {
          // Type 2 is duration in minutes
          tld[toolIndex][0].LifeType = ToolUnit.TimeSeconds;
          tld[toolIndex][0].LifeLimit = nominal * 60;
          tld[toolIndex][0].LifeValue = remainder * 60;
        }
        else if (type > 2.9 && type < 3.1) {
          // Type 3 is distance in meters
          tld[toolIndex][0].LifeType = ToolUnit.DistanceMillimeters;
          tld[toolIndex][0].LifeLimit = nominal * 1000;
          tld[toolIndex][0].LifeValue = remainder * 1000;
        }
        else {
          log.WarnFormat ("GetToolLife_EMAG: unknown type {0}", type);
          tld[toolIndex][0].LifeType = ToolUnit.Unknown;
          tld[toolIndex][0].LifeLimit = nominal;
          tld[toolIndex][0].LifeValue = remainder;
        }
      }

      return tld;
    }
  }
}
