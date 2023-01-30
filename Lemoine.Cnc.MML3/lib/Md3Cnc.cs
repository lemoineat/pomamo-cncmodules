using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Code that is returned by a MML3 function
  /// </summary>
  public enum MMLReturn : short
  {
    /// <summary>
    /// Internal error occured
    /// </summary>
    EM_INTERNAL = -1,

    /// <summary>
    /// Success
    /// </summary>
    EM_OK = 0,

    /// <summary>
    /// Illegal node number specified
    /// </summary>
    EM_NODE = 1,

    /// <summary>
    /// The handle cannot be acquired because the acquired
    /// number of handles reaches the maximum number
    /// </summary>
    EM_HANDFULL = 2,

    /// <summary>
    /// Invalid handle number specified
    /// </summary>
    EM_HANDLE = 3,

    /// <summary>
    /// Illegal value for the arguments of function specified.
    /// </summary>
    EM_DATA = 4,

    /// <summary>
    /// An error occurred by the FOCAS function
    /// </summary>
    EM_FLIB = 5,

    /// <summary>
    /// There is no option of CNC or the machine
    /// </summary>
    EM_OPTION = 6,

    /// <summary>
    /// The last message sending and receiving is not completed by same thread (rare)
    /// Or: CNC is busy
    /// </summary>
    EM_BUSY = 7,

    /// <summary>
    /// Not used now
    /// </summary>
    EM_NOREPLY = 8,

    /// <summary>
    /// The execution of the function was refused to the machine side
    /// Confirm the execution condition
    /// </summary>
    EM_REJECT = 9,

    /// <summary>
    /// Illegal CNC parameter
    /// </summary>
    EM_PARA = 10,

    /// <summary>
    /// Illegal CNC mode or not system mode
    /// </summary>
    EM_MODE = 11,

    /// <summary>
    /// The error occurred by the Windows function
    /// </summary>
    EM_WIN32 = 12,

    /// <summary>
    /// The error occurred by the socket communication
    /// </summary>
    EM_WINSOCK = 13,

    /// <summary>
    /// Data is protected by the CNC data protection function
    /// </summary>
    EM_PROTECT = 14,

    /// <summary>
    /// The message label or the size of the socket communication is illegal
    /// Or the buffer is empty or full
    /// </summary>
    EM_BUFFER = 15,

    /// <summary>
    /// The function cannot be executed due to an alarm in the machine
    /// </summary>
    EM_ALARM = 16,

    /// <summary>
    /// Reset or stop request from CNC
    /// Call the termination function
    /// </summary>
    EM_RESET = 17,

    /// <summary>
    /// Specific function which must be executed beforehand has not been executed.
    /// Otherwise that function is not available
    /// </summary>
    EM_FUNC = 18,

    /// <summary>
    /// Disconnect the communication with the machine
    /// </summary>
    EM_DISCONNECT = 19,

    /// <summary>
    /// The specified program cannot be selected
    /// </summary>
    EM_SEARCHED = 20
  }

  /// <summary>
  /// Data that can be retrieved at a specific position
  /// </summary>
  public enum Pro5ToolDataItem : UInt32
  {
    /// <summary>
    /// Magazine Number
    /// </summary>
    Magazine = 1,

    /// <summary>
    /// Pot Number
    /// </summary>
    Pot = 2,

    /// <summary>
    /// Pot kind (=0:BT, =1:HSK, =2:Adapter)
    /// </summary>
    PotKind = 3,

    /// <summary>
    /// Program Tool Number
    /// </summary>
    PTN = 4,

    /// <summary>
    /// Functional Tool Number
    /// </summary>
    FTN = 5,

    /// <summary>
    /// Individual Tool Number
    /// </summary>
    ITN = 6,

    /// <summary>
    /// Order (Priority for STS)
    /// </summary>
    Order = 7,

    /// <summary>
    /// Comment, or note
    /// </summary>
    Comment = 8,

    /// <summary>
    /// Through Spindle Coolant Enable (=0:Disable, =1:Enable)
    /// </summary>
    ThroughSpindleEnable = 9,

    /// <summary>
    /// Througt Spindle Removal Time (Unit: [ms])
    /// </summary>
    ThroughSpindleTime = 10,

    /// <summary>
    /// ATC Speed (=0:Normal, =1:Slow, =2:Middle)
    /// </summary>
    AtcSpeed = 11,

    /// <summary>
    /// M60 Disable Flag (=0:M60 Enable, =1:M60 Disable)
    /// </summary>
    M60Disable = 12,

    /// <summary>
    /// Prohibit Flag (=0:Use Possible, =1:Use Prohibit)
    /// </summary>
    Prohibit = 13,

    /// <summary>
    /// TL alarm Enable (STS is done when TL alarm ocurred) (=0:Disable, =1:Enable）
    /// </summary>
    AlarmEffective = 14,

    /// <summary>
    /// Sum of cutter
    /// </summary>
    TotalCutter = 15,

    /// <summary>
    /// Pot Size (=0:Standard, =1:Middle, =2:Large, =3: Extra-Large, =4:Small, =5:Extra-Large2)
    /// </summary>
    PotSize = 16,

    /// <summary>
    /// Prohibit Rotation
    /// </summary>
    ProhibitRotation = 17,

    /// <summary>
    /// Empty Pot (=0:Disable, =1:Enable）
    /// </summary>
    EmptyPot = 18,

    /// <summary>
    /// Irregular Shape (=0:Disable, =1:Enable）
    /// </summary>
    IrregulShape = 19,

    /// <summary>
    /// T code executed when the pot is called
    /// </summary>
    CommandedTCode = 20,

    /// <summary>
    /// Tool continuous search to TLS
    /// </summary>
    TlsContinuousSearch = 21,

    /// <summary>
    /// B Axis Rotation Prohibit (=0:Possible, =1:Prohibit)
    /// </summary>
    B_AxisRotProhibit = 22,

    /// <summary>
    /// One Touch Function Disable (=0:Possible, =1:Prohibit)
    /// </summary>
    OneTouchProhibit = 23,

    /// <summary>
    /// RakuRaku L Measuring
    /// </summary>
    RakuRakuL_Measuring = 24,

    /// <summary>
    /// Size of tool (0:Standard, 1:Middle, 2:Large, 3:Extra Large, 4:Small, 5:Extra-Large2)
    /// </summary>
    ToolSize = 25,

    /// <summary>
    /// TSC Removal Type (0:Default, 1:Air Discharge, 2:Draw Back)
    /// </summary>
    TscRemovalType = 26,

    /// <summary>
    /// Tool exist on magazine
    /// </summary>
    ExistOnMgz = 27,

    /// <summary>
    /// Tool Length for Check (Atc Magazine Interference/3D Check) [0.0001mm]/[0.00001inch]
    /// </summary>
    CheckH = 28,

    /// <summary>
    /// Tool Radius/Diameter for Check (Atc Magazine Interference/3D Check) [0.0001mm]/[0.00001inch]
    /// </summary>
    CheckD = 29,

    /// <summary>
    /// Flag that merged all cutter alarm flags
    /// </summary>
    MergedAlarmFlag = 30,

    /// <summary>
    /// O number 1 that T command execution is possible
    /// </summary>
    ONumber1 = 31,

    /// <summary>
    /// O number 2 that T command execution is possible
    /// </summary>
    ONumber2 = 32,

    /// <summary>
    /// O number 3 that T command execution is possible
    /// </summary>
    ONumber3 = 33,

    /// <summary>
    /// Through Spindle Coolant Frequency [Hz]
    /// Maximum Frequency: Machine Parameter No. 07150-00
    /// Minimum Frequency: Machine Parameter No. 07150-01
    /// </summary>
    TscFrequency = 34,

    /// <summary>
    /// Through Spindle Coolant Flow Check (=0:Disable, =1:Enable）
    /// </summary>
    TscFlowCheckEnable = 35,

    /// <summary>
    /// Radial max cutting load
    /// </summary>
    RadialMaxCuttingLoad = 36,

    /// <summary>
    /// Axial max cutting load
    /// </summary>
    AxialMaxCuttingLoad = 37,

    /// <summary>
    /// Through Spindle Coolant Frequency Setting (=0:Incomplete, =1:Complete, =2:Max)
    /// </summary>
    TscFrqSetting = 38,

    /// <summary>
    /// Controlled Point (X) [0.0001mm]/[0.00001inch]
    /// </summary>
    AngleHeadDistanceX = 39,

    /// <summary>
    /// Controlled Point (Y) [0.0001mm]/[0.00001inch]
    /// </summary>
    AngleHeadDistanceY = 40,

    /// <summary>
    /// Controlled Point (Z) [0.0001mm]/[0.00001inch]
    /// </summary>
    AngleHeadDistanceZ = 41,

    /// <summary>
    /// Tool Vector (X) [0.0001]
    /// </summary>
    AngleHeadVectorX = 42,

    /// <summary>
    /// Tool Vector (Y) [0.0001]
    /// </summary>
    AngleHeadVectorY = 43,

    /// <summary>
    /// Tool Vector (Z) [0.0001]
    /// </summary>
    AngleHeadVectorZ = 44,

    /// <summary>
    /// Ret. Prohibit at PWR Failure (=0:Possible, =1:Prohibit)
    /// </summary>
    PwrFlrRetactDisable = 45,

    /// <summary>
    /// Sister Tool Status (=0:New, =1:Wait, =2:OK, =3:NOK, =4:Test, =5:Measure)
    /// </summary>
    SisterToolStatus = 46,

    /// <summary>
    /// Tool MGZ Vibration Control
    /// </summary>
    ToolMgzVibCtrlMode = 47,

    /// <summary>
    /// 
    /// </summary>
    UseTlsNo = 48,

    /// <summary>
    /// Multi Purpose Flag
    /// </summary>
    MultiPurpose = 49,

    /// <summary>
    /// Air spindle Data No.
    /// </summary>
    AirSpindleDateNumber = 50,

    /// <summary>
    /// Fixed Pot (=0:Random Pot, =1:Fixed Pot, =2:Dummy Pot)
    /// </summary>
    FixedPot = 52,

    /// <summary>
    /// Group No
    /// </summary>
    GroupNo = 53,

    /// <summary>
    /// Shank Count
    /// </summary>
    SchankCount = 56,

    /// <summary>
    /// Seat Check (0:Disable/1:Enable)
    /// </summary>
    SeatChk = 57,

    /// <summary>
    /// Vibration Warning (Rapid)
    /// </summary>
    VibWarnRapid = 58,

    /// <summary>
    /// Vibration Alarm (Rapid)
    /// </summary>
    VibAlarmRapid = 59,

    /// <summary>
    /// Vibration Warning (Cutting)
    /// </summary>
    VibWarnCutting = 60,

    /// <summary>
    /// Vibration Alarm (Cutting)
    /// </summary>
    VibAlarmCutting = 61,

    /// <summary>
    /// TSC Pressure
    /// </summary>
    TscPressure = 62,

    /// <summary>
    /// Alarm Stop Type
    /// </summary>
    AlarmStopType = 63,

    /// <summary>
    /// Professional 5's Cutter data item (1 to 6)
    /// </summary>
    CutterNo = 101,

    /// <summary>
    /// Kind of cutter
    /// *  0:
    /// *  1: Drill
    /// *  2: Ball End Mill 3:Flat End Mill 4:Boring Bar
    /// *  5: Hale Bite 7:Tap
    /// *  8: Reamer 9:Face Mill 10:Probe
    /// * 11: Grinding Tool 12:Dresser
    /// * 14: Limited Tool 15:Air Turbine
    /// * 16: NT Attachment 17:Angle Head
    /// * 18: INCS NAKANISHI
    /// * 19: Air Turbine Fix 21:Turning Tool
    /// * 22: Chamfer Tool 23:Radius End Mill 24:Reference Tool 25:Calibration Tool 26:Setup
    /// * 27: Special Tool-1
    /// * 28: Special Tool-2
    /// </summary>
    Kind = 102,

    /// <summary>
    /// Length compensation number (Unit: [0.0001mm] or [0.00001inch])
    /// </summary>
    HGeometry = 103,

    /// <summary>
    /// Length compensation number (wear) (Unit: [0.0001mm] or [0.00001inch])
    /// </summary>
    HWear = 104,

    /// <summary>
    /// Diameter compensation number (Unit: [0.0001mm] or [0.00001inch])
    /// </summary>
    DGeometry = 105,

    /// <summary>
    /// Diameter compensation number (wear) (Unit: [0.0001mm] or [0.00001inch])
    /// </summary>
    DWear = 106,

    /// <summary>
    /// Tool Life (Time) Enable (=0:Disable, =1:Enable)
    /// </summary>
    ManageLifeTime = 107,

    /// <summary>
    /// Tool Life (Time) Alarm (Unit: [0.1s])
    /// </summary>
    LifeTimeAlarm = 108,

    /// <summary>
    /// Tool Life (Time) Warning (Unit: [0.1s])
    /// </summary>
    LifeTimeWarning = 109,

    /// <summary>
    /// Tool Life (Time) Actual (Unit: [0.1s])
    /// </summary>
    LifeTimeActual = 110,

    /// <summary>
    /// Tool Life (Distance) Enable (=0:Disable, =1:Enable)
    /// </summary>
    ManageLifeDist = 111,

    /// <summary>
    /// Tool Life (Distance) Alarm (Unit: [mm] or [0.1inch])
    /// </summary>
    LifeDistAlarm = 112,

    /// <summary>
    /// Tool Life (Distance) Warning (Unit: [mm] or [0.1inch])
    /// </summary>
    LifeDistWarning = 113,

    /// <summary>
    /// Tool Life (Distance) Actual (Unit: [mm] or [0.1inch])
    /// </summary>
    LifeDistActual = 114,

    /// <summary>
    /// Tool Life (Count) Enable (=0:Disable, =1:Enable)
    /// </summary>
    ManageLifeCount = 115,

    /// <summary>
    /// Tool Life (Count) Alarm
    /// </summary>
    LifeCountAlarm = 116,

    /// <summary>
    /// Tool Life (Count) Warning
    /// </summary>
    LifeCountWarning = 117,

    /// <summary>
    /// Tool Life (Count) Actual
    /// </summary>
    LifeCountActual = 118,

    /// <summary>
    /// SL upper limit (Unit: [0.01%])
    /// </summary>
    SLUpperRate = 119,

    /// <summary>
    /// SL lower limit (Unit: [0.01%])
    /// </summary>
    SLLowerRate = 120,

    /// <summary>
    /// AC limit (Unit: [0.01%])
    /// </summary>
    ACRate = 121,

    /// <summary>
    /// Alarm status
    /// * BIT0: Tool Broken (Detect Long),
    /// * BIT1: Tool Broken,
    /// * BIT2: AC Monitor,
    /// * BIT3: SL Monitor,
    /// * BIT4: SL Monitor (No Load),
    /// * BIT5: Tool Life,
    /// * BIT6: Tool ID,
    /// * BIT8: STS not OK
    /// </summary>
    AlarmFlag = 122,

    /// <summary>
    /// Warning status (BIT0: Tool Life)
    /// </summary>
    WarningFlag = 123,

    /// <summary>
    /// ATC-side BTS Enable Flag (=0: Not execute, =1:Execute)
    /// </summary>
    BTSOn = 124,

    /// <summary>
    /// BTS measurement interval before Machining (Unit: [ms])
    /// </summary>
    BTSBefore = 125,

    /// <summary>
    /// BTS measurement interval after Machining (Unit: [ms])
    /// </summary>
    BTSAfter = 126,

    /// <summary>
    /// BTS measurement length (Unit: [0.0001mm] or [0.00001inch])
    /// </summary>
    BTSLength = 127,

    /// <summary>
    /// Operator Call Flag (=0:Disable, =1:Enable)
    /// </summary>
    OperatorCall = 128,

    /// <summary>
    /// First Used Tool Flag (=0:Disable, =1:Enable)
    /// </summary>
    FirstUse = 129,

    /// <summary>
    /// Spindle Speed executed after M6
    /// </summary>
    SpindleSpeedM6 = 130,

    /// <summary>
    /// Coolant Type executed after M6
    /// </summary>
    CoolantKindM6 = 131,

    /// <summary>
    /// Limit Value of Spindle Rotation Speed at Machining (Unit: [min-1])
    /// </summary>
    SpindleSpeedLimit = 132,

    /// <summary>
    /// BTS action type (=0:Regular, =1:Vibration control, =2:High accuracy)
    /// </summary>
    BTSAction = 133,

    /// <summary>
    /// Spindle rotation speed limitation value by surface speed (Unit: [0.01m/s] ou [0.01inch/s])
    /// </summary>
    SurfaceSpeedLimit = 134,

    /// <summary>
    /// tool standard length HS (geometry) [0.0001mm/0.00001inch]
    /// </summary>
    StandardH = 137,

    /// <summary>
    /// Blade / Tooth
    /// </summary>
    Blade = 138,

    /// <summary>
    /// R Geometry [0.0001mm]/[0.00001inch]
    /// </summary>
    GeometryR = 139,

    /// <summary>
    /// R wear [0.0001mm]/[0.00001inch]
    /// </summary>
    WearR = 140,

    /// <summary>
    /// Radial Max. Load
    /// </summary>
    RadialMaxCLCutter = 141,

    /// <summary>
    /// Axial Max. Load
    /// </summary>
    AxialMaxCLCutter = 142,

    /// <summary>
    /// Instructed Speed (AST4)
    /// </summary>
    InstructSpeedAST4 = 143,

    /// <summary>
    /// Shifted Speed (AST4)
    /// </summary>
    ShiftSpeedAST4 = 144,

    /// <summary>
    /// Load (Max)
    /// </summary>
    MaxLoad = 147,

    /// <summary>
    /// Load (Ave)
    /// </summary>
    AveLoad = 148,

    /// <summary>
    /// Load (Min)
    /// </summary>
    MinLoad = 149,

    /// <summary>
    /// Vibration(Y) (Max)
    /// </summary>
    MaxVibrationY = 150,

    /// <summary>
    /// Vibration(Y) (Ave)
    /// </summary>
    AveVibrationY = 151,

    /// <summary>
    /// Vibration(Y) (Min)
    /// </summary>
    MinVibrationY = 152,

    /// <summary>
    /// Vibration(X) (Max)
    /// </summary>
    MaxVibrationX = 153,

    /// <summary>
    /// Vibration(X) (Ave)
    /// </summary>
    AveVibrationX = 154,

    /// <summary>
    /// Vibration(X) (Min)
    /// </summary>
    MinVibrationX = 155,

    /// <summary>
    /// Vibration(Z) (Max)
    /// </summary>
    MaxVibrationZ = 156,

    /// <summary>
    /// Vibration(Z) (Ave)
    /// </summary>
    AveVibrationZ = 157,

    /// <summary>
    /// Vibration(Z) (Min)
    /// </summary>
    MinVibrationZ = 158,

    /// <summary>
    /// AST4 Spindle speed 2 instructed by S
    /// </summary>
    InstructSpeedAST4_2 = 159,

    /// <summary>
    /// AST4 Spindle speed 2 shifted by AST4
    /// </summary>
    ShiftSpeedAST4_2 = 160,

    /// <summary>
    /// AST4 Spindle speed 3 instructed by S
    /// </summary>
    InstructSpeedAST4_3 = 161,

    /// <summary>
    /// AST4 Spindle speed 3 shifted by AST4
    /// </summary>
    ShiftSpeedAST4_3 = 162,

    /// <summary>
    /// Cut Type (=0:Standard, =1:Light Cut, =2:Heavy Cut)
    /// </summary>
    CutType = 163,

    /// <summary>
    /// Cutter note
    /// </summary>
    CutterNote = 168,

    /// <summary>
    /// Nominal Length [0.0001mm]/[0.00001inch]
    /// </summary>
    NominalLength = 502,

    /// <summary>
    /// Nominal Diameter [0.0001mm]/[0.00001inch]
    /// </summary>
    NominalDiameter = 503,

    /// <summary>
    /// Nominal Length Tolerance(+) [0.0001mm]/[0.00001inch]
    /// </summary>
    NominalLengthTolerancePlus = 504,

    /// <summary>
    /// Nominal Length Tolerance(-) [0.0001mm]/[0.00001inch]
    /// </summary>
    NominalLengthToleranceMinus = 505,

    /// <summary>
    /// Nominal Diameter Tolerance(+) [0.0001mm]/[0.00001inch]
    /// </summary>
    NominalDiameterTolerancePlus = 506,

    /// <summary>
    /// Nominal Diameter Tolerance(-) [0.0001mm]/[0.00001inch]
    /// </summary>
    NominalDiameterToleranceMinus = 507,

    /// <summary>
    /// BIT00 = ATC
    /// BIT01 = APC
    /// BIT02 = Used
    /// BIT03 = Spindle Rotation
    /// BIT04 = Spindle CW
    /// BIT05 = Spindle CCW
    /// BIT06 = TLS
    /// BIT07 = Retract at Power Failure
    /// BIT08 = One Touch Function
    /// BIT09 = B Axis Rotation
    /// BIT10 = Measurement
    /// </summary>
    ProhibitedFlag = 508,

    /// <summary>
    /// Max. Tool Length [0.0001mm]/[0.00001inch]
    /// </summary>
    MaxToolLength = 511,

    /// <summary>
    /// Min. Tool Length [0.0001mm]/[0.00001inch]
    /// </summary>
    MinToolLength = 512,

    /// <summary>
    /// Max. Tool Radius [0.0001mm]/[0.00001inch]
    /// </summary>
    MaxToolRadius = 513,

    /// <summary>
    /// Min. Tool Radius [0.0001mm]/[0.00001inch]
    /// </summary>
    MinToolRadius = 514,

    /// <summary>
    /// Z shift value [0.0001mm]/[0.00001inch]
    /// </summary>
    ZShiftValue = 601,

    /// <summary>
    /// Radius Shift Value [0.0001mm]/[0.00001inch]
    /// </summary>
    RadiusShiftValue = 602,

    /// <summary>
    /// X Shift Value [0.0001mm]/[0.00001inch]
    /// </summary>
    XShiftValue = 603,

    /// <summary>
    /// Y Shift Value [0.0001mm]/[0.00001inch]
    /// </summary>
    YShiftValue = 604,

    /// <summary>
    /// Length (0:None/1:Request/2:Complete)
    /// </summary>
    Length = 605,

    /// <summary>
    /// Radius (0:None/1:Request/2:Complete)
    /// </summary>
    Radius = 606,

    /// <summary>
    /// Spindle Speed [min-1]
    /// </summary>
    SpindleSpeed = 607,

    /// <summary>
    /// Warming-up Time [s]
    /// </summary>
    WarmingUpTime = 608,

    /// <summary>
    /// Cutting Time (After ATC) [0.1s]
    /// </summary>
    CuttingTimeAfterATC = 609,

    /// <summary>
    /// Cutting Distance (After ATC) [mm]/[0.1inch]
    /// </summary>
    CuttingDistanceAfterATC = 610,
  }

  /// <summary>
  /// Data than can be retrieved with md3pro3_get_tool_common_item, by pot
  /// </summary>
  public enum Pro3ToolCommonItem : Int16
  {
    /// <summary>
    /// Tool number (T)
    /// </summary>
    TD_PTN = 1,

    /// <summary>
    /// Individual tool number
    /// </summary>
    TD_ITN = 2,

    /// <summary>
    /// Tool life (sec, mm or count)
    /// Note: unit is known with md3pro3_toollife_info
    /// </summary>
    TD_TL = 3,

    /// <summary>
    /// Remaining or accumulate life value (sec, mm or count)
    /// Note: unit is known with md3pro3_toollife_info
    /// </summary>
    TD_REMAIN = 4,

    /// <summary>
    /// Tool length compensation value (H)
    /// </summary>
    TD_LEN = 5,

    /// <summary>
    /// Tool diameter compensation value (D)
    /// </summary>
    TD_DIA = 6,

    /// <summary>
    /// Functional tool number
    /// </summary>
    TD_FTN = 7,

    /// <summary>
    /// Tool type
    /// </summary>
    TD_TYPE = 8,

    /// <summary>
    /// Tool alarm data
    /// </summary>
    TD_ALM = 9,

    /// <summary>
    /// AC monitor spindle load value (0.1A)
    /// </summary>
    TD_AC = 10,

    /// <summary>
    /// SL monitor spindle load value (0.1A)
    /// </summary>
    TD_SL = 11,

    /// <summary>
    /// Tool type
    /// </summary>
    TD_MMC_TYPE = 12,

    /// <summary>
    /// Tool status
    /// </summary>
    TD_MMC_STT = 13,

    /// <summary>
    /// Tool Size (Class)
    /// </summary>
    TD_MMC_SIZE = 14,

    /// <summary>
    /// ATC-side BTS Enable Flag
    /// </summary>
    TD_BTS = 15,

    /// <summary>
    /// BTS measurement interval before Machining [msec]
    /// </summary>
    TD_BTS_FST = 16,

    /// <summary>
    /// BTS measurement interval after Machining [msec]
    /// </summary>
    TD_BTS_SEC = 17,

    /// <summary>
    /// Through Spindle Coolant Purge Time [sec]
    /// </summary>
    TD_AIR = 18,

    /// <summary>
    /// ATC Speed
    /// </summary>
    TD_SLOW = 19,

    /// <summary>
    /// BTS measurement length
    /// </summary>
    TD_BTS_LEN = 20,

    /// <summary>
    /// BTS action type
    /// </summary>
    TD_BTS_TYPE = 21
  }

  /// <summary>
  /// Tool life management type
  /// </summary>
  public enum Pro3ToolLifeType : short
  {
    /// <summary>
    /// Cutting time in seconds
    /// </summary>
    TLTYPE_SEC = 0,

    /// <summary>
    /// Cutting distance in meters or inches
    /// </summary>
    TLTYPE_DISTANCE = 1,

    /// <summary>
    /// Machining quantity
    /// </summary>
    TLTYPE_COUNT = 2,

    /// <summary>
    /// Cutting time in 0.1 seconds
    /// </summary>
    TLTYPE_01SEC = 3,
  }

  /// <summary>
  /// Type of compensation that can be read / write
  /// </summary>
  public enum CompensationType : UInt16
  {
    /// <summary>
    /// Tool length shape compensation
    /// </summary>
    H_GEOMETRY = 0,

    /// <summary>
    /// Tool length wear compensation
    /// </summary>
    H_WEAR = 1,

    /// <summary>
    /// Tool diameter shape compensation
    /// </summary>
    D_GEOMETRY = 2,

    /// <summary>
    /// Tool diameter wear compensation
    /// </summary>
    D_WEAR = 3
  }

  /// <summary>
  /// Interface to the library Md3Cnc.dll
  /// </summary>
  static public class Md3Cnc
  {
    /// <summary>
    /// Comment length (without the NULL terminator)
    /// </summary>
    public static readonly int CMMT_STR_L = 48;

    /// <summary>
    /// Modal buffer length (without the NULL terminator)
    /// </summary>
    public static readonly int MODAL_BUF_L = 12;

    /// <summary>
    /// CNC Software version length (without the NULL terminator)
    /// </summary>
    public static readonly int CNCSOFT_VER_L = 4;

    /// <summary>
    /// Error number of Md3cnc.dll which occurred at the last is returned.
    /// </summary>
    /// <param name="mainErr">Internal error number of Md3cnc.dll which occurred at the last is returned.</param>
    /// <param name="subErr">Error number of Fanuc library which occurred at the last is returned.</param>
    /// <returns>EM_OK(success)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_GetLastError",
               SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn GetLastError (out int mainErr, out int subErr);

    /// <summary>
    /// Get HANDLE corresponding to node_info.
    /// If same nodeInfo is specified, same handle is returned.
    /// </summary>
    /// <param name="handle">ref uint handle: HANDLE number</param>
    /// <param name="nodeInfo">"nodeNo/IP address/tcpPort/e" (e:1-EmulateMode,0-ReleaseMode)</param>
    /// <param name="timeout">long	timeout		: Timeout value when HANDLE is get</param>
    /// <returns>EM_OK(success) / EM_DATA(invalid data) / EM_HANDFULL(overconnect) /
    ///	EM_NODE(invalid node) / EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) /
    /// EM_BUSY(device is busy) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_alloc_handle",
               SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn AllocHandle (ref uint handle, string nodeInfo, int timeout);

    /// <summary>
    /// Get HANDLE corresponding to node_info.
    /// If same nodeInfo is specified, same handle is returned.
    /// </summary>
    /// <param name="handle">ref uint handle		: HANDLE number</param>
    /// <param name="nodeInfo">LPCSTR	nodeInfo	: "nodeNo/IP address/tcpPort/e" (e:1-EmulateMode,0-ReleaseMode)</param>
    /// <param name="timeout">long	timeout		: Timeout value when HANDLE is get</param>
    /// <param name="ctrlType">long	ctrlType	: Control Type of Machine (17:Pro6, 0:Other)</param>
    /// <returns>EM_OK(success) / EM_DATA(invalid data) / EM_HANDFULL(overconnect) /
    /// EM_NODE(invalid node) / EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) /
    /// EM_BUSY(device is busy) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_alloc_handle2",
               SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn AllocHandle2 (ref uint handle, string nodeInfo, int timeout, int ctrlType);

    /// <summary>
    /// Release the specified HANDLE number.
    /// </summary>
    /// <param name="handle">Handle to free</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_free_handle",
               SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn FreeHandle (uint handle);

    /// <summary>
    /// Gets the maximum number of node of the HSSB.
    /// </summary>
    /// <param name="maxNode">maxNode : pointer to the variable for the number of node.</param>
    /// <param name="emulateMode"></param>
    /// <returns>EM_OK(success)/ EM_FLIB(FANUC error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_get_max_node",
               SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn GetMaxNode (out uint maxNode, bool emulateMode);

    /// <summary>
    /// Gets the specified node information of the HSSB.
    /// </summary>
    /// <param name="nodeNo">specify the node number (0..maximum node number)
    ///	Maximum node number is the number of node is got by using get_max_node() function.</param>
    /// <param name="ioBase">Base address of I/O port of specified node is set.</param>
    /// <param name="status">Status of device installation of specified node is set.
    /// 0: not installed, 1: installed</param>
    /// <param name="cncType">Type of CNC is set.
    /// CNCTYPE_160M	1	(Series 160/180/210)
    /// CNCTYPE_150M	2	(Series 150)
    /// CNCTYPE_PMATE	3	(Power Mate)
    /// CNCTYPE_PMATEI	4	(Power Mate i)
    /// CNCTYPE_300M	9	(Series 300i)</param>
    /// <param name="nodeName">Node name is set.
    /// Node name is up to NODE_NAME_S(20) characters
    /// including 'NULL'.</param>
    /// <param name="emulateMode"></param>
    /// <returns>EM_OK(success) / EM_NODE(invalid node) / EM_FLIB(FANUC error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_get_node_info",
               SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn GetNodeInfo (Int32 nodeNo, out UInt32 ioBase,
                                               out UInt32 status, out UInt32 cncType, StringBuilder nodeName, bool emulateMode);

    //    public static extern MMLReturn cnc_version(uint handle,
    //                                           Int32 *cncType, string typeStr,
    //                                           string seriesStr, string versionStr);
    //	[Function]
    //		Read CNC type and version information string.
    //	[Arguments]
    //		uint handle	: HANDLE number
    //		long  *cncType	: Type of CNC
    //				 CNCTYPE_160M(1)/CNCTYPE_150M(2)/CNCTYPE_PMATE(3)/CNCTYPE_PMATEI(4)/CNCTYPE_300M(9)
    //		string typeStr	: CNC type string (CNCSOFT_VER_STR_S).  ex."16M"
    //		string seriesStr	: CNC series string (CNCSOFT_VER_STR_S).
    //		string versionStr: CNC version string (CNCSOFT_VER_STR_S).
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle)
    //		TRUE/FALSE
    //	[Remarks]
    //		When each data is not necessary, set NULL in each argument.
    //		The size of each string argument should be CNCSOFT_VER_STR_S
    //		or more.

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_cnc_syssoftver",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn cnc_syssoftver(uint handle,
    //                                              string pmcSeries, string pmcVersion,
    //                                              string ladderSeries, string ladderVersion,
    //                                              string cexelibSeries, string cexelibVersion,
    //                                              string cexeappSeries, string cexeappVersion);
    //	[Function]
    //		Read CNC system soft version information string.
    //	[Arguments]
    //		LPSTR	pmcSeries		: PMC soft serise
    //		LPSTR	pmcVersion		: PMC soft version
    //		LPSTR	ladderSeries	: Ladder soft serise
    //		LPSTR	ladderVersion	: Ladder soft version
    //		LPSTR	cexelibSeries	: C executer library serise
    //		LPSTR	cexelibVersion	: C executer library version
    //		LPSTR	cexeappSeries	: C executer application serise
    //		LPSTR	cexeappVersion	: C executer application version
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    //	 	EM_FLIB(FANUC error) / EM_INTERNAL(internal error)
    //	[Remarks]
    //		When each data is not necessary, set NULL pointer in each argument.
    //		The size of each data should be CNCSOFT_VER_STR_S or more.

    //    public static extern MMLReturn get_cncpara(uint handle,
    //                                           UInt16 paraNo, UInt16 dataType, Int32 *value);
    //	[Function]
    //		Read value from specified NC parameter number.
    //	[Arguments]
    //		uint handle	: HANDLE number
    //		UInt16  paraNo	: NC parameter number
    //		UInt16  dataType	: BYTE_PARA(1)/WORD_PARA(2)/LONG_PARA(3)/BYTE_AXIS_PARA(5)/WORD_AXIS_PARA(6)/LONG_AXIS_PARA(7)
    //		long  *value	: Acquired value
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid paraNo/dataType) EM_PROTECT(read pretect) /
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)
    //	[Remarks]
    //		The array size of *_Axis_PARA should be controlled axes or more.

    //    public static extern MMLReturn set_cncpara(uint handle,
    //                                           UInt16 paraNo, UInt16 dataType, Int32 *value);
    //	[Function]
    //		Write value to specified NC parameter number.
    //	[Arguments]
    //		uint handle	: HANDLE number
    //		UInt16  paraNo	: NC parameter number
    //		UInt16  dataType	: BYTE_PARA(1)/WORD_PARA(2)/LONG_PARA(3)/BYTE_AXIS_PARA(5)/WORD_AXIS_PARA(6)/LONG_AXIS_PARA(7)
    //		long  *value	: Setting value
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid paraNo/dataType) / EM_PROTECT(write pretect) /
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    public static extern MMLReturn get_cncpara_bit(uint handle,
    //                                               UInt16 paraNo, UInt16 bitNo, UInt16 dataType, BITINF *onoff);
    //	[Function]
    //		Read bit status from specified NC parameter number.
    //	[Arguments]
    //		DWORD	handle	: HANDLE number
    //		UInt16	paraNo	: NC parameter number
    //		UInt16	bitNo	: Bit number (0,1,....,7)
    //		UInt16	dataType: BIT_PARA(0)/BIT_AXIS_PARA(4)
    //		BITINF	*onoff	: Acquired value ( ON(1)/OFF(0) )
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid paraNo/bitNo/dataType) /
    //		EM_PROTECT(read pretect) / EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)
    //	[Remarks]
    //		The array size of BIT_AXIS_PARA should be controlled axes or more.

    //    public static extern MMLReturn set_cncpara_bit(uint handle,
    //                                               UInt16 paraNo, UInt16 bitNo, UInt16 dataType, BITINF *onoff);
    //	[Function]
    //		Write bit status to specified NC parameter number.
    //	[Arguments]
    //		UInt16	paraNo	: NC parameter number
    //		UInt16	bitNo	: Bit number(0,1,....,7)
    //		UInt16	dataType: BIT_PARA(0)/BIT_AXIS_PARA(4)
    //		BITINF  *onoff	: Setting value ( ON(1)/OFF(0) )
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid paraNo/bitNo/dataType) /
    //		EM_PROTECT(write pretect) / EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    public static extern MMLReturn get_pmcdata(uint handle,
    //                                           UInt16 adrType, UInt16 adrNo, UInt16 dataSize, Int32 *value);
    //	[Function]
    //		Read data from specified PMC type and address.
    //	[Arguments]
    //		DWORD	handle	: HANDLE number
    //		UInt16	adrType	: PMC address type
    //					ADR_G(0)/ADR_F(1)/ADR_Y(2)/ADR_X(3)/ADR_A(4)/ADR_R(5)/ADR_T(6)/ADR_K(7)/ADR_C(8)/ADR_D(9)
    //		UInt16	adrNo	: PMC address number
    //		UInt16	dataSize: 1(char)/2(short)/4(long) bytes
    //		long	*value	: Acquired PMC data
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data) / EM_PROTECT(read pretect) /
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_set_pmcdata",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn set_pmcdata(uint handle,
    //                                           UInt16 adrType, UInt16 adrNo, UInt16 dataSize, Int32 value);
    //	[Function]
    //		Write data to specified PMC type and address.
    //	[Arguments]
    //		DWORD	handle	 : HANDLE number
    //		UInt16	adrType	 : PMC address type
    //					ADR_G(0)/ADR_F(1)/ADR_Y(2)/ADR_X(3)/ADR_A(4)/ADR_R(5)/ADR_T(6)/ADR_K(7)/ADR_C(8)/ADR_D(9)
    //		UInt16	adrNo	 : PMC address number
    //		UInt16	dataSize : 1(char)/2(short)/4(long) bytes
    //		long	value	 : Setting PMC data
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data) / EM_PROTECT(write pretect) /
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //public static extern MMLReturn get_pmcbit(uint handle,
    //                                      UInt16 adrType, UInt16 adrNo, UInt16 bitNo, BITINF *onoff);
    //	[Function]
    //		Read bit status from specified PMC type and address.
    //	[Arguments]
    //		DWORD	handle  : HANDLE number
    //		UInt16	adrType	: PMC address type
    //					ADR_G(0)/ADR_F(1)/ADR_Y(2)/ADR_X(3)/ADR_A(4)/ADR_R(5)/ADR_T(6)/ADR_K(7)/ADR_C(8)/ADR_D(9)
    //		UInt16	adrNo	: PMC address number
    //		UInt16	bitNo	: Bit number (0,1,....,7)
    //		BITINF  *onoff	: Acquired PMC data ( ON(1)/OFF(0) )
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data) / EM_PROTECT(read pretect) /
    //	 	EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    public static extern MMLReturn set_pmcbit(uint handle,
    //                                          UInt16 adrType, UInt16 adrNo, UInt16 bitNo, BITINF onoff);
    //	[Function]
    //		Write bit status to specified PMC type and address.
    //	[Arguments]
    //		DWORD	handle	: HANDLE number
    //		UInt16	adrType	:  PMC address type
    //					ADR_G(0)/ADR_F(1)/ADR_Y(2)/ADR_X(3)/ADR_A(4)/ADR_R(5)/ADR_T(6)/ADR_K(7)/ADR_C(8)/ADR_D(9)
    //		UInt16	adrNo	: PMC address number
    //		UInt16	bitNo	: Bit number (0,1,....,7)
    //		BITINF  onoff	:  Setting PMC data ( ON(1)/OFF(0) )
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data) / EM_PROTECT(write pretect)
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    public static extern MMLReturn get_pmcbyteary(uint handle,
    //                                              UInt16 adrType, UInt16 adrNo, UInt16 dataSize, char *value);
    //	[Function]
    //		Reads byte array data from specified PMC type and address.
    //	[Arguments]
    //		DWORD	handle	 : HANDLE number
    //		UInt16	adrType	 : PMC address type
    //					ADR_G(0)/ADR_F(1)/ADR_Y(2)/ADR_X(3)/ADR_A(4)/ADR_R(5)/ADR_T(6)/ADR_K(7)/ADR_C(8)/ADR_D(9)
    //		UInt16	adrNo	 : PMC start address number
    //		UInt16	dataSize : Read byte size
    //		char	*value	 : Start address of acquired PMC data
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data) / EM_PROTECT(read pretect)
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    public static extern MMLReturn set_pmcbyteary(uint handle,
    //                                              UInt16 adrType, UInt16 adrNo, UInt16 dataSize, char *value);
    //	[Function]
    //		Write byte array data to specified PMC type and address.
    //	[Arguments]
    //		DWORD	handle	 : HANDLE number
    //		UInt16	adrType	 : PMC address type
    //					ADR_G(0)/ADR_F(1)/ADR_Y(2)/ADR_X(3)/ADR_A(4)/ADR_R(5)/ADR_T(6)/ADR_K(7)/ADR_C(8)/ADR_D(9)
    //		UInt16	adrNo	 : PMC start address number
    //		UInt16	dataSize : Write byte size
    //		char	*value	 : Start address of setting PMC data
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(nvalid handle) / EM_DATA(invalid data) / EM_PROTECT(write pretect)
    //	 	EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    public static extern MMLReturn get_pmccntlgrp(uint handle, short *grpNo);
    //	[Function]
    //		Gets the sum total group of control data to manage
    //		PMC data table (address D).
    //  [Arguments]
    //		uint handle : HANDLE number
    //		short *group : Specify the address of the variable to store
    //						the sum total group.
    //	[Return]
    //	 	EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    //	 	EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_set_pmccntlgrp",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn set_pmccntlgrp(uint handle, short grpNo);
    //	[Function]
    //		Sets the sum total group of control data to manage
    //		PMC data table (address D).
    //  [Arguments]
    //		short group : Specify the sum total group.
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data) /
    //	 	EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    public static extern MMLReturn get_pmccntldata(uint handle,
    //                                               short startGrpNo, short endGrpNo, short *tblParam,
    //                                               short *dataType, short *dataSize, short *dataAddr);
    //	[Function]
    //		Reads the control data to manage PMC data table (address D).
    //	[Arguments]
    //		uint handle     : HANDLE number
    //		short startGrpNo : specify the start group number.
    //		short endGrpNo   : specify the end group number.
    //		short *tblParam  : pointer of table parameter
    //		short *dataType  : pointer of data type
    //							(0:BYTE_TYPE,1:WORD_TYPE,2:LONG_TYPE)
    //		short *dataSize  : pointer of size of data in group (number of byte)
    //		short *dataAddr  : pointer of address of data in group (address D inside)
    //	[Retuen]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid group) /
    //	 	EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)
    //	[Remarks]
    //

    //    public static extern MMLReturn set_pmccntldata(uint handle,
    //                                               short startGrpNo, short endGrpNo, short *tblParam,
    //                                               short *dataType, short *dataSize, short *dataAddr);
    //	[Function]
    //		Writes the control data to manage PMC data table (address D).
    //	[Arguments]
    //		short startGrpNo : specify the start group number.
    //		short endGrpNo   : specify the end group number.
    //		short *tblParam  : pointer of table parameter
    //		short *dataType  : pointer of data type
    //							(0:BYTE_TYPE,1:WORD_TYPE,2:LONG_TYPE)
    //		short *dataSize  : pointer of size of data in group (number of byte)
    //		short *dataAddr  : pointer of address of data in group (address D inside)
    //	[Retuen]
    //	 	EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid group) /
    //	 	EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    public static extern MMLReturn num_of_axis(uint handle, short *numAxis);
    //	[Function]
    //		Read number of axes.
    //  [Arguments]
    //		short *numAxis : Number of axis
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle)

    /// <summary>
    /// Check an axis number
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="axisNo"></param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid axis number)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_chk_axis_no", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn chk_axis_no (uint handle, short axisNo);

    /// <summary>
    /// Read each axis name (X,Y,Z...)
    /// - When the number of controlled axes is specified, EM_DATA is restored.
    /// </summary>
    /// <param name="handle">HANDLE number</param>
    /// <param name="axisName">Axis name string (need 2 bytes: 'X'+'\0').</param>
    /// <param name="axisNo">Excluding 300i:Axis number (1-8). 300i:Axis number (1-32)</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid axis number) /
    /// EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_axis_name", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn axis_name (uint handle, StringBuilder axisName, short axisNo);

    //    public static extern MMLReturn axis_unit(uint handle,
    //                                         short *unit, short axisNo);
    //	[Function]
    //		Read least input increment of axis.
    //  [Arguments]
    //		uint handle : HANDLE number
    //		short *unit  : Aquired least input increment
    //					( MM1000(1)/MM10000(2)/MM100000(3)/MM1000000(4)/
    //						INCH10000(11)/INCH100000(12)/INCH1000000(13)/INCH10000000(14)/
    //						DEG1000(21)/DEG10000(22)/DEG100000(23)/DEG1000000(24))
    //		short axisNo : Excluding 300i:Number of Axes (1-8). 300i:Number of Axes (1-32)
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid axis number) /
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    /// <summary>
    /// Read MC status. (running, emergency, motion, CNC mode etc..)
    /// </summary>
    /// <param name="handle">HANDLE number</param>
    /// <param name="ncMode">Memory/Edit/Jog etc..</param>
    /// <param name="ncRun">Stop/Run/Hold etc..</param>
    /// <param name="ncMotion">Action/stop etc..</param>
    /// <param name="mstCodeFin">M/S/T code fin status.</param>
    /// <param name="ncEmg">EMG status.</param>
    /// <param name="editOpe">CNC side edit operation status.</param>
    /// <returns></returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_cnc_status",
               SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn cnc_status (uint handle,
                                              out short ncMode, out short ncRun, out short ncMotion,
                                              out short mstCodeFin, out short ncEmg, out short editOpe);
    //  [Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_PROTECT(read pretect)
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)
    //	[Remarks]
    //		- Refer to FANUC library manual and HSSB.H about descriptions of each result.
    //		- When each data is not necessary, set NULL in each argument.

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_chk_mem_protect",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn chk_mem_protect(uint handle, out bool protect);
    //	[Function]
    //		Check memory protect status (G46#3-6).
    //	[Arguments]
    //		DWORD  handle : HANDLE number
    //		ref bool protect: TRUE(PROTECT ON) / FALSE(PROTECT OFF)
    //  [Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_PROTECT(read pretect) /
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_chk_backedit",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn chk_backedit(uint handle, out bool edit);
    //	[Function]
    //		Read back edit condition on CNC side
    //	[Arguments]
    //		DWORD  handle	: HANDLE number
    //		ref bool edit		: TRUE(BACK editing) / FALSE
    //  [Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_PROTECT(read pretect) /
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_chk_single_block",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn chk_single_block(uint handle, out bool singleBlock);
    //	[Function]
    //		Check single block signal (F4#3)
    //	[Arguments]
    //		DWORD  handle		: HANDLE number
    //		ref bool singleBlock	: TRUE(bit ON)/FALSE(bit OFF)
    //  [Return]
    //		EM_OK(succees) / EM_HANDLE(invalid handle) / EM_PROTECT(read pretect) /
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_chk_cnc_reset",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn chk_cnc_reset(uint handle, out bool reset);
    //	[Function]
    //		Check NC reset status ON/OFF (F1#1).
    //	[Arguments]
    //		DWORD  handle	: HANDLE number
    //		ref bool reset	: TRUE(NC reset ON status) / FALSE
    //  [Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_PROTECT(read pretect) /
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_cnc_reset",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn cnc_reset(uint handle);
    //  [Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_PROTECT(read pretect) /
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    /// <summary>
    /// Check EOP (End Of Program : G52#2) bit. (Case of 300i, R13#0)
    /// </summary>
    /// <param name="handle">HANDLE number</param>
    /// <param name="eop">TRUE(ON) / FALSE</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_PROTECT(read pretect) / EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_chk_eop",
               SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn chk_eop (uint handle, out bool eop);

    /// <summary>
    /// Check start bit (G7#2) on/off. (Case of 300i, R1807#2)
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="start">LPBOOL start:TRUE(bit on) / FALSE(bit off)</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_PROTECT(read pretect) /
    /// EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_chk_start_bit",
               SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn chk_start_bit (uint handle, out bool start);

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_set_cnc_timer",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn set_cnc_timer(uint handle);
    //	[Function]
    //		set CNC time & data by the PC time & date
    //  [Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_REJECT(reject)
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    public static extern MMLReturn get_pmc2_multipurpose(uint handle, byte *value);
    //	[Function]
    //		Read 8 byte data of PMC2 multi purpose area.
    //	[Arguments]
    //		uint handle : HANDLE number
    //		char  *value : 8 byte data
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_PROTECT(read pretect) /
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_modal_hcode",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn modal_hcode(uint handle,
    //                                           out UInt32 codeVal, string codeStr, out UInt16 request);
    //  [Function]
    //      Read string and value of modal H code.
    //  [Arguments]
    //		DWORD	handle	: HANDLE number
    //      LPDWORD	codeVal	: H code value.
    //      string   codeStr	: H code string (Max. string length is MODAL_BUF_L).
    //		ref UInt16	request	: NOT_REQUEST(0)/NOW_REQUEST(1)
    //  [Return]
    //	 	EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    //	 	EM_FLIB(FANUC error) / EM_INTERNAL(internal error)
    //	[Remarks]
    //		- When each data is not necessary, set NULL in each argument.

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_modal_dcode",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn modal_dcode(uint handle,
    //                                           out UInt32 codeVal, string codeStr, out UInt16 request);
    //  [Function]
    //      Read string and value of modal D code.
    //  [Arguments]
    //		DWORD	handle	: HANDLE number
    //      out UInt32 codeVal : D code value.
    //      string   codeStr : D code string (Max. string length is MODAL_BUF_L).
    //		ref UInt16	request	: NOT_REQUEST(0)/NOW_REQUEST(1)
    //  [Return]
    //	 	EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    //	 	EM_FLIB(FANUC error) / EM_INTERNAL(internal error)
    //	[Remarks]
    //		- When each data is not necessary, set NULL in each argument.

    /// <summary>
    /// Read string and value of modal M code
    /// 
    /// When each data is not necessary, set NULL in each argument
    /// </summary>
    /// <param name="handle">HANDLE number</param>
    /// <param name="codeVal">M code value</param>
    /// <param name="codeStr">M code string (Max. string length is MODAL_BUF_L)</param>
    /// <param name="request">NOT_REQUEST(0)/NOW_REQUEST(1)</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /	EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_modal_mcode",
               SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn modal_mcode (uint handle,
                                               out UInt32 codeVal, StringBuilder codeStr, out UInt16 request);

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_modal_scode",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn modal_scode(uint handle,
    //                                           out UInt32 codeVal, string codeStr, out UInt16 request);
    //  [Function]
    //      Read string and value of modal S code.
    //  [Arguments]
    //		DWORD	handle	: HANDLE number
    //      out UInt32 codeVal	: S code value.
    //      string   codeStr	: S code string (Max. string length is MODAL_BUF_L).
    //		ref UInt16	request	: NOT_REQUEST(0)/NOW_REQUEST(1)
    //  [Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    //		EM_FLIB(FANUC error) / EM_INTERNAL(internal error)
    //	[Remarks]
    //		- When each data is not necessary, set NULL in each argument.

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_modal_tcode",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn modal_tcode(uint handle,
    //                                           out UInt32 codeVal, string codeStr, out UInt16 request);
    //  [Function]
    //      Read string and value of modal T code.
    //  [Arguments]
    //		DWORD	handle	: HANDLE number
    //      out UInt32 codeVal	: T code value.
    //      string   codeStr	: T code string (Max. string length is MODAL_BUF_L).
    //		ref UInt16	request	: NOT_REQUEST(0)/NOW_REQUEST(1)
    //  [Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    //		EM_FLIB(FANUC error) / EM_INTERNAL(internal error)
    //	[Remarks]
    //		- When each data is not necessary, set NULL in each argument.

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_modal_fcode",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn modal_fcode(uint handle,
    //                                           out UInt32 upperVal, out UInt32 underVal, string codeStr, out UInt16 request);
    //  [Function]
    //      Read string and value of modal F code.
    //  [Arguments]
    //		DWORD	handle	: HANDLE number
    //      out UInt32 upperVal: Integer part of F code value.
    //      out UInt32 underVal: Decimal part of F code value.
    //      string   codeStr	: F code string (Max. string length is MODAL_BUF_L).
    //		ref UInt16	request	: NOT_REQUEST(0)/NOW_REQUEST(1)
    //  [Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    //		EM_FLIB(FANUC error) / EM_INTERNAL(internal error)
    //	[Remarks]
    //		- When each data is not necessary, set NULL in each argument.

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_modal_gcode",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn modal_gcode(uint handle,
    //                                           out UInt32 upperVal, out UInt32 underVal, string codeStr,
    //                                           out UInt16 request, short gCodeGrp);
    //  [Function]
    //      Read string and value of modal G code, and G code group.
    //  [Arguments]
    //		DWORD	handle	: HANDLE number
    //      out UInt16  upperVal: Integer part of G code value.
    //      out UInt16  underVal: Decimal part of G code value.
    //      string   gCodeStr: G code string (example:"G03", Max. string length is MODAL_BUF_L).
    //      short   gCodeGrp: G code group (MODAL_GRP0,...MODAL_GRP20).
    //		ref UInt16	request	: NOT_REQUEST(0)/NOW_REQUEST(1)
    //  [Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    //		EM_FLIB(FANUC error) / EM_INTERNAL(internal error)
    //	[Remarks]
    //		- When each data is not necessary, set NULL in each argument.

    //    public static extern MMLReturn chk_macrono(uint handle,
    //                                           UInt16 macroNo, short *macroType);
    //  [Function]
    //      Check whether specified macro variable number is usable or not.
    //  [Arguments]
    //		DWORD	handle		: HANDLE number
    //      UInt16	macroNo		: Macro variable number.
    //		short	*macroType	: macro Type
    //				 LOCAL_VARIABLE(1) / COMMON_VARIABLE(2) / NOTUSE_VARIABLE(-1)
    //  [Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_OPTION(no option) /
    //	 	EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_get_macro_string",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn get_macro_string(uint handle,
    //                                                UInt16 macroNo, string macroVal);
    //  [Function]
    //		The macro variable is acquired as a character string.
    //  [Arguments]
    //		uint handle	: HANDLE number
    //      UInt16  macroNo	: Macro variable number.
    //      string macroVal	: Macro value.
    //  [Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid number) / EM_OPTION(no option) /
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_set_macro_string",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn set_macro_string(uint handle,
    //                                                UInt16 macroNo, string macroVal);
    //  [Function]
    //		The macro variable is set as a character string.
    //  [Arguments]
    //		uint handle	: HANDLE number
    //      UInt16  macroNo	: Macro variable number.
    //      string macroVal	: Macro string.
    //  [Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid number) /
    //		EM_OPTION(no option) / EM_PROTECT(write pretect) / EM_DISCONNECT(disconnect) /
    //		EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    /// <summary>
    /// Read current CNC alarms.
    /// 
    /// Note:
    /// - Necessary for each arguments to declare array of "out UInt16" type with
    ///   controlled axes elements * MAX_CNC_ALARM(5) * (TYPE_EXTERNAL+1)
    /// - Returns data stored from the head of this array.
    /// - When each data is not necessary, set NULL in each argument.
    /// </summary>
    /// <param name="handle">HANDLE number</param>
    /// <param name="almNum">UInt16 type array for Alarm number</param>
    /// <param name="axisNo">UInt16 type array for Axis number</param>
    /// <param name="almMsg">Array for Alarm message string (max message length = ALMMSG_L)</param>
    /// <param name="almType">UInt16 type array for Alarm Type</param>
    /// <param name="numAlm">Number of alarm items</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    /// EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_cnc_alarm", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn CncAlarm (uint handle,
                                            [In, Out] UInt16[] almNum, [In, Out] UInt16[] axisNo, StringBuilder[] almMsg,
                                            [In, Out] UInt16[] almType, ref UInt16 numAlm);

    /// <summary>
    /// Retrieve the current number of CNC alarms that have occurred
    /// </summary>
    /// <param name="handle">HANDLE number</param>
    /// <param name="numAlarm">total number of CNC alarms</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    /// EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_chk_cnc_alarm", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn ChkCncAlarm (uint handle, out UInt16 numAlarm);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="numHistory"></param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_BUSY(device is busy) / EM_PARA(invalid parameter) /
    /// EM_REJECT(device reject) / EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_total_cnc_alarm_history", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn TotalCncAlarmHistory (uint handle, out UInt16 numHistory);

    /// <summary>
    /// Retrieve the CNC alarm history data
    /// </summary>
    /// <param name="handle">handle number</param>
    /// <param name="almNum">address of the array where the alarm number is stored</param>
    /// <param name="axisNo">address of the array where alarm axis data is stored</param>
    /// <param name="almMsg">address of the array where alarm message character string is stored</param>
    /// <param name="year">address of the array where year when the alarm occurred is stored</param>
    /// <param name="month">address of the array where month when the alarm occurred is stored</param>
    /// <param name="day">address of the array where day when the alarm occurred is stored</param>
    /// <param name="hour">address of the array where hour when the alarm occurred is stored</param>
    /// <param name="min">address of the array where minute when the alarm occurred is stored</param>
    /// <param name="sec">address of the array where second when the alarm occurred is stored</param>
    /// <param name="numHistory">The number of CNC alarms is stored</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_BUSY(device is busy) / EM_PARA(invalid parameter) /
    /// EM_REJECT(device reject) / EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_cnc_alarm_history", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn CncAlarmHistory (uint handle,
                                                   ref UInt16[] almNum, ref UInt16[] axisNo, ref string[] almMsg,
                                                   ref UInt16[] year, ref UInt16[] month, ref UInt16[] day,
                                                   ref UInt16[] hour, ref UInt16[] min, ref UInt16[] sec,
                                                   out UInt16 numHistory);

    /// <summary>
    /// Read machine coordinate values
    /// </summary>
    /// <param name="handle">DWORD  handle  : HANDLE number</param>
    /// <param name="data">ref UInt32 data  : Coordinate data (Array[0,...,controlled axes])</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    /// EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_machine", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn machine (uint handle, out UInt32 data);

    /// <summary>
    /// Read absolute coordinate values.
    /// </summary>
    /// <param name="handle">DWORD  handle	: HANDLE number</param>
    /// <param name="data">ref UInt32 data		: Coordinate data (Array[0,...,controlled axes])</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    /// EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_absolute", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn absolute (uint handle, out UInt32 data);

    /// <summary>
    /// Read relative coordinate values.
    /// </summary>
    /// <param name="handle">DWORD  handle	: HANDLE number</param>
    /// <param name="data">ref UInt32 data		: Coordinate data (Array[0,...,controlled axes])</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    /// EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_relative", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn relative (uint handle, out UInt32 data);

    /// <summary>
    /// Read values of distance to go.
    /// </summary>
    /// <param name="handle">DWORD  handle	: HANDLE number</param>
    /// <param name="data">ref UInt32 data		: Coordinate data (Array[0,...,controlled axes])</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    /// EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_distance", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn distance (uint handle, out UInt32 data);

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_prepare_zero",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn prepare_zero(uint handle, bool onoff);
    //	[Function]
    //		Set NC parameter which allow spindle not at reference position to move.
    //		This function is used by AUTO ZERO of single-touch function.
    //  [Arguments]
    //		DWORD	handle	: HANDLE number
    //		BOOL	onoff	: TRUE(BIT ON) / FALSE(BIT OFF)
    //  [Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    //		EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="rate"></param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    /// EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_rapid_override", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn rapid_override (uint handle, out UInt16 rate);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="rate"></param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    /// EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_feed_override", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn feed_override (uint handle, out UInt16 rate);

    /// <summary>
    /// Read spindle feed (mm/min).
    /// </summary>
    /// <param name="handle">DWORD	handle	: HANDLE number</param>
    /// <param name="feed">LPDWORD	feed	: Feed value</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    /// EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_spindle_feed", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn spindle_feed (uint handle, out UInt32 feed);

    //    public static extern MMLReturn servo_meter(uint handle,
    //                                           short *axisNum, Int32 *data);
    //	[Function]
    //		Read the value of servo meter.
    //	[Arguments]
    //		DWORD	handle	 : HANDLE number
    //		short	*axisNum : Number of axis
    //		long	*data	 : the value of servo meter
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    //		EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="load"></param>
    /// <param name="speed"></param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    /// EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_spindle_motor", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn spindle_motor (uint handle, out UInt32 load, out UInt32 speed);

    /// <summary>
    /// Read executing NCP information
    /// Remarks:
    /// <item>When each data is not necessary, set NULL in each argument</item>
    /// </summary>
    /// <param name="handle">HANDLE number</param>
    /// <param name="oNum">O number</param>
    /// <param name="execOnum">Executing O number</param>
    /// <param name="seqNo">Sequence number</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /	EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_exec_ncp", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn exec_ncp (uint handle, out UInt32 oNum, out UInt32 execOnum, out UInt32 seqNo);

    /// <summary>
    /// Read executing NCP block (NC program contents).
    /// Remarks:
    /// <item>Finally, be sure of the acquired character string '\ 0' is entered.</item>
    /// <item>bufLen is the "number of characters to get", NULL termination is not included</item>
    /// </summary>
    /// <param name="handle">HANDLE number</param>
    /// <param name="execBlock">NC program contents</param>
    /// <param name="bufLen">Buffer length for reading data</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_exec_block", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn exec_block (uint handle, StringBuilder execBlock, short bufLen);

    /// <summary>
    /// Retrieve the machining program management data that is already registered on the CNC side. Set NULL in each unnecessary argument
    /// </summary>
    /// <param name="handle">HANDLE number</param>
    /// <param name="registerNCP">Number of registered O number on NC memory</param>
    /// <param name="remainNCP">Number of Remain O number</param>
    /// <param name="usedSize">Used NC memory size (byte)</param>
    /// <param name="remainSize">Remain NC memory size (byte)</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_mem_info", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn mem_info (uint handle, out short registerNCP, out short remainNCP, out UInt32 usedSize, out UInt32 remainSize);

    /// <summary>
    /// Acquire the information for the specified O number Information is acquired.
    /// </summary>
    /// <remarks>When each data is not necessary, set NULL in each argument.</remarks>
    /// <param name="handle">HANDLE number</param>
    /// <param name="oNumber">Specified O number. (4 or 8 figures)</param>
    /// <param name="exist">Onumber not found on NC memory</param>
    /// <param name="comment">NC program comment string (CMMT_STR_S = 49)</param>
    /// <param name="size">NC program size (byte) on memory</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid o number) /	EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_mem_dir",
                   SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn mem_dir (uint handle,
                                            int oNumber, out bool exist,
                                            StringBuilder comment, out int size);

    //    public static extern MMLReturn mem_dirall(uint handle,
    //                                          short dataLen, out UInt32 oNumber, string *comment,
    //                                          out UInt32 size, out UInt16 numProg);
    //  [Function]
    //		All O numbers,sizes and comment string (CMMT_STR_S). in the CNC memory are acquired at a time.
    //  [Arguments]
    //		DWORD	handle	 : HANDLE number
    //		short   dataLen  : Size of undermentioned array
    //		ref UInt16  oNumber  : Pointer of UInt16 array for O number acquisition
    //		string   *comment : Array of NC program comment string (CMMT_STR_S).
    //		ref UInt32 size     : Pointer of UInt32 array for size acquisition
    //		ref UInt16  numProg  : Number of acquired O numbers
    //  [Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    //		EM_FLIB(FANUC error) / EM_INTERNAL(internal error)
    //	[Remark]
    //		- When each data is not necessary, set NULL in each argument.
    // 		- O number and size is searched from O1 in CNC memory.
    //		- Getting datas are stored in the array that are set to argument.
    //	 	- When number of getting data are more then "dataLen" that is set to argument,
    //			over flow datas are omission.
    //		- The size of dataLen or more is necessary for argument of "oNumber" and "size".

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_delete_onumber",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn delete_onumber(uint handle,
    //                                              UInt32 minNumber, UInt32 maxNumber, out UInt16 numDel);
    //  [Function]
    //      Delete O number on CNC memory.
    //  [Arguments]
    //		DWORD  handle	 : HANDLE number
    //      DWORD  minNumber : Lower bound of O numbers which wants to be deleted.
    //      DWORD  maxNumber : Upper bound of O numbers which wants to be deleted.
    //		ref UInt16 numDel    : Number of deleted O numbers.
    //  [Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid o number) / EM_SEARCHED(foreground program selected) /
    //		EM_PROTECT(write protect) / EM_BUSY(CNC is busy) / EM_MODE(background editing, MDI mode in CNC side) /
    //		EM_ALARM(alarm (PS000, PS101) in CNC side) / EM_FUNC(downloading, verifying) / EM_DISCONNECT(disconnect) /
    //		EM_FLIB(FANUC erro) / EM_INTERNAL(internal error)
    //	[Remark]
    //		*When you run the O8-digit cnc_deleteo8 2 comes back (but not research and ish FANUC bug)

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_onumber_search",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn onumber_search(uint handle, UInt32 oNumber);
    //	[Function]
    //		Search NC program in CNC memory.
    //	[Arguments]
    //		uint handle	: HANDLE number
    //		DWROD oNumber	: searched O Number
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid o number) / EM_SEARCHED(foreground program selected) /
    //		EM_PROTECT(write protect) / EM_BUSY(CNC is busy) / EM_MODE(background editing) / EM_ALARM(alarm) /
    //		EM_FUNC(downloading, verifying) / EM_DISCONNECT(disconnect) / EM_FLIB(FANUC erro) / EM_INTERNAL(internal error)
    // * O8 digits When running in cnc_searcho8 2 comes back (not survey is but ish FANUC bug)

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_foreground_search",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn foreground_search(uint handle, UInt32 oNumber);
    //	[Function]
    //		Search NC program in CNC memory foreground.
    //	[Arguments]
    //		uint handle	: HANDLE number
    //		DWROD oNumber	: searched O Number
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid o number) / EM_SEARCHED(foreground program selected) /
    //		EM_PROTECT(write protect) / EM_BUSY(CNC is busy) / EM_MODE(background editing) / EM_ALARM(alarm) /
    //		EM_FUNC(downloading, verifying) / EM_DISCONNECT(disconnect) / EM_FLIB(FANUC erro) / EM_INTERNAL(internal error)

    //    public static extern MMLReturn tool_unit(uint handle, short *unit);
    //	[Function]
    //      Read least input increment of tool offset and macro data.
    //  [Arguments]
    //		uint handle: HANDLE number
    //		short *unit : Aquired least input increment
    //					( MM1000(1)/MM10000(2)/MM100000(3)/MM1000000(4)/
    //						INCH10000(11)/INCH100000(12)/INCH1000000(13)/INCH10000000(14))
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    //		EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    public static extern MMLReturn tooloffset_type(uint handle, short *range, out UInt16 memType);
    //  [Function]
    //      Read tool offset memory type (A/B/C).
    //  [Arguments]
    //		DWORD  handle	: HANDLE number
    //      short	*range  : Number of usable tool offset.
    //		ref UInt16	memType	: Tool offset type (TOOL_OFFSET_MEM_A,B,C)
    //  [Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    //		EM_FLIB(FANUC error) / EM_INTERNAL(internal error)
    //	[Remarks]
    //		- When each data is not necessary, set NULL in each argument.

    /// <summary>
    /// Read current tool offset value and H/D number.
    /// - When each data is not necessary, set NULL in each argument.
    /// </summary>
    /// <param name="handle">HANDLE number</param>
    /// <param name="hValue">Tool length offset value.</param>
    /// <param name="dValue">Tool diameter offset value.</param>
    /// <param name="hNo">H number.</param>
    /// <param name="dNo">D number.</param>
    /// <returns>TRUE / FALSE</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_modal_tooloffset",
               SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn modal_tooloffset (uint handle,
                                                    out UInt32 hValue, out UInt32 dValue, out UInt16 hNo, out UInt16 dNo);

    /// <summary>
    /// Read tool offset H/D set value at each H/D number.
    /// </summary>
    /// <param name="handle">HANDLE number</param>
    /// <param name="hValue">Tool length offset value.</param>
    /// <param name="dValue">Tool dia. offset value.</param>
    /// <param name="hNo">H number (0: not read).</param>
    /// <param name="dNo">D number (0: not read).</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    /// EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_get_tooloffset",
               SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn get_tooloffset (uint handle,
                                                  out UInt32 hValue, out UInt32 dValue, UInt16 hNo, UInt16 dNo);

    /// <summary>
    /// Write tool offset data (H/D value) at each H/D number.
    /// </summary>
    /// <param name="handle">HANDLE number</param>
    /// <param name="hValue">H value (Tool length offset).</param>
    /// <param name="dValue">D value (Tool diameter offset).</param>
    /// <param name="hNo">Tool offset H number (0: H value not write).</param>
    /// <param name="dNo">Tool offset D number (0: D value not write).</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    /// EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_set_tooloffset",
               SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn set_tooloffset (uint handle,
                                                  Int32 hValue, Int32 dValue, UInt16 hNo, UInt16 dNo);

    /// <summary>
    /// Read tool offset H/D set value at each H/D number.
    /// </summary>
    /// <param name="handle">HANDLE number</param>
    /// <param name="hValue">Tool length wear value.</param>
    /// <param name="dValue">Tool dia. wear value.</param>
    /// <param name="hNo">H number (0: not read).</param>
    /// <param name="dNo">D number (0: not read).</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_OPTION(memory type A) /
    /// EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_get_toolwear",
               SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn get_toolwear (uint handle,
                                                out UInt32 hValue, out UInt32 dValue, UInt16 hNo, UInt16 dNo);

    /// <summary>
    /// Write tool offset data (H/D value) at each H/D number.
    /// </summary>
    /// <param name="handle">HANDLE number</param>
    /// <param name="hValue">H value (Tool length offset).</param>
    /// <param name="dValue">D value (Tool diameter offset).</param>
    /// <param name="hNo">Tool wear H number (0: H value not write).</param>
    /// <param name="dNo">Tool wear D number (0: D value not write).</param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_OPTION(memory type A) /
    /// EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_set_toolwear",
               SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn set_toolwear (uint handle,
                                                Int32 hValue, Int32 dValue, UInt16 hNo, UInt16 dNo);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="compenType"></param>
    /// <param name="fromNo"></param>
    /// <param name="toNo"></param>
    /// <param name="value"></param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data) /
    /// EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_get_tool_compensation",
               SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn get_tool_compensation (uint handle,
                                                          UInt16 compenType, UInt16 fromNo, UInt16 toNo, [In] Int32[] value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="compenType"></param>
    /// <param name="fromNo"></param>
    /// <param name="toNo"></param>
    /// <param name="value"></param>
    /// <returns>EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid data) /
    /// EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)</returns>
    [DllImport ("Md3Cnc.dll", EntryPoint = "md3cnc_set_tool_compensation",
               SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern MMLReturn set_tool_compensation (uint handle,
                                                          UInt16 compenType, UInt16 fromNo, UInt16 toNo, [In, Out] Int32[] value);

    //    public static extern MMLReturn modal_workoffset(uint handle,
    //                                                out UInt32 offsetValue, short *offsetMode);
    //  [Function]
    //      Read current work offset value and offset mode.
    //  [Arguments]
    //		DWORD	handle		: HANDLE number
    //      out UInt32  offsetValue : Each axis work offset value (Array[0,...,controlled axes]).
    //      short	*offsetMode : Workoffset mode (G54_MODE,G55_MODE...EXT_MODE).
    //  [Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DISCONNECT(disconnect) /
    //		EM_FLIB(FANUC error) / EM_INTERNAL(internal error)
    //	[Remarks]
    //		- When each data is not necessary, set NULL in each argument.
    //
    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_get_workoffset",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn get_workoffset(uint handle,
    //                                              out UInt32 offsetValue, UInt16 offsetMode);
    //  [Function]
    //      Read work offset setting value at each mode.
    //  [Arguments]
    //		DWORD	handle		: HANDLE number
    //      LPLONG	offsetValue : Each axis work offset value (Array [0,...,controlled axes]).
    //      UInt16	offsetMode  : Workoffset mode (G54_MODE,G55_MODE...EXT_MODE,G54_1P+1,G54_1P+2,...,G54_1P+300).
    //  [Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid mode) /
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_set_workoffset",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn set_workoffset(uint handle,
    //                                              out UInt32 offsetValue, UInt16 offsetMode);
    //  [Function]
    //		Write work offset data at each mode (G54,G55,...).
    //  [Arguments]
    //      out UInt32 offsetValue : Each axis work offset value (Array [0,...,controlled axes]).
    //      UInt16   offsetMode  : Workoffset mode (G54_MODE,G55_MODE,...,EXT_MODE,G54_1P+1,G54_1P+2,...,G54_1P+300).
    //  [Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_DATA(invalid mode) /
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_upstart",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn upstart(uint handle, Int32 oNumber);
    //	[Function]
    //		request the upload start of specified O number to NC.
    //	[Arguments]
    //		uint handle	: HANDLE number
    //		DWROD oNumber	: uploading O Number
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_BUSY(already started) / EM_DATA(invalid o number) /
    //		EM_MODE(invalid mode) / EM_REJECT(CNC reject) / EM_ALARM(CNC alarm) / EM_PROTECT(memory protect) /
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    public static extern MMLReturn upload(uint handle, char *data, Int32 *size);
    //	[Function]
    //		Read the program content of onumber specified by upstart.
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_RESET(CNC reset) / EM_FUNC(not start yet)
    //		EM_DATA(invalid data) / EM_PROTECT(memory protect) / EM_BUFFER(buffer full) / EM_DISCONNECT(disconnect) /
    //		EM_FLIB(FANUC error) / EM_INTERNAL(internal error)
    //	[Remark]
    //		This function prior to Call, and then run the upstart.

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_upend",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn upend(uint handle);
    //	[Function]
    //		notify NC the upload end.
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_FUNC(not start yet) / EM_DATA(invalid data) /
    //		EM_PROTECT(memory protect) / EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    public static extern MMLReturn upstart_dir(uint handle, Int32 oNumber, char *dir);
    //	[Function]
    //		request the upload start of specified O number at specified directry to NC.
    //	[Arguments]
    //		uint handle	: HANDLE number
    //		DWROD oNumber	: uploading O Number
    //		char  *dir		: Upload original directory
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_BUSY(already started) / EM_DATA(invalid o number) /
    //		EM_MODE(invalid mode) / EM_REJECT(CNC reject) / EM_ALARM(CNC alarm) / EM_PROTECT(memory protect) /
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    public static extern MMLReturn upload_dir(uint handle, char *data, Int32 *size);
    //	[Function]
    //		Read the program content of onumber specified by upstart_dir.
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_RESET(CNC reset) / EM_FUNC(not start yet)
    //		EM_DATA(invalid data) / EM_PROTECT(memory protect) / EM_BUFFER(buffer full) / EM_DISCONNECT(disconnect) /
    //		EM_FLIB(FANUC error) / EM_INTERNAL(internal error)
    //	[Remark]
    //		This function prior to Call, and then run the upstart_dir.

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_upend_dir",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn upend_dir(uint handle);
    //	[Function]
    //		notify NC the specified directry upload end.
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_FUNC(not start yet) / EM_DATA(invalid data) /
    //		EM_PROTECT(memory protect) / EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_downstart",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn downstart(uint handle);
    //	[Function]
    //		request the download start to NC.
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_BUSY(already started) / EM_MODE(invalid mode) /
    //		EM_REJECT(CNC reject) / EM_ALARM(CNC alarm) / EM_PROTECT(memory protect) / EM_DISCONNECT(disconnect) /
    //		EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    public static extern MMLReturn download(uint handle, char *data, Int32 *size);
    //	[Function]
    //		output the download data to NC.
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_RESET(CNC reset) / EM_FUNC(not start yet) /
    //		EM_DATA(invalid data) / EM_PROTECT(memory protected) / EM_REJECT(memory overflow) / EM_BUFFER(buffer full) /
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_downend",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn downend(uint handle);
    //	[Function]
    //		notify NC the download end.
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_FUNC(not start yet) / EM_DATA(invalid data) /
    //		EM_PROTECT(memory protect) / EM_REJECT(memory overflow) / EM_DISCONNECT(disconnect) /
    //		EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    public static extern MMLReturn downstart_dir(uint handle, char *dir);
    //	[Function]
    //		request the download start to NC.
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_BUSY(already started) / EM_MODE(invalid mode) /
    //		EM_REJECT(CNC reject) / EM_ALARM(CNC alarm) / EM_PROTECT(memory protect) / EM_DISCONNECT(disconnect) /
    //		EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    public static extern MMLReturn download_dir(uint handle, char *data, Int32 *size);
    //	[Function]
    //		output the download data to NC.
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_RESET(CNC reset) / EM_FUNC(not start yet) /
    //		EM_DATA(invalid data) / EM_PROTECT(memory protected) / EM_REJECT(memory overflow) / EM_BUFFER(buffer full) /
    //		EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_downend_dir",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn downend_dir(uint handle);
    //	[Function]
    //		notify NC the download end.
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_FUNC(not start yet) / EM_DATA(invalid data) /
    //		EM_PROTECT(memory protect) / EM_REJECT(memory overflow) / EM_DISCONNECT(disconnect) /
    //		EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    public static extern MMLReturn dncstart(uint handle, char *filename);
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_BUSY(already started) / EM_PARA(invalid parameter) /
    //		EM_REJECT(CNC reject) / EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    public static extern MMLReturn dnc(uint handle, char *data, Int32 *size);
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_RESET(CNC reset) / EM_FUNC(not start yet) /
    //		EM_DATA(invalid data) / EM_PARA(invalid parameter) / EM_BUFFER(buffer full) / EM_DISCONNECT(disconnect) /
    //		EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_dncend",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn dncend(uint handle, short result);
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_FUNC(not start yet) / EM_DATA(invalid data) /
    //		EM_PARA(invalid parameter) / EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

    //    [DllImport("Md3Cnc.dll", EntryPoint="md3cnc_select_mainprog",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern MMLReturn select_mainprog(uint handle, string filePath);
    //	[Function]
    //		O-Number Search by file path
    //	[Arguments]
    //		uint handle	: HANDLE number
    //		string filePath	: File Path
    //	[Return]
    //		EM_OK(success) / EM_HANDLE(invalid handle) / EM_FUNC(not start yet) / EM_DATA(invalid data) /
    //		EM_PARA(invalid parameter) / EM_DISCONNECT(disconnect) / EM_FLIB(FANUC error) / EM_INTERNAL(internal error)

  }
}
