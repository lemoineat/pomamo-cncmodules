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
    ToolLifeData GetToolLife_MURATA (int firstAddress, int toolGroup)
    {
      var tld = new ToolLifeData ();
      const int maxToolNumber = 15;

      // Read all tool life data
      Import.FwLib.EW result;
      var data = ReadPmcData (m_handle, out result, Import.FwLib.Pmc.ADDRESS.D, firstAddress, 32 * maxToolNumber);
      if (result != Import.FwLib.EW.OK) {
        log.ErrorFormat ("ReadByte_MURATA: error when reading D{0}: {1}", firstAddress, result);
        ManageError ("ReadByte_MURATA", result);
        return tld;
      }

      for (int toolNumber = 0; toolNumber < maxToolNumber; toolNumber++) {
        // New tool position
        tld.AddTool ();
        tld[toolNumber].MagazineNumber = 0; // no magazine
        tld[toolNumber].PotNumber = toolNumber + 1;
        tld[toolNumber].ToolNumber = (toolNumber + 1).ToString ();
        tld[toolNumber].ToolId = (toolNumber + 1).ToString ();

        // Life of the tool
        tld[toolNumber].AddLifeDescription ();
        tld[toolNumber][0].LifeDirection = ToolLifeDirection.Down;
        int maxValue = ReadValue_MURATA (data, 32 * toolNumber, 4);
        tld[toolNumber][0].LifeLimit = maxValue;
        int currentValue = ReadValue_MURATA (data, 32 * toolNumber + 4, 4);
        tld[toolNumber][0].LifeValue = currentValue;
        tld[toolNumber][0].LifeWarningOffset = ReadValue_MURATA (data, 32 * toolNumber + 8, 1);
        tld[toolNumber][0].LifeType = ToolUnit.Parts;

        // State
        if (maxValue == 0) {
          tld[toolNumber].ToolState = ToolState.Unused;
        }
        else if (currentValue == 0) {
          tld[toolNumber].ToolState = ToolState.Expired;
        }
        else {
          tld[toolNumber].ToolState = ToolState.Available;
        }

        // Geometry
        tld[toolNumber].SetProperty ("GeometryUnit", ToolUnit.Unknown);
      }

      return tld;
    }

    int ReadValue_MURATA (byte[] data, int position, int length)
    {
      int value = 0;

      // First bytes are the strongest
      for (int i = 0; i < length; i++) {
        value += (data[position + i] << (8 * (length - 1 - i)));
      }

      return value;
    }
  }
}
