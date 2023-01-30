using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Interface to the library Md3Pro5.dll
  /// </summary>
  public class Md3Pro5 : IMd3ProX
  {
    #region IMd3ProX implementation
    /// <summary>
    /// Version number
    /// </summary>
    int IMd3ProX.Version { get { return 5; } }

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
      return Md3Pro5.GetLastError (out mainErr, out subErr);
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
      return Md3Pro5.AllocHandle (out handle, nodeInfo, sendTimeout, recvTimeout, noop, logLevel);
    }

    /// <summary>
    /// Exit and disconnect the communication to specified machine (Professional 6) and release the handle
    /// </summary>
    /// <param name="handle"></param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect)</returns>
    MMLReturn IMd3ProX.FreeHandle (UInt32 handle)
    {
      return Md3Pro5.FreeHandle (handle);
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
      var sb5 = new StringBuilder (48);
      var ret = Md3Pro5.SoftwareInfo (handle, sb1, sb2, sb3, sb4, sb5);
      platform = sb1.ToString ();
      machineSeries = sb2.ToString ();
      productVersion = sb3.ToString ();
      servicePack = sb4.ToString ();
      machineModel = sb5.ToString ();

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
      return Md3Pro5.ChkMcAlarm (handle, out totalAlarm, out totalWarn);
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
      return Md3Pro5.McAlarm (handle, alarmNo, alarmType, seriousLevel, powerOutDisable, cycleStartDisable, retryEnable,
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
      return Md3Pro5.ToolInfo (handle, out inchUnit, out ftnFig, out itnFig, out ptnFig, out manageType);
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
      return Md3Pro5.ToollifeInfo (handle, out countTypeRemain, out alarmResetMaxLife);
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
      return Md3Pro5.ToolDataItemIsEnable (handle, item, enable, sumArray);
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
      return Md3Pro5.GetToolDataItem (handle, (Pro5ToolDataItem)item, mgznNo, potNo, value, sumArray);
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
      return Md3Pro5.SetToolDataItem (handle, (Pro5ToolDataItem)item, mgznNo, potNo, value, sumArray);
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
                                          UInt32[] mgznNo, Int32[] potNo, UInt32[] cutterNo,
                                          ref Int32[] value, UInt32 sumArray)
    {
      return Md3Pro5.GetCutterDataItem (handle, (Pro5ToolDataItem)item, mgznNo, potNo, cutterNo, value, sumArray);
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
      return Md3Pro5.SetCutterDataItem (handle, (Pro5ToolDataItem)item, mgznNo, potNo, cutterNo, value, sumArray);
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
      return Md3Pro5.ClearToolData (handle, magazineNumbers, potNumbers, (UInt32)potNumbers.Length);
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
      return Md3Pro5.AtcRandomMagazine (handle, out randomAtc);
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
      return Md3Pro5.MaxAtcMagazine (handle, out actualMgzn, out outMcMgzn);
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
      return Md3Pro5.AtcMagazineInfo (handle, mgznNo, out maxPot, out mgznType, out emptyPot);
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
      return Md3Pro5.SpindleTool (handle, out mgznNo, out potNo, out cutterNo, out ftn, out itn, out ptn);
    }

    /// <summary>
    /// Retrieve the pallet number at the specified position of the specified device
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="device">Specify the number indicating decive type. VHC(=1) Vehicle, PST(=2) Pallet Stocker, MCW(=3) Machine, WSS(=5) Work Setting Station</param>
    /// <param name="devNo">Specify the devince number</param>
    /// <param name="position">Specify the number indicating position in device. DEV_TABLE(=1) Table, DEV_FRONT(=2) Front, DEV_BACK(=3) Back</param>
    /// <param name="palletNo">The pallet number is stored</param>
    /// <returns></returns>
    MMLReturn IMd3ProX.GetPalletNo (UInt32 handle, UInt32 device, UInt32 devNo, UInt32 position, out UInt32 palletNo)
    {
      return Md3Pro5.GetPalletNo (handle, device, devNo, position, out palletNo);
    }
    #endregion // IMd3ProX implementation

    #region Dll binding
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_GetLastError", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn GetLastError (out Int32 mainErr, out Int32 subErr);

    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_alloc_handle", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn AllocHandle (out UInt32 handle, [In] string nodeInfo,
                                        Int32 sendTimeout, Int32 recvTimeout,
                                        Int32 noop, byte logLevel);

    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_free_handle", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn FreeHandle (UInt32 handle);

    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_software_info", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn SoftwareInfo (UInt32 handle, StringBuilder platform, StringBuilder series,
                                         StringBuilder version, StringBuilder servicePack, StringBuilder mcName);

    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_chk_mc_alarm", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn ChkMcAlarm (UInt32 handle, out UInt32 totalAlarm, out UInt32 totalWarn);

    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_mc_alarm", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn McAlarm (UInt32 handle, [In, Out] UInt32[] alarmNo, [In, Out] byte[] alarmType, [In, Out] byte[] seriousLevel,
                                    [In, Out] byte[] powerOutDisable, [In, Out] byte[] cycleStartDisable, [In, Out] byte[] retryEnable,
                                    [In, Out] bool[] failedNcReset, [In, Out] SYSTEMTIME[] occuredTime, ref UInt32 sumArray,
                                    StringBuilder ncMessage);

    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_tool_info", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn ToolInfo (UInt32 handle, out int inchUnit, out UInt32 ftnFig,
                                     out UInt32 itnFig, out UInt32 ptnFig, out UInt32 manageType);

    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_toollife_info", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn ToollifeInfo (UInt32 handle, out bool countTypeRemain, out bool alarmResetMaxLife);

    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_atc_random_magazine", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn AtcRandomMagazine (UInt32 handle, out bool randomAtc);

    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_atc_magazine_info", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn AtcMagazineInfo (UInt32 handle, UInt32 mgznNo, out UInt32 maxPot, out Int32 mgznType, out UInt32 emptyPot);

    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_max_atc_magazine", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn MaxAtcMagazine (UInt32 handle, out UInt32 actualMgzn, out int outMcMgzn);

    /// <summary>
    /// Retrieve the setting value of the specified machine parameter. Two or more setting
    /// values of machine parameter can be retrieved at a time
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="paraNo"></param>
    /// <param name="value"></param>
    /// <param name="sumArray"></param>
    /// <returns></returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_get_mcpara",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn GetMcpara (UInt32 handle, [In] UInt32[] paraNo,
                                       [In, Out] Int32[] value, UInt32 sumArray);

    /// <summary>
    /// Retrieve the system mode
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="sysMode"></param>
    /// <returns></returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_chk_system_mode",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn ChkSystemMode (UInt32 handle, out byte sysMode);
    #endregion // Dll binding

    #region Other methods

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_chk_maintenance_mode",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn ChkMaintenanceMode(UInt32 handle, out bool maintMode);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_chk_auto_unloading_mode",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn ChkAutoUnloadingMode(UInt32 handle, out bool unloadMode);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_chk_operator_call",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn ChkOperatorCall(UInt32 handle, out Int32 opeCall);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_chk_backedit",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn ChkBackedit(UInt32 handle, out bool mode, out UInt32 oNumber);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_change_nc_mode",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn ChangeNcMode(UInt32 handle, UInt32 ncMode, bool modeLockEffective);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_optional_block_skip_is_enable",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn OptionalBlockSkipIsEnable(UInt32 handle, out bool enable);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_set_optional_block_skip",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn SetOptionalBlockSkip(UInt32 handle,
    //                                                    byte bkSkip2, byte bkSkip3, byte bkSkip4, byte bkSkip5,
    //                                                    byte bkSkip6, byte bkSkip7, byte bkSkip8, byte bkSkip9);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_get_on_duty",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn GetOnDuty(UInt32 handle, out bool duty);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_set_on_duty",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn SetOnDuty(UInt32 handle, bool duty);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_can_auto_poweroff",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn CanAutoPoweroff(UInt32 handle, out bool enable);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_auto_poweroff",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn AutoPoweroff(UInt32 handle);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="opeCall"></param>
    /// <returns></returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_system_cycle_start",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn SystemCycleStart (UInt32 handle, byte opeCall);

    /// <summary>
    /// Request the execution of a cycle start to Professional 5
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="finish"></param>
    /// <param name="finCondition"></param>
    /// <returns></returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_chk_system_mach_finish",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn ChkSystemMachFinish (UInt32 handle, out byte finish, out UInt32 finCondition);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_system_cycle_abort",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn SystemCycleAbort(UInt32 handle, bool abnormalFinish);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_chk_macro_read_request",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn ChkMacroReadRequest(UInt32 handle, out bool request);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_macro_read_finish",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn MacroReadFinish(UInt32 handle);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_max_alarm_history",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn MaxAlarmHistory(UInt32 handle, out UInt32 maxHistory);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_mc_alarm_history",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn McAlarm_history(UInt32 handle, out UInt32 alarmNo, out byte alarmType,
    //                                                 out byte seriousLevel, out byte resetType, LPSYSTEMTIME occuredTime,
    //                                                 LPSYSTEMTIME resetTime, out UInt32 sumArray, string ncMessage);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_mc_alarm_reset",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn McAlarmReset(UInt32 handle, UInt32 resetTime);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="spindleNo"></param>
    /// <param name="gear"></param>
    /// <param name="direction"></param>
    /// <returns>O if success, otherwise failed</returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_spindle_status",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn SpindleStatus (UInt32 handle, UInt32 spindleNo, out Int32 gear, out Int32 direction);

    /// <summary>
    /// Retrieve the gear position and the status of the spindle
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="spindleNo"></param>
    /// <param name="sCode"></param>
    /// <param name="instruct"></param>
    /// <param name="actual"></param>
    /// <returns>O if success, otherwise failed</returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_spindle_speed",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn SpindleSpeed (UInt32 handle, UInt32 spindleNo, out UInt32 sCode, out UInt32 instruct, out UInt32 actual);

    /// <summary>
    /// Retrieve the spindle rotation time of the machining commanded by MML3
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="millingTime"></param>
    /// <returns></returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_get_milling_time",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn GetMillingTime (UInt32 handle, out UInt32 millingTime);

    /// <summary>
    /// Retrieve the status of the ATC magazine.
    /// * Set NULL in arguments about unnecessary information
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="armStandby">The status of the ATC arm is stored
    /// True: standby, False: moving the ATC arm</param>
    /// <param name="mgznStatus">Manual intervention mode of the ATC magazine is stored
    /// PMC_MAS_MODE_OFF(=0): Manual intervention is OFF
    /// PMC_MAS_MODE_TURNING_ON(=1): Waiting for manual intervention to turn ON (ATC magazine pot indexing)
    /// PMC_MAS_MODE_ON(=2): Manual intervention is ON
    /// PMC_MAS_MODE_ TURNING_OFF(=3): Waiting for manual intervention to turn OFF (ATC magazine original position is returning)
    /// </param>
    /// <returns>0 if ok, otherwise failed</returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_atc_status",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn AtcStatus (UInt32 handle, out bool armStandby, out Int32 mgznStatus);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_atc_magazine_action_start",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn AtcMagazineActionStart(UInt32 handle, UInt32 actType, UInt32 mgznNo, Int32 potNo, UInt32 tlsNo);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_chk_atc_magazine_action_finish",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn ChkAtcMagazineActionFinish(UInt32 handle, out UInt32 actType, out UInt32 mgznNo, out Int32 potNo,
    //                                                          out UInt32 tlsNo, out bool finish, out UInt32 finCondition);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_get_operation_panel_disable",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn GetOperationPanelDisable(UInt32 handle, UInt32 opePanel, out bool disable);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_set_operation_panel_disable",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn SetOperationPanelDisable(UInt32 handle, UInt32 opePanel, bool disable);


    /// <summary>
    /// Retrieve the pallet number at the specified position of the specified device
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="device">Specify the number indicating decive type. VHC(=1) Vehicle, PST(=2) Pallet Stocker, MCW(=3) Machine, WSS(=5) Work Setting Station</param>
    /// <param name="devNo">Specify the devince number</param>
    /// <param name="position">Specify the number indicating position in device. DEV_TABLE(=1) Table, DEV_FRONT(=2) Front, DEV_BACK(=3) Back</param>
    /// <param name="palletNo">The pallet number is stored</param>
    /// <returns></returns>
    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_get_pallet_no",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn GetPalletNo(UInt32 handle, UInt32 device, UInt32 devNo, UInt32 position, out UInt32 palletNo);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_set_pallet_no",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn SetPalletNo(UInt32 handle, UInt32 device, UInt32 devNo, UInt32 position, UInt32 palletNo);

    /// <summary>
    /// Retrieve the information of pallet changer
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="apcArmType">The type of pallet changer arm is stored
    /// PMC_APC_TYPE_NO_APC(=0): there is no pallet changer
    /// PMC_APC_TYPE_TURN(=1): turn type
    /// PMC_APC_TYPE_SHUTTLE(=2): shuttle type
    /// </param>
    /// <param name="maxMgznPallet">The number of the pallet in the magazine is stored</param>
    /// <param name="maxOutPallet">The number of the pallet outside the magazine is stored</param>
    /// <param name="faces">The number of faces per pallet is stored</param>
    /// <returns>0 if ok, otherwise failed</returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_apc_info",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn ApcInfo (UInt32 handle, out Int32 apcArmType, out UInt32 maxMgznPallet, out UInt32 maxOutPallet, out UInt32 faces);

    /// <summary>
    /// Retrieve the status of pallet changer
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="armStandby">The status of pallet changer arm is stored
    /// True: standby, false: moving the pallet changer</param>
    /// <param name="stckStatus">Manual intervention mode of the pallet buffer is stored
    /// PMC_MAS_MODE_OFF(=0): Manual intervention is OFF
    /// PMC_MAS_MODE_TURNING_ON(=1): Waiting for manual intervention to turn ON (Moving the pallet changer)
    /// PMC_MAS_MODE_ON(=2): Manual intervention is ON</param>
    /// <returns>0 if ok, otherwise failed</returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_apc_status",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn ApcStatus (UInt32 handle, out bool armStandby, out Int32 stckStatus);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_pm_info",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn PmInfo(UInt32 handle, out UInt32 type, out UInt32 maxPlt, out UInt32 maxPst, out UInt32 maxWss);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_pm_trans_start",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn PmTransStart(UInt32 handle, PMB_TRANS_COMMAND *command);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_pm_chk_trans_finish",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn PmChkTransFinish(UInt32 handle, out bool finish, out UInt32 commandStatus,
    //                                                    out UInt32 finishStatus, out UInt32 alarmNo, PMB_TRANS_COMMAND *command);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_pm_get_wss_status",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn PmGetWssStatus(UInt32 handle, out UInt32 manual,
    //                                              out UInt32 doorUnlock, out UInt32 doorOpen, out UInt32 maintenance);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_pm_get_wss_operation",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn PmGetWssOperation(UInt32 handle, UInt32 wssNo, out UInt32 opeType);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_pm_set_wss_operation",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn PmSetWssOperation(UInt32 handle, UInt32 wssNo, UInt32 opeType);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_chk_operation_panel_switch",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn ChkOperationPanelSwitch(UInt32 handle, UInt32 switchType,
    //                                                       UInt32 devNo, out bool push, out Int32 data1, out Int32 data2);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_accept_operation_panel_switch",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn AcceptOperationPanelSwitch(UInt32 handle, UInt32 switchType, UInt32 devNo, bool accept);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_cancel_operation_panel_switch",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn CancelOperationPanelSwitch(UInt32 handle, UInt32 switchType, UInt32 devNo);

    /// <summary>
    /// Retrieve the tool information of spindle
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="mgznNo"></param>
    /// <param name="potNo"></param>
    /// <param name="cutterNo"></param>
    /// <param name="ftn"></param>
    /// <param name="itn"></param>
    /// <param name="ptn"></param>
    /// <returns>O if success, otherwise failed</returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_spindle_tool",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn SpindleTool (UInt32 handle, out UInt32 mgznNo, out Int32 potNo,
                                               out UInt32 cutterNo, out UInt32 ftn, out UInt32 itn, out UInt32 ptn);

    /// <summary>
    /// Retrieve the tool information of next tool
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="mgznNo"></param>
    /// <param name="potNo"></param>
    /// <param name="cutterNo"></param>
    /// <param name="ftn"></param>
    /// <param name="itn"></param>
    /// <param name="ptn"></param>
    /// <returns>O if success, otherwise failed</returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_next_tool",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn NextTool (UInt32 handle, out UInt32 mgznNo, out Int32 potNo,
                                            out UInt32 cutterNo, out UInt32 ftn, out UInt32 itn, out UInt32 ptn);

    /// <summary>
    /// Retrieve the information of TLS pot of the specified TLS number
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="mgznNo">The magazine number of the tool on specified TLS is stored</param>
    /// <param name="potNo">The pot number of the tool on specified TLS is stored</param>
    /// <param name="tlsNo">Specify the TLS number</param>
    /// <returns>O if success, otherwise failed</returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_tls_potno",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn TlsPotno (UInt32 handle, out UInt32 mgznNo, out Int32 potNo, UInt32 tlsNo);

    /// <summary>
    /// Retrieve the number of TLS (Tool Loading Station).
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="maxTls">The number of TLS is stored</param>
    /// <returns>O if success, otherwise failed</returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_max_tls",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn MaxTls (UInt32 handle, out UInt32 maxTls);

    /// <summary>
    /// Retrieve whether the specified tool data item is effective.
    /// Two or more tool data items can be checked at a time.
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="item">Specify the address of the array where the number indicating the tool data item is stored</param>
    /// <param name="enable">The value whether specified each item is effective is stored</param>
    /// <param name="sumArray">Specify the element count of item and enable array</param>
    /// <returns>O if success, otherwise failed</returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_tool_data_item_is_enable",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn ToolDataItemIsEnable (UInt32 handle, [In] Pro5ToolDataItem[] item,
                                                        [In, Out] UInt32[] enable, UInt32 sumArray);

    /// <summary>
    /// Retrieve the values of specified tool data item to two or more tools.
    /// * There is a tool data item which cannot be used according to the model and the option
    ///   Confirm whether the specified data item can be used with "ToolDataItemIsEnable"
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="item">Specify the tool data item of the table below</param>
    /// <param name="mgznNo">Specify the address of the array where the magazine number is stored</param>
    /// <param name="potNo">Specify the address of the array where the pot number is stored</param>
    /// <param name="value">The retrieving value is stored in the array</param>
    /// <param name="sumArray">Specify the element count of mgznNo, potNo and value array</param>
    /// <returns>O if success, otherwise failed</returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_get_tool_data_item",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn GetToolDataItem (UInt32 handle, Pro5ToolDataItem item,
                                                   [In] UInt32[] mgznNo, [In] Int32[] potNo,
                                                   [In, Out] Int32[] value, UInt32 sumArray);

    /// <summary>
    /// Change the value of the specified tool data item to two or more tools
    /// * There is a tool data item which cannot be used according to the model and the option
    ///   Confirm whether the specified data item can be used with "ToolDataItemIsEnable"
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="item">Specify the tool data item of the table below</param>
    /// <param name="mgznNo">Specify the address of the array where the magazine number is stored</param>
    /// <param name="potNo">Specify the address of the array where the pot number is stored</param>
    /// <param name="value">Specify the address of the array where the set value is stored</param>
    /// <param name="sumArray">Specify the element count of mgznNo, potNo and value array</param>
    /// <returns>O if success, otherwise failed</returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_set_tool_data_item",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn SetToolDataItem (UInt32 handle, Pro5ToolDataItem item,
                                                   [In] UInt32[] mgznNo, [In] Int32[] potNo,
                                                   [In] Int32[] value, UInt32 sumArray);

    /// <summary>
    /// Retrieve the values of specified cutter data item to two or more tool cutters
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="item">Specify the cutter data item of the table below</param>
    /// <param name="mgznNo">Specify the address of the array where the magazine number is stored</param>
    /// <param name="potNo">Specify the address of the array where the pot number is stored</param>
    /// <param name="cutterNo">Specify the address of the array where the cutter number is stored</param>
    /// <param name="value">The retrieving value is stored in the array</param>
    /// <param name="sumArray">Specify the element count of mgznNo, potNo, cutterNo and value array</param>
    /// <returns>O if success, otherwise failed</returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_get_cutter_data_item",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn GetCutterDataItem (UInt32 handle, Pro5ToolDataItem item,
                                                     [In] UInt32[] mgznNo, [In] Int32[] potNo,
                                                     [In] UInt32[] cutterNo, [In, Out] Int32[] value,
                                                     UInt32 sumArray);
    /// <summary>
    /// Change the value of the specified cutter data item to two or more tool cutters
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="item">Specify the cutter data item of the table below</param>
    /// <param name="mgznNo">Specify the address of the array where the magazine number is stored</param>
    /// <param name="potNo">Specify the address of the array where the pot number is stored</param>
    /// <param name="cutterNo">Specify the address of the array where the cutter number is stored</param>
    /// <param name="value">Specify the address of the array where the set value is stored</param>
    /// <param name="sumArray">Specify the element count of mgznNo, potNo, cutterNo and value array</param>
    /// <returns>O if success, otherwise failed</returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_set_cutter_data_item",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn SetCutterDataItem (UInt32 handle, Pro5ToolDataItem item,
                                                     [In] UInt32[] mgznNo, [In] Int32[] potNo,
                                                     [In] UInt32[] cutterNo, [In] Int32[] value,
                                                     UInt32 sumArray);

    /// <summary>
    /// Clear the data of the specified tool. Two or more tools can be specified at a time
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="mgznNo">Specify the address of the array where the magazine number of
    /// the tool to clear data is stored</param>
    /// <param name="potNo">Specify the address of the array where the pot number of the tool
    /// to clear data is stored</param>
    /// <param name="sumArray">Specify the element count of mgznNo and potNo array</param>
    /// <returns>O if success, otherwise failed</returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_clear_tool_data",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn ClearToolData (UInt32 handle, [In] UInt32[] mgznNo,
                                           [In] Int32[] potNo, UInt32 sumArray);

    /// <summary>
    /// Retrieve the value of the specified pallet data item to two or more pallets
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="item">Specify the pallet data item of the table below</param>
    /// <param name="palletNo">Specify the address of the array where the pallet number is stored</param>
    /// <param name="value">The retrieving value is stored in the array</param>
    /// <param name="sumArray">Specify the element count of palletNo and value array</param>
    /// <returns></returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_get_pallet_data_item",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn GetPalletDataItem (UInt32 handle, UInt32 item,
                                                     [In] UInt32[] palletNo, [In, Out] Int32[] value,
                                                     UInt32 sumArray);

    /// <summary>
    /// Change the value of the specified pallet data item to two or more pallets
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="item">Specify the pallet data item of the table below</param>
    /// <param name="palletNo">Specify the address of the array where the pallet number is stored</param>
    /// <param name="value">Specify the address of the array where the set value is stored</param>
    /// <param name="sumArray">Specify the element count of palletNo and value array</param>
    /// <returns></returns>
    [DllImport ("Md3Pro5.dll", EntryPoint = "md3pro5_set_pallet_data_item",
               SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn SetPalletDataItem (UInt32 handle, UInt32 item,
                                                     [In] UInt32[] palletNo, [In] Int32[] value, UInt32 sumArray);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_get_face_data_item",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn GetFaceDataItem(UInt32 handle, UInt32 item,
    //                                               [In] UInt32[] palletNo, [In] UInt32[] faceNo, [In, Out] Int32[] value, UInt32 sumArray);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_set_face_data_item",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn SetFaceDataItem(UInt32 handle, UInt32 item,
    //                                               [In] UInt32[] palletNo, [In] UInt32[] faceNo, [In] Int32[] value, UInt32 sumArray);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_get_face_start_date",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn GetFaceStartDate(UInt32 handle,
    //                                                [In] UInt32[] palletNo, [In] UInt32[] faceNo, LPSYSTEMTIME sysTime, UInt32 sumArray);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_get_face_finish_date",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn GetFaceFinishDate(UInt32 handle,
    //                                                 [In] UInt32[] palletNo, [In] UInt32[] faceNo, LPSYSTEMTIME sysTime, UInt32 sumArray);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_clear_work_data",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn ClearWorkData(UInt32 handle, [In] UInt32[] palletNo, UInt32 sumArray);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_get_event_counter",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn GetEventCounter(UInt32 handle, UInt32 eventType, out UInt32 eventCounter);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_tool_trans_system_info",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn ToolTransSystemInfo(UInt32 handle, unit_t *ownUnit,
    //                                                   out UInt32 totalOtherUnit, out UInt32 eachUnitPortableTool, unit_t *lpOtherUnitArray, UInt32 sumOtherUnitArray);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_tool_trans_system_get_unloaded_pot",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn ToolTransSystemGetUnloaded_pot(UInt32 handle, unit_t dstUnit, out UInt32 ownMgznNo,
    //                                                              out Int32 ownPotNo, out UInt32 dstMgznNo, out Int32 dstPotNo, UInt32 sumArray);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_tool_trans_system_get_loaded_pot",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn ToolTransSystemGetLoadedPot(UInt32 handle, unit_t srcUnit, out UInt32 ownMgznNo,
    //                                                           out Int32 ownPotNo, out UInt32 srcMgznNo, out Int32 srcPotNo, UInt32 sumArray);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_get_arbitrary_machine_status",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn GetArbitraryMachineStatus(UInt32 handle, UInt32 statusKind1, UInt32 statusKind2, UInt32 statusKind3, UInt32 statusKind4,
    //                                                         out Int32 outputData1, out Int32 outputData2, out Int32 outputData3, out Int32 outputData4);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_get_work_data",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn GetWorkData(UInt32 handle, out UInt32 palletNo, out byte workData, UInt32 sumArray);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_set_work_data",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn SetWorkData(UInt32 handle, out byte workData, UInt32 sumArray);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_get_iac_data",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn GetIacData(UInt32 handle, out UInt32 palletNo, out byte iacData, UInt32 sumArray);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_set_iac_data",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn SetIacData(UInt32 handle, out byte iacData, UInt32 sumArray);

    //    [DllImport("Md3Pro5.dll", EntryPoint="md3pro5_tool_data_Indiv",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    static extern MMLReturn ToolDataIndiv(UInt32 handle, out UInt32 MgznNo, out Int32 potNo,
    //                                             out UInt32 cutterNo, out UInt32 ftn, out UInt32 itn, out UInt32 ptn, out UInt32 updateCounter, UInt32 sumArray);
    #endregion // Other methods
  }
}
