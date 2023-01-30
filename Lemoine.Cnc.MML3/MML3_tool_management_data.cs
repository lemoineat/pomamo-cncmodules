// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.SharedData;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Part of the class MML3 dealing with tool management data
  /// </summary>
  public partial class MML3
  {
    #region Members
    bool m_toolManagementDataInitialized = false;
    readonly IList<MML3_position> m_positions = new List<MML3_position> ();
    ToolLifeData m_toolLifeData = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Number of registered tools
    /// </summary>
    public int RegisteredToolNumber
    {
      get
      {
        CheckProXConnection ();

        if (!m_toolManagementDataInitialized) {
          InitializeToolManagementData ();
        }

        return m_positions.Count;
      }
    }

    /// <summary>
    /// Get an object fully describing the life of all tools
    /// </summary>
    public ToolLifeData ToolLifeData
    {
      get
      {
        CheckProXConnection ();

        if (!m_toolManagementDataInitialized) {
          InitializeToolManagementData ();
        }

        return m_toolManagementDataInitialized ? m_toolLifeData : null;
      }
    }

    /// <summary>
    /// Current tool number
    /// </summary>
    public int ToolNumber
    {
      get
      {
        CheckProXConnection ();

        uint magazineNumber, cutterNumber, ftn, itn, ptn;
        int potNumber;
        var ret = m_md3ProX.SpindleTool (m_proXhandle, out magazineNumber, out potNumber,
                                        out cutterNumber, out ftn, out itn, out ptn);
        ManageProXResult ("SpindleTool", ret);
        return (int)ptn;
      }
    }
    #endregion // Getters / Setters

    #region Private methods
    void InitializeToolManagementData ()
    {
      // Restore data
      m_positions.Clear ();
      m_toolLifeData = new ToolLifeData ();

      // List of all positions
      ListPositions ();

      // CONFIGURATION //

      // Unit
      int inchUnit; // 1 is inch, 0 is mm
      uint ftnFig,  // Number of effective digits of the functionnal tool number
      itnFig,       // --------------------------------- individual tool number
      ptnFig,       // --------------------------------- program tool number
      manageType;   // 0: FTN is functional tool number
      // 1: PTN is functional tool number
      // 2: functional tool number not used
      MMLReturn ret = m_md3ProX.ToolInfo (m_proXhandle, out inchUnit, out ftnFig, out itnFig, out ptnFig, out manageType);
      ManageProXResult ("ToolInfo", ret);

      // Toollife: up or down?
      bool countTypeRemain;   // True is down, false is up
      bool alarmResetMaxLife; // True is "set remaining life to full" at tool life alarm reset
      ret = m_md3ProX.ToollifeInfo (m_proXhandle, out countTypeRemain, out alarmResetMaxLife);
      ManageProXResult ("ToolLifeInfo", ret);

      // Random pot number?
      bool randomAtc;
      ret = m_md3ProX.AtcRandomMagazine (m_proXhandle, out randomAtc);
      ManageProXResult ("AtcRandomMagazine", ret);

      if (randomAtc) {
        // Cannot use tool management functions
        log.Info ("MML3_tool_management_data: random pot number management");
      }

      // Magazine and pot numbers
      UInt32[] magazineNumbers = Enumerable.Repeat ((UInt32)0, m_positions.Count).ToArray ();
      Int32[] potNumbers = Enumerable.Repeat ((Int32)0, m_positions.Count).ToArray ();
      UInt32[] cutterNumbers = Enumerable.Repeat ((UInt32)0, m_positions.Count).ToArray ();
      for (int i = 0; i < m_positions.Count; i++) {
        magazineNumbers[i] = m_positions[i].MagazineNumber;
        potNumbers[i] = m_positions[i].PotNumber;
        cutterNumbers[i] = m_positions[i].CutterNumber;
      }

      // DATA //

      if (m_md3ProX.Version == 3) {
        // T code
        Int32[] tCodes = Enumerable.Repeat ((Int32)0, m_positions.Count).ToArray ();
        ret = m_md3ProX.GetToolDataItem (m_proXhandle, (int)Pro3ToolCommonItem.TD_PTN, magazineNumbers, potNumbers,
                                         ref tCodes, (UInt32)tCodes.Length);
        if (ret != MMLReturn.EM_OK) {
          ManageProXResult ("GetToolDataItem", ret);
        }

        // Status
        ToolState[] status = Enumerable.Repeat (ToolState.Unknown, m_positions.Count).ToArray ();
        Int32[] alarmFlags = Enumerable.Repeat ((Int32)0, m_positions.Count).ToArray ();
        ret = m_md3ProX.GetCutterDataItem (m_proXhandle, (int)Pro3ToolCommonItem.TD_ALM,
                                          magazineNumbers, potNumbers, cutterNumbers,
                                          ref alarmFlags, (UInt32)alarmFlags.Length);
        ManageProXResultContinue ("GetCutterDataItem(TD_ALM)", ret);
        if ((ret != MMLReturn.EM_OK) && (ret != MMLReturn.EM_DATA)) {
          return;
        }

        for (int i = 0; i < alarmFlags.Length; i++) {
          if ((alarmFlags[i] & 0x01) != 0 || (alarmFlags[i] & 0x02) != 0) {
            // Tool broken
            status[i] = ToolState.Broken;
          }
          else if ((alarmFlags[i] & 0x20) != 0) {
            // Tool expired
            status[i] = ToolState.Expired;
          }
          else {
            status[i] = ToolState.Available;
          }
        }

        // Geometry
        ToolUnit geometryUnit = (inchUnit != 0) ? ToolUnit.DistanceInch : ToolUnit.DistanceMillimeters;

        Int32[] compensationH = Enumerable.Repeat ((Int32)0, m_positions.Count).ToArray ();
        ret = m_md3ProX.GetCutterDataItem (m_proXhandle, (int)Pro3ToolCommonItem.TD_LEN,
                                          magazineNumbers, potNumbers, cutterNumbers,
                                          ref compensationH, (UInt32)compensationH.Length);
        ManageProXResultContinue ("GetCutterDataItem(TD_LEN)", ret);
        if ((ret != MMLReturn.EM_OK) && (ret != MMLReturn.EM_DATA)) {
          return;
        }

        Int32[] compensationD = Enumerable.Repeat ((Int32)0, m_positions.Count).ToArray ();
        ret = m_md3ProX.GetCutterDataItem (m_proXhandle, (int)Pro3ToolCommonItem.TD_DIA,
                                          magazineNumbers, potNumbers, cutterNumbers,
                                          ref compensationD, (UInt32)compensationD.Length);
        ManageProXResultContinue ("GetCutterDataItem(TD_DIA)", ret);
        if ((ret != MMLReturn.EM_OK) && (ret != MMLReturn.EM_DATA)) {
          return;
        }

        // ATC speed
        Int32[] atcSpeed = Enumerable.Repeat (0, m_positions.Count).ToArray ();
        ret = m_md3ProX.GetToolDataItem (m_proXhandle, (int)Pro3ToolCommonItem.TD_SLOW,
                                        magazineNumbers, potNumbers,
                                        ref atcSpeed, (UInt32)atcSpeed.Length);
        if (ret != MMLReturn.EM_OK) {
          for (int i = 0; i < atcSpeed.Length; i++) {
            atcSpeed[i] = -1; // Unknown
          }
          ManageProXResultContinue ("GetCutterDataItem(TD_SLOW)", ret);
          if ((ret != MMLReturn.EM_OK) && (ret != MMLReturn.EM_DATA)) {
            return;
          }
        }

        // Add tool with their associated tool life
        for (int i = 0; i < m_positions.Count; i++) {
          AddTool ((int)magazineNumbers[i], (int)potNumbers[i], tCodes[i], status[i],
                   geometryUnit, compensationH[i], compensationD[i], atcSpeed[i]);
        }

        // Current value per position
        Int32[] lifeValues = Enumerable.Repeat ((Int32)0, m_positions.Count).ToArray ();
        ret = m_md3ProX.GetCutterDataItem (m_proXhandle, (int)Pro3ToolCommonItem.TD_REMAIN,
                                          magazineNumbers, potNumbers, cutterNumbers,
                                          ref lifeValues, (UInt32)lifeValues.Length);
        ManageProXResult ("GetCutterDataItem(TD_REMAIN)", ret);

        // Limit per position
        Int32[] lifeLimits = Enumerable.Repeat ((Int32)0, m_positions.Count).ToArray ();
        ret = m_md3ProX.GetCutterDataItem (m_proXhandle, (int)Pro3ToolCommonItem.TD_TL,
                                           magazineNumbers, potNumbers, cutterNumbers,
                                           ref lifeLimits, (UInt32)lifeLimits.Length);
        ManageProXResult ("GetCutterDataItem(TD_TL)", ret);

        // Tool life type and coeff
        var toolLifeType = (m_md3ProX as Md3Pro3).GetToolLifeType (m_proXhandle);
        ToolUnit toolUnit;
        double coef = 1;
        switch (toolLifeType) {
        case Pro3ToolLifeType.TLTYPE_SEC:
          toolUnit = ToolUnit.TimeSeconds;
          break;
        case Pro3ToolLifeType.TLTYPE_DISTANCE:
          if (inchUnit == 0) {
            toolUnit = ToolUnit.DistanceMillimeters;
          } else {
            toolUnit = ToolUnit.DistanceInch;
            coef = 10;
          }
          break;
        case Pro3ToolLifeType.TLTYPE_COUNT:
          toolUnit = ToolUnit.NumberOfCycles;
          break;
        case Pro3ToolLifeType.TLTYPE_01SEC:
          toolUnit = ToolUnit.TimeSeconds;
          coef = 10;
          break;
        default:
          throw new Exception ("Unknown tool life type for Pro3: " + toolLifeType);
        }

        for (int i = 0; i < m_positions.Count; i++) {
          AddToolLife (i, countTypeRemain ? ToolLifeDirection.Down : ToolLifeDirection.Up,
                      toolUnit, (double)lifeValues[i] / coef,
                      (double)lifeLimits[i] / coef, null);
        }
      } else {
        // Data items to check
        Pro5ToolDataItem[] dataItems =
        {
          Pro5ToolDataItem.PTN,
          // Tool life (duration)
          Pro5ToolDataItem.ManageLifeTime,
          Pro5ToolDataItem.LifeTimeAlarm,
          Pro5ToolDataItem.LifeTimeWarning,
          Pro5ToolDataItem.LifeTimeActual,
          // Tool life (distance)
          Pro5ToolDataItem.ManageLifeDist,
          Pro5ToolDataItem.LifeDistAlarm,
          Pro5ToolDataItem.LifeDistWarning,
          Pro5ToolDataItem.LifeDistActual,
          // Tool life (count)
          Pro5ToolDataItem.ManageLifeCount,
          Pro5ToolDataItem.LifeCountAlarm,
          Pro5ToolDataItem.LifeCountWarning,
          Pro5ToolDataItem.LifeCountActual,
          // Status
          Pro5ToolDataItem.AlarmFlag,
          //Pro5ToolDataItem.WarningFlag,
          Pro5ToolDataItem.FirstUse,
          // Geometry
          Pro5ToolDataItem.HGeometry,
          Pro5ToolDataItem.DGeometry,
          // Other
          Pro5ToolDataItem.AtcSpeed
        };
        UInt32[] enabledItems = Enumerable.Repeat ((UInt32)0, dataItems.Length).ToArray ();
        ret = m_md3ProX.ToolDataItemIsEnable (m_proXhandle, dataItems, ref enabledItems, (UInt32)dataItems.Length);
        ManageProXResult ("ToolDataItemIsEnable", ret);

        // T code
        Int32[] tCodes = Enumerable.Repeat ((Int32)0, m_positions.Count).ToArray ();
        if (enabledItems[Array.IndexOf (dataItems, Pro5ToolDataItem.PTN)] != 0) {
          ret = m_md3ProX.GetToolDataItem (m_proXhandle, (int)Pro5ToolDataItem.PTN, magazineNumbers, potNumbers,
                                          ref tCodes, (UInt32)tCodes.Length);
          ManageProXResult ("GetToolDataItem", ret);
        }

        // First use
        Int32[] firstUse = Enumerable.Repeat ((Int32)0, m_positions.Count).ToArray ();
        if (enabledItems[Array.IndexOf (dataItems, Pro5ToolDataItem.FirstUse)] != 0) {
          ret = m_md3ProX.GetCutterDataItem (m_proXhandle, (int)Pro5ToolDataItem.FirstUse,
                                            magazineNumbers, potNumbers, cutterNumbers,
                                            ref firstUse, (UInt32)firstUse.Length);
          ManageProXResultContinue ("GetCutterDataItem", ret);
          if ((ret != MMLReturn.EM_OK) && (ret != MMLReturn.EM_DATA)) {
            return;
          }
        }

        // Status
        ToolState[] status = Enumerable.Repeat (ToolState.Unknown, m_positions.Count).ToArray ();
        if (enabledItems[Array.IndexOf (dataItems, Pro5ToolDataItem.AlarmFlag)] != 0) {
          Int32[] alarmFlags = Enumerable.Repeat ((Int32)0, m_positions.Count).ToArray ();
          ret = m_md3ProX.GetCutterDataItem (m_proXhandle, (int)Pro5ToolDataItem.AlarmFlag,
                                            magazineNumbers, potNumbers, cutterNumbers,
                                            ref alarmFlags, (UInt32)alarmFlags.Length);
          ManageProXResultContinue ("GetCutterDataItem(AlarmFlag)", ret);
          if ((ret != MMLReturn.EM_OK) && (ret != MMLReturn.EM_DATA)) {
            return;
          }

          for (int i = 0; i < alarmFlags.Length; i++) {
            if ((alarmFlags[i] & 0x01) != 0 || (alarmFlags[i] & 0x02) != 0) {
              // Tool broken
              status[i] = ToolState.Broken;
            }
            else if ((alarmFlags[i] & 0x20) != 0) {
              // Tool expired
              status[i] = ToolState.Expired;
            }
            else {
              status[i] = (firstUse[i] == 0) ? ToolState.Available : ToolState.New;
            }
          }
        }

        // Geometry
        ToolUnit geometryUnit = (inchUnit != 0) ? ToolUnit.DistanceInch : ToolUnit.DistanceMillimeters;

        Int32[] compensationH = Enumerable.Repeat ((Int32)0, m_positions.Count).ToArray ();
        if (enabledItems[Array.IndexOf (dataItems, Pro5ToolDataItem.HGeometry)] != 0) {
          ret = m_md3ProX.GetCutterDataItem (m_proXhandle, (int)Pro5ToolDataItem.HGeometry,
                                            magazineNumbers, potNumbers, cutterNumbers,
                                            ref compensationH, (UInt32)compensationH.Length);
          ManageProXResultContinue ("GetCutterDataItem(CompensationH)", ret);
          if ((ret != MMLReturn.EM_OK) && (ret != MMLReturn.EM_DATA)) {
            return;
          }
        }

        Int32[] compensationD = Enumerable.Repeat ((Int32)0, m_positions.Count).ToArray ();
        if (enabledItems[Array.IndexOf (dataItems, Pro5ToolDataItem.DGeometry)] != 0) {
          ret = m_md3ProX.GetCutterDataItem (m_proXhandle, (int)Pro5ToolDataItem.DGeometry,
                                            magazineNumbers, potNumbers, cutterNumbers,
                                            ref compensationD, (UInt32)compensationD.Length);
          ManageProXResultContinue ("GetCutterDataItem(CompensationD)", ret);
          if ((ret != MMLReturn.EM_OK) && (ret != MMLReturn.EM_DATA)) {
            return;
          }
        }

        // ATC speed
        Int32[] atcSpeed = Enumerable.Repeat (0, m_positions.Count).ToArray ();
        if (enabledItems[Array.IndexOf (dataItems, Pro5ToolDataItem.AtcSpeed)] != 0) {
          ret = m_md3ProX.GetToolDataItem (m_proXhandle, (int)Pro5ToolDataItem.AtcSpeed,
                                          magazineNumbers, potNumbers,
                                          ref atcSpeed, (UInt32)atcSpeed.Length);
          if (ret != MMLReturn.EM_OK) {
            for (int i = 0; i < atcSpeed.Length; i++) {
              atcSpeed[i] = -1;
            }
            ManageProXResultContinue ("GetCutterDataItem(AtcSpeed)", ret);
            if ((ret != MMLReturn.EM_OK) && (ret != MMLReturn.EM_DATA)) {
              return;
            }
          }
        }
        else {
          log.InfoFormat ("Atc Speed not enabled");
          for (int i = 0; i < atcSpeed.Length; i++) {
            atcSpeed[i] = -1;
          }
        }

        // Add tool with their associated tool life
        for (int i = 0; i < m_positions.Count; i++) {
          AddTool ((int)magazineNumbers[i], (int)potNumbers[i], tCodes[i], status[i],
                   geometryUnit, compensationH[i], compensationD[i], atcSpeed[i]);
        }

        // Duration?
        GetToolLife (dataItems, enabledItems,
                     Pro5ToolDataItem.ManageLifeTime, Pro5ToolDataItem.LifeTimeAlarm,
                     Pro5ToolDataItem.LifeTimeActual, Pro5ToolDataItem.LifeTimeWarning,
                     magazineNumbers, potNumbers, cutterNumbers,
                     countTypeRemain ? ToolLifeDirection.Down : ToolLifeDirection.Up,
                     ToolUnit.TimeSeconds, 10);

        // Distance?
        if (inchUnit != 0) { // Inch
          GetToolLife (dataItems, enabledItems,
                       Pro5ToolDataItem.ManageLifeDist, Pro5ToolDataItem.LifeDistAlarm,
                       Pro5ToolDataItem.LifeDistActual, Pro5ToolDataItem.LifeDistWarning,
                       magazineNumbers, potNumbers, cutterNumbers,
                       countTypeRemain ? ToolLifeDirection.Down : ToolLifeDirection.Up,
                       ToolUnit.DistanceInch, 10);
        }
        else { // Mm
          GetToolLife (dataItems, enabledItems,
                       Pro5ToolDataItem.ManageLifeDist, Pro5ToolDataItem.LifeDistAlarm,
                       Pro5ToolDataItem.LifeDistActual, Pro5ToolDataItem.LifeDistWarning,
                       magazineNumbers, potNumbers, cutterNumbers,
                       countTypeRemain ? ToolLifeDirection.Down : ToolLifeDirection.Up,
                       ToolUnit.DistanceMillimeters, 1);
        }

        // Count?
        GetToolLife (dataItems, enabledItems,
                     Pro5ToolDataItem.ManageLifeCount, Pro5ToolDataItem.LifeCountAlarm,
                     Pro5ToolDataItem.LifeCountActual, Pro5ToolDataItem.LifeCountWarning,
                     magazineNumbers, potNumbers, cutterNumbers,
                     countTypeRemain ? ToolLifeDirection.Down : ToolLifeDirection.Up,
                     ToolUnit.NumberOfTimes, 1);
      }

      // Initialization done
      m_toolManagementDataInitialized = true;
    }

    void ListPositions ()
    {
      // Magazine number
      UInt32 actualMgzn;
      Int32 outMcMgzn; // if a storage area outside the ATC magazine exists

      MMLReturn ret;
      if ((ret = m_md3ProX.MaxAtcMagazine (m_proXhandle, out actualMgzn, out outMcMgzn)) == MMLReturn.EM_OK) {
        log.DebugFormat ("ListPositions: {0} magazine(s) found", actualMgzn);
        for (UInt32 mgznNo = 1; mgznNo <= actualMgzn; mgznNo++) {
          // Pot number
          UInt32 maxPot;
          Int32 mgznType; // The ATC magazine type is stored (cf page 13 ref Pro5)
          UInt32 emptyPot;
          if ((ret = m_md3ProX.AtcMagazineInfo (m_proXhandle, mgznNo, out maxPot,
                                                out mgznType, out emptyPot)) == MMLReturn.EM_OK) {
            log.DebugFormat ("ListPositions: {0} pot(s) found in magazine {1}", maxPot, mgznNo);

            // Get the number of cutters for all pots of this magazine
            Int32[] cutterPerPot = Enumerable.Repeat ((Int32)1, (int)maxPot).ToArray ();
            if (m_md3ProX.Version > 3) {
              UInt32[] magazineNumbers = Enumerable.Repeat ((UInt32)mgznNo, (int)maxPot).ToArray ();
              Int32[] potNumbers = Enumerable.Repeat ((Int32)0, (int)maxPot).ToArray ();
              for (Int32 potNo = 1; potNo <= maxPot; potNo++) {
                potNumbers[potNo - 1] = potNo;
              }

              ret = m_md3ProX.GetToolDataItem (m_proXhandle, (int)Pro5ToolDataItem.TotalCutter, magazineNumbers, potNumbers,
                ref cutterPerPot, (uint)cutterPerPot.Length);
              if (ret != MMLReturn.EM_OK) {
                log.ErrorFormat ("Couldn't get the number of cutters per pot for magazine {0}: {1}", mgznNo, ret);
                for (int i = 0; i < cutterPerPot.Length; i++) {
                  cutterPerPot[i] = 1; // Back to 1 cutter per pot
                }
              }
            }

            // Add positions
            for (Int32 potNo = 1; potNo <= maxPot; potNo++) {
              // Get the number of cutters
              int cutters = cutterPerPot[potNo - 1];
              
              for (UInt32 cutterNo = 1; cutterNo <= cutters; cutterNo++) {
                m_positions.Add (new MML3_position (mgznNo, potNo, cutterNo));
              }
            }
              
          } else {
            // Failed
            m_toolManagementDataInitialized = true;
            ManageProXResult ("AtcMagazineInfo", ret);
          }
        }
      } else {
        // Failed
        m_toolManagementDataInitialized = true;
        ManageProXResult ("MaxAtcMagazine", ret);
      }
    }

    void AddTool (int magazineNumber, int potNumber, int toolNumber, ToolState toolState,
                  ToolUnit geometryUnit, int compensationH, int compensationD, int atcSpeed)
    {
      m_toolLifeData.AddTool ();
      int index = m_toolLifeData.ToolNumber - 1;
      m_toolLifeData[index].MagazineNumber = magazineNumber;
      m_toolLifeData[index].PotNumber = potNumber;
      m_toolLifeData[index].ToolNumber = toolNumber.ToString ();
      m_toolLifeData[index].ToolId = toolNumber.ToString ();
      m_toolLifeData[index].ToolState = toolState;

      // Geometry
      m_toolLifeData[index].SetProperty ("GeometryUnit", geometryUnit);

      // multiplier: 0.00001 on Pro5/6 inch, 0.0001 on Pro5/6 mm, 0.0001 on Pro3 inch or mm
      double multiplier = ((geometryUnit == ToolUnit.DistanceInch) && (m_md3ProX.Version != 3)) ? 0.00001 : 0.0001;
      m_toolLifeData[index].SetProperty ("LengthCompensation", multiplier * (double)compensationH);
      m_toolLifeData[index].SetProperty ("CutterCompensation", multiplier * (double)compensationD);
      switch (atcSpeed) {
      case -1:
        // No property
        break;
      case 0:
        m_toolLifeData[index].SetProperty ("AtcSpeed", "normal");
        break;
      case 1:
        m_toolLifeData[index].SetProperty ("AtcSpeed", "slow");
        break;
      case 2:
        m_toolLifeData[index].SetProperty ("AtcSpeed", "middle");
        break;
      default:
        m_toolLifeData[index].SetProperty ("AtcSpeed", "unknown: " + atcSpeed);
        break;
      }

      // store geo for current tool
      log.Debug ($"AddTool: geomtry for current tool={toolNumber}, geoH={multiplier * (double)compensationH}, geoD={multiplier * (double)compensationD}");
      m_toolCompensationHList[toolNumber] = multiplier * (double)compensationH;
      m_toolCompensationDList[toolNumber] = multiplier * (double)compensationD;
    }

    void GetToolLife (Pro5ToolDataItem[] dataItems, UInt32[] enabledItems,
                     Pro5ToolDataItem itemManaged, Pro5ToolDataItem itemAlarm,
                     Pro5ToolDataItem itemActual, Pro5ToolDataItem itemWarning,
                     UInt32[] magazineNumbers, Int32[] potNumbers, UInt32[] cutterNumbers,
                     ToolLifeDirection lifeDirection, ToolUnit lifeType, double coef)
    {
      if (enabledItems[Array.IndexOf (dataItems, itemManaged)] != 0 &&
          enabledItems[Array.IndexOf (dataItems, itemAlarm)] != 0 &&
          enabledItems[Array.IndexOf (dataItems, itemActual)] != 0) {

        // Is defined per position?
        Int32[] lifeDefined = Enumerable.Repeat ((Int32)0, m_positions.Count).ToArray ();
        var ret = m_md3ProX.GetCutterDataItem (m_proXhandle, (int)itemManaged,
                                              magazineNumbers, potNumbers, cutterNumbers,
                                              ref lifeDefined, (UInt32)lifeDefined.Length);
        ManageProXResult ("GetCutterDataItem:" + itemManaged, ret);

        // Current value per position
        Int32[] lifeValues = Enumerable.Repeat ((Int32)0, m_positions.Count).ToArray ();
        ret = m_md3ProX.GetCutterDataItem (m_proXhandle, (int)itemActual,
                                          magazineNumbers, potNumbers, cutterNumbers,
                                          ref lifeValues, (UInt32)lifeValues.Length);
        ManageProXResult ("GetCutterDataItem:" + itemActual, ret);

        // Limit per position
        Int32[] lifeLimits = Enumerable.Repeat ((Int32)0, m_positions.Count).ToArray ();
        ret = m_md3ProX.GetCutterDataItem (m_proXhandle, (int)itemAlarm,
                                          magazineNumbers, potNumbers, cutterNumbers,
                                          ref lifeLimits, (UInt32)lifeLimits.Length);
        ManageProXResult ("GetCutterDataItem:" + itemAlarm, ret);

        // Warning per position
        Int32[] lifeWarnings = Enumerable.Repeat ((Int32)0, m_positions.Count).ToArray ();
        bool warnEnabled = (enabledItems[Array.IndexOf (dataItems, itemWarning)] != 0);
        if (warnEnabled) {
          ret = m_md3ProX.GetCutterDataItem (m_proXhandle, (int)itemWarning,
                                            magazineNumbers, potNumbers, cutterNumbers,
                                            ref lifeWarnings, (UInt32)lifeWarnings.Length);
          ManageProXResult ("GetCutterDataItem:" + itemWarning, ret);
        }

        for (int i = 0; i < m_positions.Count; i++) {
          if (lifeDefined[i] != 0) {
            AddToolLife (i, lifeDirection,
                        lifeType, (double)lifeValues[i] / coef,
                        (double)lifeLimits[i] / coef,
                        warnEnabled ? (int?)lifeWarnings[i] / coef : null);
          }
        }
      }
    }

    void AddToolLife (int index, ToolLifeDirection direction, ToolUnit type, double value, double limit,
                     double? warning)
    {
      m_toolLifeData[index].AddLifeDescription ();
      int index2 = m_toolLifeData[index].LifeDescriptionNumber - 1;
      m_toolLifeData[index][index2].LifeDirection = direction;
      m_toolLifeData[index][index2].LifeType = type;
      m_toolLifeData[index][index2].LifeValue = value;
      m_toolLifeData[index][index2].LifeLimit = limit;
      if (warning.HasValue) {
        if (direction == ToolLifeDirection.Down) {
          m_toolLifeData[index][index2].LifeWarningOffset = warning;
        }
        else if (direction == ToolLifeDirection.Up) {
          m_toolLifeData[index][index2].LifeWarningOffset = limit - warning.Value;
        }
      }

    }
    #endregion // Private methods

    #region Tool property reader
    IDictionary<string, List<string>> m_toolProperties = null;
    IList<string> m_missingProperties = null;

    /// <summary>
    /// Log all tool properties in a log file. Everything is written only once, then you have to reinit the input module
    /// It is intended to be used with LemCncGui. Run it then close the software and check the logs.
    /// </summary>
    public IDictionary<string, List<string>> ToolProperties {
      get {
        ReadToolProperties ();
        return m_toolProperties;
      }
    }

    /// <summary>
    /// Return the list of properties that cannot be read
    /// </summary>
    public IList<string> MissingProperties {
      get {
        ReadToolProperties ();
        return m_missingProperties;
      }
    }

    /// <summary>
    /// List of tool positions
    /// </summary>
    public IList<string> ToolPositions {
      get {
        ReadToolProperties ();

        // Format and list all positions
        IList<string> toolPositions = new List<string> ();
        for (int i = 0; i < m_positions.Count; i++) {
          string positionDisplay = string.Format (
              "Magazine {0} pot {1} cutter {2}",
              m_positions[i].MagazineNumber,
              m_positions[i].PotNumber,
              m_positions[i].CutterNumber
            );
          toolPositions.Add (positionDisplay);
        }
        return toolPositions;
      }
    }

    void ReadToolProperties()
    {
      // Only read once, when the connection is made
      CheckProXConnection ();
      if (m_toolProperties != null) {
        return;
      }

      // Prepare the variables that will contain the results
      m_toolProperties = new Dictionary<string, List<string>> ();
      m_missingProperties = new List<string> ();

      // Get magazine, pot and cutter numbers
      m_positions.Clear ();
      m_toolLifeData = new ToolLifeData ();
      ListPositions ();

      UInt32[] magazineNumbers = Enumerable.Repeat ((UInt32)0, m_positions.Count).ToArray ();
      Int32[] potNumbers = Enumerable.Repeat ((Int32)0, m_positions.Count).ToArray ();
      UInt32[] cutterNumbers = Enumerable.Repeat ((UInt32)0, m_positions.Count).ToArray ();
      for (int i = 0; i < m_positions.Count; i++) {
        magazineNumbers[i] = m_positions[i].MagazineNumber;
        potNumbers[i] = m_positions[i].PotNumber;
        cutterNumbers[i] = m_positions[i].CutterNumber;
      }

      // Prepare all properties
      int[] properties;
      if (m_md3ProX.Version == 3) {
        var allItems = Enum.GetValues (typeof (Pro3ToolCommonItem)).Cast<Pro3ToolCommonItem> ().ToArray ();
        properties = Enumerable.Repeat (0, allItems.Count ()).ToArray ();
        for (int i = 0; i < allItems.Count (); i++) {
          properties[i] = (int)allItems[i];
        }
      } else {
        var allItems =  Enum.GetValues (typeof (Pro5ToolDataItem)).Cast<Pro5ToolDataItem> ().ToArray ();
        properties = Enumerable.Repeat (0, allItems.Count ()).ToArray ();
        for (int i = 0; i < allItems.Count (); i++) {
          properties[i] = (int)allItems[i];
        }
      }

      UInt32[] enabledProperties = Enumerable.Repeat ((UInt32)1, properties.Length).ToArray ();
      if (m_md3ProX.Version != 3) {
        // Not all properties are enabled
        var ret = m_md3ProX.ToolDataItemIsEnable (m_proXhandle,
          Enum.GetValues (typeof (Pro5ToolDataItem)).Cast<Pro5ToolDataItem> ().ToArray (),
          ref enabledProperties, (UInt32)properties.Length);
        if ((ret != MMLReturn.EM_OK) && (ret != MMLReturn.EM_DATA)) {
          log.FatalFormat ("ToolDataItemIsEnable returned: {0}", ret);
          return;
        }
      }

      // Loop through all possible properties
      for (int i = 0; i < properties.Length; i++)
      {
        int property = properties[i];
        string propertyDisplay = (property).ToString () + '|' +
          (m_md3ProX.Version == 3 ? ((Pro3ToolCommonItem)property).ToString () : ((Pro5ToolDataItem)property).ToString ());

        // Check that we can read the property
        if (enabledProperties[i] == 0) {
          m_missingProperties.Add (propertyDisplay);
          continue;
        }

        // Read the property values
        Int32[] data = Enumerable.Repeat ((Int32)0, m_positions.Count).ToArray ();
        MMLReturn ret;
        if (property < 100) {
          ret = m_md3ProX.GetToolDataItem (m_proXhandle, property,
                                           magazineNumbers, potNumbers,
                                           ref data, (UInt32)data.Length);
        }
        else {
          ret = m_md3ProX.GetCutterDataItem (m_proXhandle, property,
                                             magazineNumbers, potNumbers, cutterNumbers,
                                             ref data, (UInt32)data.Length);
        }

        if (ret == MMLReturn.EM_DATA) {
          log.ErrorFormat ("EM_DATA received for property {0}", propertyDisplay);
        }

        if (ret != MMLReturn.EM_OK) {
          m_missingProperties.Add (propertyDisplay);
          continue;
        }

        // Store the result
        m_toolProperties[propertyDisplay] = new List<string> ();
        for (int j = 0; j < data.Length; j++) {
          m_toolProperties[propertyDisplay].Add (((int)data[j]).ToString ());
        }
      }

      // TEST: try to write all values on the first tool
/*      try {
        foreach (string propertyStr in m_toolProperties.Keys) {
          // Load key / values / magazine / pot
          int propertyInt = Int32.Parse (propertyStr.Split ('|')[0]);
          List<string> values = m_toolProperties[propertyStr];
          if (values == null || values.Count == 0)
            continue;
          int value = Int32.Parse (values[0]);
          uint magazine = magazineNumbers[0];
          int pot = potNumbers[0];

          // Write a property of the first pot / magazine
          log.FatalFormat ("Try to write value {0} for property {1} at pot {2}, magazine {3}",
            value, (Pro3ToolCommonItem)propertyInt, pot, magazine);
          var result = SetToolDataItem (propertyInt,
            new UInt32[] { magazine },
            new Int32[] { pot },
            new Int32[] { value });
          log.FatalFormat ("Result is: {0}", result);
        }
      } catch (Exception e) {
        log.FatalFormat ("Write exception: {0}", e);
      }*/
    }

    /// <summary>
    /// Update the property of a series of {magazine, pot}
    /// </summary>
    /// <param name="property">Property to set (warning: value in Pro3ToolCommonItem OR Pro5ToolDataItem)</param>
    /// <param name="magazineNumbers">Array of magazines to update</param>
    /// <param name="potNumbers">Array of pots to update</param>
    /// <param name="cutterNumbers">Array of pots to update</param>
    /// <param name="values">Array of values</param>
    /// <returns>Result of the function, 0 if OK</returns>
    public MMLReturn SetToolDataItem (int property, UInt32[] magazineNumbers, Int32[] potNumbers, UInt32[] cutterNumbers, Int32[] values)
    {
      CheckProXConnection ();
      MMLReturn ret;

      if ((int)property < 100) {
        ret = m_md3ProX.SetToolDataItem (m_proXhandle, property,
                                         magazineNumbers, potNumbers,
                                         values, (UInt32)values.Length);
      }
      else {
        ret = m_md3ProX.SetCutterDataItem (m_proXhandle, property,
                                           magazineNumbers, potNumbers, cutterNumbers,
                                           values, (UInt32)values.Length);
      }

      return ret;
    }

    /// <summary>
    /// Clear the data of the specified tools. Two or more tools can be specified at a time
    /// </summary>
    /// <param name="magazineNumbers">Array of magazines to clear</param>
    /// <param name="potNumbers">Array of pots to clear</param>
    /// <returns></returns>
    public MMLReturn ClearToolData (UInt32[] magazineNumbers, Int32[] potNumbers)
    {
      CheckProXConnection ();
      return m_md3ProX.ClearToolData (m_proXhandle, magazineNumbers, potNumbers);
    }
    #endregion // Special requests
  }
}
