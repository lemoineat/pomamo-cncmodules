// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// HeidenhainDNC input module
  /// </summary>
  public partial class HeidenhainDNC
  {
    #region Properties
    /// <summary>
    /// Access to Access to \TABLE\'\USR\table file path'\... (is set by default)
    /// </summary>
    public string DataUserPwd
    {
      get { return m_interfaceManager.GetPassword (HeidenhainDNCLib.DNC_ACCESS_MODE.DNC_ACCESS_MODE_USR); }
      set { m_interfaceManager.SetPassword (HeidenhainDNCLib.DNC_ACCESS_MODE.DNC_ACCESS_MODE_USR, value); }
    }

    /// <summary>
    /// Access to \TABLE\'\OEM\table file path'\... and \CFG\...,
    /// OEM protection level; Password required
    /// </summary>
    public string DataOemPwd
    {
      get { return m_interfaceManager.GetPassword (HeidenhainDNCLib.DNC_ACCESS_MODE.DNC_ACCESS_MODE_OEM); }
      set { m_interfaceManager.SetPassword (HeidenhainDNCLib.DNC_ACCESS_MODE.DNC_ACCESS_MODE_OEM, value); }
    }

    /// <summary>
    /// Access to \TABLE\'\OEME\table file path'\..., Password required
    /// </summary>
    public string DataOemEncryptedPwd
    {
      get { return m_interfaceManager.GetPassword (HeidenhainDNCLib.DNC_ACCESS_MODE.DNC_ACCESS_MODE_OEM_ENCRYPTED); }
      set { m_interfaceManager.SetPassword (HeidenhainDNCLib.DNC_ACCESS_MODE.DNC_ACCESS_MODE_OEM_ENCRYPTED, value); }
    }

    /// <summary>
    /// Access to \TABLE\'\SYS\table file path'\..., Password required
    /// </summary>
    public string DataSysPwd
    {
      get { return m_interfaceManager.GetPassword (HeidenhainDNCLib.DNC_ACCESS_MODE.DNC_ACCESS_MODE_SYS); }
      set { m_interfaceManager.SetPassword (HeidenhainDNCLib.DNC_ACCESS_MODE.DNC_ACCESS_MODE_SYS, value); }
    }

    /// <summary>
    /// Access to \PLC\... (R/W), Password required
    /// </summary>
    public string DataPlcPwd
    {
      get { return m_interfaceManager.GetPassword (HeidenhainDNCLib.DNC_ACCESS_MODE.DNC_ACCESS_MODE_PLCDATAACCESS); }
      set { m_interfaceManager.SetPassword (HeidenhainDNCLib.DNC_ACCESS_MODE.DNC_ACCESS_MODE_PLCDATAACCESS, value); }
    }

    /// <summary>
    /// Access to \SPLC\... (RO)
    /// </summary>
    public string DataSplcPwd
    {
      get { return m_interfaceManager.GetPassword (HeidenhainDNCLib.DNC_ACCESS_MODE.DNC_ACCESS_MODE_SPLCDATAACCESS); }
      set { m_interfaceManager.SetPassword (HeidenhainDNCLib.DNC_ACCESS_MODE.DNC_ACCESS_MODE_SPLCDATAACCESS, value); }
    }

    /// <summary>
    /// Access to \GEO\... (RO)
    /// </summary>
    public string DataGeoPwd
    {
      get { return m_interfaceManager.GetPassword (HeidenhainDNCLib.DNC_ACCESS_MODE.DNC_ACCESS_MODE_GEODATAACCESS); }
      set { m_interfaceManager.SetPassword (HeidenhainDNCLib.DNC_ACCESS_MODE.DNC_ACCESS_MODE_GEODATAACCESS, value); }
    }

    /// <summary>
    /// Access to \CFG\... For write access password required
    /// </summary>
    public string DataCfgPwd
    {
      get { return m_interfaceManager.GetPassword (HeidenhainDNCLib.DNC_ACCESS_MODE.DNC_ACCESS_MODE_CFGDATAACCESS); }
      set { m_interfaceManager.SetPassword (HeidenhainDNCLib.DNC_ACCESS_MODE.DNC_ACCESS_MODE_CFGDATAACCESS, value); }
    }

    /// <summary>
    /// Access to \TABLE\... (R/W)
    /// </summary>
    public string DataTablePwd
    {
      get { return m_interfaceManager.GetPassword (HeidenhainDNCLib.DNC_ACCESS_MODE.DNC_ACCESS_MODE_TABLEDATAACCESS); }
      set { m_interfaceManager.SetPassword (HeidenhainDNCLib.DNC_ACCESS_MODE.DNC_ACCESS_MODE_TABLEDATAACCESS, value); }
    }

    /// <summary>
    /// Multiplier when retrieving data
    /// </summary>
    public int Multiplier { get; set; }
    #endregion // Properties

    #region Get methods
    /// <summary>
    /// Get a data, return a bool
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public bool GetBoolData (string param)
    {
      bool result;
      try {
        result = m_interfaceManager.InterfaceData.GetData<bool> (param);
      }
      catch (Exception ex) {
        ProcessException (ex, "HeidenhainDNC", "GetBoolData", true);
        throw;
      }
      return result;
    }

    /// <summary>
    /// Get a data, return an int
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public int GetIntData (string param)
    {
      int result;
      try {
        result = m_interfaceManager.InterfaceData.GetData<int> (param);
        ;
      }
      catch (Exception ex) {
        ProcessException (ex, "HeidenhainDNC", "GetIntData", true);
        throw;
      }
      return result;
    }

    /// <summary>
    /// Get a data, return a double
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public double GetDoubleData (string param)
    {
      double result;
      try {
        result = m_interfaceManager.InterfaceData.GetData<double> (param);
      }
      catch (Exception ex) {
        ProcessException (ex, "HeidenhainDNC", "GetDoubleData", true);
        throw;
      }
      return result;
    }

    /// <summary>
    /// Get a data, return a string
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public string GetStringData (string param)
    {
      string result;
      try {
        result = m_interfaceManager.InterfaceData.GetData<string> (param);
      }
      catch (Exception ex) {
        ProcessException (ex, "HeidenhainDNC", "GetStringData", true);
        throw;
      }
      return result;
    }

    /// <summary>
    /// Get a data, return an int multiplied by the property Multiplier
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public int GetDataWithMultiplier (string param)
    {
      return (int)(GetDoubleData (param) * Multiplier);
    }

    /// <summary>
    /// Tool life data
    /// </summary>
    public ToolLifeData ToolLifeData
    {
      get
      {
        ToolLifeData result;
        try {
          result = m_interfaceManager.InterfaceData.ToolLifeData;
        }
        catch (Exception ex) {
          ProcessException (ex, "HeidenhainDNC", "ToolLifeData", true);
          throw;
        }
        return result;
      }
    }
    #endregion // Get methods
  }
}