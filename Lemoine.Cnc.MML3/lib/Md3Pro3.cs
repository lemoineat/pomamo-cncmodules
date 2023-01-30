using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Lemoine.Core.Log;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Interface to the library Md3Pro3.dll
  /// </summary>
  public class Md3Pro3 : IMd3ProX
  {
    ILog log = LogManager.GetLogger (typeof (Md3Pro3).FullName);

    #region IMd3ProX implementation
    /// <summary>
    /// Version number
    /// </summary>
    int IMd3ProX.Version { get { return 3; } }

    /// <summary>
    /// Initialize for a new reading
    /// </summary>
    void IMd3ProX.Initialize ()
    {
      m_preparedToolValuesPerPot.Clear ();
    }

    /// <summary>
    /// Error number of Md3Pro6.dll which occurred at the last is returned.
    /// </summary>
    /// <param name="mainErr">Internal error number of Md3Pro6.dll which occurred at the last is returned.</param>
    /// <param name="subErr">Error number of Fanuc library which occurred at the last is returned.</param>
    /// <returns>EM_OK if success, otherwise failed</returns>
    MMLReturn IMd3ProX.GetLastError (out Int32 mainErr, out Int32 subErr)
    {
      return Md3Pro3.GetLastError (out mainErr, out subErr);
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
    /// <param name="logLevel">Communication Log (1-No log, 1-send/receive data log, 2-send/receive respons log, 4-send/receive BODY data log)</param>
    /// <returns>EM_OK(success) / EM_DATA(invalid data) / EM_HANDFULL(overconnect) /
    /// EM_NODE(invalid node) / EM_DISCONNECT(disconnect) / EM_BUSY(device is busy) / EM_INTERNAL(internal error)</returns>
    MMLReturn IMd3ProX.AllocHandle (out UInt32 handle, string nodeInfo,
                                   Int32 sendTimeout, Int32 recvTimeout,
                                   Int32 noop, byte logLevel)
    {
      return Md3Pro3.AllocHandle (out handle, nodeInfo, sendTimeout + recvTimeout, 1, noop);
    }

    /// <summary>
    /// Exit and disconnect the communication to specified machine (Professional 3) and release the handle
    /// </summary>
    /// <param name="handle"></param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect)</returns>
    MMLReturn IMd3ProX.FreeHandle (UInt32 handle)
    {
      return Md3Pro3.FreeHandle (handle);
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

      var sb = new StringBuilder (48);
      var ret = Md3Pro3.ProVersion (handle, sb);
      productVersion = sb.ToString ();

      // The second letter becomes K, the third letter E becomes J
      // The last numbers become 0
      // M{XX}NN{NN} => M{KJ}NN{00}
      productVersion = Regex.Replace (productVersion, @"M.{2}([0-9]{2})[0-9]{2}.*", @"MKJ${1}00");

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
      bool alarm = false;
      var ret = Md3Pro3.ChkMcAlarm (handle, out alarm);
      totalAlarm = alarm ? (uint)64 : 0;
      totalWarn = 0;
      return ret;
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
      int length = alarmNo.Length;
      var tmpTaskID = new UInt16[length];
      var tmpAlarmType = new UInt16[length];
      var tmpRetryEnable = new UInt16[length];
      UInt16 tmpSum = 0;
      var ret = Md3Pro3.McAlarm (handle, tmpTaskID, alarmNo, tmpAlarmType, tmpRetryEnable, ref tmpSum);

      sumArray = tmpSum;
      for (int i = 0; i < length; i++) {
        alarmType[i] = (byte)tmpAlarmType[i];
        seriousLevel[i] = 0;
        powerOutDisable[i] = 0;
        cycleStartDisable[i] = 0;
        retryEnable[i] = (byte)tmpRetryEnable[i];
        failedNcReset[i] = false;
        occuredTime[i] = new SYSTEMTIME ();
        occuredTime[i].wYear = 0;
      }

      return MMLReturn.EM_OK;
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
      randomAtc = false;
      return MMLReturn.EM_OK;
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
    /// <returns>EM_OK is ok, otherwise failed</returns>
    MMLReturn IMd3ProX.MaxAtcMagazine (UInt32 handle, out UInt32 actualMgzn, out int outMcMgzn)
    {
      actualMgzn = 1;
      outMcMgzn = 0;
      return MMLReturn.EM_OK;
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
    /// * PMC_ATC_MGZN_TYPE_CHAIN(=1) : Chain
    /// * PMC_ATC_MGZN_TYPE_MATRIX(=2) : Matrix
    /// * PMC_ATC_MGZN_TYPE_DISC(=3) : Disc
    /// * PMC_ATC_MGZN_CARTRIDGE(=4) : Cartridge
    /// * PMC_ATC_MGZN_RING(=5) : Ring
    /// * PMC_ATC_MGZN_ARRAY(=6) : H2J,H2,H5 type
    /// * PMC_ATC_MGZN_DRUM(=7) : Drum
    /// * PMC_ATC_MGZN_DOUBLE_RING(=8) : Double ring
    /// * PMC_ATC_MGZN_C17_MATRIX(=9) : Pot-less
    /// * PMC_ATC_MGZN_EGG_POT(=10) : Egg pot
    /// If PMC_MAGAZINE_OUTMC is specified for mgznNo, 0 is stored.</param>
    /// <param name="emptyPot">0 is stored in V series</param>
    /// <returns>EM_OK if ok, otherwise failed</returns>
    MMLReturn IMd3ProX.AtcMagazineInfo (UInt32 handle, UInt32 mgznNo, out UInt32 maxPot, out Int32 mgznType, out UInt32 emptyPot)
    {
      MMLReturn ret = MMLReturn.EM_OK;
      emptyPot = 0; // Not used
      mgznType = 1; // Not used
      if (mgznNo == 1) {
        UInt16 tmpMaxPot = 0;
        ret = Md3Pro3.MaxPot (handle, out tmpMaxPot);
        maxPot = tmpMaxPot;
      }
      else {
        maxPot = 0;
      }

      return ret;
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
      manageType = 1;
      var ret = Md3Pro3.ToolInfo (handle, out Int16 tmpInchUnit, out Int16 tmpFtnFig, out Int16 tmpItnFig, out Int16 tmpPtnFig);

      inchUnit = tmpInchUnit;
      ftnFig = (UInt32)tmpFtnFig;
      itnFig = (UInt32)tmpItnFig;
      ptnFig = (UInt32)tmpPtnFig;

      return ret;
    }

    /// <summary>
    /// Retrieve the tool life information
    /// Life type is stored: must be called before "ToolDataItemIsEnable"
    /// Unit of distance is known with the function "ToolInfo"
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="countTypeRemain">True: count down, False: count up</param>
    /// <param name="alarmResetMaxLife">The setting whether remain Full at TL Alarm Reset is stored.
    /// True: set remaining life to full, False: do not change remaining life</param>
    /// <returns>EM_OK if success, otherwise failed</returns>
    MMLReturn IMd3ProX.ToollifeInfo (UInt32 handle, out bool countTypeRemain, out bool alarmResetMaxLife)
    {
      var ret = Md3Pro3.ToollifeInfo (handle, out _, out Int16 tmpCountType, out alarmResetMaxLife);
      countTypeRemain = (tmpCountType == 1);
      return ret;
    }

    /// <summary>
    /// Get the tool life type (specific to Pro3)
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    public Pro3ToolLifeType GetToolLifeType (UInt32 handle)
    {
      Md3Pro3.ToollifeInfo (handle, out Pro3ToolLifeType tmpLifeType, out _, out _);
      return tmpLifeType;
    }

    /// <summary>
    /// Retrieve whether the specified tool data item is effective.
    /// Two or more tool data items can be checked at a time.
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="item">Specify the address of the array where the number indicating the tool data item is stored</param>
    /// <param name="enable">The value whether specified each item is effective is stored</param>
    /// <param name="sumArray">Specify the element count of item and enable array</param>
    /// <returns>EM_OK if success, otherwise failed</returns>
    MMLReturn IMd3ProX.ToolDataItemIsEnable (UInt32 handle, Pro5ToolDataItem[] item,
                                             ref UInt32[] enable, UInt32 sumArray)
    {
      // Not available for Pro3 since it uses another list
      return MMLReturn.EM_INTERNAL;
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
    /// <returns>EM_OK if success, otherwise failed</returns>
    MMLReturn IMd3ProX.GetToolDataItem (UInt32 handle, int item,
                                        UInt32[] mgznNo, Int32[] potNo,
                                        ref Int32[] value, UInt32 sumArray)
    {
      // Convert array
      var pots = Enumerable.Repeat ((UInt16)0, potNo.Count ()).ToArray ();
      for (int i = 0; i < potNo.Count (); i++) {
        pots[i] = (UInt16)potNo[i];
      }

      return ReadToolDataItem (handle, (Pro3ToolCommonItem)item, pots, ref value);
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
      // Convert array
      var pots = Enumerable.Repeat ((UInt16)0, potNo.Count ()).ToArray ();
      for (int i = 0; i < potNo.Count (); i++) {
        pots[i] = (UInt16)potNo[i];
      }

      return WriteToolDataItem (handle, (Pro3ToolCommonItem)item, pots, value);
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
    /// <returns>EM_OK if success, otherwise failed</returns>
    MMLReturn IMd3ProX.GetCutterDataItem (UInt32 handle, int item,
                                         UInt32[] mgznNo, Int32[] potNo,
                                         UInt32[] cutterNo, ref Int32[] value,
                                         UInt32 sumArray)
    {
      // Convert array
      var pots = Enumerable.Repeat ((UInt16)0, potNo.Count ()).ToArray ();
      for (int i = 0; i < potNo.Count (); i++) {
        pots[i] = (UInt16)potNo[i];
      }

      return ReadToolDataItem (handle, (Pro3ToolCommonItem)item, pots, ref value);
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
      // Convert array
      var pots = Enumerable.Repeat ((UInt16)0, potNo.Count ()).ToArray ();
      for (int i = 0; i < potNo.Count (); i++) {
        pots[i] = (UInt16)potNo[i];
      }

      return WriteToolDataItem (handle, (Pro3ToolCommonItem)item, pots, value);
    }

    /// <summary>
    /// Clear the data of the specified tools. Two or more tools can be specified at a time
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="magazineNumbers">Array of magazines to clear (not used)</param>
    /// <param name="potNumbers">Array of pots to clear</param>
    /// <returns></returns>
    MMLReturn IMd3ProX.ClearToolData (UInt32 handle, UInt32[] magazineNumbers, Int32[] potNumbers)
    {
      // Fill properties with 0
      var values = Enumerable.Repeat ((Int32)0, potNumbers.Count ()).ToArray ();

      // Convert pot array
      var pots = Enumerable.Repeat ((UInt16)0, potNumbers.Count ()).ToArray ();
      for (int i = 0; i < potNumbers.Count (); i++) {
        pots[i] = (UInt16)potNumbers[i];
      }

      MMLReturn ret = MMLReturn.EM_OK;
      foreach (Pro3ToolCommonItem property in Enum.GetValues (typeof (Pro3ToolCommonItem))) {
        // Don't reset the MMC_TYPE (this is a string)
        if (property == Pro3ToolCommonItem.TD_MMC_TYPE) {
          continue;
        }

        try {
          ret = WriteToolDataItem (handle, (Pro3ToolCommonItem)property, pots, values);
          if (ret != MMLReturn.EM_OK) {
            log.ErrorFormat ("Couldn't reset property {0}: result is {1}", (Pro3ToolCommonItem)property, ret);
            break;
          }
        } catch (Exception e) {
          log.ErrorFormat ("Couldn't reset property {0}: exception is {1}", (Pro3ToolCommonItem)property, e);
        }
      }

      return ret;
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
      mgznNo = 0;
      cutterNo = 0;
      UInt16 tmp;
      var result = SpindleTool (handle, out tmp, out ftn, out itn, out ptn);
      potNo = tmp;

      return result;
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
      if ((3 == device) && (1 == position)) {
        UInt16 pltno;
        var result = GetTablePltno (handle, out pltno);
        palletNo = (UInt32)pltno;
        return result;
      }
      else {
        log.Error ($"GetPalletNo: device={device} position={position} not supported");
        throw new Exception ("Device or position is not supported");
      }
    }
    #endregion // IMd3ProX implementation

    #region // Additional logic
    readonly IDictionary<int, IDictionary<Pro3ToolCommonItem, object>> m_preparedToolValuesPerPot =
      new Dictionary<int, IDictionary<Pro3ToolCommonItem, object>> ();
    readonly IDictionary<Pro3ToolCommonItem, bool> m_allowedProperties = new Dictionary<Pro3ToolCommonItem, bool> ();

    MMLReturn CheckAllowedProperties(UInt32 handle)
    {
      // Read what kind of property is allowed
      var ret = OptionalTldtDefine (handle, out bool mmcData, out bool btsData,
                                    out bool airData, out bool slowData, out bool btsLenData);
      if (ret != MMLReturn.EM_OK) {
        log.ErrorFormat ("Couldn't check what tool properties can be read. OptionalTldtDefine returned {0}", ret);
        return ret;
      }

      m_allowedProperties[Pro3ToolCommonItem.TD_PTN] = true;
      m_allowedProperties[Pro3ToolCommonItem.TD_ITN] = true;
      m_allowedProperties[Pro3ToolCommonItem.TD_TL] = true;
      m_allowedProperties[Pro3ToolCommonItem.TD_REMAIN] = true;
      m_allowedProperties[Pro3ToolCommonItem.TD_LEN] = true;
      m_allowedProperties[Pro3ToolCommonItem.TD_DIA] = true;
      m_allowedProperties[Pro3ToolCommonItem.TD_FTN] = true;
      m_allowedProperties[Pro3ToolCommonItem.TD_TYPE] = true;
      m_allowedProperties[Pro3ToolCommonItem.TD_ALM] = true;
      m_allowedProperties[Pro3ToolCommonItem.TD_AC] = true;
      m_allowedProperties[Pro3ToolCommonItem.TD_SL] = true;
      m_allowedProperties[Pro3ToolCommonItem.TD_MMC_TYPE] = mmcData;
      m_allowedProperties[Pro3ToolCommonItem.TD_MMC_STT] = mmcData;
      m_allowedProperties[Pro3ToolCommonItem.TD_MMC_SIZE] = mmcData;
      m_allowedProperties[Pro3ToolCommonItem.TD_BTS] = btsData;
      m_allowedProperties[Pro3ToolCommonItem.TD_BTS_FST] = btsData;
      m_allowedProperties[Pro3ToolCommonItem.TD_BTS_SEC] = btsData;
      m_allowedProperties[Pro3ToolCommonItem.TD_AIR] = airData;
      m_allowedProperties[Pro3ToolCommonItem.TD_SLOW] = slowData;
      m_allowedProperties[Pro3ToolCommonItem.TD_BTS_LEN] = btsLenData;
      m_allowedProperties[Pro3ToolCommonItem.TD_BTS_TYPE] = btsData;

      return MMLReturn.EM_OK;
    }

    MMLReturn ReadToolDataItem (UInt32 handle, Pro3ToolCommonItem item, UInt16[] potNo, ref Int32[] value)
    {
      // Check that potNo and value have the same length
      if (potNo.Length != value.Length) {
        throw new Exception ("potNo and value must have the same length");
      }

      // Check that we can read this data
      if (!m_allowedProperties.ContainsKey(item)) {
        var ret = CheckAllowedProperties (handle);
        if (ret != MMLReturn.EM_OK) {
          return ret;
        }
      }
      if (!m_allowedProperties[item]) {
        return MMLReturn.EM_DATA;
      }

      // Min pot, max pot, number of values to read
      UInt16 minPot = potNo[0];
      UInt16 maxPot = potNo[0];
      for (int i = 1; i < potNo.Length; i++) {
        if (potNo[i] < minPot) {
          minPot = potNo[i];
        }

        if (potNo[i] > maxPot) {
          maxPot = potNo[i];
        }
      }
      int numberOfValues = maxPot - minPot + 1;

      // Maybe data needs to be read
      bool acquisitionNeeded = false;
      for (UInt16 pot = minPot; pot <= maxPot; pot++) {
        if (!m_preparedToolValuesPerPot.ContainsKey (pot)) {
          m_preparedToolValuesPerPot[pot] = new Dictionary<Pro3ToolCommonItem, object> ();
        }

        if (!m_preparedToolValuesPerPot[pot].ContainsKey (item)) {
          acquisitionNeeded = true;
        }
      }

      if (acquisitionNeeded) {
        // Read data
        MMLReturn ret = MMLReturn.EM_DATA;

        try {
          if ((int)item <= (int)Pro3ToolCommonItem.TD_SL)
          {
            UInt32[] ptn = Enumerable.Repeat ((UInt32)0, numberOfValues).ToArray ();
            UInt32[] ftn = Enumerable.Repeat ((UInt32)0, numberOfValues).ToArray ();
            UInt32[] itn = Enumerable.Repeat ((UInt32)0, numberOfValues).ToArray ();
            UInt32[] tl = Enumerable.Repeat ((UInt32)0, numberOfValues).ToArray ();
            UInt32[] remain = Enumerable.Repeat ((UInt32)0, numberOfValues).ToArray ();
            Int32[] len = Enumerable.Repeat ((Int32)0, numberOfValues).ToArray ();
            Int32[] dia = Enumerable.Repeat ((Int32)0, numberOfValues).ToArray ();
            UInt16[] type = Enumerable.Repeat ((UInt16)0, numberOfValues).ToArray ();
            UInt16[] alm = Enumerable.Repeat ((UInt16)0, numberOfValues).ToArray ();
            UInt16[] ac = Enumerable.Repeat ((UInt16)0, numberOfValues).ToArray ();
            UInt16[] sl = Enumerable.Repeat ((UInt16)0, numberOfValues).ToArray ();
            ret = GetToolCommon (handle, minPot, maxPot, ptn, ftn, itn, tl, remain, len, dia, type, alm, ac, sl);
            if (ret == MMLReturn.EM_OK) {
              // Store data
              for (int pot = minPot; pot <= maxPot; pot++) {
                int index = pot - minPot;
                m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_PTN] = ptn[index];
                m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_FTN] = ftn[index];
                m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_ITN] = itn[index];
                m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_TL] = tl[index];
                m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_REMAIN] = remain[index];
                m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_LEN] = len[index];
                m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_DIA] = dia[index];
                m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_TYPE] = type[index];
                m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_ALM] = alm[index];
                m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_AC] = ac[index];
                m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_SL] = sl[index];
              }
            }
          }
          else if (item == Pro3ToolCommonItem.TD_MMC_TYPE || item == Pro3ToolCommonItem.TD_MMC_STT || item == Pro3ToolCommonItem.TD_MMC_SIZE) 
          {
            StringBuilder[] type = new StringBuilder[numberOfValues];
            for (int i = 0; i < numberOfValues; i++) {
              type[i] = new StringBuilder (128);
            }

            UInt16[] status = Enumerable.Repeat ((UInt16)0, numberOfValues).ToArray ();
            UInt16[] size = Enumerable.Repeat ((UInt16)0, numberOfValues).ToArray ();
            ret = GetToolMmc (handle, minPot, maxPot, type, status, size);
            if (ret == MMLReturn.EM_OK) {
              // Store data
              for (int pot = minPot; pot <= maxPot; pot++) {
                int index = pot - minPot;
                m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_MMC_TYPE] = type[index];
                m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_MMC_STT] = status[index];
                m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_MMC_SIZE] = size[index];
              }
            }
          }
          else if (item == Pro3ToolCommonItem.TD_BTS || item == Pro3ToolCommonItem.TD_BTS_FST || item == Pro3ToolCommonItem.TD_BTS_SEC ||
            item == Pro3ToolCommonItem.TD_BTS_TYPE)
          {
            bool[] btsOn = Enumerable.Repeat (false, numberOfValues).ToArray ();
            UInt16[] first = Enumerable.Repeat ((UInt16)0, numberOfValues).ToArray ();
            UInt16[] second = Enumerable.Repeat ((UInt16)0, numberOfValues).ToArray ();
            UInt16[] actType = Enumerable.Repeat ((UInt16)0, numberOfValues).ToArray ();
            ret = GetToolBts (handle, minPot, maxPot, btsOn, first, second, actType);
            if (ret == MMLReturn.EM_OK) {
              // Store data
              for (int pot = minPot; pot <= maxPot; pot++) {
                int index = pot - minPot;
                m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_BTS] = btsOn[index];
                m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_BTS_FST] = first[index];
                m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_BTS_SEC] = second[index];
                m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_BTS_TYPE] = actType[index];
              }
            }
          }
          else if (item == Pro3ToolCommonItem.TD_AIR)
          {
            UInt16[] time = Enumerable.Repeat ((UInt16)0, numberOfValues).ToArray ();
            ret = GetToolAir (handle, minPot, maxPot, time);
            if (ret == MMLReturn.EM_OK) {
              // Store data
              for (int pot = minPot; pot <= maxPot; pot++) {
                m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_AIR] = time[pot - minPot];
              }
            }
          }
          else if (item == Pro3ToolCommonItem.TD_SLOW)
          {
            UInt32[] atcSpeed = Enumerable.Repeat ((UInt32)0, numberOfValues).ToArray ();
            ret = GetToolSlow (handle, minPot, maxPot, atcSpeed);
            if (ret == MMLReturn.EM_OK) {
              // Store data
              for (int pot = minPot; pot <= maxPot; pot++) {
                m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_SLOW] = atcSpeed[pot - minPot];
              }
            }
          }
          else if (item == Pro3ToolCommonItem.TD_BTS_LEN)
          {
            Int32[] length = Enumerable.Repeat (0, numberOfValues).ToArray ();
            ret = GetToolBtsLength (handle, minPot, maxPot, length);
            if (ret == MMLReturn.EM_OK) {
              // Store data
              for (int pot = minPot; pot <= maxPot; pot++) {
                m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_BTS_LEN] = length[pot - minPot];
              }
            }
          }
        } catch (Exception e) {
          log.ErrorFormat ("Couldn't read tool property {0}: {1}", item, e);
          ret = MMLReturn.EM_INTERNAL;
        }

        if (ret != MMLReturn.EM_OK) {
          return ret;
        }
      }

      // Copy data
      for (int i = 0; i < potNo.Length; i++) {
        UInt16 pot = potNo[i];
        if (item == Pro3ToolCommonItem.TD_BTS) // boolean
{
          value[i] = ((bool)m_preparedToolValuesPerPot[pot][item] ? 1 : 0);
        }
        else if (item == Pro3ToolCommonItem.TD_MMC_TYPE) // Stringbuilder
{
          return MMLReturn.EM_DATA; // Not returned as an int
        }
        else // Can be cast as an Int32
{
          value[i] = Convert.ToInt32 (m_preparedToolValuesPerPot[pot][item]);
        }
      }
      return MMLReturn.EM_OK;
    }

    MMLReturn WriteToolDataItem (UInt32 handle, Pro3ToolCommonItem item, UInt16[] potNo, Int32[] value)
    {
      // potNo and value must have the same length
      if (potNo.Length != value.Length) {
        throw new Exception ("potNo and value must have the same length");
      }

      // Check that we can write this data
      MMLReturn ret = MMLReturn.EM_OK;
      if (!m_allowedProperties.ContainsKey (item)) {
        ret = CheckAllowedProperties (handle);
        if (ret != MMLReturn.EM_OK) {
          return ret;
        }
      }
      if (!m_allowedProperties[item]) {
        return MMLReturn.EM_DATA;
      }

      try {
        if ((int)item <= (int)Pro3ToolCommonItem.TD_SL)
        {
          // Set all values one by one with the function "SetToolCommonItem"
          for (int i = 0; i < potNo.Length; i++) {
            ret = SetToolCommonItem (handle, potNo[i], item, value[i]);
            if (ret != MMLReturn.EM_OK) {
              log.ErrorFormat ("WriteToolDataItem: Couldn't set value {0} for property {1} of pot {2}: {3}", value[i], item, potNo[i], ret);
              return ret;
            }
          }
        }
        else if (item == Pro3ToolCommonItem.TD_MMC_TYPE || item == Pro3ToolCommonItem.TD_MMC_STT || item == Pro3ToolCommonItem.TD_MMC_SIZE)
        {
          // Three kinds of properties will be updated at the same time, so we read them all
          // First, we erase them otherwise they will not be read
          for (int i = 0; i < potNo.Length; i++) {
            UInt16 pot = potNo[i];
            if (m_preparedToolValuesPerPot.ContainsKey (pot)) {
              if (m_preparedToolValuesPerPot[pot].ContainsKey (Pro3ToolCommonItem.TD_MMC_TYPE)) {
                m_preparedToolValuesPerPot[pot].Remove (Pro3ToolCommonItem.TD_MMC_TYPE);
              }

              if (m_preparedToolValuesPerPot[pot].ContainsKey (Pro3ToolCommonItem.TD_MMC_STT)) {
                m_preparedToolValuesPerPot[pot].Remove (Pro3ToolCommonItem.TD_MMC_STT);
              }

              if (m_preparedToolValuesPerPot[pot].ContainsKey (Pro3ToolCommonItem.TD_MMC_SIZE)) {
                m_preparedToolValuesPerPot[pot].Remove (Pro3ToolCommonItem.TD_MMC_SIZE);
              }
            }
          }

          int[] tmp = new int[potNo.Length];
          ret = ReadToolDataItem (handle, Pro3ToolCommonItem.TD_MMC_STT, potNo, ref tmp);

          StringBuilder[] mmcType = new StringBuilder[potNo.Length];
          UInt16[] mmcStt = new UInt16[potNo.Length];
          UInt16[] mmcSize = new UInt16[potNo.Length];
          for (int i = 0; i < potNo.Length; i++) {
            UInt16 pot = potNo[i];
            mmcType[i] = ((StringBuilder)m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_MMC_TYPE]);
            mmcStt[i] = ((UInt16)m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_MMC_STT]);
            mmcSize[i] = ((UInt16)m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_MMC_SIZE]);
          }

          if (ret == MMLReturn.EM_OK) {
            // Update the right kind of data
            if (item == Pro3ToolCommonItem.TD_MMC_TYPE) {
              return MMLReturn.EM_DATA; // Needs to be set as a string
            }
            else if (item == Pro3ToolCommonItem.TD_MMC_STT) {
              for (int i = 0; i < potNo.Length; i++) {
                mmcStt[i] = (UInt16)value[i];
              }
            }
            else if (item == Pro3ToolCommonItem.TD_MMC_SIZE) {
              for (int i = 0; i < potNo.Length; i++) {
                mmcSize[i] = (UInt16)value[i];
              }
            }

            // Write data
            for (int i = 0; i < potNo.Length; i++) {
              try {
                var types = new StringBuilder[1];
                types[0] = new StringBuilder (mmcType[i].ToString (), 128);
                ret = SetToolMmc (handle, potNo[i], potNo[i],
                  types,
                  new UInt16[] { mmcStt[i] },
                  new UInt16[] { mmcSize[i] });
                if (ret != MMLReturn.EM_OK) {
                  log.ErrorFormat ("WriteToolDataItem: Couldn't set value {0} for property {1} of pot {2}: {3}", value[i], item, potNo[i], ret);
                  return ret;
                }
              }
              catch (Exception e) {
                log.ErrorFormat ("Couldn't use SetToolMmc for pot {0}: {1}", potNo[i], e);
                return MMLReturn.EM_INTERNAL;
              }
            }
          }
        }
        else if (item == Pro3ToolCommonItem.TD_BTS || item == Pro3ToolCommonItem.TD_BTS_FST || item == Pro3ToolCommonItem.TD_BTS_SEC ||
          item == Pro3ToolCommonItem.TD_BTS_TYPE)
        {
          // Four kinds of properties will be updated at the same time, so we read them all
          // First, we erase them otherwise they will not be read
          for (int i = 0; i < potNo.Length; i++) {
            UInt16 pot = potNo[i];
            if (m_preparedToolValuesPerPot.ContainsKey(pot)) {
              if (m_preparedToolValuesPerPot[pot].ContainsKey (Pro3ToolCommonItem.TD_BTS)) {
                m_preparedToolValuesPerPot[pot].Remove (Pro3ToolCommonItem.TD_BTS);
              }

              if (m_preparedToolValuesPerPot[pot].ContainsKey (Pro3ToolCommonItem.TD_BTS_FST)) {
                m_preparedToolValuesPerPot[pot].Remove (Pro3ToolCommonItem.TD_BTS_FST);
              }

              if (m_preparedToolValuesPerPot[pot].ContainsKey (Pro3ToolCommonItem.TD_BTS_SEC)) {
                m_preparedToolValuesPerPot[pot].Remove (Pro3ToolCommonItem.TD_BTS_SEC);
              }

              if (m_preparedToolValuesPerPot[pot].ContainsKey (Pro3ToolCommonItem.TD_BTS_TYPE)) {
                m_preparedToolValuesPerPot[pot].Remove (Pro3ToolCommonItem.TD_BTS_TYPE);
              }
            }
          }

          int[] tmp = new int[potNo.Length];
          ret = ReadToolDataItem (handle, Pro3ToolCommonItem.TD_BTS, potNo, ref tmp);

          bool[] current_bts = new bool[potNo.Length];
          UInt16[] current_bts_fst = new UInt16[potNo.Length];
          UInt16[] current_bts_sec = new UInt16[potNo.Length];
          UInt16[] current_bts_type = new UInt16[potNo.Length];
          for (int i = 0; i < potNo.Length; i++) {
            UInt16 pot = potNo[i];
            current_bts[i] = (bool)m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_BTS];
            current_bts_fst[i] = (UInt16)m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_BTS_FST];
            current_bts_sec[i] = (UInt16)m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_BTS_SEC];
            current_bts_type[i] = (UInt16)m_preparedToolValuesPerPot[pot][Pro3ToolCommonItem.TD_BTS_TYPE];
          }

          if (ret == MMLReturn.EM_OK)
          {
            // Update the right kind of data
            if (item == Pro3ToolCommonItem.TD_BTS) {
              for (int i = 0; i < potNo.Length; i++) {
                current_bts[i] = (value[i] != 0);
              }
            } else if (item == Pro3ToolCommonItem.TD_BTS_FST) {
              for (int i = 0; i < potNo.Length; i++) {
                current_bts_fst[i] = (UInt16)value[i];
              }
            } else if (item == Pro3ToolCommonItem.TD_BTS_SEC) {
              for (int i = 0; i < potNo.Length; i++) {
                current_bts_sec[i] = (UInt16)value[i];
              }
            } else if (item == Pro3ToolCommonItem.TD_BTS_TYPE) {
              for (int i = 0; i < potNo.Length; i++) {
                current_bts_type[i] = (UInt16)value[i];
              }
            }

            // Write data
            for (int i = 0; i < potNo.Length; i++) {
              ret = SetToolBts (handle, potNo[i], potNo[i],
                new bool[] { current_bts[i] },
                new UInt16[] { current_bts_fst[i] },
                new UInt16[] { current_bts_sec[i] },
                new UInt16[] { current_bts_type[i] });
              if (ret != MMLReturn.EM_OK) {
                log.ErrorFormat ("WriteToolDataItem: Couldn't set value {0} for property {1} of pot {2}: {3}", value[i], item, potNo[i], ret);
                return ret;
              }
            }
          }
        }
        else if (item == Pro3ToolCommonItem.TD_AIR)
        {
          UInt16[] time = Enumerable.Repeat ((UInt16)0, value.Length).ToArray ();
          for (int i = 0; i < value.Length; i++) {
            time[i] = (UInt16)value[i];
          }

          // Write data
          for (int i = 0; i < potNo.Length; i++) {
            ret = SetToolAir (handle, potNo[i], potNo[i], new UInt16[] { time[i] });
            if (ret != MMLReturn.EM_OK) {
              log.ErrorFormat ("WriteToolDataItem: Couldn't set value {0} for property {1} of pot {2}: {3}", value[i], item, potNo[i], ret);
              return ret;
            }
          }
        }
        else if (item == Pro3ToolCommonItem.TD_SLOW) {
          UInt32[] slowTool = Enumerable.Repeat ((UInt32)0, value.Length).ToArray ();
          for (int i = 0; i < value.Length; i++) {
            slowTool[i] = (UInt32)value[i];
          }

          // Write data
          for (int i = 0; i < potNo.Length; i++) {
            ret = SetToolSlow (handle, potNo[i], potNo[i], new UInt32[] { slowTool[i] });
            if (ret != MMLReturn.EM_OK) {
              log.ErrorFormat ("WriteToolDataItem: Couldn't set value {0} for property {1} of pot {2}: {3}", value[i], item, potNo[i], ret);
              return ret;
            }
          }
        }
        else if (item == Pro3ToolCommonItem.TD_BTS_LEN)
        {
          // Write data
          for (int i = 0; i < potNo.Length; i++) {
            ret = SetToolBtsLength (handle, potNo[i], potNo[i], new Int32[] { value[i] });
            if (ret != MMLReturn.EM_OK) {
              log.ErrorFormat ("WriteToolDataItem: Couldn't set value {0} for property {1} of pot {2}: {3}", value[i], item, potNo[i], ret);
              return ret;
            }
          }
        }
      }
      catch (Exception e) {
        log.ErrorFormat ("Couldn't write tool property {0}: {1}", item, e);
        ret = MMLReturn.EM_INTERNAL;
      }

      return ret;
    }
    #endregion

    #region Dll binding
    /// <summary>
    /// Error number of Md3Pro3.dll which occurred at the last is returned.
    /// </summary>
    /// <param name="mainErr">Internal error number of Md3Pro3.dll which occurred at the last is returned.</param>
    /// <param name="subErr">Error number of Fanuc library which occurred at the last is returned.</param>
    /// <returns>EM_OK if success, otherwise failed</returns>
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_GetLastError", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn GetLastError (out Int32 mainErr, out Int32 subErr);

    /// <summary>
    /// Retrieve the handle number to communicate with Professional 3. (Each thread)
    /// When this function is executed, retrieve the FOCAS2 library handle to use the FOCAS2
    /// function in md3pro3.dll. The retrieving library handle is stored and used within md3pro3.dll.
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="nodeInfo">"nodeNo/IP address/tcpPort/e" (e:1-EmulateMode,0-ReleaseMode)</param>
    /// <param name="timeout">Specify the timeout value of FOCAS2: count at the HSSB connection or time [sec] at the Ethernet connection</param>
    /// <param name="trylimit">Specify the retry count when the parameter is read</param>
    /// <param name="sleepTime">Specify waiting time [ms] until retrying</param>
    /// <returns>EM_OK(success) / EM_DATA(invalid data) / EM_HANDFULL(overconnect) /
    /// EM_NODE(invalid node) / EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) /
    /// EM_BUSY(device is busy) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_alloc_handle", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn AllocHandle (out UInt32 handle, [In] string nodeInfo,
                                        Int32 timeout, Int32 trylimit, Int32 sleepTime);

    /// <summary>
    /// Exit and disconnect the communication to specified machine (Professional 5) and release the handle
    /// </summary>
    /// <param name="handle"></param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error)</returns>
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_free_handle", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn FreeHandle (UInt32 handle);

    /// Read Professional-3 version string.
    /// <param name="handle">Specify the handle number</param>
    /// <param name="version"> buffer (PRO3_VER_STR_S).</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle)</returns>
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_pro_version", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn ProVersion (UInt32 handle, StringBuilder version);

    /// <summary>
    /// Retrieve the pallet number
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="pltNo">The pallet number is stored. 0 when there is no pallet, -1 is set when the number cannot be confirmed</param>
    /// <returns>EM_OK (success) / EM_HANDLE(invalid handle)</returns>
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_get_table_pltno", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn GetTablePltno (UInt32 handle, out UInt16 pltNo);

    /// <summary>
    /// Read current Pro3 machine alarm status
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="alarm">True if a machine alarm occured, otherwise False</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle)</returns>
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_chk_mc_alarm", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn ChkMcAlarm (UInt32 handle, out bool alarm);

    /// <summary>
    /// Read current Pro3 alarms. (Alarm for each tasks in MPC5)
    /// Necessary for each arguments(except numAlm) to declare arrays of PRO3_TASK_L(=64) indices of WORD/DWORD.
    /// Return data writed from the head of this array.
    /// </summary>
    /// <param name="handle">handle number</param>
    /// <param name="taskID">address of the array where PC side task number is stored</param>
    /// <param name="almNo">address of the array where the machine alarm number is stored</param>
    /// <param name="almType">address of the array where the machine alarm type is stored
    /// PMC_ALM_ALARM(=1) Alarm
    /// PMC_ALM_WARNING(=2) Warning</param>
    /// <param name="rtyOk">address of the array where the flag whether it is possible to retry is stored.
    /// PMC_ALM_RETRY_DISABLE(=0) Impossible to retry.
    /// PMC_ALM_RETRY_ENABLE (=1) Possible to retry</param>
    /// <param name="numAlm">The number of current machine alarms is stored</param>
    /// <returns></returns>
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_mc_alarm", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn McAlarm (UInt32 handle, [In, Out] UInt16[] taskID, [In, Out] UInt32[] almNo,
                                    [In, Out] UInt16[] almType, [In, Out] UInt16[] rtyOk, ref UInt16 numAlm);

    /// <summary>
    /// Retrieve the tool information. (The tool data unit and effective digits, etc)
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="unit">0 is mm, 1 is inch</param>
    /// <param name="ftn">Number of effective digits of the functional tool number</param>
    /// <param name="itn">Individual tool number (ITN)</param>
    /// <param name="ptn">Program tool number (PTN), T number</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle)</returns>
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_tool_info", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn ToolInfo (UInt32 handle, out Int16 unit, out Int16 ftn, out Int16 itn, out Int16 ptn);

    /// <summary>
    /// Retrieve the tool life information
    /// </summary>
    /// <param name="handle">Specify the handle number</param>
    /// <param name="lifeType">Type of tool life
    /// 0: time in seconds, 1: distance mm or inch, 2: machining quantity, 3: time in 0.1 s</param>
    /// <param name="countType">1: count down, 0: count up</param>
    /// <param name="alarmReset">The setting whether remain Full at TL Alarm Reset is stored.
    /// True: set remaining life to full, False: do not change remaining life</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle)</returns>
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_toollife_info", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn ToollifeInfo (UInt32 handle, out Pro3ToolLifeType lifeType, out Int16 countType, out bool alarmReset);

    //  [Function]
    //    Read number of magazine pots.
    //  [Argument]
    //    DWORD handle  : HANDLE number
    //    Int16 *atcSize  : ATC magazine size (number of pots).
    //  [Result]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_max_pot", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn MaxPot (UInt32 handle, out UInt16 maxPot);

    //  [Function]
    //    Read atc magazine type.
    //  [Arguments]
    //    DWORD handle  : HANDLE number
    //    LPWORD atcMag : type of ATC magazine
    //      ATCTYPE_NO_ATC(0)/ATCTYPE_RNDM_CHAIN(1)/ATCTYPE_FIX_CHAIN(2)/
    //      ATCTYPE_CARTRIDGE(3)/ATCTYPE_MATRIX(4)/ATCTYPE_TURN_EGGPOT(5)/
    //      ATCTYPE_UNTURN_EGGPOT(6)/ATCTYPE_DBL_OUTER_DBL_MAG(7)/ATCTYPE_DOUBLE_MAG(8)
    //    LPWORD dtcMag : type of DTC magazine
    //      DTCTYPE_NO_DTC(0)/DTCTYPE_TOOL_DATA(1)/DTCTYPE_NO_TOOL_DATA(2)
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //  [Remarks]
    //    - When each data is not necessary, set NULL in each argument.
    //    - Refer to ProDef.H about description of return value.
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_atc_type", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn AtcType (UInt32 handle, out UInt16 atcMag, out UInt16 dtcMag);

    //  [Function]
    //    Read all data for tool of specified pot.
    //  [Argument]
    //    DWORD  handle   : HANDLE number
    //    WORD    fromPot : pot number.
    //    WORD    toPot   : pot number.
    //    LPDWORD ptn     : Program Tool Number.
    //    LPDWORD ftn     : Functional Tool Number.
    //    LPDWORD itn     : Individual Tool Number.
    //    LPDWORD tl      : Set value of TL monitor (least input increment:sec.).
    //    LPDWORD remain  : Remain or used tool life for TL monitor (least input increment:sec.).
    //    LPLONG  len     : Tool length (least input increment:0.001mm/0.0001inch).
    //    LPLONG  dia     : Tool diameter (least input increment:0.001mm/0.0001inch).
    //    LPWORD  type    : Tool type
    //    LPWORD  alm     : Alarm status
    //    LPWORD  ac      : Set value of AC monitor (least input increment:0.1A).
    //    LPWORD  sl      : Set value of SL monitor (least input increment:0.1A).
    //  [Result]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data)
    //  [Remarks]
    //    When each data is not necessary, set NULL in each argument.
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_get_tool_common", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn GetToolCommon (UInt32 handle, UInt16 fromPot, UInt16 toPot,
                                          [In, Out] UInt32[] ptn, [In, Out] UInt32[] ftn, [In, Out] UInt32[] itn,
                                          [In, Out] UInt32[] tl, [In, Out] UInt32[] remain,
                                          [In, Out] Int32[] len, [In, Out] Int32[] dia,
                                          [In, Out] UInt16[] type, [In, Out] UInt16[] alm,
                                          [In, Out] UInt16[] ac, [In, Out] UInt16[] sl);

    //  [Function]
    //    This function is used to read the individual data for tool specified pot.
    //  [Argument]
    //    DWORD handle : HANDLE number
    //    WORD  pot    : pot number
    //    Int16 item   : item name (TD_FTN,TD_ITN....)
    //    long  *value : Aquired data
    //  [Result]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //[DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_get_tool_common_item", SetLastError = true, CharSet = CharSet.Ansi)]
    //static extern MMLReturn GetToolCommonItem (UInt32 handle, UInt16 pot, Pro3ToolCommonItem item, out Int32 value);

    //  [Function]
    //    This function is used to write the individual data for tool specified pot.
    //  [Argument]
    //    DWORD handle : HANDLE number
    //    WORD  pot    : pot number.
    //    Int16 item   : item name (TD_FTN,TD_ITN....).
    //    long  value  : setting value.
    //  [Result]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data)
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_set_tool_common_item", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn SetToolCommonItem (UInt32 handle, UInt16 pot, Pro3ToolCommonItem item, Int32 value);

    //  [Function]
    //    Function to acquire presence of the optional tool data.
    //  [Arguments]
    //    DWORD  handle    : HANDLE number
    //    LPBOOL  mmcData     : for ModuleMMC
    //    LPBOOL  btsData     : for ATC side BTS
    //    LPBOOL  airData     : for Spindle through air purge
    //    LPDWORD slowData    : for Slow ATC
    //    LPBOOL  btsLenData  : for tool length of ATC side BTS
    //  [Result]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //  [Remarks]
    //    When each data is not necessary, set NULL in each argument.
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_optional_tldt_define", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn OptionalTldtDefine (UInt32 handle, out bool mmcData, out bool btsData,
                                                out bool airData, out bool slowData, out bool btsLenData);

    //  [Function]
    //    Function to acquire the data for slow ATC.
    //  [Arguments]
    //    DWORD handle : HANDLE number
    //    WORD    fromPot      : pot number.
    //    WORD    toPot        : pot number.
    //    LPDWORD slowTool     : Slow ATC
    //  [Result]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_OPTION(no option) / EM_DATA(invalid data)
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_get_tool_slow", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn GetToolSlow (UInt32 handle, UInt16 fromPot, UInt16 toPot, [In, Out] UInt32[] slowTool);

    //  [Function]
    //    Function to set the data for slow ATC.
    //  [Arguments]
    //    DWORD  handle    : HANDLE number
    //    WORD    fromPot    : pot number.
    //    WORD    toPot    : pot number.
    //    LPDWORD slowTool  : Slow ATC time
    //  [Result]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_OPTION(no option) / EM_DATA(invalid data)
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_set_tool_slow", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn SetToolSlow (UInt32 handle, UInt16 fromPot, UInt16 toPot, [In, Out] UInt32[] slowTool);

    //  [Function]
    //    Function to acquire the tool data for module MMC.
    //  [Arguments]
    //    DWORD  handle    : HANDLE number
    //    WORD    fromPot     : pot number.
    //    WORD    toPot       : pot number.
    //    LPSTR   *toolType   : Tool type.
    //    LPWORD  status      : Tool status.
    //    LPWORD  size        : Tool size (Class).
    //        TOOLCLS_STD(1)/TOOLCLS_MID(2)/TOOLCLS_LARGE(3)/TOOLCLS_EXTRA(4)/
    //        TOOLCLS_FIXED(5)/TOOLCLS_EMP(8)
    //  [Result]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_OPTION(no option) / EM_DATA(invalid data)
    //  [Remarks]
    //    When each data is not necessary, set NULL in each argument.
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_get_tool_mmc", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn GetToolMmc (UInt32 handle, UInt16 fromPot, UInt16 toPot, StringBuilder[] toolType,
                                        [In, Out] UInt16[] status, [In, Out] UInt16[] size);

    //  [Function]
    //    Function to set the tool data for module MMC.
    //  [Arguments]
    //    DWORD  handle    : HANDLE number
    //    WORD    fromPot     : pot number.
    //    WORD    toPot       : pot number.
    //    LPSTR   *toolType   : Tool type.
    //    LPWORD  status      : Tool status.
    //    LPWORD  size        : Tool size (Class).
    //        TOOLCLS_STD(1)/TOOLCLS_MID(2)/TOOLCLS_LARGE(3)/TOOLCLS_EXTRA(4)/
    //        TOOLCLS_FIXED(5)/TOOLCLS_EMP(8)/TOOLCLS_BORE(10)
    //  [Result]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_OPTION(no option) / EM_DATA(invalid data)
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_set_tool_mmc", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn SetToolMmc (UInt32 handle, UInt16 fromPot, UInt16 toPot,
                                        StringBuilder[] toolType,
                                        [In, Out] UInt16[] status, [In, Out] UInt16[] size);

    //  [Function]
    //    Function to acquire the data for ATC side BTS
    //  [Arguments]
    //    DWORD  handle    : HANDLE number
    //    WORD   fromPot      : pot number.
    //    WORD   toPot        : pot number.
    //    LPBOOL btsOn        : ATC side BTS enable/disable
    //    LPWORD firstTime    : Value before machining (msec).
    //    LPWORD secondTime  : Value after machining (msec).
    //    LPWORD btsActType   : Action type of BTS stylus (0:Standard/1:Vibration Control/2:Low Speed).
    //  [Result]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_OPTION(no option) / EM_DATA(invalid data)
    //  [Remarks]
    //    When each data is not necessary, set NULL in each argument.
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_get_tool_bts", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn GetToolBts (UInt32 handle, UInt16 fromPot, UInt16 toPot, [In, Out] bool[] btsOn,
                                        [In, Out] UInt16[] firstTime, [In, Out] UInt16[] secondTime, [In, Out] UInt16[] btsActType);

    //  [Function]
    //    Function to set the data for ATC side BTS
    //  [Arguments]
    //    DWORD handle : HANDLE number
    //    WORD   fromPot       : pot number.
    //    WORD   toPot         : pot number.
    //    LPBOOL btsOn         : ATC side BTS enable/disable
    //    LPWORD firstTime     : Value before machining (msec).
    //    LPWORD secondTime    : Value after machining (msec).
    //    LPWORD btsActType    : Action type of BTS stylus (0:Standard/1:Vibration Control/2:Low Speed).
    //  [Result]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_OPTION(no option) / EM_DATA(invalid data)
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_set_tool_bts", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn SetToolBts (UInt32 handle, UInt16 fromPot, UInt16 toPot, [In, Out] bool[] btsOn,
                                        [In, Out] UInt16[] firstTime, [In, Out] UInt16[] secondTime, [In, Out] UInt16[] btsActType);

    //  [Function]
    //    Function to acquire the data for seting time of spindle through air purge.
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    WORD   fromPot  : pot number.
    //    WORD   toPot    : pot number.
    //    LPWORD time     : Acquired time of spindle through air purge
    //  [Result]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_OPTION(no option) / EM_DATA(invalid data)
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_get_tool_air", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn GetToolAir (UInt32 handle, UInt16 fromPot, UInt16 toPot, [In, Out] UInt16[] time);

    //  [Function]
    //    Function to set the data for seting time of spindle through air purge.
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    WORD   fromPot  : pot number.
    //    WORD   toPot    : pot number.
    //    LPWORD time     : Set time of spindle through air purge
    //  [Result]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_OPTION(no option) / EM_DATA(invalid data)
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_set_tool_air", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn SetToolAir (UInt32 handle, UInt16 fromPot, UInt16 toPot, [In, Out] UInt16[] time);

    //  [Function]
    //    Function to acquire the measuring length of ATC side BTS
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    WORD    fromPot : pot number.
    //    WORD    toPot   : pot number.
    //    LPLONG  length  : measuring value
    //  [Result]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_OPTION(no option) / EM_DATA(invalid data)
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_get_tool_bts_length", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn GetToolBtsLength (UInt32 handle, UInt16 fromPot, UInt16 toPot, [In, Out] Int32[] length);

    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    WORD    fromPot : pot number.
    //    WORD    toPot   : pot number.
    //    LPLONG  length  : measuring value
    //  [Result]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_OPTION(no option) / EM_DATA(invalid data)
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_set_tool_bts_length", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn SetToolBtsLength (UInt32 handle, UInt16 fromPot, UInt16 toPot, [In, Out] Int32[] length);

    //  [Function]
    //    Read spindle tool informations.
    //  [Arguments]
    //    DWORD handle: HANDLE number
    //    LPWORD  pot  : pot number
    //    LPDWORD ftn : FTN
    //    LPDWORD itn : ITN
    //    LPDWORD ptn : T number
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //  [Remarks]
    //    When NULL is set in the argument, the value is not acquired.
    [DllImport ("Md3Pro3.dll", EntryPoint = "md3pro3_spindle_tool", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern MMLReturn SpindleTool (UInt32 handle, out UInt16 pot, out UInt32 ftn, out UInt32 itn, out UInt32 ptn);
    #endregion // Dll binding


    #region Other methods
    //  [Function]
    //    Request to process machine I/F2 data from PC to Pro3.
    //  [Arguments]
    //      DWORD  handle  : HANDLE number
    //    LPBOOL success  : TRUE(success)/FALSE(wait for the response of the previous request.)
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_machineIF2_request(DWORD handle, LPBOOL success);

    //  [Function]
    //    Cancel the request for process machine I/F2 data from PC to Pro3.
    //      DWORD  handle  : HANDLE number
    //    LPBOOL success  : TRUE(success)/FALSE(wait for the response of the previous request.)
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_machineIF2_request_cancel(DWORD handle, LPBOOL success);

    //  [Function]
    //    Presence of response of Pro3 to machine I/F2 data processing request.
    //  [Arguments]
    //    DWORD  handle : HANDLE number
    //    LPBOOL finish :TRUE(Pro3 finish the processing of the data.)
    //            /FALSE(Pro3 don't finish the processing of the data.)
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_machineIF2_response(DWORD handle, LPBOOL finish);

    //  [Function]
    //    Read the data from machine I/F2.
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    LPWORD  funcNo  :
    //    LPWORD  retCode :
    //    LPWORD  wData1  :
    //    LPWORD  wData2  :
    //    LPDWORD dwData1 :
    //    LPDWORD dwData2 :
    //    LPDWORD dwData3 :
    //    LPDWORD dwData4 :
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //  [Remarks]
    //    - When each data is not necessary, set NULL in each argument.
    //Int16 md3pro3_get_machineIF2(DWORD handle, LPWORD funcNo, LPWORD retCode, LPWORD wData1, LPWORD wData2,
    //                      LPDWORD dwData1, LPDWORD dwData2, LPDWORD dwData3, LPDWORD dwData4);

    //  [Function]
    //    Write data to machine I/F2.
    //  [Arguments]
    //    DWORD handle  : HANDLE number
    //    WORD  funcNo  :
    //    WORD  retCode  :
    //    WORD  wData1    :
    //    WORD  wData2    :
    //    DWORD dwData1   :
    //    DWORD dwData2   :
    //    DWORD dwData3   :
    //    DWORD dwData4   :
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_set_machineIF2(DWORD handle, WORD funcNo, WORD retCode, WORD wData1, WORD wData2,
    //                      DWORD dwData1, DWORD dwData2, DWORD dwData3, DWORD dwData4);

    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_get_machineIF4(DWORD handle, LPDWORD data);

    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_get_machineIF4_bit(DWORD handle, DWORD mask, LPBOOL value);

    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_set_machineIF4_bit(DWORD handle, DWORD mask, BOOL value);

    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_get_machineIF5(DWORD handle, LPDWORD data);

    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_get_machineIF5_bit(DWORD handle, DWORD mask, LPBOOL value);

    //  [Function]
    //    Read value from specified machine parameter number.
    //  [Arguments]
    //    DWORD handle  : HANDLE number
    //    WORD  paraNo  : machine parameter number
    //    long  *value  : Acquired value
    //    long  defValue  : default value
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_get_mcpara(DWORD handle, WORD paraNo, long *value, long defValue);

    //  [Function]
    //    Set value to specified machine parameter number.
    //  [Arguments]
    //    DWORD handle  : HANDLE number
    //    WORD  paraNo    : machine parameter number
    //    long  value     : Setting value
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_set_mcpara(DWORD handle, WORD paraNo, long value);

    //  [Function]
    //    system mode bit ON/OFF read
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_chk_system_mode(DWORD handle, LPBOOL sysMode);

    //  [Function]
    //    maintenance mode bit ON/OFF read
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_chk_maintenance_mode(DWORD handle, LPBOOL maintMode);

    //  [Function]
    //    auto unloading mode bit ON/OFF read
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_chk_auto_unloading_mode(DWORD handle, LPBOOL aumMode);

    //  [Function]
    //    no more pallet bit ON/OFF read
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_chk_no_more_pallet_mode(DWORD handle, LPBOOL nmpMode);

    //  [Function]
    //    operator call bit ON/OFF read
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_chk_operator_call(DWORD handle, LPBOOL opeCall);

    //  [Function]
    //    shutdown request read
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_chk_shutdown_request(DWORD handle, LPBOOL request);

    //  [Function]
    //    SED bit ON/OFF operation
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_set_start_disable(DWORD handle, BOOL flg);

    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_refresh_custom_screen(DWORD handle);

    //  [Function]
    //    This function check whether turn on tool data edit flag of professonal-3 or PC.
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    LPBOOL editPC  :
    //    LPBOOL editPro3 :
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //  [Remarks]
    //    - When each data is not necessary, set NULL in each argument.
    //Int16 md3pro3_chk_tldt_edit(DWORD handle, LPBOOL editPC, LPBOOL editPro3);

    //  [Function]
    //    Turn on the bit9 of machine I/F, and enables to operate tool data from PC.
    //    Tool data edit mode of PC is turned on.
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    LPBOOL success  : TRUE (success) / FALSE(disable to turn on edit mode because edit mode of MPC5 is on.)
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_tldt_edit_start(DWORD handle, LPBOOL success);

    //  [Function]
    //    Turns off the bit9 of machine I/F, turns off tool data edit mode of PC.
    //  [Arguments]
    //    DWORD handle : HANDLE number
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_tldt_edit_finish(DWORD handle);

    //  [Function]
    //    on-duty bit ON/OFF read
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_get_on_duty(DWORD handle, LPBOOL duty);

    //  [Function]
    //    on-duty bit ON/OFF operation
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_set_on_duty(DWORD handle, BOOL duty);

    //  [Function]
    //    This function check whether can use auto-poweroff function.
    //  [Arguments]
    //    DWORD  handle : HANDLE number
    //    LPBOOL enable : TURE / FALSE
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_can_auto_poweroff(DWORD handle, LPBOOL enable);

    //  [Function]
    //    auto power off bit ON/OFF operation
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_get_auto_poweroff(DWORD handle, LPBOOL apo);

    //  [Function]
    //    auto power off bit ON/OFF operation
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_set_auto_poweroff(DWORD handle, BOOL apo);

    //  [Function]
    //    Execute the cycle start of the system mode for Professional-3
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    BOOL   opeCall  : TRUE(occur operator-call before machining)
    //    LPBOOL success  : TRUE(success)/FALSE(now machining)
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_system_cycle_start(DWORD handle, BOOL opeCall, LPBOOL success);

    //  [Function]
    //    Examine whether machining of the system mode finished.
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    LPBOOL finish  : TRUE(machining finish) /FALSE(now machining)
    //    long  *confition: program finish condition
    //        when *condition=0 and *finish=TRUE, normal finish.
    //        when *condition!=0 and *finish=TRUE, abnormal finish.
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //  [Remarks]
    //    - When each data is not necessary, set NULL in each argument.
    //Int16 md3pro3_chk_system_mach_finish(DWORD handle, LPBOOL finish, long *condition);

    //  [Function]
    //    Get machining status.
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_chk_system_mach_status(DWORD handle, LPBOOL stx, LPBOOL eopx, long *condition);

    //  [Function]
    //    Execute the cycle abort of the system mode for Professional-3.
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    LPBOOL success  : TRUE(success)/FALSE(now not machining)
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_system_cycle_abort(DWORD handle, LPBOOL success);

    //  [Function]
    //    Get spindle milling time.
    //  [Arguments]
    //    DWORD handle: HANDLE number
    //    long *value  : milling time
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_FUNC(function error)
    //Int16 md3pro3_get_milling_time(DWORD handle, long *value);

    //  [Arguments]
    //    DWORD handle : HANDLE number
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_macro_read_finish(DWORD handle);

    //  [Function]
    //    Read maximun alarm history table.
    //  [Arguments]
    //    DWORD  handle    : HANDLE number
    //    LPWORD maxHistory   : PRO3_HIS_L(20) / PRO3_HIS2_L(100)
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_max_alarm_history(DWORD handle, LPWORD numHis);

    //  [Function]
    //    Read Pro3 alarm history.
    //  [Arguments]
    //    Necessary for each arguments to declare arrays of PRO3_HIS_L(=20) or PRO3_HIS2_L(=100) indices of WORD/DWORD.
    //    Returns data writed from the head of this array.
    //    DWORD handle  : HANDLE number
    //    LPWORD taskID  : PC side task number
    //    LPDWORD almNum  : PC alarm number
    //    LPWORD almWarn  : Alarm(1)/Warning(2)
    //    LPWORD rtyOk    : retry OK(1) /else(0)
    //    LPWORD year     :-+
    //    LPWORD month    : | Alarm date
    //    LPWORD day      :-+
    //    LPWORD hour     :-+
    //    LPWORD min      : | Alarm time
    //    LPWORD sec      :-+
    //    LPWORD numHis   : Acquired number of alarm history
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //  [Remarks]
    //    When NULL is set in the argument, the value is not acquired.
    //Int16 md3pro3_mc_alarm_history(DWORD handle, LPWORD taskID, LPDWORD almNo, LPWORD almType,
    //                        LPWORD rtyOk, LPWORD year, LPWORD month, LPWORD day,
    //                        LPWORD hour, LPWORD min, LPWORD sec, LPWORD numHis);

    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_mc_alarm_reset(DWORD handle, long execTime);

    //  [Function]
    //    Read spindle status.
    //  [Arguments]
    //    DWORD handle  :  HANDLE number
    //    LPWORD position :  gear position.
    //              SPDL_NO_GEAR(0)/SPDL_LOW_GEAR(1)/SPDL_HI_GEAR(2)/SPDL_MID_GEAR(3)
    //    LPWORD direction:  Countering:SPDL_CONTOUR(4)/Orientation:SPDL_ORIENT(3)
    //              CCW:SPDL_REVERSE(2)/CW:SPDL_FORWARD(1)/STOP:SPDL_STOP(0)
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //  [Remarks]
    //    - When each data is not necessary, set NULL in each argument.
    //Int16 md3pro3_spindle_status(DWORD handle, LPWORD position, LPWORD direction);

    //  [Function]
    //    Return spindle speed (min-1).
    //  [Arguments]
    //    DWORD  handle    : HANDLE number
    //    LPWORD spdlSpeed  : Spindle speed (min-1)
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //  [Remarks]
    //    The acquired value is an instructed value.
    //Int16 md3pro3_spindle_speed(DWORD handle, LPWORD spdlSpeed);

    //  [Function]
    //    Read tool monitor setting value.
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    LPWORD slUppLmt : SL upper limit value.
    //    LPWORD acUppLmt : AC upper limit value.
    //    LPWORD acLowLmt : AC lower limit value.
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //  [Remarks]
    //    - When each data is not necessary, set NULL in each argument.
    //Int16 md3pro3_toolmoni_setting(DWORD handle, LPWORD slUppLmt, LPWORD acUppLmt, LPWORD acLowLmt);

    //  [Function]
    //    Read tool monitor current value.
    //  [Arguments]
    //    DWORD  handle    : HANDLE number
    //    LPWORD loadVal    : Spindle load value.
    //      LPWORD noLoadVal  : Current of spindle at noload.
    //      LPWORD actualVal    : Actual current of spindle.
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //  [Remarks]
    //    - When each data is not necessary, set NULL in each argument.
    //Int16 md3pro3_spindle_load(DWORD handle, LPWORD loadVal, LPWORD noLoadVal, LPWORD actualVal);

    //  [Function]
    //    Get MC spindle override switch is enable or disable.
    //  [Arguments]
    //    DWORD  handle : HANDLE number
    //    LPBOOL status : override switch status. TRUE:Enable/FALSE:Disable
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_spindle_override_sw_status(DWORD handle, LPBOOL swStatus);

    //  [Function]
    //    Make MC spindle overraide swith enable.
    //  [Arguments]
    //    DWORD handle : HANDLE number
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_spindle_override_sw_enable(DWORD handle);

    //  [Function]
    //    Get current spindle override.
    //  [Arguments]
    //    DWORD  handle : HANDLE number
    //    LPWORD rate   : current spindle override
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_get_spindle_override(DWORD handle, LPWORD rate);

    //  [Function]
    //    Set spindle override from PC
    //  [Arguments]
    //    DWORD handle : HANDLE number
    //    WORD  rate   : setting spindle override
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data)
    //Int16 md3pro3_set_spindle_override(DWORD handle, WORD rate);

    //  [Function]
    //    Get feed override switch is enable or disable.
    //  [Arguments]
    //    DWORD  handle : HANDLE number
    //    LPBOOL status : override switch status. TRUE : Enable, FALSE : Disable
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_feed_override_sw_status(DWORD handle, LPBOOL swStatus);

    //  [Function]
    //    Make MC feed overraide swith enable.
    //  [Arguments]
    //    DWORD handle : HANDLE number
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_feed_override_sw_enable(DWORD handle);

    //  [Function]
    //    Set feed override from PC
    //  [Arguments]
    //    DWORD handle : HANDLE number
    //    WORD rate            : override
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data)
    //Int16 md3pro3_set_feed_override(DWORD handle, WORD rate);

    //  [Function]
    //    Read ATC magazine status.
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    LPWORD armStdby : STANDBY/NOT
    //    LPWORD magStatus: MANUAL/AUTO/INDEXING
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //  [Remarks]
    //    - When each data is not necessary, set NULL in each argument.
    //    - Refer to ProDef.H about description of return value.
    //Int16 md3pro3_atc_status(DWORD handle, LPWORD armStdby, LPWORD magStatus);

    //  [Function]
    //    Read next tool information.
    //  [Arguments]
    //    DWORD handle: HANDLE number
    //    LPWORD  pot  : pot number
    //    LPDWORD ftn : FTN
    //    LPDWORD itn : ITN
    //    LPDWORD ptn : T number
    //  [Return]
    //    TRUE / FALSE
    //  [Remarks]
    //    When NULL is set in the argument, the value is not acquired.
    //Int16 md3pro3_next_tool(DWORD handle, LPWORD pot, LPDWORD ftn, LPDWORD itn, LPDWORD ptn);

    //  [Function]
    //    Read tool pot number on TLS.
    //  [Arguments]
    //    DWORD handle: HANDLE number
    //    LPWORD  pot  : pot number
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_tls_potno(DWORD handle, LPWORD pot);

    //  [Function]
    //    tool loading station bit ON/OFF read
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_get_tls_disable(DWORD handle, LPBOOL disable);

    //  [Function]
    //    tool loading station bit ON/OFF operation
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_set_tls_disable(DWORD handle, BOOL disable);

    //  [Function]
    //    Read apc type.
    //  [Arguments]
    //    DWORD handle : HANDLE number
    //    LPWORD apcArm: type of APC arm
    //      APCTYPE_NO_APC(0)/APCTYPE_TURN(1)/APCTYPE_SHUTTLE(2)/
    //      APCTYPE_DIRECT(3)/APCTYPE_TURN_BUF_STOCKER(4)
    //    LPWORD pltMag: type of pallet magazine
    //      PMTYPE_NO_PM(0)/PMTYPE_TYPE1(1)/PMTYPE_TYPE2(2)/PMTYPE_TYPE3(3)
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //  [Remarks]
    //    - When each data is not necessary, set NULL in each argument.
    //    - Refer to ProDef.H about description of return value.
    //Int16 md3pro3_apc_type(DWORD handle, LPWORD apcArm, LPWORD pltMag);

    //  [Function]
    //    Read Pallet Changer status.
    //  [Arguments]
    //    DWORD handle    : HANDLE number
    //    LPWORD armStdby     : Arm status (STANDBY/NOT)
    //    LPWORD stckStatus   : Stocker status (MANUAL/AUTO)
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //  [Remarks]
    //    - When each data is not necessary, set NULL in each argument.
    //    - Refer to ProDef.H about description of return value.
    //Int16 md3pro3_apc_status(DWORD handle, LPWORD armStdby, LPWORD stckStatus);

    //  [Function]
    //    Read current pallet number on table.
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    LPWORD  pltNo  : Pallet number
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_get_table_pltno(DWORD handle, LPWORD pltNo);

    //  [Function]
    //    Set current pallet number on table.
    //  [Arguments]
    //    DWORD  handle : HANDLE number
    //    WORD  pltNo  : Specified pallet number
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_set_table_pltno(DWORD handle, WORD pltNo);

    //  [Function]
    //    Read pallet number on MC buffer.
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    LPWORD  pltNo  : Pallet number
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_get_front_pltno(DWORD handle, LPWORD pltNo);

    //  [Function]
    //    Set current pallet number on front storage.
    //  [Arguments]
    //    DWORD handle : HANDLE number
    //    WORD  pltNo  : Specified pallet number
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_set_front_pltno(DWORD handle, WORD pltNo);

    //  [Function]
    //    Read pallet number on back storage.
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    LPWORD  pltNo   : Pallet number
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_get_back_pltno(DWORD handle, LPWORD pltNo);

    //  [Function]
    //    Set current pallet number on back storage.
    //  [Arguments]
    //    DWORD  handle : HANDLE number
    //    WORD  pltNo  : Specified pallet number
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_set_back_pltno(DWORD handle, WORD pltNo);

    //  [Function]
    //    Get pallet magazine type.
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_pm_type(DWORD handle, LPWORD pmType);

    //  [Function]
    //    Get total number of pallet nubmer.
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_pm_plt(DWORD handle, LPWORD pmPlt);

    //  [Function]
    //    Get total number of pallet storage.
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_pm_pst(DWORD handle, LPWORD pmPst);

    //  [Function]
    //    Get total number of WSS.
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_pm_wss(DWORD handle, LPWORD pmWss);

    //  [Function]
    //    Get vehicle command data.
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_pm_get_vhc_command_data(DWORD handle, pmVhcCmnd_r *cmndData);

    //  [Function]
    //    Set vehicle command data.
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_pm_set_vhc_command_data(DWORD handle, long fromTo, long cmdType,
    //                               long palletNo, unit_t srcUnit, unit_t dstUnit);

    //  [Function]
    //    Get vehicle command status.
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_pm_get_vhc_command_state(DWORD handle, pmVhcCmndStat_r *cmndStat);

    //  [Function]
    //    Get Start/Cancel/Finish signal status.
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_pm_get_vhc_signal(DWORD handle, WORD sigType, long *status);

    //  [Function]
    //    Turn on/off Start/Cancel signal.
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //  [Remarks]
    //    - If this function is called for Finish signal, FALSE is returned.
    //Int16 md3pro3_pm_set_vhc_signal(DWORD handle, WORD sigType, long status);

    //  [Function]
    //    Read current pallet number on specified WSS with PM.
    //  [Arguments]
    //    DWORD handle : HANDLE number
    //    WORD   wss           : WSS number
    //    LPWORD pallet        : Pallet number
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_get_pm_wss_pltno(DWORD handle, WORD wssNo, LPWORD pltNo);

    //  [Function]
    //    Set pallet number on specified wss with PM.
    //  [Arguments]
    //    DWORD handle : HANDLE number
    //    WORD wss             : WSS number
    //    WORD pallet          : Pallet number
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_set_pm_wss_pltno(DWORD handle, WORD wssNo, WORD pltNo);

    //  [Function]
    //    Get pallet return botton status on specified wss with PM.
    //  [Arguments]
    //    DWORD handle : HANDLE number
    //    WORD wss             : WSS number
    //    long *status         : Pallet return button status
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_get_pm_wss_start(DWORD handle, WORD wssNo, long *status);

    //  [Function]
    //    Set WSS start flag with PM.
    //  [Arguments]
    //    DWORD handle : HANDLE number
    //    WORD wss     : WSS number
    //    long status  : Start flag (1Pallet number
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_set_pm_wss_start(DWORD handle, WORD wssNo, long status);

    //  [Function]
    //    Get pallet return botton status on specified wss with PM.
    //  [Arguments]
    //    DWORD handle  : HANDLE number
    //    WORD wss      : WSS number
    //    LPBOOL status : Pallet return button status
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_get_pm_wss_finish(DWORD handle, WORD wssNo, LPBOOL status);

    //  [Function]
    //    Get manual interrupt mode status on specified wss with PM.
    //  [Arguments]
    //    DWORD handle  : HANDLE number
    //    LPWORD mode     : Manual interrupt mode status raw value
    //    LPWORD wss1Mode  : Manual interrupt mode status of WSS1
    //    LPWORD wss2Mode : Manual interrupt mode status of WSS2
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_get_pm_wss_manual_mode_status(DWORD handle, LPWORD mode,
    //                               LPWORD wss1Mode, LPWORD wss2Mode);

    //  [Function]
    //    Convert tool alarm data (bit-field) to each element.
    //  [Argument]
    //    WORD   almData : Bit field data
    //    LPBOOL bts2 : TRUE(BTS2 alarm) / FALSE(no alarm)
    //    LPBOOL bts  : TRUE(BTS alarm)  / FALSE(no alarm)
    //    LPBOOL ac   : TRUE(AC alarm)   / FALSE(no alarm)
    //    LPBOOL sl   : TRUE(SL alarm)   / FALSE(no alarm)
    //    LPBOOL tl   : TRUE(TL alarm)   / FALSE(no alarm)
    //  [Result]
    //    EM_OK(success)
    //Int16 md3pro3_split_tool_alarm(WORD almData, LPBOOL bts2, LPBOOL bts, LPBOOL ac, LPBOOL sl, LPBOOL tl);

    //  [Function]
    //    Convert each element of tool alarm to WORD data.
    //  [Argument]
    //    BOOL bts2   : TRUE(BTS2 alarm) / FALSE(no alarm)
    //    BOOL bts    : TRUE(BTS alarm)  / FALSE(no alarm)
    //    BOOL ac     : TRUE(AC alarm)   / FALSE(no alarm)
    //    BOOL sl     : TRUE(SL alarm)   / FALSE(no alarm)
    //    BOOL tl     : TRUE(TL alarm)   / FALSE(no alarm)
    //    LPWORD almData : Tool alarm status data (bit field)
    //  [Result]
    //    EM_OK(success)
    //Int16 md3pro3_make_tool_alarm(BOOL bts2, BOOL bts, BOOL ac, BOOL sl, BOOL tl, LPWORD almData);

    //  [Function]
    //    Convert mmc tool type data (bit-field) to each element.
    //  [Argument]
    //    WORD   bitField   : Bit field data
    //    LPBOOL firstUse   : TRUE(first use tool)
    //    LPBOOL opeCall    : TRUE(operator call tool)
    //    LPBOOL alarm      : TRUE(alarm tool)
    //    LPBOOL prohibit   : TRUE(prohibit use tool)
    //    LPBOOL almDisable : TRUE(tool alarm disable)
    //    LPBOOL weight     : Weight Tool.
    //    LPBOOL thruCoolant: TRUE(Spindle through coolant tool)
    //  [Result]
    //    EM_OK(success)
    //Int16 md3pro3_split_tool_mmctype(WORD bitField, LPBOOL firstUse, LPBOOL opeCall, LPBOOL alarm,
    //                                           LPBOOL prohibit, LPBOOL almDisable, LPBOOL weight, LPBOOL thruCoolant);

    //  [Function]
    //    Convert each element of mmc tool type data to WORD data.
    //  [Argument]
    //    BOOL firstUse   : TRUE(first use tool)
    //    BOOL opeCall    : TRUE(operator call tool)
    //    BOOL alarm      : TRUE(alarm tool)
    //    BOOL prohibit   : TRUE(prohibit use tool)
    //    BOOL almDisable : TRUE(tool alarm disable)
    //    BOOL weight     : Weight Tool.
    //    BOOL thruCoolant: TRUE(Spindle through coolant tool)
    //    LPWORD bitField : Bit field data
    //  [Result]
    //    EM_OK(success)
    //Int16 md3pro3_make_tool_mmctype(BOOL firstUse, BOOL opeCall, BOOL alarm, BOOL prohibit,
    //                                          BOOL almDisable, BOOL weight, BOOL thruCoolant, LPWORD bitField);

    //  [Function]
    //    Check input value on screen according to item.
    //  [Arguments]
    //    DWORD handle  : HANDLE number
    //    Int16 item      : Constant indicating each data item of tool.
    //    long  value     : Input value (It is necessary to casted for LONG).
    //    BOOL  *suitable  : TRUE(within range) / FALSE
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data)
    //Int16 md3pro3_chk_tool_value(DWORD handle, Int16 item, long value, LPBOOL suitable);

    //  [Function]
    //    Write all data for tool of specified.
    //  [Argument]
    //    DWORD  handle  : HANDLE number
    //    WORD    fromPot : pot number.
    //    WORD    toPot   : pot number.
    //    LPDWORD ptn     : Program Tool Number.
    //    LPDWORD ftn     : Functional Tool Number.
    //    LPDWORD itn     : Individual Tool Number.
    //    LPDWORD tl      : Set value of TL monitor (least input increment:sec.).
    //    LPDWORD remain  : Remain or used tool life for TL monitor (least input increment:sec.).
    //    LPDWORD len     : Tool length (least input increment:0.001mm/0.0001inch).
    //    LPDWORD dia     : Tool diameter (least input increment:0.001mm/0.0001inch).
    //    LPWORD  type    : Tool type
    //    LPWORD  alm     : Alarm status
    //    LPWORD  ac      : Set value of AC monitor (least input increment:0.1A).
    //    LPWORD  sl      : Set value of SL monitor (least input increment:0.1A).
    //  [Result]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data)
    //Int16 md3pro3_set_tool_common(DWORD handle, WORD fromPot, WORD toPot, LPDWORD ptn, LPDWORD ftn,
    //                                             LPDWORD itn, LPDWORD tl, LPDWORD remain, LPDWORD len, LPDWORD dia,
    //                                             LPWORD type, LPWORD alm, LPWORD ac, LPWORD sl);

    //  [Arguments]
    //    DWORD      handle : HANDLE number
    //    toolIdData_r  *data  :
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_get_toolid_data(DWORD handle, toolIdData_r *data);

    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_OPTION(no option)
    //Int16 md3pro3_get_toolin_accept(DWORD handle, LPBOOL onOff, LPBYTE status, LPWORD pot);

    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_OPTION(no option)
    //Int16 md3pro3_set_toolin_accept(DWORD handle, BOOL onOff, BYTE status, WORD potNo);

    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_OPTION(no option)
    //Int16 md3pro3_get_toolout_accept(DWORD handle, LPBOOL onOff, LPBYTE status, LPWORD pot);

    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_OPTION(no option)
    //Int16 md3pro3_set_toolout_accept(DWORD handle, BOOL onOff, BYTE status, WORD potNo);

    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_OPTION(no option)
    //Int16 md3pro3_chk_toolin(DWORD handle, LPBOOL request, LPBOOL finish, LPBYTE status);

    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_OPTION(no option)
    //Int16 md3pro3_chk_toolout(DWORD handle, LPBOOL request, LPBOOL finish, LPBYTE status);

    //  [Function]
    //    Read number of pallets that is setting data in Professional-3.
    //  [Arguments]
    //    DWORD  handle    : HANDLE number
    //    LPWORD maxPallet  : Number of pallets
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_max_pallet(DWORD handle, LPWORD maxPallet);

    //  [Function]
    //    Read number of faces that is setting data in Professional-3.
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    LPWORD maxFace  : Number of faces
    //    LPWORD maxFace  :
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_max_face(DWORD handle, LPWORD maxFace);

    //  [Function]
    //    Divide block skip data into each block skip number.
    //  [Arguments]
    //    WORD blockSkip  : Divided block skip data (bit field)
    //    LPBOOL bkskip2  : TRUE(block skip No.2) / FALSE(not block skip)
    //    LPBOOL bkskip3  : TRUE(block skip No.3) / FALSE(not block skip)
    //     :      :           :                           :
    //    LPBOOL bkskip9  : TRUE(block skip No.9) / FALSE(not block skip)
    //  [Return]
    //    EM_OK(success)
    //Int16 md3pro3_split_bkskip(WORD blockSkip, LPBOOL bkskip2, LPBOOL bkskip3, LPBOOL bkskip4, LPBOOL bkskip5,
    //                                          LPBOOL bkskip6, LPBOOL bkskip7, LPBOOL bkskip8, LPBOOL bkskip9);

    //  [Function]
    //    Expand each block skip number to bit field.
    //  [Arguments]
    //    BOOL bkskip2 : TRUE(block skip No.2)/FALSE(not block skip)
    //    BOOL bkskip3 : TRUE(block skip No.3)/FALSE
    //     :      :        :                           :
    //    BOOL bkskip9 : TRUE(block skip No.9)/FALSE
    //    LPWORD blockSkip : Created WORD data (bit field).
    //  [Return]
    //    EM_OK(success)
    //Int16 md3pro3_make_bkskip(BOOL bkskip2, BOOL bkskip3, BOOL bkskip4, BOOL bkskip5,
    //                                         BOOL bkskip6, BOOL bkskip7, BOOL bkskip8, BOOL bkskip9, LPWORD blockSkip);

    //  [Function]
    //    Divide alarm status into each alarm item.
    //  [Arguments]
    //    DWORD almData     : Divided alarm status (bit field).
    //    LPBOOL toolChk    : TRUE(tool unchecked alarm)  / FALSE(not alarm)
    //    LPBOOL bts        : TRUE(BTS alarm)        / FALSE(not alarm)
    //    LPBOOL bts2       : TRUE(BTS2 alarm)      / FALSE(not alarm)
    //    LPBOOL ac         : TRUE(AC alarm)        / FALSE(not alarm)
    //    LPBOOL sl         : TRUE(SL alarm)        / FALSE(not alarm)
    //    LPBOOL tl         : TRUE(TL alarm)        / FALSE(not alarm)
    //    LPBOOL spareTool  : TRUE(spare tool alarm)    / FALSE(not alarm)
    //    LPBOOL wkMeasure  : TRUE(work measurement alarm)/ FALSE(not alarm)
    //  [Return]
    //    EM_OK(success)
    //Int16 md3pro3_split_work_alarm(DWORD almData, LPBOOL toolChk, LPBOOL bts, LPBOOL bts2,
    //                                         LPBOOL ac, LPBOOL sl, LPBOOL tl, LPBOOL spareTool, LPBOOL wkMeasure);

    //  [Function]
    //    Expand each alarm item into bit field of alarm status.
    //  [Arguments]
    //    BOOL toolChk    : TRUE(tool unchecked alarm)   / FALSE(not alarm)
    //    BOOL bts        : TRUE(BST alarm)              / FALSE(not alarm)
    //    BOOL bts2       : TRUE(BST2 alarm)             / FALSE(not alarm)
    //    BOOL ac         : TRUE(Ac alarm)               / FALSE(not alarm)
    //    BOOL sl         : TRUE(SL alarm)               / FALSE(not alarm)
    //    BOOL tl         : TRUE(TL alarm)               / FALSE(not alarm)
    //    BOOL spareTool  : TRUE(spare tool alarm)       / FALSE(not alarm)
    //    BOOL wkMeasure  : TRUE(work measurement alarm) / FALSE(not alarm)
    //    LPDWORD almData : Created WORD data (bit field).
    //  [Return]
    //    EM_OK(success)
    //Int16 md3pro3_make_work_alarm(BOOL toolChk, BOOL bts, BOOL bts2, BOOL ac,
    //                    BOOL sl, BOOL tl, BOOL spareTool, BOOL wkMeasure, LPDWORD almData);

    //  [Function]
    //    It is checked whether input value is in range of each warkdata item.
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    WORD   item     : Constant indicating each data item of workdata.
    //    LONG   value    : Input value (casted to LONG).
    //    LPBOOL suitable  : TRUE (val lies in the range) / FALSE(val is unusable for specified item.)
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data)
    //  [Remarks]
    //    Some items of work data are able to be defined as WORD,
    //    but this function require items to be casted to LONG.
    //Int16 md3pro3_chk_work_value(DWORD handle, WORD item, long value, LPBOOL suitable);

    //  [Function]
    //    This function read work data of specified item (except comment) for specified face,
    //    specified pallet.
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    WORD   pltNo    : Pallet number
    //    WORD   faceNo  : Face number
    //    WORD   item     : Constant indicating each data item of workdata.
    //    LPLONG value    : Aquired data
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data)
    //  [Remarks]
    //    - Some items of work data is able to be defined as WORD,
    //      but this function returns values casting to LONG.
    //    - In the case reading comments from work data, use mdwkdt_get_comment().
    //Int16 md3pro3_get_work_item(DWORD handle, WORD pltNo, WORD faceNo, WORD item, LPLONG value);

    //  [Function]
    //    This function write work data of specified item (except comment) for specified face,
    //    specified pallet.
    //  [Arguments]
    //    DWORD handle : HANDLE number
    //    WORD  pltNo  : Pallet number
    //    WORD  faceNo : Face number
    //    WORD  item   : Constant indicating each data item of workdata.
    //    long  value  : Setting value
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data)
    //  [Remarks]
    //    - Some items of work data is able to be defined as WORD,
    //      but you must be set the values casting to LONG.
    //    - In the case writting comments to work data, use mdwkdt_set_comment().
    //Int16 md3pro3_set_work_item(DWORD handle, WORD pltNo, WORD faceNo, WORD item, long value);

    //  [Function]
    //    This function read WORK-OFFSET of each axes from work data
    //    for specified face and specified pallet.
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    WORD    pltNo   : Pallet number
    //    WORD    faceNo  : Face number
    //    LPLONG  xAxis   : X-axis offset
    //    LPLONG  yAxis   : Y-axis offset
    //    LPLONG  zAxis   : Z-axis offset
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data)
    //  [Remarks]
    //    Result of each argument is expressed with least input increment.
    //    For example, in the case X-axis offset value is 999.999, xAxis value become 999999.
    //    When each data is not necessary, set NULL in each argument.
    //Int16 md3pro3_get_work_offset(DWORD handle, WORD pltNo, WORD faceNo,
    //                       LPLONG xAxis, LPLONG yAxis, LPLONG zAxis);

    //  [Function]
    //    The function write WORK-OFFSET of each axes to work offset data
    //    for specified face and pallet.
    //  [Arguments]
    //    DWORD handle  : HANDLE number
    //    WORD  pltNo     : Pallet number
    //    WORD  faceNo    : Face number
    //    long  xAxis     : X-axis offset
    //    long  yAxis     : Y-axis offset
    //    long  zAxis     : Z-axis offset
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data)
    //Int16 md3pro3_set_work_offset(DWORD handle, WORD pltNo, WORD faceNo,
    //                       LONG xAxis, LONG yAxis, LONG zAxis);

    //  [Function]
    //    Read date and time at which machining starts for specified face and specified pallet.
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    WORD   pltNo    : Pallet number
    //    WORD   faceNo   : Face number
    //    LPWORD month    : Started month
    //    LPWORD day      : Started day
    //    LPWORD hour     : Started hour
    //    LPWORD minute   : Started minute
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data)
    //  [Remarks]
    //    When each data is not necessary, set NULL in each argument.
    //Int16 md3pro3_get_work_start_date(DWORD handle, WORD pltNo, WORD faceNo,
    //                                         LPWORD month, LPWORD day, LPWORD hour, LPWORD minute);

    //  [Function]
    //    Read date and time at which machining complete (M30 execute) for specified face and pallet.
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    WORD   pltNo    : Pallet number
    //    WORD   faceNo   : Face number
    //    LPWORD month    : Completed month
    //    LPWORD day      : Completed day
    //    LPWORD hour     : Completed hour
    //    LPWORD minute   : Completed minute
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data)
    //  [Remarks]
    //    When each data is not necessary, set NULL in each argument.
    //Int16 md3pro3_get_work_finish_date(DWORD handle, WORD pltNo, WORD faceNo,
    //                                          LPWORD month, LPWORD day, LPWORD hour, LPWORD minute);

    //  [Function]
    //    Read comment of work data of specified face and pallet.
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    WORD   pltNo    : Pallet number
    //    WORD   faceNo   : Face number
    //    LPSTR  comment  : Comment
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data)
    //  [Remarks]
    //    Argument cmt is necessary by WKDT_CMT_S bytes or more.
    //Int16 md3pro3_get_work_comment(DWORD handle, WORD pltNo, WORD faceNo, LPSTR comment);

    //  [Function]
    //    The function write comment string to work data for specified face and pallet.
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    WORD   pltNo    : Pallet number.
    //    WORD   faceNo   : Face number.
    //    LPSTR  comment  : Comment.
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data)
    //  [Remarks]
    //    The length of comment string is WKDT_CMT_L characters or less.
    //Int16 md3pro3_set_work_comment(DWORD handle, WORD pltNo, WORD faceNo, LPSTR comment);

    //  [Function]
    //    Read all data items of work data for specified face and pallet.
    //  [Arguments]
    //    DWORD  handle    : HANDLE number
    //    LPWORD  pltNo       : Pallet number
    //    LPWORD  faceNo      : Face number
    //    LPLONG  workOffset  : Work offset value for each axes
    //    LPDWORD almPtn    : Tool number which became alarm
    //    LPDWORD almNo    : Alarm number (MC alarm)
    //    LPWORD  startMonth  : Started month
    //    LPWORD  startDay    : Started day
    //    LPWORD  startHour   : Started hour
    //    LPWORD  startMinute : Started minute
    //    LPWORD  finishMonth : Completed Month
    //    LPWORD  finishDay   : Completed day
    //    LPWORD  finishHour  : Completed hour
    //    LPWORD  finishMinute: Completed minutes
    //    LPDWORD runTime     : Machining time [min]
    //    LPDWORD autoTime    : Spindle rotation time [min]
    //    LPSTR   comment     : Comment
    //    LPDWORD alarmOnum   : O number which became alarm
    //    LPDWORD programNo   : NC program number (O number)
    //    LPDWORD alarmFlg    : Alarm status (bit field). Refer to mdwkdt_split_alm().
    //    LPWORD  readyFlg    : Ready status
    //    LPWORD  blockSkip   : Block skip (bit field). Refer to mdwkdt_split_bkskip().
    //    LPWORD  endFlg      : The machining completion flag (Only P/M system).
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //  [Remarks]
    //    - Argument cmt is necessary by WKDT_CMT_S bytes or more.
    //    - Allocate an array with 3 elements to argument workOffset.
    //      Offset values are writed as follows;
    //            workOffset[0] = (X-axis offset value)
    //            workOffset[1] = (Y-axis offset value)
    //            workOffset[2] = (Z-axis offset value)
    //    - Results of workOffset are expressed with least input increment.
    //      For example, in the case X-axis offset value is 999.999,
    //      workOffset[0] value becomes 999999.
    //    - When each data is not necessary, set NULL in each argument.
    //Int16 md3pro3_get_work_all(DWORD handle, WORD pltNo, WORD faceNo, LPLONG workOffset,
    //                        LPDWORD almPtn, LPDWORD almNo, LPWORD startMonth, LPWORD startDay,
    //                        LPWORD startHour, LPWORD startMinute, LPWORD finishMonth, LPWORD finishDay,
    //                        LPWORD finishHour, LPWORD finishMinute, LPDWORD runTime, LPDWORD autoTime,
    //                        LPSTR comment, LPDWORD alarmOnum, LPDWORD programNo, LPDWORD alarmFlg,
    //                        LPWORD readyFlg, LPWORD blockSkip, LPWORD endFlg);

    //  [Function]
    //    Write all work data items for specified face of specified pallet.
    //  [Arguments]
    //    DWORD  handle    : HANDLE number
    //    WORD    pltNo       : Pallet number
    //    WORD    faceNo      : Face number
    //    LPLONG  workOffset  : Work offset value for each axes
    //    DWORD   almPtn    : Tool number which became alarm
    //    DWORD   almNo    : Alarm number (MC alarm)
    //    WORD    startMonth  : Started month
    //    WORD    startDay    : Started day
    //    WORD    startHour  : Started hour
    //    WORD    startMinute : Started minute
    //    WORD    endMonth    : Completed Month
    //    WORD    endDay      : Completed day
    //    WORD    endHour     : Completed hour
    //    WORD    endMinute   : Completed minutes
    //    DWORD   runTime     : Machining time [min]
    //    DWORD   autoTime    : Spindle rotation time [min]
    //    LPSTR   comment     : Comment (within WKDT_CMT_L characters)
    //    DWORD   alarmOnum  : O number which became alarm
    //    DWORD   programNo   : NC program number (O number)
    //    DWORD   alarmFlg    : Alarm status (bit field). Refer to mdwkdt_split_alm().
    //    WORD    readyFlg    : Ready status
    //    WORD    blockSkip   : Block skip (bit field). Refer to mdwkdt_split_bkskip().
    //    WORD    endFlg      : The machining completion flag (Only P/M system).
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //  [Remarks]
    //    - Write offset values to argument workOffset as follows;
    //            workOffset[0] = (X-axis offset value)
    //            workOffset[1] = (Y-axis offset value)
    //            workOffset[2] = (Z-axis offset value)
    //    -The workOffset are expressed with least input increment.
    //      For example, in the case X-axis offset value is 999.999,
    //      write 999999 to workOffset[0].
    //Int16 md3pro3_set_work_all(DWORD handle, WORD pltNo, WORD faceNo, LPLONG workOffset,
    //                      DWORD almPtn, DWORD almNo, WORD startMonth, WORD startDay,
    //                      WORD startHour, WORD startMinute, WORD finishMonth, WORD finishDay,
    //                      WORD finishHour, WORD finishMinute, DWORD runTime, DWORD autoTime,
    //                      LPSTR comment, DWORD alarmOnum, DWORD programNo, DWORD alarmFlg,
    //                      WORD readyFlg, WORD blockSkip, WORD endFlg);

    //  [Function]
    //    MHA bit ON/OFF read.
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_get_trans_error(DWORD handle, LPBOOL alarm);

    //  [Function]
    //    MHA bit ON/OFF operation.
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_set_trans_error(DWORD handle, BOOL alarm);

    //  [Function]
    //    Request the release or setting of memory protect.
    //  [Arguments]
    //    DWORD  handle  : HANDLE number
    //    BOOL    release : TRUE(memory protect release) / FALSE(memory protect set)
    //    DWORD   timeout : [msec]
    //  [Return]
    //    EM_OK(success) / EM_HANDLE(invalid handle)
    //Int16 md3pro3_request_mem_protect_release(DWORD handle, BOOL release, DWORD timeout);
    #endregion // Other methods
  }
}
