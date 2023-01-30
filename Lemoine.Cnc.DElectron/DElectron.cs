// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using AxprjZ32ComLib;
using prjZ32ComLib;

namespace Lemoine.Cnc
{
  /// <summary>
  /// DElectron input module
  /// </summary>
  public sealed class DElectron: BaseCncModule, ICncModule, IDisposable
  {
    enum ConnectionStatus
    {
      /// <summary>
      /// Not connected to DElectron machine
      /// </summary>
      NOT_CONNECTED,
      
      /// <summary>
      /// Connecting to DElectron machine
      /// </summary>
      CONNECTING,
      
      /// <summary>
      /// Connected to DElectron machine
      /// </summary>
      CONNECTED
    }
    
    #region Members
    ConnectionStatus m_connectionStatus = ConnectionStatus.NOT_CONNECTED;
    AxZ32FastCodos m_FC = null;
    DElectronCommands m_commands = null;
    int m_port = 4031;
    bool m_connectionError = false;
    int m_sleepDurationMs = 100;
    #endregion

    #region Getters / Setters
    /// <summary>
    /// Host address of the CNC
    /// </summary>
    public string HostAddress { get; set; }
    
    /// <summary>
    /// Pause before processing the events following an order, in ms
    /// (could work with a value as low as 5 ms, 100 ms is more secure)
    /// </summary>
    public int PauseForEventProcessing {
      get { return m_sleepDurationMs; }
      set { m_sleepDurationMs = value; }
    }
    #endregion

    #region Constructors, destructor
    /// <summary>
    /// Default constructor
    /// </summary>
    public DElectron() : base("Lemoine.Cnc.In.DElectron")
    {
      // Prepare AxZ32FastCodos and commands
      Init();
    }

    /// <summary>
    /// <see cref="IDisposable.Dispose" />
    /// </summary>
    public void Dispose()
    {
      GC.SuppressFinalize(this);
      if (m_connectionStatus != ConnectionStatus.NOT_CONNECTED) {
        m_FC.Disconnect();
      }
    }
    #endregion // Constructor, destructor

    #region Get methods
    /// <summary>
    /// Return true if an error occured during a connexion
    /// If connected, the boolean is reset to false
    /// </summary>
    /// <param name="param">unused string</param>
    /// <returns></returns>
    public bool GetConnectionError(string param)
    {
      return m_connectionError;
    }
    
    /// <summary>
    /// Extract a bit from the value
    /// </summary>
    /// <param name="param">X-Y:An, X being the number of the bit, Y the command, An the arguments</param>
    /// <returns></returns>
    public bool GetBit(string param)
    {
      int arg = ExtractFunctionArgument(ref param);
      
      // Extract the byte number "arg / 8"
      byte bTmp = GetOutputObjects(param)[arg / 8];
      
      // Extract the bit number "arg % 8" inside
      return ((bTmp & (0x1 << (arg % 8))) != 0);
    }
    
    /// <summary>
    /// Extract a byte from the value
    /// </summary>
    /// <param name="param">X-Y:An, X being the number of the byte, Y the command, An the arguments</param>
    /// <returns></returns>
    public int GetByte(string param)
    {
      int arg = ExtractFunctionArgument(ref param);
      
      // Return the byte number "arg"
      return GetOutputObjects(param)[arg];
    }
    
    /// <summary>
    /// Extract a char from the value
    /// </summary>
    /// <param name="param">X-Y:An, X being the number of the byte, Y the command, An the arguments</param>
    /// <returns></returns>
    public char GetChar(string param)
    {
      int arg = ExtractFunctionArgument(ref param);
      
      // The char has to be in the range A-Z or a-z
      byte bTmp = GetOutputObjects(param)[arg];
      if ((bTmp < 65 || bTmp > 90) && (bTmp < 97 || bTmp > 122)) {
        throw new Exception("Invalid char in GetChar: value " + bTmp);
      }

      return (char)bTmp;
    }
    
    /// <summary>
    /// Get a INT16 from the value of a variable, return it as an INT32
    /// </summary>
    /// <param name="param">X-Y:An, X being the offset in bytes, Y the command, An the arguments</param>
    /// <returns>the value of the parameter</returns>
    public Int32 GetInt16(string param)
    {
      int arg = ExtractFunctionArgument(ref param);
      return (Int32)BitConverter.ToInt16(GetOutputObjects(param), arg);
    }
    
    /// <summary>
    /// Get a INT32 from the value of a variable
    /// </summary>
    /// <param name="param">X-Y:An, X being the offset in bytes, Y the command, An the arguments</param>
    /// <returns>the value of the parameter</returns>
    public Int32 GetInt32(string param)
    {
      int arg = ExtractFunctionArgument(ref param);
      return BitConverter.ToInt32(GetOutputObjects(param), arg);
    }
    
    /// <summary>
    /// Get the value of a variable as a INT64
    /// </summary>
    /// <param name="param">X-Y:An, X being the offset in bytes, Y the command, An the arguments</param>
    /// <returns>the value of the parameter</returns>
    public Int64 GetInt64(string param)
    {
      int arg = ExtractFunctionArgument(ref param);
      return BitConverter.ToInt64(GetOutputObjects(param), arg);
    }
    
    /// <summary>
    /// Get the value of a variable as a double
    /// </summary>
    /// <param name="param">X-Y:An, X being the offset in bytes, Y the command, An the arguments</param>
    /// <returns>the value of the parameter</returns>
    public double GetDouble(string param)
    {
      int arg = ExtractFunctionArgument(ref param);
      return BitConverter.ToDouble(GetOutputObjects(param), arg);
    }
    
    /// <summary>
    /// Extract an override from the value
    /// A conversion is done to have it in percent
    /// </summary>
    /// <param name="param">X-Y:An, X being the number of the byte, Y the command, An the arguments</param>
    /// <returns></returns>
    public float GetOverride(string param)
    {
      int arg = ExtractFunctionArgument(ref param);
      return 100.0f + ((float)GetOutputObjects(param)[arg] - 160.0f) * 0.625f;
    }
    
    /// <summary>
    /// Get the current alarms
    /// </summary>
    /// <returns></returns>
    public IList<CncAlarm> GetAlarms()
    {
      var alarms = new List<CncAlarm>();
      
      // NC alarm
      var ncAlarm = (UInt32)GetInt32("1-FCODOS16_ALARMS:0:0");
      if (ncAlarm != 0) {
        var alarm = new CncAlarm("DElectron", GetNCAlarmType(ncAlarm), "CN" + ncAlarm.ToString("X4"));
        alarms.Add(alarm);
      }
      
      // Mu alarm
      var muAlarm = (UInt32)GetInt32("2-FCODOS16_ALARMS:0:0");
      if (muAlarm != 0) {
        var alarm = new CncAlarm("DElectron", "PLC", "MU" + ncAlarm.ToString("X4"));
        alarms.Add(alarm);
      }
      
      return alarms;
    }
    
    string GetNCAlarmType(UInt32 number)
    {
      string type = (number & 0x00ff).ToString("X4");
      
      switch (number & 0x00ff) {
        case 0x02:
          type = "Initialization";
          break;
        case 0x07:
          type = "FlorenZ system";
          break;
        case 0x08:
          type = "Z-Star";
          break;
        case 0x09:
          type = "Link";
          break;
        case 0x10:
          type = "CMOS";
          break;
        case 0x12:
          type = "Axis detector";
          break;
        case 0x13:
          type = "Axis movement";
          break;
        case 0x14:
          type = "Machine programming and usage";
          break;
      }
      
      return type;
    }
    #endregion // Get methods
    
    #region Methods
    void Init() // Also called by "ResetConnection"
    {
      // Disconnect if necessary
      if (m_FC != null && m_connectionStatus != ConnectionStatus.NOT_CONNECTED) {
        m_FC.Disconnect();
      }

      m_connectionStatus = ConnectionStatus.NOT_CONNECTED;
      m_connectionError = false;
      
      // Create a new driver
      m_FC = new AxZ32FastCodos();
      m_FC.CreateControl();
      m_FC.FastCodosError += OnFastCodosError;
      m_FC.Errorgen += OnErrorGen;
      m_FC.SocketError += OnSocketError;
      m_FC.UnattendedError += OnUnattendedError;
      m_FC.ConnectEvent += OnConnect;
      m_FC.DisconnectEvent += OnDisconnect;
      m_FC.Scanning += OnScanning;
      m_FC.CommandsReady += OnCommandsReady;
      
      // Prepare commands
      m_commands = new DElectronCommands(m_FC);
      m_commands.ResetConnection += Init;
    }
    
    /// <summary>
    /// Start method
    /// </summary>
    public void Start()
    {
      log.Info("Start");
      Application.DoEvents();
      CheckConnection();
      if (m_connectionStatus == ConnectionStatus.CONNECTED) {
        m_commands.Start();
      }

      Thread.Sleep(PauseForEventProcessing);
      Application.DoEvents();
    }
    
    /// <summary>
    /// Split the command
    /// "0-FCODOS16_AXIS_QUOTE:0:0" becomes "FCODOS16_AXIS_QUOTE:0:0" and the value "0" is returned
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    int ExtractFunctionArgument(ref string param)
    {
      int functionArgument = -1;
      int separatorIndex = param.IndexOf("-");
      
      // "-" allowed to be at the second or third position
      if (param.Length == 0 || separatorIndex < 1 || separatorIndex > 2 ||
          !int.TryParse(param.Substring(0, separatorIndex), out functionArgument) || functionArgument < 0) {
        string text = "Wrong param format: " + param;
        log.Error(text);
        throw new ArgumentException(text);
      }
      
      param = param.Remove(0, separatorIndex + 1);
      return functionArgument;
    }
    
    byte[] GetOutputObjects(string param)
    {
      // We store the different parameters even if we are not connected
      // Exception may be raised if values are not present and if connected
      // If not connected, the storage only is allowed
      object [] outputObjects = m_commands.GetValue(param, m_connectionStatus == ConnectionStatus.CONNECTED);
      
      if (m_connectionStatus != ConnectionStatus.CONNECTED) {
        throw new Exception("Cannot retrieve the value, not connected");
      }

      // Convert each data to bytes
      IList<byte[]> listBytes = new List<byte[]>();
      for (int i = 0; i < outputObjects.Count(); i++) {
        byte [] bytesTmp;
        if (outputObjects[i] is double) {
          bytesTmp = BitConverter.GetBytes((double)outputObjects[i]);
        } else if (outputObjects[i] is Int32) {
          bytesTmp = BitConverter.GetBytes((Int32)outputObjects[i]);
        } else {
          throw new Exception("Type " + outputObjects[i].GetType() + " is not handled");
        }
        listBytes.Add(bytesTmp);
      }
      
      // Concat bytes
      int size = 0;
      foreach (byte[] bytesTmp in listBytes) {
        size += bytesTmp.Count();
      }

      byte[] bytes = new byte[size];
      int pos = 0;
      foreach (byte[] bytesTmp in listBytes) {
        for (int i = 0; i < bytesTmp.Count(); i++) {
          bytes[pos + i ] = bytesTmp[i];
        }

        pos += bytesTmp.Count();
      }
      
      return bytes;
    }
    
    void CheckConnection()
    {
      if (m_connectionStatus == ConnectionStatus.NOT_CONNECTED) {
        try {
          m_connectionStatus = ConnectionStatus.CONNECTING;
          m_FC.Connect(HostAddress, m_port, "", 0, SocksType.SOCKS_NONE);
          Application.DoEvents();
          m_connectionError = (m_connectionStatus == ConnectionStatus.NOT_CONNECTED);
          log.InfoFormat("Tried to connect to {0}:{1}", HostAddress, m_port);
        } catch (Exception e) {
          log.Error("Cannot connect to DElectron machine " + HostAddress, e);
          m_connectionStatus = ConnectionStatus.NOT_CONNECTED;
          m_connectionError = true;
        }
      }
    }
    
    void ChangePort()
    {
      if (m_port == 4002) {
        m_port = 4031;
      }
      else {
        m_port--;
      }

      log.Info("Port changed to " + m_port);
    }
    #endregion // Methods
    
    #region Event reactions
    void OnConnect(object sender, __Z32FastCodos_ConnectEvent e)
    {
      m_connectionStatus = ConnectionStatus.CONNECTED;
      m_connectionError = false;
      log.Info("DElectron machine connected, port = " + e.lngRemotePort);
    }
    
    void OnCommandsReady(object sender, __Z32FastCodos_CommandsReadyEvent e)
    {
      log.Info("Commands ready for DElectron machine");
      
      // Update values for each command
      m_commands.CommandsReady();
    }
    
    void OnDisconnect(object sender, EventArgs e)
    {
      log.Info("DElectron machine disconnected\n");
      m_connectionStatus = ConnectionStatus.NOT_CONNECTED;
    }
    
    void OnScanning(object sender, __Z32FastCodos_ScanningEvent e) {}
    
    void OnFastCodosError(object sender, __Z32FastCodos_FastCodosErrorEvent e)
    {
      string text = "Fast codos error " + e.intErrCode + " received (" + e.strDescription + ")";
      log.FatalFormat(text);
    }
    
    void OnErrorGen(object sender, __Z32FastCodos_ErrorgenEvent e)
    {
      string text = "Error gen " + e.lngErrorGen + " received";
      log.FatalFormat(text);
    }
    
    void OnSocketError(object sender, __Z32FastCodos_SocketErrorEvent e)
    {
      // The behaviour of frmTestFC.frm regarding the port management is restored
      // But the error code 10061 is described nowhere
      if (e.intErrCode == 10061) {
        ChangePort();
        m_FC.Disconnect();
      }
      
      string text = "Socket error " + e.intErrCode + " received (" + e.strDescription + ")";
      log.FatalFormat(text);
    }
    
    void OnUnattendedError(object sender, __Z32FastCodos_UnattendedErrorEvent e)
    {
      string text = "Unattended error " + e.lngErrNumber + " received (" + e.strErrDescription +
        ")\nProcedure: " + e.strErrProcedure;
      log.FatalFormat(text);
    }
    #endregion // Event reactions
  }
}
