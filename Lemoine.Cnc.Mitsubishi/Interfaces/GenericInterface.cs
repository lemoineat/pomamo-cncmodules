// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using Lemoine.Core.Log;
using EZNCAUTLib;


namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of GenericInterface.
  /// </summary>
  public abstract class GenericMitsubishiInterface : IMitsubishiInterface
  {
    #region Getters / Setters
    /// <summary>
    /// Logger
    /// </summary>
    static public ILog Logger { get; set; }

    /// <summary>
    /// Communication object
    /// </summary>
    static public DispEZNcCommunication CommunicationObject { get; set; }

    /// <summary>
    /// System type
    /// </summary>
    static public Mitsubishi.MitsubishiSystemType SystemType { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    protected GenericMitsubishiInterface () { }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Reinitialize data, preceeding a new acquisition
    /// </summary>
    public void ResetData ()
    {
      try {
        ResetDataInstance ();
      }
      catch (Exception ex) {
        Logger.ErrorFormat ("Mitsubishi - Couldn't reset data for interface '{0}': {1}", this.GetType ().Name, ex.Message);
      }
    }
    #endregion // Methods

    #region Methods to override
    /// <summary>
    /// Work to do on "start", before new data will be read
    /// Already within a try catch
    /// </summary>
    protected abstract void ResetDataInstance ();
    #endregion // Methods
  }
}
