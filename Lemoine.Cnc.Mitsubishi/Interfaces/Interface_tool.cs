// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Interface_tool.
  /// </summary>
  public class Interface_tool : GenericMitsubishiInterface
  {
    enum ToolLifeProperty
    {
      TOOL_NUMBER,
      STATUS,
      LENGTH,
      RADIUS
    }

    #region Members
    ToolLifeData m_tld = null;
    #endregion Members

    #region Protected methods
    /// <summary>
    /// Work to do on "start", before new data will be read
    /// Already within a try catch
    /// </summary>
    protected override void ResetDataInstance ()
    {
      m_tld = null;
    }
    #endregion // Protected methods

    #region Get methods
    /// <summary>
    /// Get the current tool life data
    /// </summary>
    public ToolLifeData ToolLifeData
    {
      get
      {
        if (m_tld == null) {
          LoadToolLifeData ();
        }

        return m_tld;
      }
    }

    /// <summary>
    /// Get the tool offset
    /// </summary>
    /// <param name="toolSetNumber"/>
    /// <param name="kind"> Possible values:
    /// O X
    /// 1 Z
    /// 3 X wear
    /// 4 Z wear
    /// </param>
    /// <returns></returns>
    public double GetOffset (int toolSetNumber, int kind)
    {
      int toolOffsetType = 6; // L system type

      double value = 0;
      int toolNosePointNumber = 0;
      var errorNumber = 0;
      if ((errorNumber = CommunicationObject.Tool_GetOffset (toolOffsetType, kind, toolSetNumber, out value, out toolNosePointNumber)) != 0) {
        throw new ErrorCodeException (errorNumber, "Tool_GetOffset." + kind);
      }
      return value;
    }
    #endregion Get methods

    #region Private methods
    void LoadToolLifeData ()
    {
      // Create a new container for tool life data
      m_tld = new ToolLifeData ();
      if (SystemType == Mitsubishi.MitsubishiSystemType.MAGIC_CARD_64 ||
          SystemType == Mitsubishi.MitsubishiSystemType.MAGIC_BOARD_64 ||
          SystemType == Mitsubishi.MitsubishiSystemType.MELDAS_600L_6X5L) {
        return;
      }

      // Read the groups
      object objGroups = null;
      var errorCode = CommunicationObject.Tool_GetToolLifeGroupList (out objGroups);
      if (errorCode != 0) {
        throw new ErrorCodeException (errorCode, "Tool_GetToolLifeGroupList");
      }

      var groups = ConvertToArray<int> (objGroups);

      // Read the tools within the group
      foreach (var group in groups) {
        object objTools = null;
        errorCode = CommunicationObject.Tool_GetToolLifeToolNoList (group, out objTools);
        if (errorCode != 0) {
          throw new ErrorCodeException (errorCode, "Tool_GetToolLifeToolNoList." + group);
        }

        var toolNumbers = ConvertToArray<int> (objTools);

        // Read the properties for each tool
        foreach (var toolNumber in toolNumbers) {
          object objData = null;
          errorCode = CommunicationObject.Tool_GetToolLifeValue (group, toolNumber, out objData);
          if (errorCode != 0) {
            throw new ErrorCodeException (errorCode, "Tool_GetToolLifeValue." + group + "." + toolNumber);
          }

          var datas = ConvertToArray<string> (objData);

          int indexTool = m_tld.AddTool ();
          m_tld[indexTool].MagazineNumber = group;
          m_tld[indexTool].PotNumber = toolNumber;
          m_tld[indexTool].ToolId = toolNumber.ToString ();

          // Number of values set
          int valuesSet = -1;
          for (int arrayIndex = 0; arrayIndex < datas.Length; arrayIndex++) {
            string value = datas[arrayIndex];
            double result;
            if (!String.IsNullOrEmpty (value) && double.TryParse (value, out result)) {
              valuesSet = arrayIndex;
            }
          }

          switch (valuesSet) {
            case 5: // MELDAS_700L type II, MELDAS_800L type II
              SetToolLifeProperty (indexTool, ToolLifeProperty.TOOL_NUMBER, datas[0]);
              // Offset# is ignored (datas[1])
              SetToolLifeProperty (indexTool, ToolLifeProperty.STATUS, datas[3]);
              SetToolLifeValue (indexTool, datas[4], datas[2], datas[5]);
              break;
            case 6: // MELDAS_C70 (L) type II, MELDAS_C6_C64 (L) type II
              SetToolLifeProperty (indexTool, ToolLifeProperty.TOOL_NUMBER, datas[0]);
              // Group is ignored (datas[1])
              // Offset# is ignored (datas[3])
              SetToolLifeProperty (indexTool, ToolLifeProperty.STATUS, datas[4]);
              SetToolLifeValue (indexTool, datas[2], datas[6], datas[5]);
              break;
            case 10: // MELDAS_600M_6X5M, MELDAS_700M, MELDAS_800M, MELDAS_C70 (M), MELDAS_C6_C64 (M)
              SetToolLifeProperty (indexTool, ToolLifeProperty.TOOL_NUMBER, datas[0]);
              SetToolLifeProperty (indexTool, ToolLifeProperty.STATUS, datas[1]);
              SetToolLifeProperty (indexTool, ToolLifeProperty.LENGTH, datas[3]);
              SetToolLifeProperty (indexTool, ToolLifeProperty.RADIUS, datas[4]);
              // Aux is ignored (datas[7])
              // Length wear is ignored (datas[8])
              // Diameter wear is ignored (datas[9])
              // Group is ignored (datas[10])
              SetToolLifeValue (indexTool, datas[2], datas[6], datas[5]);
              break;
            default:
              Logger.ErrorFormat ("Mitsubishi.Interface_tool - Couldn't set tool data values if valuesSet is '{0}'", valuesSet);
              break;
          }
        }
      }
    }

    T[] ConvertToArray<T> (object obj)
    {
      if (obj == null) {
        throw new Exception ("Mitsubishi.Interface_tool - Cannot convert null into int[]");
      }

      if (!obj.GetType ().IsArray) {
        throw new Exception ("Mitsubishi.Interface_tool - The object to convert into an array is not an array");
      }

      if (obj.GetType ().GetElementType () == typeof (T)) {
        throw new Exception ("Mitsubishi.Interface_tool - The array contains object that are not 'int'");
      }

      return obj as T[];
    }

    void SetToolLifeValue (int indexTool, String type, String current, String max)
    {
      try {
        // Type, current and limit
        var toolUnit = Lemoine.Core.SharedData.ToolUnit.Unknown;
        switch (int.Parse (type)) {
          case 0:
            toolUnit = Lemoine.Core.SharedData.ToolUnit.TimeSeconds;
            break;
          case 1:
            toolUnit = Lemoine.Core.SharedData.ToolUnit.NumberOfTimes;
            break;
          default:
            m_tld[indexTool].Properties["tool_life_type"] = type;
            Logger.ErrorFormat ("Mitsubishi.Interface_tool - Unknown tool life type '{0}' for system '{1}'", type, SystemType);
            break;
        }
        double currentValue = double.Parse (current);
        double maxValue = double.Parse (max);

        var indexToolLife = m_tld[indexTool].AddLifeDescription ();
        m_tld[indexTool][indexToolLife].LifeDirection = Lemoine.Core.SharedData.ToolLifeDirection.Up;
        m_tld[indexTool][indexToolLife].LifeType = toolUnit;
        m_tld[indexTool][indexToolLife].LifeValue = currentValue;
        m_tld[indexTool][indexToolLife].LifeLimit = maxValue;
      }
      catch (Exception ex) {
        Logger.ErrorFormat ("Mitsubishi.Interface_tool - Couldn't set tool life value '{0}' for the system '{1}': {2}",
                           "type " + type + ", current " + current + ", max " + max, SystemType, ex.Message);
      }
    }

    void SetToolLifeProperty (int indexTool, ToolLifeProperty property, String value)
    {
      try {
        switch (property) {
          case ToolLifeProperty.TOOL_NUMBER:
            m_tld[indexTool].ToolNumber = value;
            m_tld[indexTool].ToolId = value;
            break;
          case ToolLifeProperty.STATUS:
            // 0 to 3 or ...
            switch (int.Parse (value)) { // Not documented => to be validated
              case 0:
                m_tld[indexTool].ToolState = Lemoine.Core.SharedData.ToolState.Unknown;
                break;
              case 1:
                m_tld[indexTool].ToolState = Lemoine.Core.SharedData.ToolState.Available;
                break;
              case 2:
                m_tld[indexTool].ToolState = Lemoine.Core.SharedData.ToolState.Expired;
                break;
              default:
                m_tld[indexTool].Properties["status"] = value;
                Logger.ErrorFormat ("Mitsubishi.Interface_tool - Unknown status '{0}' used by the system '{1}'", value, SystemType);
                break;
            }
            break;
          case ToolLifeProperty.LENGTH:
            m_tld[indexTool].Properties["length"] = value;
            break;
          case ToolLifeProperty.RADIUS:
            m_tld[indexTool].Properties["radius"] = value;
            break;
        }
      }
      catch (Exception ex) {
        Logger.ErrorFormat ("Mitsubishi.Interface_tool - Couldn't use the value '{0}', as '{1}' for the system '{2}': {3}",
                           value, property, SystemType, ex.Message);
      }
    }
    #endregion Private methods
  }

  /// <summary>
  /// Description of Interface_tool_3.
  /// </summary>
  public class Interface_tool_3 : Interface_tool
  {

  }
}
