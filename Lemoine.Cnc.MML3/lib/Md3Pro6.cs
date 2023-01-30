using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Interface to the library Md3Pro6.dll
  /// </summary>
  public class Md3Pro6 : IMd3ProX
  {
    #region IMd3ProX implementation
    /// <summary>
    /// Version number
    /// </summary>
    int IMd3ProX.Version { get { return 6; } }

    /// <summary>
    /// Initialize for a new reading
    /// </summary>
    void IMd3ProX.Initialize () { }

    /// <summary>
    /// Error number of Md3Pro6.dll which occurred at the last is returned.
    /// </summary>
    /// <param name="mainErr">Internal error number of Md3Pro6.dll which occurred at the last is returned.</param>
    /// <param name="subErr">Error number of Fanuc library which occurred at the last is returned.</param>
    /// <returns>EM_OK if success, otherwise failed</returns>
    MMLReturn IMd3ProX.GetLastError (out Int32 mainErr, out Int32 subErr)
    {
      return Md3Pro6.GetLastError (out mainErr, out subErr);
    }

    /// <summary>
    /// Retrieve the handle number to communicate with Professional 6. When this function is executed,
    /// make the communication connection with the machine and retrieve static data of the machine.
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="nodeInfo">"nodeNo/IP address/tcpPort/e" (e:1-EmulateMode,0-ReleaseMode)</param>
    /// <param name="sendTimeout"></param>
    /// <param name="recvTimeout"></param>
    /// <param name="noop">Specify the transmission cycle of the NOOP message in seconds</param>
    /// <param name="logLevel">Communication Log (1-No log,1-send/receive data log, 2-send/receive respons log, 4-send/receive BODY data log)</param>
    /// <returns>EM_OK(success) / EM_DATA(invalid data) / EM_HANDFULL(overconnect) /
    /// EM_NODE(invalid node) / EM_DISCONNECT(disconnect) / EM_BUSY(device is busy) / EM_INTERNAL(internal error)</returns>
    MMLReturn IMd3ProX.AllocHandle (out UInt32 handle, string nodeInfo,
                                   Int32 sendTimeout, Int32 recvTimeout,
                                   Int32 noop, byte logLevel)
    {
      return Md3Pro6.AllocHandle (out handle, nodeInfo, sendTimeout, recvTimeout, noop, logLevel);
    }

    /// <summary>
    /// Exit and disconnect the communication to specified machine (Professional 6) and release the handle
    /// </summary>
    /// <param name="handle"></param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect)</returns>
    MMLReturn IMd3ProX.FreeHandle (UInt32 handle)
    {
      return Md3Pro6.FreeHandle (handle);
    }

    /// <summary>
    /// Retrieve the version of the professional X
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="productVersion">pro3, pro5, pro6</param>
    /// <param name="machineModel">pro5, pro6</param>
    /// <param name="machineSeries">pro5, pro6</param>
    /// <param name="platform">pro5</param>
    /// <param name="servicePack">pro5</param>
    /// <param name="serialNumber">pro6</param>
    /// <returns></returns>
    MMLReturn IMd3ProX.SoftwareInfo (UInt32 handle, out string productVersion, out string machineModel, out string machineSeries,
                                    out string platform, out string servicePack, out string serialNumber)
    {
      productVersion = "";
      machineModel = "";
      machineSeries = "";
      platform = "";
      servicePack = "";
      serialNumber = "";

      var sb1 = new StringBuilder (48);
      var sb2 = new StringBuilder (48);
      var sb3 = new StringBuilder (48);
      var sb4 = new StringBuilder (48);
      var ret = Md3Pro6.SoftwareInfo (handle, sb1, sb2, sb3, sb4);
      productVersion = sb1.ToString ();
      machineModel = sb2.ToString ();
      machineSeries = sb3.ToString ();
      serialNumber = sb4.ToString ();

      return ret;
    }

    /// <summary>
    /// Retrieve the number of the current machine alarm and warning
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="totalAlarm"></param>
    /// <param name="totalWarn"></param>
    /// <returns></returns>
    MMLReturn IMd3ProX.ChkMcAlarm (UInt32 handle, out UInt32 totalAlarm, out UInt32 totalWarn)
    {
      return Md3Pro6.ChkMcAlarm (handle, out totalAlarm, out totalWarn);
    }

    /// <summary>
    /// Retrieve the current machine alarm data
    /// </summary>
    /// <param name="handle">handle number</param>
    /// <param name="alarmNo">address of the array where the machine alarm number is stored</param>
    /// <param name="alarmType">address of the array where the machine alarm type is stored
    /// PMC_ALM_ALARM(=1) Alarm
    /// PMC_ALM_WARNING(=2) Warning</param>
    /// <param name="seriousLevel">address of the array where the serious level is stored
    /// PMC_ALM_LEVEL_NORMAL(=0) Normal machine alarm
    /// PMC_ALM_LEVEL_DAMAGE(=1) Serious machine alarm which damages machine</param>
    /// <param name="powerOutDisable">address of the array where the auto power off flag is stored
    /// PMC_ALM_POUT_ENABLE(=0) Requires auto power off
    /// PMC_ALM_POUT_DISABLE(=1) Not need auto power off such as operation mistake</param>
    /// <param name="cycleStartDisable">address of the array where the flag whether it is possible to cycle start is stored
    /// PMC_ALM_CYCLE_ENABLE(=0) Possible to execute cycle start
    /// PMC_ALM_CYCLE_DISABLE(=1) Impossible to execute cycle start</param>
    /// <param name="retryEnable">address of the array where the flag whether it is possible to retry is stored.
    /// PMC_ALM_RETRY_DISABLE(=0) Impossible to retry.
    /// PMC_ALM_RETRY_ENABLE (=1) Possible to retry</param>
    /// <param name="failedNcReset">address of the array where the flag whether NC
    /// reset is executed by the reset operation of the machine alarm is stored.
    /// 0: Not execute NC reset, 1: Execute NC reset</param>
    /// <param name="occuredTime">address of the SYSTEMTIME structure array
    /// where the alarm occurred date is stored</param>
    /// <param name="sumArray">Specify the array element count of above arguments.
    /// After the function is executed, the number of current machine alarms is stored</param>
    /// <returns></returns>
    MMLReturn IMd3ProX.McAlarm (UInt32 handle, ref UInt32[] alarmNo, ref byte[] alarmType, ref byte[] seriousLevel,
                               ref byte[] powerOutDisable, ref byte[] cycleStartDisable, ref byte[] retryEnable,
                               ref bool[] failedNcReset, ref SYSTEMTIME[] occuredTime, ref UInt32 sumArray)
    {
      return Md3Pro6.McAlarm (handle, alarmNo, alarmType, seriousLevel, powerOutDisable, cycleStartDisable, retryEnable,
                             failedNcReset, occuredTime, ref sumArray, null);
    }

    /// <summary>
    /// Retrieve the tool information. (The tool data unit and effective digits, etc)
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="inchUnit">0 is mm, 1 is inch</param>
    /// <param name="ftnFig">Number of effective digits of the functional tool number (FTN)</param>
    /// <param name="itnFig">Individual tool number (ITN)</param>
    /// <param name="ptnFig">Program tool number (PTN)</param>
    /// <param name="manageType">The deciding method of the using tool if the tool is exchanged is stored.
    /// 0: FTN is functional tool number
    /// 1: PTN is functional tool number
    /// 2: Not use functional tool number (The Spare tool change function is invalid.)</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle)</returns>
    MMLReturn IMd3ProX.ToolInfo (UInt32 handle, out int inchUnit, out UInt32 ftnFig,
                                out UInt32 itnFig, out UInt32 ptnFig, out UInt32 manageType)
    {
      return Md3Pro6.ToolInfo (handle, out inchUnit, out ftnFig, out itnFig, out ptnFig, out manageType);
    }

    /// <summary>
    /// Retrieve the tool life information
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="countTypeRemain">True: count down, False: count up</param>
    /// <param name="alarmResetMaxLife">The setting whether remain Full at TL Alarm Reset is stored.
    /// True: set remaining life to full, False: do not change remaining life</param>
    /// <returns>O if success, otherwise failed</returns>
    MMLReturn IMd3ProX.ToollifeInfo (UInt32 handle, out bool countTypeRemain, out bool alarmResetMaxLife)
    {
      return Md3Pro6.ToollifeInfo (handle, out countTypeRemain, out alarmResetMaxLife);
    }

    /// <summary>
    /// Retrieve whether the specified tool data item is effective.
    /// Two or more tool data items can be checked at a time.
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="item">Specify the address of the array where the number indicating the tool data item is stored</param>
    /// <param name="enable">The value whether specified each item is effective is stored</param>
    /// <param name="sumArray">Specify the element count of item and enable array</param>
    /// <returns>O if success, otherwise failed</returns>
    MMLReturn IMd3ProX.ToolDataItemIsEnable (UInt32 handle, Pro5ToolDataItem[] item,
                                            ref UInt32[] enable, UInt32 sumArray)
    {
      return Md3Pro6.ToolDataItemIsEnable (handle, item, enable, sumArray);
    }

    /// <summary>
    /// Retrieve the values of specified tool data item to two or more tools.
    /// * There is a tool data item which cannot be used according to the model and the option
    ///   Confirm whether the specified data item can be used with "ToolDataItemIsEnable"
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="item">Specify the tool data item (warning: value in Pro3ToolCommonItem OR Pro5ToolDataItem)</param>
    /// <param name="mgznNo">Specify the address of the array where the magazine number is stored</param>
    /// <param name="potNo">Specify the address of the array where the pot number is stored</param>
    /// <param name="value">The retrieving value is stored in the array</param>
    /// <param name="sumArray">Specify the element count of mgznNo, potNo and value array</param>
    /// <returns>O if success, otherwise failed</returns>
    MMLReturn IMd3ProX.GetToolDataItem (UInt32 handle, int item,
                                       UInt32[] mgznNo, Int32[] potNo,
                                       ref Int32[] value, UInt32 sumArray)
    {
      return Md3Pro6.GetToolDataItem (handle, (Pro5ToolDataItem)item, mgznNo, potNo, value, sumArray);
    }

    /// <summary>
    /// Change the value of the specified tool data item to two or more tools.
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="item">Specify the tool data item (warning: value in Pro3ToolCommonItem OR Pro5ToolDataItem)</param>
    /// <param name="mgznNo">Specify the address of the array where the magazine number is stored</param>
    /// <param name="potNo">Specify the address of the array where the pot number is stored</param>
    /// <param name="value">Specify the address of the array where the set value is stored</param>
    /// <param name="sumArray">Specify the element count of mgznNo, potNo and value array</param>
    /// <returns></returns>
    MMLReturn IMd3ProX.SetToolDataItem (UInt32 handle, int item,
                                        UInt32[] mgznNo, Int32[] potNo,
                                        Int32[] value, UInt32 sumArray)
    {
      return Md3Pro6.SetToolDataItem (handle, (Pro5ToolDataItem)item, mgznNo, potNo, value, sumArray);
    }

    /// <summary>
    /// Retrieve the values of specified cutter data item to two or more tool cutters
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="item">Specify the cutter data item (warning: value in Pro3ToolCommonItem OR Pro5ToolDataItem)</param>
    /// <param name="mgznNo">Specify the address of the array where the magazine number is stored</param>
    /// <param name="potNo">Specify the address of the array where the pot number is stored</param>
    /// <param name="cutterNo">Specify the address of the array where the cutter number is stored</param>
    /// <param name="value">The retrieving value is stored in the array</param>
    /// <param name="sumArray">Specify the element count of mgznNo, potNo, cutterNo and value array</param>
    /// <returns>O if success, otherwise failed</returns>
    MMLReturn IMd3ProX.GetCutterDataItem (UInt32 handle, int item,
                                         UInt32[] mgznNo, Int32[] potNo,
                                         UInt32[] cutterNo, ref Int32[] value,
                                         UInt32 sumArray)
    {
      return Md3Pro6.GetCutterDataItem (handle, (Pro5ToolDataItem)item, mgznNo, potNo, cutterNo, value, sumArray);
    }

    /// <summary>
    /// Change the value of the specified cutter data item to two or more tool cutters
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="item">Specify the cutter data item (warning: value in Pro3ToolCommonItem OR Pro5ToolDataItem)</param>
    /// <param name="mgznNo">Specify the address of the array where the magazine number is stored</param>
    /// <param name="potNo">Specify the address of the array where the pot number is stored</param>
    /// <param name="cutterNo">Specify the address of the array where the cutter number is stored</param>
    /// <param name="value">Specify the address of the array where the set value is stored</param>
    /// <param name="sumArray">Specify the element count of mgznNo, potNo, cutterNo and value array</param>
    /// <returns></returns>
    MMLReturn IMd3ProX.SetCutterDataItem (UInt32 handle, int item,
                                          UInt32[] mgznNo, Int32[] potNo, UInt32[] cutterNo,
                                          Int32[] value, UInt32 sumArray)
    {
      return Md3Pro6.SetCutterDataItem (handle, (Pro5ToolDataItem)item, mgznNo, potNo, cutterNo, value, sumArray);
    }

    /// <summary>
    /// Clear the data of the specified tools. Two or more tools can be specified at a time
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="magazineNumbers">Array of magazines to clear</param>
    /// <param name="potNumbers">Array of pots to clear</param>
    /// <returns></returns>
    MMLReturn IMd3ProX.ClearToolData (UInt32 handle, UInt32[] magazineNumbers, Int32[] potNumbers)
    {
      return Md3Pro6.ClearToolData (handle, magazineNumbers, potNumbers, (UInt32)potNumbers.Length);
    }

    /// <summary>
    /// Retrieve the management type of the tool number of the ATC magazine
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="randomAtc">The management type of the tool number of the ATC magazine is stored
    /// True: random pot number
    /// False: fixed pot number</param>
    /// <returns></returns>
    MMLReturn IMd3ProX.AtcRandomMagazine (UInt32 handle, out bool randomAtc)
    {
      return Md3Pro6.AtcRandomMagazine (handle, out randomAtc);
    }

    /// <summary>
    /// Retrieve the number of ATC magazines
    /// * Set NULL in arguments about unnecessary information
    /// * The outside tool is a storage area of the tool data outside the ATC magazine. When
    ///   machine parameter No.12012 "No. of the Outside Tools" is set, TRUE is stored in outMcMgzn
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="actualMgzn">The number of ATC magazines is stored</param>
    /// <param name="outMcMgzn">The flag if there is the outside tool is stored</param>
    /// <returns>0 is ok, otherwise failed</returns>
    MMLReturn IMd3ProX.MaxAtcMagazine (UInt32 handle, out UInt32 actualMgzn, out int outMcMgzn)
    {
      return Md3Pro6.MaxAtcMagazine (handle, out actualMgzn, out outMcMgzn);
    }

    /// <summary>
    /// Retrieve the number of tool and type of ATC magazine
    /// * Set NULL in arguments about unnecessary information
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="mgznNo">Specify the ATC magazine number.
    /// Specify PMC_MAGAZINE_OUTMC (=0) defined in
    /// Pro5MosDef.h to retrieve information on the outside tool</param>
    /// <param name="maxPot">The number of the tool is stored</param>
    /// <param name="mgznType">The ATC magazine type is stored.
    /// * PMC_ATC_MGZN_TYPE_CHAIN(=1) :Chain
    /// * PMC_ATC_MGZN_TYPE_MATRIX(=2) :Matrix
    /// * PMC_ATC_MGZN_TYPE_DISC(=3) :Disc
    /// * PMC_ATC_MGZN_CARTRIDGE(=4) :Cartridge
    /// * PMC_ATC_MGZN_RING(=5) :Ring
    /// * PMC_ATC_MGZN_ARRAY(=6) : H2J,H2,H5 type
    /// * PMC_ATC_MGZN_DRUM(=7) :Drum
    /// * PMC_ATC_MGZN_DOUBLE_RING(=8) :Double ring
    /// * PMC_ATC_MGZN_C17_MATRIX(=9) :Pot-less
    /// * PMC_ATC_MGZN_EGG_POT(=10) :Egg pot
    /// If PMC_MAGAZINE_OUTMC is specified for mgznNo, 0 is stored.</param>
    /// <param name="emptyPot">0 is stored in V series</param>
    /// <returns>0 if ok, otherwise failed</returns>
    MMLReturn IMd3ProX.AtcMagazineInfo (UInt32 handle, UInt32 mgznNo, out UInt32 maxPot, out Int32 mgznType, out UInt32 emptyPot)
    {
      return Md3Pro6.AtcMagazineInfo (handle, mgznNo, out maxPot, out mgznType, out emptyPot);
    }

    /// <summary>
    /// Get information about the spindle (current tool being used)
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="mgznNo"></param>
    /// <param name="potNo"></param>
    /// <param name="cutterNo"></param>
    /// <param name="ftn"></param>
    /// <param name="itn"></param>
    /// <param name="ptn"></param>
    /// <returns></returns>
    MMLReturn IMd3ProX.SpindleTool (UInt32 handle, out UInt32 mgznNo, out Int32 potNo,
                                   out UInt32 cutterNo, out UInt32 ftn, out UInt32 itn, out UInt32 ptn)
    {
      return Md3Pro6.SpindleTool (handle, out mgznNo, out potNo, out cutterNo, out ftn, out itn, out ptn);
    }

    /// <summary>
    /// Retrieve the pallet number at the specified position of the specified device
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="device">Specify the number indicating decive type. VHC(=1) Vehicle, PST(=2) Pallet Stocker, MCW(=3) Machine, WSS(=5) Work Setting Station</param>
    /// <param name="devNo">Specify the device number</param>
    /// <param name="position">Specify the number indicating position in device. DEV_TABLE(=1) Table, DEV_FRONT(=2) Front, DEV_BACK(=3) Back</param>
    /// <param name="palletNo">The pallet number is stored</param>
    /// <returns></returns>
    MMLReturn IMd3ProX.GetPalletNo (UInt32 handle, UInt32 device, UInt32 devNo, UInt32 position, out UInt32 palletNo)
    {
      return Md3Pro6.GetPalletNo (handle, device, devNo, position, out palletNo);
    }
    #endregion // IMd3ProX implementation

    #region Dll binding
    [DllImport ("Md3Pro6.dll", EntryPoint = "md3pro6_GetLastError", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn GetLastError (out Int32 mainErr, out Int32 subErr);

    [DllImport ("Md3Pro6.dll", EntryPoint = "md3pro6_alloc_handle", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn AllocHandle (out UInt32 handle, [In] string nodeInfo,
                                        Int32 sendTimeout, Int32 recvTimeout,
                                        Int32 noop, byte logLevel);

    [DllImport ("Md3Pro6.dll", EntryPoint = "md3pro6_free_handle", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn FreeHandle (UInt32 handle);

    [DllImport ("Md3Pro6.dll", EntryPoint = "md3pro6_software_info", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn SoftwareInfo (UInt32 handle, StringBuilder productVersion, StringBuilder machineModel, StringBuilder machineSeries, StringBuilder serialNumber);

    [DllImport ("Md3Pro6.dll", EntryPoint = "md3pro6_chk_mc_alarm", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn ChkMcAlarm (UInt32 handle, out UInt32 totalAlarm, out UInt32 totalWarn);

    [DllImport ("Md3Pro6.dll", EntryPoint = "md3pro6_mc_alarm", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn McAlarm (UInt32 handle, [In, Out] UInt32[] alarmNo, [In, Out] byte[] alarmType, [In, Out] byte[] seriousLevel,
                                    [In, Out] byte[] powerOutDisable, [In, Out] byte[] cycleStartDisable, [In, Out] byte[] retryEnable,
                                    [In, Out] bool[] failedNcReset, [In, Out] SYSTEMTIME[] occuredTime, ref UInt32 sumArray,
                                    StringBuilder ncMessage);

    [DllImport ("Md3Pro6.dll", EntryPoint = "md3pro6_tool_info", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn ToolInfo (UInt32 handle, out int inchUnit, out UInt32 ftnFig,
                                     out UInt32 itnFig, out UInt32 ptnFig, out UInt32 manageType);

    [DllImport ("Md3Pro6.dll", EntryPoint = "md3pro6_toollife_info", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn ToollifeInfo (UInt32 handle, out bool countTypeRemain, out bool alarmResetMaxLife);

    [DllImport ("Md3Pro6.dll", EntryPoint = "md3pro6_atc_random_magazine", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn AtcRandomMagazine (UInt32 handle, out bool randomAtc);

    [DllImport ("Md3Pro6.dll", EntryPoint = "md3pro6_tool_data_item_is_enable", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn ToolDataItemIsEnable (UInt32 handle, [In] Pro5ToolDataItem[] item,
                                                 [In, Out] UInt32[] enable, UInt32 sumArray);

    [DllImport ("Md3Pro6.dll", EntryPoint = "md3pro6_get_tool_data_item", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn GetToolDataItem (UInt32 handle, Pro5ToolDataItem item,
                                            [In] UInt32[] mgznNo, [In] Int32[] potNo,
                                            [In, Out] Int32[] value, UInt32 sumArray);

    [DllImport ("Md3Pro6.dll", EntryPoint = "md3pro6_get_cutter_data_item", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn GetCutterDataItem (UInt32 handle, Pro5ToolDataItem item,
                                              [In] UInt32[] mgznNo, [In] Int32[] potNo,
                                              [In] UInt32[] cutterNo, [In, Out] Int32[] value,
                                              UInt32 sumArray);

    [DllImport ("Md3Pro6.dll", EntryPoint = "md3pro6_max_atc_magazine", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn MaxAtcMagazine (UInt32 handle, out UInt32 actualMgzn, out int outMcMgzn);

    [DllImport ("Md3Pro6.dll", EntryPoint = "md3pro6_atc_magazine_info", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn AtcMagazineInfo (UInt32 handle, UInt32 mgznNo, out UInt32 maxPot, out Int32 mgznType, out UInt32 emptyPot);

    [DllImport ("Md3Pro6.dll", EntryPoint = "md3pro6_spindle_tool", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn SpindleTool (UInt32 handle, out UInt32 mgznNo, out Int32 potNo,
                                        out UInt32 cutterNo, out UInt32 ftn, out UInt32 itn, out UInt32 ptn);

    [DllImport ("Md3Pro6.dll", EntryPoint = "md3pro6_get_pallet_no", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn GetPalletNo (UInt32 handle, UInt32 device, UInt32 devNo, UInt32 position, out UInt32 palletNo);

    [DllImport ("Md3Pro6.dll", EntryPoint = "md3pro6_set_tool_data_item", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn SetToolDataItem (UInt32 handle, Pro5ToolDataItem item,
                                             [In] UInt32[] mgznNo, [In] Int32[] potNo,
                                             [In] Int32[] value, UInt32 sumArray);

    [DllImport ("Md3Pro6.dll", EntryPoint = "md3pro6_set_cutter_data_item", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn SetCutterDataItem (UInt32 handle, Pro5ToolDataItem item,
                                               [In] UInt32[] mgznNo, [In] Int32[] potNo,
                                               [In] UInt32[] cutterNo, [In] Int32[] value,
                                               UInt32 sumArray);

    [DllImport ("Md3Pro6.dll", EntryPoint = "md3pro6_clear_tool_data", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn ClearToolData (UInt32 handle, [In] UInt32[] mgznNo, [In] Int32[] potNo, UInt32 sumArray);
    #endregion // Dll binding
  }
}

//short md3pro6_get_mcpara(DWORD handle, LPDWORD paraNo, LPLONG value, DWORD sumArray);

//short md3pro6_get_mcpara2(DWORD handle, LPDWORD paraNo, LPDWORD index, LPLONG value, DWORD sumArray);

//short md3pro6_chk_system_mode(DWORD handle, LPBOOL sysMode);

//short md3pro6_chk_maintenance_mode(DWORD handle, LPBOOL maintMode);

//short md3pro6_chk_auto_unloading_mode(DWORD handle, LPBOOL unloadMode);

//short md3pro6_chk_operator_call(DWORD handle, LPLONG opeCall);

//short md3pro6_change_nc_mode(DWORD handle, DWORD ncMode, BOOL modeLockEffective);

//short md3pro6_optional_block_skip_is_enable(DWORD handle, LPBOOL enable);

//	[Arguments]
//		unsigned long  handle [in] : HANDLE number
//		BYTE  bkSkip2,...,bkSkip9 [in] :
//				PMC_BDT_OFF: Block skip is turned OFF.
//				PMC_BDT_ON : Block skip is turned ON.
//				others (PMC_BDT_XX) : Block skip is not changed.
//short md3pro6_set_optional_block_skip(DWORD handle,
//                           BYTE bkSkip2, BYTE bkSkip3, BYTE bkSkip4, BYTE bkSkip5,
//                           BYTE bkSkip6, BYTE bkSkip7, BYTE bkSkip8, BYTE bkSkip9);

//short md3pro6_get_on_duty(DWORD handle, LPBOOL duty);

//short md3pro6_set_on_duty(DWORD handle, BOOL duty);

//short md3pro6_can_auto_poweroff(DWORD handle, LPBOOL enable);

//short md3pro6_auto_poweroff(DWORD handle);

//short md3pro6_system_cycle_start(DWORD handle, BOOL opeCall);

//short md3pro6_chk_system_mach_finish(DWORD handle, LPBOOL finish, LPDWORD finCondition);

//short md3pro6_system_cycle_abort(DWORD handle, BOOL abnormalFinish);

//short md3pro6_chk_macro_read_request(DWORD handle, LPBOOL request);

//short md3pro6_macro_read_finish(DWORD handle);

//short md3pro6_max_alarm_history(DWORD handle, LPDWORD maxHistory);

//short md3pro6_mc_alarm_history(DWORD handle, LPDWORD alarmNo, LPBYTE alarmType,
//													LPBYTE seriousLevel, LPBYTE resetType, LPSYSTEMTIME occuredTime,
//													LPSYSTEMTIME resetTime, LPDWORD sumArray, LPSTR ncMessage);

//short md3pro6_mc_alarm_reset(DWORD handle, DWORD resetTime);

//short md3pro6_spindle_status(DWORD handle, DWORD spindleNo, LPLONG gear, LPLONG direction);

//short md3pro6_spindle_speed(DWORD handle, DWORD spindleNo, LPDWORD sCode, LPDWORD instruct, LPDWORD actual);

//short md3pro6_get_milling_time(DWORD handle, LPDWORD millingTime);

//short md3pro6_atc_status(DWORD handle, LPBOOL armStandby, LPLONG mgznStatus);

//short md3pro6_atc_magazine_action_start(DWORD handle, DWORD actType, DWORD tlsNo, DWORD mgznNo, LONG potNo);

//short md3pro6_chk_atc_magazine_action_finish(DWORD handle, LPDWORD actType, LPDWORD tlsNo,
//															LPDWORD mgznNo, LPLONG potNo, LPBOOL finish, LPDWORD finCondition);

//short md3pro6_get_operation_panel_disable(DWORD handle, DWORD opePanel, LPBOOL disable);

//short md3pro6_set_operation_panel_disable(DWORD handle, DWORD opePanel, BOOL disable);

//short md3pro6_set_pallet_no(DWORD handle, DWORD device, DWORD devNo, DWORD position, DWORD palletNo);

//short md3pro6_apc_info(DWORD handle, LPLONG apcArmType, LPDWORD maxMgznPallet, LPDWORD maxOutPallet, LPDWORD faces);

//short md3pro6_apc_status(DWORD handle, LPBOOL armStandby, LPLONG stckStatus);

//short md3pro6_pm_info(DWORD handle, LPDWORD type, LPDWORD maxPlt, LPDWORD maxPst, LPDWORD maxWss,
//							LPLONG wssType, LPLONG pmMcEmptyPotNo, LPLONG pmWssEmptyPotNo);

//short md3pro6_pm_trans_start(DWORD handle, PMB_TRANS_COMMAND *command);

//short md3pro6_pm_chk_trans_finish(DWORD handle, LPBOOL finish, LPDWORD commandStatus,
//												 LPDWORD finishStatus, LPDWORD alarmNo, PMB_TRANS_COMMAND *command);

//short md3pro6_pm_get_wss_status(DWORD handle, LPDWORD manual,
//											   LPDWORD doorUnlock, LPDWORD doorOpen, LPDWORD maintenance);

//short md3pro6_pm_get_wss_operation(DWORD handle, DWORD wssNo, LPDWORD opeType);

//short md3pro6_pm_set_wss_operation(DWORD handle, DWORD wssNo, DWORD opeType);

//short md3pro6_chk_operation_panel_switch(DWORD handle, DWORD switchType,
//														DWORD devNo, LPBOOL push, LPLONG data1, LPLONG data2);

//short md3pro6_accept_operation_panel_switch(DWORD handle, DWORD switchType, DWORD devNo, BOOL accept);

//short md3pro6_cancel_operation_panel_switch(DWORD handle, DWORD switchType, DWORD devNo);

//short md3pro6_next_tool(DWORD handle, LPDWORD mgznNo, LPLONG potNo,
//									   LPDWORD cutterNo, LPDWORD ftn, LPDWORD itn, LPDWORD ptn);

//short md3pro6_tls_potno(DWORD handle, DWORD tlsNo, LPDWORD mgznNo, LPLONG potNo);

//short md3pro6_max_tls(DWORD handle, LPDWORD maxTls);

//short md3pro6_get_pallet_data_item(DWORD handle, DWORD item, LPDWORD palletNo, LPLONG value, DWORD sumArray);

//short md3pro6_set_pallet_data_item(DWORD handle, DWORD item, LPDWORD palletNo, LPLONG value, DWORD sumArray);

//short md3pro6_get_face_data_item(DWORD handle, DWORD item,
//												LPDWORD palletNo, LPDWORD faceNo, LPLONG value, DWORD sumArray);

//short md3pro6_set_face_data_item(DWORD handle, DWORD item,
//												LPDWORD palletNo, LPDWORD faceNo, LPLONG value, DWORD sumArray);

//short md3pro6_get_face_start_date(DWORD handle,
//												 LPDWORD palletNo, LPDWORD faceNo, LPSYSTEMTIME sysTime, DWORD sumArray);

//short md3pro6_get_face_finish_date(DWORD handle,
//												 LPDWORD palletNo, LPDWORD faceNo, LPSYSTEMTIME sysTime, DWORD sumArray);

//short md3pro6_clear_work_data(DWORD handle, LPDWORD palletNo, DWORD sumArray);

//short md3pro6_get_arbitrary_machine_status(DWORD handle, DWORD statusKind1, DWORD statusKind2, DWORD statusKind3, DWORD statusKind4,
//												 LPLONG outputData1, LPLONG outputData2, LPLONG outputData3, LPLONG outputData4);

//short md3pro6_get_work_data(DWORD handle, LPDWORD palletNo, LPBYTE workData, DWORD sumArray);

//short md3pro6_set_work_data(DWORD handle, LPBYTE workData, DWORD sumArray);

//short md3pro6_get_iac_data(DWORD handle, LPDWORD palletNo, LPBYTE iacData, DWORD sumArray);

//short md3pro6_set_iac_data(DWORD handle, LPBYTE iacData, DWORD sumArray);

//short md3pro6_tool_data_Indiv(DWORD handle, LPDWORD MgznNo, LPLONG potNo,
//									   LPDWORD cutterNo, LPDWORD ftn, LPDWORD itn, LPDWORD ptn, LPDWORD updateCounter, DWORD sumArray);
