// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Interface_data.
  /// </summary>
  public class Interface_data: GenericInterface<HeidenhainDNCLib.IJHDataAccess3>
  {
    class ToolData {
      bool m_otherReasonNotToBeOk = false;
      int m_number = 0;
      bool m_numberProvided = false;
      public int Number {
        get { return m_number; }
        set {
          m_number = value;
          m_numberProvided = true;
          m_otherReasonNotToBeOk |= (value == 0);
        }
      }
      
      string m_name;
      bool m_nameProvided = false;
      public string Name {
        get { return m_name; }
        set {
          m_name = value;
          m_nameProvided = true;
          m_otherReasonNotToBeOk |= (value == "NULLWERKZEUG");
        }
      }
      
      double m_current;
      bool m_currentProvided = false;
      public double Current {
        get { return m_current; }
        set { m_current = value; m_currentProvided = true; }
      }
      
      public double? warning;
      public double? limit;
      public double? compensationD;
      public double? compensationH;
      
      public bool IsValid { get { return m_numberProvided && m_nameProvided && m_currentProvided && !m_otherReasonNotToBeOk; } }
      public IList<string> MissingVariables {
        get {
          var list = new List<string>();
          
          if (!m_numberProvided) {
            list.Add("T");
          }

          if (!m_nameProvided) {
            list.Add("NAME");
          }

          if (!m_currentProvided) {
            list.Add("CUR_TIME or CUR.TIME");
          }

          return list;
        }
      }
    }
    
    #region Members
    ToolLifeData m_toolLifeData = null;
    IList<string> m_toolMissingVariables = new List<string>();
    readonly IList<string> m_toolAvailableVariables = new List<string>();
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Interface_data() : base(HeidenhainDNCLib.DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHDATAACCESS) {}
    #endregion // Constructors

    #region Protected methods
    /// <summary>
    /// Extra work during the initialization
    /// Already within a try catch
    /// </summary>
    protected override void InitializeInstance(InterfaceParameters parameters)
    {
      // Set the passwords
      foreach (HeidenhainDNCLib.DNC_ACCESS_MODE accessMode in Enum.GetValues(typeof(HeidenhainDNCLib.DNC_ACCESS_MODE))) {
        var password = parameters.GetPassword(accessMode);
        if (!String.IsNullOrEmpty(password)) {
          // Try catch here because we need to fill all the passwords, even if one of them is wrong
          try {
            m_interface.SetAccessMode(accessMode, password);
          } catch (Exception ex) {
            Logger.ErrorFormat("HeidenhainDNC - couldn't set the password {0} for the access mode {1}: {2}", password, accessMode, ex);
          }
        }
      }
    }
    
    /// <summary>
    /// Work to do on "start", before new data will be read
    /// Already within a try catch
    /// </summary>
    protected override void ResetDataInstance()
    {
      m_toolLifeData = null;
    }
    #endregion // Protected methods
    
    #region Get methods
    /// <summary>
    /// Get a data corresponding to a param
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public T GetData<T>(string param)
    {
      Logger.InfoFormat("HeidenhainDNC.GetData try to read " + param);
      
      var dataEntry = m_interface.GetDataEntry(param);
      var result = GetValueFromDataEntry<T>(dataEntry);
      
      // Free the IJHDataEntry
      foreach (HeidenhainDNCLib.JHDataEntryProperty property in dataEntry.propertyList) {
        Marshal.ReleaseComObject(property);
      }
      Marshal.ReleaseComObject(dataEntry);
      
      // Return the result
      return result;
    }
    
    /// <summary>
    /// Return the current tool life data
    /// </summary>
    public ToolLifeData ToolLifeData {
      get {
        if (m_toolLifeData == null) {
          ReadToolLifeData ();
        }

        return m_toolLifeData;
      }
    }
    #endregion // Get methods
    
    #region Private methods
    T GetValueFromDataEntry<T>(HeidenhainDNCLib.IJHDataEntry dataEntry)
    {
      // Throw an exception if the result is a node
      if (dataEntry.bIsNode) {
        Logger.Error("HeidenhainDNC.GetData - " + dataEntry.bstrFullName + " is a node");
        throw new Exception("HeidenhainDNC.GetData - " + dataEntry.bstrFullName + " is a node");
      }
      
      // Find the property kind DATA
      var dataEntryPropertyList = dataEntry.propertyList;
      HeidenhainDNCLib.IJHDataEntryProperty dataEntryProperty = dataEntry.propertyList
        .get_property(HeidenhainDNCLib.DNC_DATAENTRY_PROPKIND.DNC_DATAENTRY_PROPKIND_DATA);
      try {
        return CustomConvert<T> (dataEntryProperty.varValue);
      } catch (Exception) {
        string txt = String.Format("HeidenhainDNC.GetData, couldn't cast {0} of type {1} into a {2}",
                                   dataEntryProperty.varValue, dataEntryProperty.varValue.GetType(), typeof(T));
        Logger.Error(txt);
        throw new Exception(txt);
      }
    }
    
    T CustomConvert<T>(object obj)
    {
      T val;
      if (typeof(T) == typeof(double) || typeof(T) == typeof(int) || typeof(T) == typeof(bool) || typeof(T) == typeof(string)) {
        val = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(obj.ToString());
      }
      else {
        val = (T)obj;
      }

      return val;
    }
    
    /// <summary>
    /// Tool life variables described in "DIN ISO PROGRAMMING" §5.2
    /// </summary>
    void ReadToolLifeData()
    {
      if (m_toolAvailableVariables.Count > 0 && m_toolMissingVariables.Count > 0) {
        string msg = String.Format("HeidenhainDNC.GetData - missing variable(s) {0} for reading tool life data. Available variables are {1}.",
                                   string.Join(", ", m_toolMissingVariables.ToArray()),
                                   string.Join(", ", m_toolAvailableVariables.ToArray()));
        Logger.Error(msg);
        throw new Exception(msg);
      }
      var tld = new ToolLifeData();
      
      HeidenhainDNCLib.IJHDataEntry toolTable = null;
      HeidenhainDNCLib.IJHDataEntryList toolLines = null;
      HeidenhainDNCLib.IJHDataEntry toolLine = null;
      HeidenhainDNCLib.IJHDataEntryList toolCells = null;
      HeidenhainDNCLib.IJHDataEntry toolCell = null;

      try {
        toolTable = m_interface.GetDataEntry(@"\TABLE\TOOL\T");
        toolLines = toolTable.childList;
        int toolLinesCount = toolLines.Count;

        int toolNumber = 0;
        var variableList = new List<string>();
        for (int i = 0; i < toolLinesCount; i++)
        {
          bool fillVariablesName = (m_toolAvailableVariables.Count == 0);
          toolLine = toolLines[i];
          var toolDataTmp = new ToolData();
          
          // Browse all data from the tool
          toolCells = toolLine.childList;
          for (int j = 0; j < toolCells.Count; j++) {
            toolCell = toolCells[j];
            string variableName = toolCell.bstrName;
            if (fillVariablesName) {
              m_toolAvailableVariables.Add(variableName);
            }

            switch (variableName) {
              case "T":
                toolDataTmp.Number = GetValueFromDataEntry<int>(toolCell);
                break;
              case "NAME":
                toolDataTmp.Name = GetValueFromDataEntry<string>(toolCell);
                break;
              case "L":
                toolDataTmp.compensationH = GetValueFromDataEntry<double>(toolCell);
                break;
              case "R":
                toolDataTmp.compensationD = GetValueFromDataEntry<double>(toolCell);
                break;
              case "TIME1":
                toolDataTmp.limit = GetValueFromDataEntry<double>(toolCell);
                break;
              case "TIME2":
                toolDataTmp.warning = GetValueFromDataEntry<double>(toolCell);
                break;
              case "CUR_TIME": case "CUR.TIME":
                toolDataTmp.Current = GetValueFromDataEntry<double>(toolCell);
                break;
            }
            
            if (toolCell != null) {
              Marshal.ReleaseComObject(toolCell);
              toolCell = null;
            }
          }
          if (fillVariablesName) {
            m_toolMissingVariables = toolDataTmp.MissingVariables;
          }

          // Valid tool?
          if (toolDataTmp.IsValid) {
            tld.AddTool();
            tld[toolNumber].PotNumber = i + 1;
            tld[toolNumber].ToolId = toolDataTmp.Name;
            tld[toolNumber].ToolNumber = toolDataTmp.Number.ToString();
            tld[toolNumber].SetProperty("CutterCompensation", toolDataTmp.compensationD);
            tld[toolNumber].SetProperty("LengthCompensation", toolDataTmp.compensationH);
            tld[toolNumber].ToolState = Lemoine.Core.SharedData.ToolState.Available;
            
            tld[toolNumber].AddLifeDescription();
            tld[toolNumber][0].LifeValue = toolDataTmp.Current * 60; // Convert to seconds
            tld[toolNumber][0].LifeDirection = Lemoine.Core.SharedData.ToolLifeDirection.Up;
            if (toolDataTmp.limit.HasValue) {
              tld[toolNumber][0].LifeLimit = toolDataTmp.limit.Value * 60;
              if (toolDataTmp.warning.HasValue) {
                tld[toolNumber][0].LifeWarningOffset = toolDataTmp.limit.Value * 60 -
                  toolDataTmp.warning.Value * 60;
              }
            }
            
            tld[toolNumber][0].LifeType = Lemoine.Core.SharedData.ToolUnit.TimeSeconds;
            toolNumber++;
          }

          if (toolLine != null) {
            Marshal.ReleaseComObject(toolLine);
          }

          toolLine = null;
          if (toolCells != null) {
            Marshal.ReleaseComObject(toolCells);
          }

          toolCells = null;
        }
      }
      catch (Exception)
      {
        if (toolTable != null) {
          Marshal.ReleaseComObject(toolTable);
        }

        if (toolLines != null) {
          Marshal.ReleaseComObject(toolLines);
        }

        if (toolLine != null) {
          Marshal.ReleaseComObject(toolLine);
        }

        if (toolCells != null) {
          Marshal.ReleaseComObject(toolCells);
        }

        if (toolCell != null) {
          Marshal.ReleaseComObject(toolCell);
        }

        throw;
      }
      
      if (toolTable != null) {
        Marshal.ReleaseComObject(toolTable);
      }

      if (toolLines != null) {
        Marshal.ReleaseComObject(toolLines);
      }

      if (toolLine != null) {
        Marshal.ReleaseComObject(toolLine);
      }

      if (toolCells != null) {
        Marshal.ReleaseComObject(toolCells);
      }

      if (toolCell != null) {
        Marshal.ReleaseComObject(toolCell);
      }

      m_toolLifeData = tld;
    }
    #endregion // Private methods
  }
}

