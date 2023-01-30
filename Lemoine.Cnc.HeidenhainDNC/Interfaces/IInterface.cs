// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using Lemoine.Core.Log;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of IInterface.
  /// </summary>
  public interface IInterface
  {
    /// <summary>
    /// True if it happened that we had this interface valid, False if never, null if we never checked
    /// </summary>
    bool? ValidInThePast { get; }
    
    /// <summary>
    /// Logger
    /// </summary>
    ILog Logger { get; set; }
    
    /// <summary>
    /// Return true if the interface can be used
    /// </summary>
    bool Valid { get; }
    
    /// <summary>
    /// Initialize the interface
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    bool Initialize(HeidenhainDNCLib.JHMachineInProcess machine, InterfaceParameters parameters);
    
    /// <summary>
    /// Reinitialize data, preceeding a new acquisition
    /// </summary>
    void ResetData();
    
    /// <summary>
    /// Close the interface and free the resources
    /// </summary>
    void Close();
  }
}
