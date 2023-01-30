// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.IO;
using System.Reflection;
using Lemoine.Info;
using System.Diagnostics;

namespace Lemoine.Cnc
{
  enum MmlConnection
  {
    ProX,
    Cnc,
  }

  /// <summary>
  /// Makino MML input module
  /// </summary>
  public partial class MML3 : BaseCncModule, ICncModule, IDisposable
  {
    #region Members
    string m_ipAddress = "";
    int m_portNumber = 11212;
    string m_cncIpAddress = "";
    int m_cncPortNumber = 8193;

    bool m_disposed = false; // Track whether Dispose has been called.
    IMd3ProX m_md3ProX = null; // This will be found automatically
    ConnectionManager m_connectionManager = new ConnectionManager ();

    UInt32 m_proXhandle = 0;

    UInt32 m_cncHandle = 0;

    readonly TimeSpan SEND_TIMEOUT = TimeSpan.FromSeconds (10);
    readonly TimeSpan REPLY_TIMEOUT = TimeSpan.FromSeconds (10);
    readonly TimeSpan NOOP_CYCLE = TimeSpan.FromSeconds (120);
    readonly byte LOG_LEVEL = 0;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// ProX version: 3, 5 or 6
    /// 
    /// 0 (default): try to guess it (try first Pro6, then Pro5, then Pro3)
    /// </summary>
    public int ProXVersion { get; set; } = 0;

    /// <summary>
    /// Node number
    /// 0- 7: HSSB
    /// 8-99: Ethernet
    /// </summary>
    public int NodeNumber { get; set; }

    /// <summary>
    /// Device name or ip address of the machine
    /// MUST be provided
    /// </summary>
    public string IpAddress
    {
      get
      {
        if (string.IsNullOrEmpty (m_ipAddress)) {
          log.Fatal ("IpAddress.get: empty or empty");
        }
        return m_ipAddress;
      }
      set
      {
        Debug.Assert (!string.IsNullOrEmpty (value));
        if (string.IsNullOrEmpty (value)) {
          log.Fatal ("IpAddress.set: trying to set an empty IP address");
        }
        m_ipAddress = value;
      }
    }

    /// <summary>
    /// Port number (standard is 11212)
    /// </summary>
    public int Port
    {
      get { return m_portNumber; }
      set { m_portNumber = value; }
    }

    /// <summary>
    /// Device name or ip address of the machine, CNC part
    /// 
    /// By Default, consider the same IP address
    /// </summary>
    public string CncIpAddress
    {
      get
      {
        if (string.IsNullOrEmpty (m_cncIpAddress)) {
          return this.IpAddress;
        }
        else {
          return m_cncIpAddress;
        }
      }
      set
      {
        m_cncIpAddress = value;
      }
    }

    /// <summary>
    /// Cnc Port number (standard is 8193)
    /// </summary>
    public int CncPort
    {
      get { return m_cncPortNumber; }
      set { m_cncPortNumber = value; }
    }

    /// <summary>
    /// False is release mode
    /// True is emulate mode
    /// Default is false and shouldn't be changed
    /// </summary>
    public bool Emulate { get; set; }

    /// <summary>
    /// Does a connection to the control end in error? (ProX part)
    /// </summary>
    public bool ConnectionErrorProX
    {
      get
      {
        try {
          CheckProXConnection ();
        }
        catch (Exception) {
          return true;
        }

        return false;
      }
    }

    /// <summary>
    /// Does a connection to the control end in error? (Cnc part)
    /// </summary>
    public bool ConnectionErrorCnc
    {
      get
      {
        try {
          CheckCncConnection ();
        }
        catch (Exception) {
          return true;
        }

        return false;
      }
    }

    string NodeInfo
    {
      get
      {
        return string.Format ("{0}/{1}/{2}/{3}",
                             this.NodeNumber,
                             this.IpAddress,
                             this.Port,
                             this.Emulate ? "1" : "0");
      }
    }

    string CncNodeInfo
    {
      get
      {
        return string.Format ("{0}/{1}/{2}/{3}",
                             this.NodeNumber,
                             this.CncIpAddress,
                             this.CncPort,
                             this.Emulate ? "1" : "0");
      }
    }

    /// <summary>
    /// Version of the MML3 library, found automatically
    /// 
    /// -1 is returned if it is not known yet
    /// </summary>
    public int Md3ProVersion
    {
      get
      {
        if (null == m_md3ProX) {
          return -1;
        }
        else {
          return m_md3ProX.Version;
        }
      }
    }
    #endregion // Getters / Setters

    #region Constructor, destructor
    /// <summary>
    /// Default constructor
    /// </summary>
    public MML3 () : base ("Lemoine.Cnc.In.MML3")
    {
      // Default values
      this.NodeNumber = 8;
      this.Emulate = false;

      SEND_TIMEOUT = ConfigSet.LoadAndGet<TimeSpan> ("Makino.SendTimeout", TimeSpan.FromSeconds (10));
      REPLY_TIMEOUT = ConfigSet.LoadAndGet<TimeSpan> ("Makino.ReplyTimeout", TimeSpan.FromSeconds (10));
      // Transmission cycle duration of the NOOP message
      NOOP_CYCLE = ConfigSet.LoadAndGet<TimeSpan> ("Makino.NoopCycle", TimeSpan.FromSeconds (120));

      // Possible options for the log level:
      //  0: no write to file
      //  1: basic information of send/receive data (label, datetime, ...)
      //  2: response log of send/receive data (succeeded/failed)
      //  4: Dump information of the body from send/receive data
      LOG_LEVEL = (byte)ConfigSet.LoadAndGet<int> ("Makino.LogLevel", 0);
    }

    /// <summary>
    /// Destructor: free unmanaged resources in case Dispose has not been called
    /// </summary>
    ~MML3 ()
    {
      Dispose (false);
    }

    /// <summary>
    /// Dispose method to free Fanuc resources
    /// Do not make this method virtual.
    /// A derived class should not be able to override this method.
    /// <see cref="IDisposable.Dispose" />
    /// </summary>
    public void Dispose ()
    {
      Dispose (true);
      // This object will be cleaned up by the Dispose method.
      // Therefore, you should call GC.SupressFinalize to
      // take this object off the finalization queue
      // and prevent finalization code for this object
      // from executing a second time.
      GC.SuppressFinalize (this);
    }

    /// <summary>
    /// Dispose(bool disposing) executes in two distinct scenarios.
    /// If disposing equals true, the method has been called directly
    /// or indirectly by a user's code. Managed and unmanaged resources
    /// can be disposed.
    /// If disposing equals false, the method has been called by the
    /// runtime from inside the finalizer and you should not reference
    /// other objects. Only unmanaged resources can be disposed.
    /// 
    /// Note all the variables are not really needed here.
    /// But they are nevertheless here because this class could be used an example
    /// for other classes that could need them.
    /// </summary>
    /// <param name="disposing">Dispose also the managed resources</param>
    protected virtual void Dispose (bool disposing)
    {
      // Check to see if Dispose has already been called.
      if (!this.m_disposed) {
        // If disposing equals true, dispose all managed
        // and unmanaged resources.
        if (disposing) {
          // Dispose managed resources
        }

        // Call the appropriate methods to clean up
        // unmanaged resources here.
        // If disposing is false,
        // only the following code is executed.
        Disconnect ();
      }
      m_disposed = true;
    }
    #endregion // Constructor, destructor

    #region Methods
    /// <summary>
    /// Start method
    /// </summary>
    public bool Start ()
    {
      log.Info ("Start");

      // Everything must be reinitialized
      m_toolManagementDataInitialized = false;

      CncStart ();

      return true;
    }

    /// <summary>
    /// Finish method
    /// </summary>
    public void Finish ()
    {

    }

    /// <summary>
    /// Disconnect from the control, free the handle
    /// </summary>
    void Disconnect ()
    {
      Disconnect (MmlConnection.ProX);
      Disconnect (MmlConnection.Cnc);
    }

    void ManageProXResult (string function, MMLReturn result)
    {
      ManageResult (function, result, MmlConnection.ProX, true);
    }

    void ManageProXResultContinue (string function, MMLReturn result)
    {
      ManageResult (function, result, MmlConnection.ProX, false);
    }

    void ManageCncResult (string function, MMLReturn result)
    {
      ManageResult (function, result, MmlConnection.Cnc, true);
    }

    void ManageResult (string function, MMLReturn result, MmlConnection connection, bool throwException)
    {
      switch (result) {
      case MMLReturn.EM_OK:
        log.DebugFormat ("ManageMMLReturn: {0} was successful", function);
        return;
      case MMLReturn.EM_HANDLE:
      case MMLReturn.EM_WINSOCK:
      case MMLReturn.EM_DISCONNECT:
      case MMLReturn.EM_NOREPLY:
        log.ErrorFormat ("ManageMMLReturn: {1} required to disconnect in {0}", function, result);
        Disconnect (connection);
        goto default;
      default:
        log.ErrorFormat ("ManageMMLReturn {0} failed with {1}", function, result);
        LogLastError (function, result, connection);
        if (throwException) {
          string message = string.Format ("{0} error: {1}", function, result);
          throw new Exception (message);
        }
        break;
      }
    }

    void LogLastError (string function, MMLReturn result, MmlConnection connection)
    {
      switch (connection) {
      case MmlConnection.ProX:
        LogLastErrorProX (function, result);
        break;
      case MmlConnection.Cnc:
        LogLastErrorCnc (function, result);
        break;
      default:
        log.FatalFormat ("ManageMMLReturn: unknown connection {0}", connection);
        Debug.Assert (false);
        break;
      }
    }

    void LogLastErrorProX (string function, MMLReturn result)
    {
      if (null == m_md3ProX) {
        return;
      }

      int mainErr;
      int subErr;
      var getLastErrorResult = m_md3ProX.GetLastError (out mainErr, out subErr);
      if (MMLReturn.EM_OK != getLastErrorResult) {
        log.FatalFormat ("LogLastErrorPro{0}: GetLastError returned {1} which is unexpected",
          m_md3ProX.Version, getLastErrorResult);
        return;
      }
      log.ErrorFormat ("LogLastErrorPro{0}: errors main={1} sub={2} for function={3}",
        m_md3ProX.Version, mainErr, subErr, function);
    }

    void LogLastErrorCnc (string function, MMLReturn result)
    {
      int mainErr;
      int subErr;
      var getLastErrorResult = Md3Cnc.GetLastError (out mainErr, out subErr);
      if (MMLReturn.EM_OK != getLastErrorResult) {
        log.FatalFormat ("LogLastErrorProX: GetLastError returned {0} which is unexpected", getLastErrorResult);
        return;
      }
      log.ErrorFormat ("LogLastErrorProX: errors main={0} sub={1} for function={2}", mainErr, subErr, function);
    }

    void Disconnect (MmlConnection connection)
    {
      switch (connection) {
      case MmlConnection.ProX:
        FreeCurrentProXHandle ();
        break;
      case MmlConnection.Cnc:
        FreeCncHandle ();
        break;
      default:
        log.FatalFormat ("ManageMMLReturn: unknown connection {0}", connection);
        Debug.Assert (false);
        break;
      }
    }

    bool IsCncConnectionActive ()
    {
      return (0 != m_cncHandle);
    }

    void AllocCncHandle ()
    {
      Debug.Assert (null != m_md3ProX);

      FreeCncHandle ();

      MMLReturn result;
      try {
        result = Md3Cnc.AllocHandle (ref m_cncHandle, this.CncNodeInfo, (int)SEND_TIMEOUT.TotalSeconds);
      }
      catch (Exception ex) {
        log.Error ("AllocCncHandle: exception in AllocHandle", ex);
        FreeCncHandle ();
        throw;
      }
      if (MMLReturn.EM_OK != result) {
        log.ErrorFormat ("AllocCncHandle: {0} was returned for node info {1}", result, this.CncNodeInfo);
        FreeCncHandle ();
        throw new Exception ("Cnc AllocHandle exception");
      }
    }

    void FreeCncHandle ()
    {
      if (0 != m_cncHandle) {
        try {
          Md3Cnc.FreeHandle (m_cncHandle);
        }
        catch (Exception ex) {
          log.Error ("FreeCncHandle: exception", ex);
        }
        m_cncHandle = 0;
      }
    }

    void CheckCncConnection ()
    {
      if (IsCncConnectionActive ()) {
        return;
      }

      if (!m_connectionManager.CanConnectCnc ()) {
        log.Debug ("CheckCncConnection: delay the new Cnc connection attempt");
        throw new Exception ("New Cnc connection attempt delayed");
      }

      // Else initialize a new connection
      try {
        AllocCncHandle ();
      }
      catch (Exception ex) {
        log.Error ("CheckCncConnection: AllocCncHandle failed", ex);
        throw;
      }
      log.Info ("CheckCncConnection: connection is successful");
    }

    bool IsProXConnectionActive ()
    {
      return (null != m_md3ProX)
        && (0 != m_proXhandle);
    }

    void AllocProXHandle ()
    {
      if (null != m_md3ProX) {
        AllocCurrentProXHandle ();
      }
      else { // null == m_md3ProX (unknown)
        switch (this.ProXVersion) {
        case 0:
          if (TryNewProXHandle (new Md3Pro6 ())) {
            log.Info ("AllocProXHandle: valid version is 6");
            return;
          }
          if (TryNewProXHandle (new Md3Pro5 ())) {
            log.Info ("AllocProXHandle: valid version is 5");
            return;
          }
          if (TryNewProXHandle (new Md3Pro3 ())) {
            log.Info ("AllocProXHandle: valid version is 3");
            return;
          }
          log.ErrorFormat ("AllocProXHandle: none of the IMd3Pro version was ok");
          throw new Exception ("No valid ProX version");
        case 6:
          m_md3ProX = new Md3Pro6 ();
          AllocCurrentProXHandle ();
          return;
        case 5:
          m_md3ProX = new Md3Pro5 ();
          AllocCurrentProXHandle ();
          return;
        case 3:
          m_md3ProX = new Md3Pro3 ();
          AllocCurrentProXHandle ();
          return;
        default:
          log.Fatal ($"AllocProXHandle: invalid ProX version {this.ProXVersion}");
          throw new Exception ("Invalid ProX version");
        }
      }
    }

    /// <summary>
    /// Note: These are the standard replies when using wrong alloc handle functions:
    /// 
    /// Using md3pro6_alloc_handle:
    ///   On Pro5 -> EM_BUFFER
    ///   On Pro3 -> EM_DISCONNECT
    ///   
    /// Using md3pro5_alloc_handle:
    ///   On Pro6 -> EM_BUFFER
    ///   On Pro3 -> EM_DISCONNECT
    ///   
    /// Using md3pro3_alloc_handle:
    ///   On Pro6 -> EM_DATA
    ///   On Pro5 -> EM_DATA
    ///   
    /// There is a way to determine if a machine is Pro6.
    /// NC parameter 7100#0 is ALWAYS 1 for Pro6 and is ALWAYS 0 for Pro5/Pro3.
    /// So you could connect to the CNC side and check that parameter bit first.
    /// For detecting Pro3 vs Pro5, I do not know of a way to do that.
    /// If you attempt to connect to Pro3 using Pro5 (or Pro6) alloc handle, after about 6 seconds the return is EM_DISCONNECT.
    /// 
    /// (From John Spaeth 2020-01-23)
    /// </summary>
    void AllocCurrentProXHandle ()
    {
      Debug.Assert (null != m_md3ProX);

      FreeCurrentProXHandle ();

      MMLReturn result;
      try {
        result = m_md3ProX.AllocHandle (out m_proXhandle, this.NodeInfo, (int)SEND_TIMEOUT.TotalMilliseconds, (int)REPLY_TIMEOUT.TotalMilliseconds, (int)NOOP_CYCLE.TotalSeconds, LOG_LEVEL);
      }
      catch (Exception ex) {
        log.Error ("AllocCurrentProXHandle: exception in AllocHandle", ex);
        FreeCurrentProXHandle ();
        throw;
      }
      if ((0 == this.ProXVersion)
        && (3 < m_md3ProX.Version)
        && ((MMLReturn.EM_BUFFER == result) || (MMLReturn.EM_DISCONNECT == result))) {
        log.Info ($"AllocCurrentProXHandle: {result} returned since Pro{m_md3ProX.Version} is not the right version");
        FreeCurrentProXHandle ();
        throw new Exception ("ProX AllocHandle exception since not right Pro version");
      }
      if (MMLReturn.EM_OK != result) {
        log.ErrorFormat ("AllocCurrentProXHandle: {0} was returned for Pro{1} and node info {2}", result, m_md3ProX.Version, this.NodeInfo);
        LogLastErrorProX ("AllocHandle", result);
        FreeCurrentProXHandle ();
        throw new Exception ("ProX AllocHandle exception");
      }
    }

    void FreeCurrentProXHandle ()
    {
      if ((null != m_md3ProX) && (0 != m_proXhandle)) {
        try {
          m_md3ProX.FreeHandle (m_proXhandle);
        }
        catch (Exception ex) {
          log.Error ("FreeCurrentProXHandle: exception", ex);
        }
        m_proXhandle = 0;
      }
    }

    bool TryNewProXHandle (IMd3ProX md3ProX)
    {
      Debug.Assert (null != md3ProX);

      FreeCurrentProXHandle ();
      m_md3ProX = md3ProX;
      log.DebugFormat ("TryNewProXHandle: try with version {0}", m_md3ProX.Version);
      try {
        AllocCurrentProXHandle ();
        return true;
      }
      catch (Exception ex) {
        log.Info ($"TryNewProXHandle: failed for version {m_md3ProX.Version}", ex);
        m_md3ProX = null;
        return false;
      }
    }

    void CheckProXConnection ()
    {
      if (IsProXConnectionActive ()) {
        return;
      }

      if (!m_connectionManager.CanConnectProX ()) {
        log.Debug ("CheckProXConnection: delay the new ProX connection attempt");
        throw new Exception ("New ProX connection attempt delayed");
      }

      // Else try to connect...
      try {
        AllocProXHandle ();
      }
      catch (Exception ex) {
        log.Error ("CheckProXConnection: AllocProcXHandle failed", ex);
        throw;
      }
      log.Info ("CheckProXConnection: connection is successful");
    }

    /// <summary>
    /// Current pallet number
    /// </summary>
    /// <param name="param">not used</param>
    public string GetPalletNumber (string param)
    {
      CheckProXConnection ();

      uint palletNumber;
      var result = m_md3ProX.GetPalletNo (m_proXhandle, 3, 0, 1, out palletNumber);
      ManageProXResult ("GetPalletNumber", result);

      if (log.IsDebugEnabled) {
        log.DebugFormat ($"GetPalletNumber: return {palletNumber}");
      }

      return palletNumber.ToString ();
    }
    #endregion // Methods
  }
}
