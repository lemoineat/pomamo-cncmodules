using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// SystemTime structure
  /// </summary>
  public struct SYSTEMTIME
  {
    /// <summary>
    /// Number of the year, in [1601 ; 30827]
    /// </summary>
    public ushort wYear;

    /// <summary>
    /// Month, in [1 ; 12]
    /// </summary>
    public ushort wMonth;

    /// <summary>
    /// Day of the week, 0 being sunday and 6 being saturday
    /// </summary>
    public ushort wDayOfWeek;

    /// <summary>
    /// Day of the month, in [1 ; 31]
    /// </summary>
    public ushort wDay;

    /// <summary>
    /// Hour, in [0 ; 23]
    /// </summary>
    public ushort wHour;

    /// <summary>
    /// Minute, in [0 ; 59]
    /// </summary>
    public ushort wMinute;

    /// <summary>
    /// Second, in [0 ; 59]
    /// </summary>
    public ushort wSecond;

    /// <summary>
    /// Millisecond, in [0 ; 999]
    /// </summary>
    public ushort wMilliseconds;
  }

  /// <summary>
  /// Description of IMd3ProX.
  /// </summary>
  public interface IMd3ProX
  {
    /// <summary>
    /// Version number
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Initialize for a new reading
    /// </summary>
    void Initialize ();

    /// <summary>
    /// Error number of Md3Pro6.dll which occurred at the last is returned.
    /// </summary>
    /// <param name="mainErr">Internal error number of Md3Pro6.dll which occurred at the last is returned.</param>
    /// <param name="subErr">Error number of Fanuc library which occurred at the last is returned.</param>
    /// <returns>EM_OK if success, otherwise failed</returns>
    MMLReturn GetLastError (out Int32 mainErr, out Int32 subErr);

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
    MMLReturn AllocHandle (out UInt32 handle, string nodeInfo,
                          Int32 sendTimeout, Int32 recvTimeout,
                          Int32 noop, byte logLevel);

    /// <summary>
    /// Exit and disconnect the communication to specified machine (Professional 6) and release the handle
    /// </summary>
    /// <param name="handle"></param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect)</returns>
    MMLReturn FreeHandle (UInt32 handle);

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
    MMLReturn SoftwareInfo (UInt32 handle, out string productVersion, out string machineModel, out string machineSeries,
                           out string platform, out string servicePack, out string serialNumber);

    /// <summary>
    /// Retrieve the number of the current machine alarm and warning
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="totalAlarm"></param>
    /// <param name="totalWarn"></param>
    /// <returns></returns>
    MMLReturn ChkMcAlarm (UInt32 handle, out UInt32 totalAlarm, out UInt32 totalWarn);

    /// <summary>
    /// Retrieve the pallet number at the specified position of the specified device.
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
    MMLReturn ToolInfo (UInt32 handle, out int inchUnit, out UInt32 ftnFig,
                       out UInt32 itnFig, out UInt32 ptnFig, out UInt32 manageType);

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
    MMLReturn McAlarm (UInt32 handle, ref UInt32[] alarmNo, ref byte[] alarmType, ref byte[] seriousLevel,
                      ref byte[] powerOutDisable, ref byte[] cycleStartDisable, ref byte[] retryEnable,
                      ref bool[] failedNcReset, ref SYSTEMTIME[] occuredTime, ref UInt32 sumArray);

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
    MMLReturn SpindleTool (UInt32 handle, out UInt32 mgznNo, out Int32 potNo,
                          out UInt32 cutterNo, out UInt32 ftn, out UInt32 itn, out UInt32 ptn);

    /// <summary>
    /// Retrieve the tool life information
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="countTypeRemain">True: count down, False: count up</param>
    /// <param name="alarmResetMaxLife">The setting whether remain Full at TL Alarm Reset is stored.
    /// True: set remaining life to full, False: do not change remaining life</param>
    /// <returns>O if success, otherwise failed</returns>
    MMLReturn ToollifeInfo (UInt32 handle, out bool countTypeRemain, out bool alarmResetMaxLife);

    /// <summary>
    /// Retrieve the management type of the tool number of the ATC magazine
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="randomAtc">The management type of the tool number of the ATC magazine is stored
    /// True: random pot number
    /// False: fixed pot number</param>
    /// <returns></returns>
    MMLReturn AtcRandomMagazine (UInt32 handle, out bool randomAtc);

    /// <summary>
    /// Retrieve whether the specified tool data item is effective.
    /// Two or more tool data items can be checked at a time.
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="item">Specify the address of the array where the number indicating the tool data item is stored</param>
    /// <param name="enable">The value whether specified each item is effective is stored</param>
    /// <param name="sumArray">Specify the element count of item and enable array</param>
    /// <returns>O if success, otherwise failed</returns>
    MMLReturn ToolDataItemIsEnable (UInt32 handle, Pro5ToolDataItem[] item,
                                   ref UInt32[] enable, UInt32 sumArray);

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
    MMLReturn GetToolDataItem (UInt32 handle, int item,
                               UInt32[] mgznNo, Int32[] potNo,
                               ref Int32[] value, UInt32 sumArray);

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
    MMLReturn SetToolDataItem (UInt32 handle, int item,
                               UInt32[] mgznNo, Int32[] potNo,
                               Int32[] value, UInt32 sumArray);

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
    MMLReturn GetCutterDataItem (UInt32 handle, int item,
                                 UInt32[] mgznNo, Int32[] potNo, UInt32[] cutterNo,
                                 ref Int32[] value, UInt32 sumArray);

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
    MMLReturn SetCutterDataItem (UInt32 handle, int item,
                                 UInt32[] mgznNo, Int32[] potNo, UInt32[] cutterNo,
                                 Int32[] value, UInt32 sumArray);

    /// <summary>
    /// Clear the data of the specified tools. Two or more tools can be specified at a time
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="magazineNumbers">Array of magazines to clear</param>
    /// <param name="potNumbers">Array of pots to clear</param>
    /// <returns></returns>
    MMLReturn ClearToolData (UInt32 handle, UInt32[] magazineNumbers, Int32[] potNumbers);

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
    MMLReturn MaxAtcMagazine (UInt32 handle, out UInt32 actualMgzn, out int outMcMgzn);

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
    MMLReturn AtcMagazineInfo (UInt32 handle, UInt32 mgznNo, out UInt32 maxPot, out Int32 mgznType, out UInt32 emptyPot);

    /// <summary>
    /// Retrieve the pallet number at the specified position of the specified device
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="device">Specify the number indicating decive type. VHC(=1) Vehicle, PST(=2) Pallet Stocker, MCW(=3) Machine, WSS(=5) Work Setting Station</param>
    /// <param name="devNo">Specify the devince number</param>
    /// <param name="position">Specify the number indicating position in device. DEV_TABLE(=1) Table, DEV_FRONT(=2) Front, DEV_BACK(=3) Back</param>
    /// <param name="palletNo">The pallet number is stored</param>
    /// <returns></returns>
    MMLReturn GetPalletNo (UInt32 handle, UInt32 device, UInt32 devNo, UInt32 position, out UInt32 palletNo);
  }
}
