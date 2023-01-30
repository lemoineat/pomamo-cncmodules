// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Interface_configuration.
  /// </summary>
  public class Interface_configuration: GenericInterface<HeidenhainDNCLib.IJHConfiguration>
  {
    #region Members
    readonly IDictionary<int, IDictionary<string, Tuple<HeidenhainDNCLib.DNC_AXISTYPE, int>>> m_channelInfo =
      new Dictionary<int, IDictionary<string, Tuple<HeidenhainDNCLib.DNC_AXISTYPE, int>>> ();
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Interface_configuration() : base(HeidenhainDNCLib.DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHCONFIGURATION) {}
    #endregion // Constructors

    #region Protected methods
    /// <summary>
    /// Extra work during the initialization
    /// Already within a try catch
    /// </summary>
    protected override void InitializeInstance(InterfaceParameters parameters)
    {
      // Reset data
      m_channelInfo.Clear();
      
      // Browse all channels
      HeidenhainDNCLib.JHChannelInfoList channelInfoList = m_interface.GetChannelInfo();
      foreach (HeidenhainDNCLib.IJHChannelInfo channelInfo in channelInfoList) {
        m_channelInfo[channelInfo.lChannelId] = new Dictionary<string, Tuple<HeidenhainDNCLib.DNC_AXISTYPE, int>> ();
        
        // Browse all axes
        var axisInfoList = channelInfo.pAxisInfoList;
        foreach (HeidenhainDNCLib.IJHAxisInfo axisInfo in axisInfoList) {
          m_channelInfo[channelInfo.lChannelId][axisInfo.bstrAxisName.ToLower()] =
            new Tuple<HeidenhainDNCLib.DNC_AXISTYPE, int>(axisInfo.axisType, axisInfo.lAxisId);
          Marshal.ReleaseComObject(axisInfo);
        }
        Marshal.ReleaseComObject(channelInfo);
      }
      Marshal.ReleaseComObject(channelInfoList);
    }
    
    /// <summary>
    /// Work to do on "start", before new data will be read
    /// Already within a try catch
    /// </summary>
    protected override void ResetDataInstance()
    {
      // Nothing
    }
    #endregion // Protected methods
    
    #region Get methods
    /// <summary>
    /// Return a formatted string to show all axes referenced in the control
    /// </summary>
    public string AllAxes {
      get {
        string str = "";
        foreach (int channelId in m_channelInfo.Keys) {
          str += "[channel " + channelId + ": ";
          foreach (string axisName in m_channelInfo[channelId].Keys) {
            str += axisName + m_channelInfo[channelId][axisName].Item2 + ",";
          }

          str = str.TrimEnd(',') + "]";
        }
        return str;
      }
    }
    
    /// <summary>
    /// Return the id of a specific axis, or -1 if not found
    /// </summary>
    /// <param name="axisName"></param>
    /// <param name="channel"></param>
    /// <returns></returns>
    public int GetAxisId(string axisName, int channel)
    {
      if (!m_channelInfo.Keys.Contains(channel)) {
        return -1;
      }

      foreach (string name in m_channelInfo[channel].Keys) {
        if (string.Equals(name, axisName, StringComparison.InvariantCultureIgnoreCase)) {
          return m_channelInfo[channel][name].Item2;
        }
      }

      return -1;
    }
    #endregion // Get methods
  }
}
