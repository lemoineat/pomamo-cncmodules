// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using Lemoine.Core.SharedData;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Fanuc_tool_management_data.
  /// </summary>
  public partial class Fanuc
  {
    #region Members
    bool m_toolLifeDataInitialized = false; // Reset to false at each start
    ToolLifeData m_toolLifeData = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Defines where the data has to be read to create tool life data
    /// Possible options are:
    ///  * fanuc, by default
    ///  * fanuc2 (new way that should be faster but not tested yet),
    ///  * murata_left,
    ///  * murata_right,
    ///  * custom (based on a configuration file),
    ///  * emag
    /// </summary>
    public string ToolLifeDataInput { get; set; }

    /// <summary>
    /// Get an object fully describing the life of all tools
    /// </summary>
    /// <param name="param">Can be a configuration file depending on the way we read tool life data</param>
    /// <returns></returns>
    public ToolLifeData GetToolLifeData (string param)
    {
      if (!m_toolLifeDataInitialized) {
        InitializeToolManagementData (param);
      }

      return m_toolLifeData;
    }
    #endregion // Getters / Setters

    #region Private functions
    void InitializeToolManagementData (string param)
    {
      m_toolLifeData = null;

      try {
        switch (ToolLifeDataInput) {
        case "murata_left":
          m_toolLifeData = GetToolLife_MURATA (4600, 0);
          break;
        case "murata_right":
          m_toolLifeData = GetToolLife_MURATA (5080, 1);
          break;
        case "fanuc":
          m_toolLifeData = GetToolLife_FANUC ();
          break;
        case "fanuc2":
          m_toolLifeData = GetToolLife_FANUC2 ();
          break;
        case "custom":
          m_toolLifeData = GetToolLife_CUSTOM (param);
          break;
        case "emag":
          m_toolLifeData = GetToolLife_EMAG ();
          break;
        default:
          throw new Exception ("InitializeToolManagementData: Bad ToolLifeDataInput");
        }
      }
      catch (Exception e) {
        log.ErrorFormat ("InitializeToolManagementData: Couldn't initialize tool life data for {0}: {1}",
                        ToolLifeDataInput, e.Message);
      }

      // Initialization done
      m_toolLifeDataInitialized = true;
    }

    ToolLifeData GetToolLife_FANUC ()
    {
      // We get information from the Fanuc library
      var toolManagementData = new List<Import.FwLib.IODBTLMNG> ();
      int index = 0; // Index of data to read
      short number = 1; // Number of data to read, should remain 1
      Import.FwLib.EW result;
      bool stop = false;
      do {
        var data = new Import.FwLib.IODBTLMNG ();
        result = (Import.FwLib.EW)Import.FwLib.Cnc.rdtool (m_handle, (short)(index + 1), ref number, out data);
        if (result == Import.FwLib.EW.BUSY) {
          log.Error ("GetToolLife_FANUC: machine is BUSY");
          throw new Exception ("GetToolLife_FANUC: machine is BUSY");
        }
        if (result != Import.FwLib.EW.OK) {
          if (result == Import.FwLib.EW.NOOPT) {
            log.ErrorFormat ("GetToolLife_FANUC: error when reading number for index {0}: no option", index);
          }
          else {
            log.ErrorFormat ("GetToolLife_FANUC: error when reading number for index {0}: {1}", index, result);
          }

          ManageError ("GetToolLife_FANUC", result);
        }
        else if (number == 1) {
          if (data.pot != 0 && data.magazine != 0 && data.life_stat > 1) // exclusion of "not registered" and "unused"
{
            toolManagementData.Add (data);
          }
        }
        else {
          log.ErrorFormat ("GetToolLife_FANUC: number is {0} for index {1}", number, index);
        }

        index++;
        stop = (result != Import.FwLib.EW.OK) || // stop if error
          (number != 1) || // or if the number of data to read is not 1
          (index >= 999) || // index must be less than 999 otherwise an error occurs in rdtool
          (data.life_stat == 0); // stop if "not registered"
      } while (!stop);

      // Format data
      return FormatToolLife_FANUC (toolManagementData);
    }

    // Another way to find the tool life
    ToolLifeData GetToolLife_FANUC2 ()
    {
      // We get information from the Fanuc library
      var toolManagementData = new List<Import.FwLib.IODBTLMNG> ();

      // Read magazine 1, 11 and 21 (for tools that are currently used by the spindle)
      ReadMagazine (1, ref toolManagementData);
      ReadMagazine (11, ref toolManagementData);
      ReadMagazine (21, ref toolManagementData);

      // Format data
      return FormatToolLife_FANUC (toolManagementData);
    }

    void ReadMagazine (short numMagazine, ref List<Import.FwLib.IODBTLMNG> data)
    {
      short readNumber;
      var magazines = new Import.FwLib.IODBTLMAG (); // Contains 10 elements
      int iteration = 0;
      Import.FwLib.EW result;

      do {
        readNumber = 10;

        // Initialize the structures
        for (short i = 0; i < readNumber; i++) {
          magazines.iodbtlmag[i].magazine = numMagazine;
          magazines.iodbtlmag[i].pot = (short)(i + readNumber * iteration + 1);
        }

        // Try to read 10 values
        result = (Import.FwLib.EW)Import.FwLib.Cnc.rdmagazine (m_handle, ref readNumber, out magazines);

        if (result == Import.FwLib.EW.BUSY) {
          log.Error ("ReadMagazine_FANUC: machine is BUSY");
          throw new Exception ("ReadMagazine_FANUC: machine is BUSY");
        }

        if (result == Import.FwLib.EW.OK) {
          // Read {readNumber} tool life
          for (short i = 0; i < readNumber; i++) {
            short number = 1; // Read one by one
            result = (Import.FwLib.EW)Import.FwLib.Cnc.rdtool (
              m_handle, magazines.iodbtlmag[i].tool_index, ref number, out Import.FwLib.IODBTLMNG toolData);

            if (result == Import.FwLib.EW.BUSY) {
              log.Error ("rdtool_FANUC: machine is BUSY");
              throw new Exception ("rdtool_FANUC: machine is BUSY");
            }

            if (result != Import.FwLib.EW.OK) {
              log.ErrorFormat ("GetToolLife_FANUC: error when reading number for index {0}: {1}",
                              magazines.iodbtlmag[i].tool_index, result);
              ManageError ("GetToolLife_FANUC", result);
              readNumber = 0;
            }
            else if (number == 1 && toolData.pot != 0 && toolData.magazine != 0 && toolData.life_stat > 1) {
              // (exclusion of "not registered" and "unused")
              // Add the result
              data.Add (toolData);
            }

            if (toolData.life_stat == 0 || number == 0) // stop if "not registered"
{
              readNumber = 0;
            }
          }
        }
        else {
          readNumber = 0;
          log.ErrorFormat ("GetToolLife_FANUC: error with the function rdmagazine: {0}", result);
        }

      } while (readNumber > 0 && iteration++ < 100);
    }

    ToolLifeData FormatToolLife_FANUC (IList<Import.FwLib.IODBTLMNG> toolManagementData)
    {
      var tld = new ToolLifeData ();

      log.DebugFormat ("FormatToolLife_FANUC, number of tools: {0}", toolManagementData.Count);
      for (int numTool = 0; numTool < toolManagementData.Count; numTool++) {
        tld.AddTool ();
        tld[numTool].MagazineNumber = toolManagementData[numTool].magazine;
        tld[numTool].PotNumber = toolManagementData[numTool].pot;
        tld[numTool].ToolNumber = toolManagementData[numTool].T_code.ToString ();
        tld[numTool].ToolId = tld[numTool].ToolNumber.ToString ();

        // Compensations
        short lookUpCutter = toolManagementData[numTool].D_code;
        short lookUpLength = toolManagementData[numTool].H_code;
        Import.FwLib.ODBTOFS tof;
        if ((Import.FwLib.EW)Import.FwLib.Cnc.rdtofs (m_handle, lookUpCutter, 0 /* wear - cutter radius */, 8, out tof) == Import.FwLib.EW.OK) {
          tld[numTool].SetProperty ("CutterCompensation_wear", tof.data);
        }

        if ((Import.FwLib.EW)Import.FwLib.Cnc.rdtofs (m_handle, lookUpCutter, 1 /* geometry - cutter radius */, 8, out tof) == Import.FwLib.EW.OK) {
          tld[numTool].SetProperty ("CutterCompensation", tof.data);
        }

        if ((Import.FwLib.EW)Import.FwLib.Cnc.rdtofs (m_handle, lookUpCutter, 2 /* wear - tool length */, 8, out tof) == Import.FwLib.EW.OK) {
          tld[numTool].SetProperty ("LengthCompensation_wear", tof.data);
        }

        if ((Import.FwLib.EW)Import.FwLib.Cnc.rdtofs (m_handle, lookUpCutter, 3 /* geometry - tool length */, 8, out tof) == Import.FwLib.EW.OK) {
          tld[numTool].SetProperty ("LengthCompensation", tof.data);
        }

        tld[numTool].SetProperty ("GeometryUnit", ToolUnit.Unknown); // The unit depends on other parameters

        // Status of the tool
        int stateNumber = toolManagementData[numTool].life_stat;
        switch (stateNumber) {
        case 0:
          tld[numTool].ToolState = ToolState.NotRegistered;
          break;
        case 1:
          tld[numTool].ToolState = ToolState.Unused;
          break;
        case 2:
          tld[numTool].ToolState = ToolState.Available;
          break;
        case 3:
          tld[numTool].ToolState = ToolState.Expired;
          break;
        case 4:
          tld[numTool].ToolState = ToolState.Broken;
          break;
        default:
          tld[numTool].ToolState = ToolState.Unknown;
          break;
        }

        // Unit
        int info = toolManagementData[numTool].tool_info;
        ToolUnit type = ((info & 0x02) == 0) ? ToolUnit.Parts : ToolUnit.TimeSeconds;

        // Life description: up
        tld[numTool].AddLifeDescription ();
        tld[numTool][0].LifeDirection = ToolLifeDirection.Up;
        tld[numTool][0].LifeType = type;
        tld[numTool][0].LifeValue = toolManagementData[numTool].life_count;

        // according to Andreas Burkert from Chiron, rest life is the warning offset
        tld[numTool][0].LifeWarningOffset =
          (toolManagementData[numTool].rest_life > 0 &&
           toolManagementData[numTool].rest_life < toolManagementData[numTool].max_life) ?
          toolManagementData[numTool].rest_life : (double?)null;
        tld[numTool][0].LifeLimit = toolManagementData[numTool].max_life;

        log.DebugFormat ("FormatToolLife_FANUC: get life description for index {0}", numTool);
        log.DebugFormat ("FormatToolLife_FANUC: magazine is {0}, pot is {1}, tool is {2}",
                        tld[numTool].MagazineNumber, tld[numTool].PotNumber, tld[numTool].ToolNumber);
        log.DebugFormat ("FormatToolLife_FANUC: state is {0}, current life is {1}, warning offset is {2}, max life is {3}, info is {4}",
                        tld[numTool].ToolState, toolManagementData[numTool].life_count,
                        toolManagementData[numTool].rest_life, toolManagementData[numTool].max_life,
                        toolManagementData[numTool].tool_info);
      }

      return tld;
    }
    #endregion Private functions
  }
}
