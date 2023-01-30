// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Threading;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Mitsubishi input module
  /// </summary>
  public sealed partial class Mitsubishi : BaseCncModule, ICncModule, IDisposable
  {
    /// <summary>
    /// System types based on EZSocketNcDef.h
    /// </summary>
    public enum MitsubishiSystemType
    {
      /// <summary>
      /// Unknown system
      /// </summary>
      UNKNOWN = -1,

      /// <summary>
      /// EZNC_SYS_MAGICCARD64
      /// </summary>
      MAGIC_CARD_64 = 0,

      /// <summary>
      /// EZNC_SYS_MAGICBOARD64
      /// </summary>
      MAGIC_BOARD_64 = 1,

      /// <summary>
      /// EZNC_SYS_MELDAS6X5L
      /// </summary>
      MELDAS_600L_6X5L = 2,

      /// <summary>
      /// EZNC_SYS_MELDAS6X5M
      /// </summary>
      MELDAS_600M_6X5M = 3,

      /// <summary>
      /// EZNC_SYS_MELDASC6C64
      /// </summary>
      MELDAS_C6_C64 = 4,

      /// <summary>
      /// EZNC_SYS_MELDAS700L
      /// </summary>
      MELDAS_700L = 5,

      /// <summary>
      /// EZNC_SYS_MELDAS700M
      /// </summary>
      MELDAS_700M = 6,

      /// <summary>
      /// EZNC_SYS_MELDASC70
      /// </summary>
      MELDAS_C70 = 7,

      /// <summary>
      /// EZNC_SYS_MELDAS800L
      /// </summary>
      MELDAS_800L = 8,

      /// <summary>
      /// EZNC_SYS_MELDAS800M
      /// </summary>
      MELDAS_800M = 9
    };

    #region Members
    readonly InterfaceManager m_interfaceManager = null;
    #endregion

    #region Getters / Setters
    /// <summary>
    /// May not be necessary. IP address or hostName of the CNC
    /// </summary>
    public string HostAddress { get; set; }

    /// <summary>
    /// May not be necessary. Default is 64758 for C64 and 683 for CNC700
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Possible values are listed in MitsubishiSystemType
    /// </summary>
    public string SystemType
    {
      get {
        return m_systemTypeStr;
      }
      set {
        m_systemTypeStr = value;
        try {
          m_interfaceManager.SystemType = (MitsubishiSystemType)Enum.Parse (typeof (MitsubishiSystemType), m_systemTypeStr);
        }
        catch (Exception ex) {
          log.Error ($"SystenType: unknown system type '{m_systemTypeStr}'", ex);
          m_interfaceManager.SystemType = MitsubishiSystemType.UNKNOWN;
        }
      }
    }
    string m_systemTypeStr = "";

    /// <summary>
    /// Number from 1 to 255
    /// </summary>
    public int NcCardNumber
    {
      get {
        return m_ncCardNumber;
      }
      set {
        if (value < 1 || value > 255) {
          throw new Exception ("Mitsubishi - NcCardNumber must be in [1 ; 255] but is " + value);
        }

        m_ncCardNumber = value;
      }
    }
    int m_ncCardNumber = 1;

    /// <summary>
    /// Set part system or PLC axis part system
    /// When the value is 0, it means that no part system is designated
    /// To set PLC axis part system, set 255 (EZNC_PLCAXIS)
    /// </summary>
    public int HeadNumber
    {
      get { return m_headNumber; }
      set { m_headNumber = value; }
    }
    int m_headNumber = -1;

    /// <summary>
    /// Connection error
    /// </summary>
    public bool ConnectionError { get; set; }
    #endregion

    #region Constructors, destructor
    /// <summary>
    /// Default constructor
    /// </summary>
    public Mitsubishi () : base ("Lemoine.Cnc.In.Mitsubishi")
    {
      m_interfaceManager = new InterfaceManager ();
      ConnectionError = false;
    }

    /// <summary>
    /// <see cref="IDisposable.Dispose" />
    /// </summary>
    public void Dispose ()
    {
      GC.SuppressFinalize (this);
      m_interfaceManager.Close ();
    }
    #endregion // Constructor, destructor

    #region Methods
    /// <summary>
    /// Start method
    /// </summary>
    public bool Start ()
    {
      log.Info ("Start");

      if (string.IsNullOrEmpty (this.HostAddress)) {
        log.Error ("Start: The host address must be provided");
        return false;
      }

      // Logger
      m_interfaceManager.Logger = log;

      // Initialize the communication object and check the interfaces
      bool ret = m_interfaceManager.Start ();
      if (!ret) {
        ConnectionError = true;
        return false;
      }

      // Connection to the machine if it's not done
      if (!m_interfaceManager.ConnectionOpen) {
        try {
          log.Info ("Start: Opening the connection");
          m_interfaceManager.InterfaceCommunication.Open (HostAddress, Port, NcCardNumber, HeadNumber);
          m_interfaceManager.ConnectionOpen = true;
        }
        catch (Exception ex) {
          ConnectionError = true;
          log.Error ($"Start: opening the connection failed", ex);
          ProcessException (ex);
          return false;
        }
      }

      // Reinitialize data
      m_interfaceManager.ResetData ();

      ConnectionError = false;
      return true;
    }

    void ProcessException (Exception ex)
    {
      if (ex is NotReadyException) {
        // Just an info
        var nrEx = ex as NotReadyException;
        log.Info (nrEx.Message);
      }
      else {
        // Possibly reinitialize the module depending on the error number
        if (ex is ErrorCodeException) {
          ConnectionError = true;
          var errorNumber = (ex as ErrorCodeException).ErrorNumber;
          if (errorNumber == 0x8202000A // Not connected
            || errorNumber == 0x80040196 // Application does not fit into prepared buffer
            || errorNumber == 0x81008001 // Unknown but we were stuck here
            || errorNumber == 0x80050D04 // Unknown but we were stuck here
          ) {
            log.Fatal ($"ProcessException: close the interface due to the error: {errorNumber}", ex);
            m_interfaceManager.Close ();
          }
          else if (errorNumber == 0x80B00304 // We were stuck on "No submodule" also (don't know what it is)
            || errorNumber == 0x80010105 // RPC_E_SERVERFAULT
            ) { // Suicide!
            log.Fatal ($"ProcessException: error={errorNumber} message={ex.Message} => exit", ex);
            Lemoine.Core.Environment.LogAndForceExit (ex, log);
            Thread.Sleep (Timeout.Infinite);
          }
        }

        // Log what happened and throw a new exception
        log.Error ($"ProcessException: message={ex.Message}", ex);
      }
    }
    #endregion // Methods
  }
}
