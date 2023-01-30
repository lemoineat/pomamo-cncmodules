// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Interface_position.
  /// </summary>
  public class Interface_position: GenericInterface<HeidenhainDNCLib.JHAxesPositionStreaming>
  {
    #region Members
    bool m_positionsInitialized = false;
    readonly IDictionary<int, double> m_positions = new Dictionary<int, double>();
    readonly IList<int> m_streamedChannels = new List<int>();
    
    Array m_timeStamp = new DateTime[1];
    Array m_lineNumber = new Int32[1];
    Array m_feedMode = new HeidenhainDNCLib.DNC_FEED_MODE[1];
    Array m_spindleState = new HeidenhainDNCLib.DNC_SPINDLE_STATE[1];
    Array m_motionType = new HeidenhainDNCLib.DNC_POSSAMPLES_MOTION_TYPE[1];
    Array m_blockEndpoint = new bool[1];
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Interface_position() : base(HeidenhainDNCLib.DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHAXESPOSITIONSTREAMING) {}
    #endregion // Constructors

    #region Protected methods
    /// <summary>
    /// Extra work during the initialization
    /// Already within a try catch
    /// </summary>
    protected override void InitializeInstance(InterfaceParameters parameters)
    {
      m_streamedChannels.Clear();
      m_interface.SetNotificationTimeLimit(1000);
    }
    
    /// <summary>
    /// Work to do on "start", before new data will be read
    /// Already within a try catch
    /// </summary>
    protected override void ResetDataInstance()
    {
      m_positionsInitialized = false;
    }
    #endregion // Protected methods
    
    #region Get methods
    /// <summary>
    /// Get all positions
    /// </summary>
    /// <returns></returns>
    public IDictionary<int, double> GetPositions()
    {
      // Update the channels that have to be read
      const int channel = 0;
      if (!m_streamedChannels.Contains(channel)) {
        m_streamedChannels.Add(channel); // /!\ only one channel possible for i530
        m_interface.StartStreaming(channel);
      }
      
      // Initialize if necessary
      if (!m_positionsInitialized) {
        InitializePositions ();
      }

      if (!m_positionsInitialized) {
        throw new Exception("HeidenheinDNC:Position - Couldn't initialize the positions");
      }

      return m_positions;
    }
    #endregion // Get methods
    
    #region Private methods
    void InitializePositions()
    {
      HeidenhainDNCLib.JHAxisPositionListList listPositionSamples = m_interface.GetAxesPositionSamples(
        ref m_timeStamp, ref m_lineNumber, ref m_feedMode,
        ref m_spindleState, ref m_motionType, ref m_blockEndpoint);
      
      if (listPositionSamples.Count == 0) {
        FreeJHAxisPositionListList(listPositionSamples);
        throw new Exception("HeidenhainDNC:InitializePositions - No new positions");
      }
      
      // Get the last sample set
      var positionSamples = listPositionSamples[listPositionSamples.Count - 1];
      foreach (HeidenhainDNCLib.IJHAxisPosition positionSample in positionSamples) {
        m_positions[positionSample.lAxisId] = positionSample.dPosition;
      }

      FreeJHAxisPositionListList (listPositionSamples);
      
      m_positionsInitialized = true;
    }
    
    void FreeJHAxisPositionListList(HeidenhainDNCLib.JHAxisPositionListList positionListList)
    {
      foreach (HeidenhainDNCLib.IJHAxisPositionList positionList in positionListList) {
        foreach (HeidenhainDNCLib.IJHAxisPosition position in positionList) {
          Marshal.ReleaseComObject(position);
        }

        Marshal.ReleaseComObject(positionList);
      }
      Marshal.ReleaseComObject(positionListList);
    }
    #endregion // Private methods
  }
}
