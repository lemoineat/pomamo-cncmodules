// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

#include <vcclr.h>
#include <string>
#include <list>
#include <regex>
#include "WinToHeros.h"
#include "HeidenhainLsv2.h"
#include "StringConversion.h"
#include "NumberConversion.h"
#include "lsv2/lsv2_ctrl.h"
#include "lsv2/lsv2_def.h"
#include "lsv2/lsv2_file.h"
#include "lsv2/lsv2_err.h"
#include "ToolData.h"

#undef GetCurrentDirectory

using namespace System::Collections::Generic;
using namespace System::Globalization;
using namespace System::IO;
using namespace System::Text::RegularExpressions;
using namespace Lemoine::Conversion;
using namespace Lemoine::Cnc;
//using namespace std;

#define DLL_NAME "LSV2D32C"
#define TABLE_LINE_SIZE (512)

namespace Lemoine
{
  namespace Cnc
  {
    bool CALLBACK LSV2BlockHook(void*)
    {
      return true;
    }

    DWORD _lswap(DWORD l)
    {
#ifdef _NOSWAP 
      return (l);
#else  
      return (l >> 24 |
        (l & 0x0000FF00L) << 8 |
        (l & 0x00FF0000L) >> 8 |
        (l & 0x000000FFL) << 24);
#endif
    }

    WORD _wswap(WORD w)
    {
#ifdef _NOSWAP 
      return (w);
#else  
      return (w >> 8 | (w & 0xFF) << 8);
#endif
    }

    HeidenhainLsv2::HeidenhainLsv2()
      : Lemoine::Cnc::BaseCncModule("Lemoine.Cnc.In.HeidenhainLsv2")
      , m_spindleLoadPLCAddress(nullptr)
      , m_multiplier(1)
      , m_keepPlcConnection(false)
      , m_keepDncConnection(false)
      , m_connected(false)
      , m_lsv2Library(NULL)
      , m_hPort(new HANDLE())
      , m_lsv2Para(new LSV2PARA())
      , m_isLoggedDnc(false)
      , m_isLoggedFile(false)
      , m_isLoggedPlc(false)
      , m_isLoggedData(false)
      , m_model(Model::HEID_UNKNOWN)
      , m_parameterCache(gcnew System::Collections::Generic::Dictionary<String^, String^>)
      , m_overrideValues(false)
      , m_programValues(false)
      , m_programStatusOk(false)
      , m_programStatus(LSV2_PROGRAM_STATUS_STARTED)
      , m_downloadDateTime(nullptr)
      , m_lastGetFromFileNameList(gcnew Hashtable())
      , m_lastValueFromFileNameList(gcnew Hashtable())
      , m_downloadTableValues(gcnew Hashtable())
      , m_xAxisName("X")
      , m_yAxisName("Y")
      , m_zAxisName("Z")
      , m_uAxisName("U")
      , m_vAxisName("V")
      , m_wAxisName("W")
      , m_aAxisName("A")
      , m_bAxisName("B")
      , m_cAxisName("C")
      , m_toolLifeData(nullptr)
      , m_toolMissingVariables(gcnew List<String^>())
      , m_toolAvailableVariables(gcnew List<String^>())
    {
      *m_hPort = INVALID_HANDLE_VALUE;
    }

    HeidenhainLsv2::!HeidenhainLsv2()
    {
      Disconnect();
      delete m_hPort;
      delete m_lsv2Para;
      FreeLibrary(m_lsv2Library);
    }

    void HeidenhainLsv2::LoadLsv2Library()
    {
      if (0 == this->CncAcquisitionId) {
        log->ErrorFormat("LoadLsv2Library: "
          "CncAcquisitionId has not been set yet (={0}) "
          "=> could not load any Lsv2Library",
          this->CncAcquisitionId);
        throw gcnew Exception("CncAcquisitionId not set");
      }

      DirectoryInfo^ assemblyDirectory =
        Directory::GetParent(Lemoine::Info::AssemblyInfo::AbsolutePath);
      Directory::SetCurrentDirectory(assemblyDirectory->FullName);
      String^ currentDirectory = Directory::GetCurrentDirectory();
      log->InfoFormat("HeidenhainLsv2: "
        "Current directory is {0}",
        currentDirectory);
      String^ srcDllName = String::Format("{0}.dll",
        DLL_NAME);
      String^ dllName = String::Format("{0}-{1}.dll",
        DLL_NAME,
        this->CncAcquisitionId);
      try {
        File::Copy(srcDllName,
          dllName,
          true);
      }
      catch (IOException^ ex) {
        log->ErrorFormat("HeidenhainLsv2: "
          "could not copy {0} to {1} directory={2}, "
          "because the file is in use "
          "{3} ",
          srcDllName, dllName,
          currentDirectory,
          ex);
        throw ex;
      }
      catch (Exception^ ex) {
        log->FatalFormat("HeidenhainLsv2: "
          "could not copy {0} to {1} directory={2}, "
          "{3}",
          srcDllName, dllName,
          currentDirectory,
          ex);
        throw ex;
      }
      pin_ptr<const wchar_t> dllName2 = PtrToStringChars(dllName);
      m_lsv2Library = LoadLibraryW(dllName2);
      if (NULL == m_lsv2Library) {
        log->FatalFormat("HeidenhainLsv2: "
          "Failed to load dll {0} !",
          dllName);
        throw gcnew Exception("Could not load " + dllName);
      }
      // Initialize the lsv2 function pointers
#undef API
#define API(RTYPE, METHOD, ARGS) \
    { \
    if ( (METHOD = (RTYPE (__stdcall *) ARGS)GetProcAddress (m_lsv2Library, #METHOD)) == NULL) { \
    log->FatalFormat ("HeidenhainLsv2: GetProcAddress of method {0} failed", #METHOD);\
    throw gcnew Exception ("GetProcAddress failed");\
    } \
    }
#include "Lsv2Api.h"
    }

    bool HeidenhainLsv2::CheckConnection()
    {
      if (false == this->m_connected) {
        log->InfoFormat("CheckConnection: "
          "the CNC is not connected: try to connect");

        // 0. Check the connection parameters
        if (this->IPAddress == nullptr) {
          log->Error("CheckConnection: "
            "no IP Address is given");
          return false;
        }

        // 1. Check the LSV2 library is loaded, else load it
        if (NULL == m_lsv2Library) {
          LoadLsv2Library();
        }

        // 2. LSV2Open
        DWORD baudRate = 0;
        System::Diagnostics::Debug::Assert(this->IPAddress != nullptr);
        std::string ipAddress2 = ConvertToStdString(this->IPAddress);
        if (false == LSV2Open(m_hPort, ipAddress2.c_str(), &baudRate, true)) {
          int errorCode = GetLastError();
          log->ErrorFormat("CheckConnection: "
            "LSV2Open failed with error {0}",
            errorCode);
          return false;
        }

        // 3. BlockHook
        if (false == LSV2SetBlockHook(*m_hPort, LSV2BlockHook)) {
          log->Warn("CheckConnection: "
            "LSV2SetBlockHook failed !");
        }

        // 4. INSPECT Login
        if (false == LSV2Login(*m_hPort, "INSPECT", NULL)) {
          log->Error("CheckConnection: "
            "LSV2Login INSPECT failed !");
          LogLsv2Error("LSV2Login/INSPECT");
          Disconnect();
          return false;
        }

        // 5. LSV2ReceivePara
        if (false == LSV2ReceivePara(*m_hPort, m_lsv2Para)) {
          log->Error("CheckConnection: "
            "LSV2ReceivePara failed !");
          LogLsv2Error("LSV2ReceivePara");
          Disconnect();
          return false;
        }

        // 6. Version
        char ncModel[80];
        memset(&ncModel[0], 0, 80);
        char ncVersion[80];
        memset(&ncVersion[0], 0, 80);
        if (false == LSV2ReceiveVersions(*m_hPort, ncModel, ncVersion, NULL, NULL)) {
          log->Error("CheckConnections: "
            "LSV2ReceiveVersions failed");
          LogLsv2Error("LSV2ReceiveVersions");
          Disconnect();
          return false;
        }
        else {
          modelString = ConvertToManagedString(ncModel);
          version = ConvertToManagedString(ncVersion);
          if (modelString->Contains("426")) {
            m_model = Model::HEID_426;
          }
          else if (modelString->Contains("430")) {
            m_model = Model::HEID_430;
          }
          else if (modelString->Contains("530")) {
            m_model = Model::HEID_530;
          }
          else if (modelString->Contains("6000i")) {
            m_model = Model::HEID_530;
          }
          else if (version->Contains("530")) {
            m_model = Model::HEID_530;
          }
          else if (modelString->Contains("640")) {
            m_model = Model::HEID_640;
          }
          log->InfoFormat("CheckConnections: "
            "ReceiveVersions returned ncModel={0} ncVersion={1} model={2}",
            modelString, version, m_model);
        }

        this->m_connected = true;
      }

      return true;
    }

    bool HeidenhainLsv2::CheckDNCConnection()
    {
      if (false == CheckConnection()) {
        log->ErrorFormat("CheckDNCConnection: "
          "connection to the CNC failed");
        return false;
      }

      if (true == m_isLoggedDnc) {
        log->Debug("CheckDNCConnection: "
          "DNC connection is already ok");
        return true;
      }

      if (false == LSV2Login(*m_hPort, "DNC", NULL)) {
        log->Error("CheckDNCConnection: "
          "LSV2Login DNC failed !");
        LogLsv2Error("LSV2Login/DNC");
        return false;
      }

      m_isLoggedDnc = true;
      return true;
    }

    bool HeidenhainLsv2::CheckFileConnection()
    {
      if (false == CheckConnection()) {
        log->ErrorFormat("CheckFileConnection: "
          "connection to the CNC failed");
        return false;
      }

      if (true == m_isLoggedFile) {
        log->Debug("CheckFileConnection: "
          "FILE connection is already ok");
        return true;
      }

      if (false == LSV2Login(*m_hPort, "FILE", NULL)) {
        log->Error("CheckFileConnection: "
          "LSV2Login FILE failed !");
        LogLsv2Error("LSV2Login/FILE");
        return false;
      }

      m_isLoggedFile = true;
      return true;
    }

    bool HeidenhainLsv2::CheckPLCConnection()
    {
      if (false == CheckConnection()) {
        log->ErrorFormat("CheckPLCConnection: "
          "connection to the CNC failed");
        return false;
      }

      if (true == m_isLoggedPlc) {
        log->Debug("CheckPLCConnection: "
          "PLC connection is already ok");
        return true;
      }

      if (false == LSV2Login(*m_hPort, "PLCDEBUG", NULL)) {
        log->Error("CheckPLCConnection: "
          "LSV2Login PLCDEBUG failed !");
        LogLsv2Error("LSV2Login/PLCDEBUG");
        return false;
      }

      m_isLoggedPlc = true;
      return true;
    }

    bool HeidenhainLsv2::CheckDataConnection()
    {
      if (false == CheckConnection()) {
        log->Error("CheckDataConnection: "
          "connection to the CNC failed");
        return false;
      }

      if (true == m_isLoggedData) {
        log->Debug("CheckDataConnection: "
          "Data connection is already ok");
        return true;
      }

      if (m_model < Model::HEID_530) {
        log->ErrorFormat("CheckDataConnection: "
          "DATA login is not supported on model {0} (< 530)",
          this->modelString);
        return false;
      }

      if (String::Compare(this->version, "340490 03") < 0) {
        log->InfoFormat("CanAccessDataProperty: "
          "DATA login is not supported on version {0} (< 340490 03)",
          this->version);
        return false;
      }

      if (false == LSV2Login(*m_hPort, "DATA", NULL)) {
        log->Error("CheckDataConnection: "
          "LSV2Login DATA failed !");
        LogLsv2Error("LSV2Login/DATA");
        return false;
      }

      m_isLoggedData = true;
      return true;
    }

    void HeidenhainLsv2::Disconnect()
    {
      log->Debug("Disconnect /B");

      // 0. *hPort invalid
      if (*m_hPort == INVALID_HANDLE_VALUE) {
        log->Info("Disconnect: "
          "invalid handle => already disconnected");
        this->m_connected = false;
        return;
      }

      // 1. Logout DNC
      LogoutDnc();

      // 2. Logout FILE
      if (m_isLoggedFile) {
        System::Diagnostics::Debug::Assert(*m_hPort != INVALID_HANDLE_VALUE);
        if (false == LSV2Logout(*m_hPort, "FILE")) {
          log->Error("Disconnect: "
            "LSV2Logout FILE failed !");
          LogLsv2Error("LSV2Logout/FILE");
        }
        m_isLoggedFile = false;
      }

      // 3. Logout PLC
      LogoutPlc();

      // 4. Logout DATA
      if (m_isLoggedData) {
        System::Diagnostics::Debug::Assert(*m_hPort != INVALID_HANDLE_VALUE);
        if (false == LSV2Logout(*m_hPort, "DATA")) {
          log->Error("Disconnect: "
            "LSV2Logout DATA failed !");
          LogLsv2Error("LSV2Logout/DATA");
        }
        m_isLoggedData = false;
      }

      // 5. Logout INSPECT
      System::Diagnostics::Debug::Assert(*m_hPort != INVALID_HANDLE_VALUE);
      if (false == LSV2Logout(*m_hPort, "INSPECT")) {
        log->Error("Disconnect: "
          "LSV2Logout INSPECT failed !");
        LogLsv2Error("LSV2Logout/INSPECT");
      }

      // 6. Free other resources
      System::Diagnostics::Debug::Assert(*m_hPort != INVALID_HANDLE_VALUE);
      if (false == LSV2Logout(*m_hPort, "")) {
        log->Error("Disconnect: "
          "LSV2Logout failed !");
        LogLsv2Error("LSV2Logout");
      }

      // 7. Close
      System::Diagnostics::Debug::Assert(*m_hPort != INVALID_HANDLE_VALUE);
      if (false == LSV2Close(*m_hPort)) {
        log->Error("Disconnect: "
          "LSV2Close failed !");
        LogLsv2Error("LSV2Close");
      }

      // 8. Reset the remaining parameters
      *m_hPort = INVALID_HANDLE_VALUE;
      this->m_connected = false;
    }

    void HeidenhainLsv2::LogoutPlc()
    {
      if (m_isLoggedPlc) {
        System::Diagnostics::Debug::Assert(*m_hPort != INVALID_HANDLE_VALUE);
        if (false == LSV2Logout(*m_hPort, "PLCDEBUG")) {
          log->Error("LogoutPlc: "
            "LSV2Logout PLCDEBUG failed !");
          LogLsv2Error("LSV2Logout/PLCDEBUG");
        }
        m_isLoggedPlc = false;
      }
    }

    void HeidenhainLsv2::LogoutDnc()
    {
      if (m_isLoggedDnc) {
        System::Diagnostics::Debug::Assert(*m_hPort != INVALID_HANDLE_VALUE);
        if (false == LSV2Logout(*m_hPort, "DNC")) {
          log->Error("Disconnect: "
            "LSV2Logout DNC failed !");
          LogLsv2Error("LSV2Logout/DNC");
        }
        m_isLoggedDnc = false;
      }
    }

    DWORD HeidenhainLsv2::LogLsv2Error(String^ method)
    {
      DWORD errorCode = GetLastError();
      if (*m_hPort == INVALID_HANDLE_VALUE) {
        log->ErrorFormat("LogLsv2Error: "
          "{0} could not establish connection to the control",
          method);
      }

      char errorText[257];
      DWORD dwTextLen = 256;
      LSV2GetErrStringEx(*m_hPort, errorCode, errorText, &dwTextLen, 0);
      log->ErrorFormat("LogLsv2Error: "
        "{0} returned error {1}: {2}",
        method, errorCode, ConvertToManagedString(errorText));
      LSV2GetTCPErrorDetails(errorText, &dwTextLen);
      log->ErrorFormat("LogLsv2Error: "
        "{0} returned detailed TCP error {1}",
        method, ConvertToManagedString(errorText));
      return errorCode;
    }

    long HeidenhainLsv2::GetPLCValue(char type, long address)
    {
      if (false == CheckPLCConnection()) {
        log->ErrorFormat("GetPLCValue: "
          "connection to PLC failed");
        throw gcnew Exception("No PLC connection");
      }

      int length;
      long rawAddress;
      long value;
      if (false == GetPLCTypeInfo(type, address, &length, &rawAddress)) {
        LogLsv2Error("GetPLCTypeInfo");
        log->Error("GetPLCValue: "
          "GetPLCTypeInfo failed !");
        if (m_model < Model::HEID_530) {
          log->Info("GetPLCValue: "
            "4xx series => disconnect is required");
          Disconnect();
        }
        // TODO: m_isLoggedPlc = false required in some other cases ?
        throw gcnew Exception("GetPLCTypeInfo failed");
      }

      unsigned char byteBuf[256];
      if (false == LSV2ReceiveMem(*m_hPort, rawAddress, length, byteBuf)) {
        LogLsv2Error("LSV2ReceiveMem");
        log->Error("GetPLCValue: "
          "LSV2ReceiveMem failed");
        if (m_model < Model::HEID_530) {
          log->Info("GetPLCValue: "
            "4xx series => disconnect is required");
          Disconnect();
        }
        // TODO: m_isLoggedPlc = false required in some other cases ?
        throw gcnew Exception("LSV2ReceiveMem failed");
      }
      else {
        bool littleEndian = ((m_lsv2Para->lsv2version_flags_ex & V_EX_INTEL) != 0);
        switch (type) {
        case 'B': // Byte
          value = (long)(signed char)byteBuf[0];
          break;
        case 'W': // Word
          if (littleEndian) {
            value = (long)*((short*)byteBuf);
          }
          else {
            value = (long)_wswap(*((WORD*)byteBuf));
          }
          break;
        case 'D': // Doppelwort
          if (littleEndian)
            value = *((long*)byteBuf);
          else
            value = (long)_lswap(*((DWORD*)byteBuf));
          break;
        default: // Boolean
          value = (byteBuf[0] & 0x80) ? 1 : 0;
          break;
        }

        log->DebugFormat("GetPLCValue: "
          "got value {0} from PLC {1}{2}",
          value, Char::ToString(type), address);
        return value;
      }
    }

    long HeidenhainLsv2::GetPLCValue(String^ parameter)
    {
      std::string typeString = ConvertToStdString(parameter->Substring(0, 1));
      if (parameter->Substring(1, 1)->Equals("\\")) {
        return GetPLCValue(typeString[0], long::Parse(parameter->Substring(2)));
      }
      else {
        return GetPLCValue(typeString[0], long::Parse(parameter->Substring(1)));
      }
    }

    bool HeidenhainLsv2::GetPLCBoolValue(String^ parameter)
    {
      return (0 != GetPLCValue(parameter));
    }

    bool HeidenhainLsv2::GetPLCTypeInfo(char type, long address, int* length, long* rawAddress)
    {
      long base;
      switch (type) {
      case 'M':
        base = m_lsv2Para->markerstart;
        *length = 1;
        break;
      case 'I':
        base = m_lsv2Para->inputstart;
        *length = 1;
        break;
      case 'O':
        base = m_lsv2Para->outputstart;
        *length = 1;
        break;
      case 'T':
        base = m_lsv2Para->timerstart;
        *length = 1;
        break;
      case 'C':
        base = m_lsv2Para->counterstart;
        *length = 1;
        break;
      case 'B':
        base = m_lsv2Para->wordstart;
        *length = 1;
        break;
      case 'W':
        base = m_lsv2Para->wordstart;
        *length = 2;
        break;
      case 'D':
        base = m_lsv2Para->wordstart;
        *length = 4;
        break;
      default:
        log->ErrorFormat("GetPLCTypeInfo: "
          "Unknown type {0}",
          type);
        return false;
      }

      *rawAddress = base + address;

      return true;
    }

    bool HeidenhainLsv2::Start()
    {
      m_overrideValues = false;
      m_programValues = false;
      m_programStatusOk = false;
      m_executionModeOk = false;
      return CheckConnection();
    }

    void HeidenhainLsv2::Finish()
    {
      if ((false == m_keepPlcConnection)
        && (true == m_isLoggedPlc)) {
        // Free the PLC connection each time
        // because there is a very limited number of allowed
        // simulaneous PLC connection,
        // to leave some chance to some other systems
        // to connect to PLC
        LogoutPlc();
      }
      if ((false == m_keepDncConnection)
        && (true == m_isLoggedDnc)) {
        // Free the DNC connection each time
        // because there can be some concurrent access problems
        // when several systems with a DNC connections are used
        LogoutDnc();
      }
    }

    [Obsolete("Remove the implementation since there was some weakness in the implementation", true)]
    double HeidenhainLsv2::GetValueFromStampFile(String^ parameters)
    {
      if ((parameters == nullptr) || (parameters->Equals(""))) {
        log->ErrorFormat("GetValueFromStampFile: "
          "invalid value, no parameters");
        throw gcnew ArgumentException("empty parameters argument");
      }

      array<String^>^ params = parameters->Split(gcnew array<Char> {',', ';'});
      if (params->Length < 4) {
        log->ErrorFormat("GetValueFromStampFile: "
          "invalid number of parameters in parameters argument");
        throw gcnew ArgumentException("not enough parameters in parameters argument");
      }
      int skipTime = 15;
      if (params->Length >= 5) {
        try {
          skipTime = Int32::Parse(params[5]);
        }
        catch (Exception^ ex) {
          log->ErrorFormat("GetValueFromStampFile: "
            "invalid skipTime parameter {0}, {1}",
            params[5], ex);
          throw gcnew ArgumentException("Invalid skipTime parameter");
        }
      }
      return GetValueFromStampFile(params[1], params[2], params[3], (wchar_t)params[4][0], skipTime);
    }

    double HeidenhainLsv2::GetValueFromStampFile(String^ pathName, String^ mainStampFile,
      String^ stampFilePrefix, Char separator, int skipTime)
    {
      throw gcnew NotImplementedException("Remove the implementation since there was some weakness in the implementation");
    }

    double HeidenhainLsv2::GetValueFromFileName(String^ parameters)
    {
      String^ stringResult = GetStringValueFromFileName(parameters);
      double result = double::Parse(stringResult, System::Globalization::CultureInfo::InvariantCulture);
      return result;
    }

    String^ HeidenhainLsv2::GetStringValueFromFileName(String^ parameters)
    {
      if ((parameters == nullptr) || (parameters->Equals(""))) {
        log->Error("GetValueFromFileName: "
          "invalid value, no parameters");
        throw gcnew ArgumentException("empty parameters argument");
      }

      array<String^>^ params = parameters->Split(gcnew array<Char> {',', ';'});
      if (params->Length < 3) {
        log->Error("GetValueFromFileName: "
          "invalid number of parameters in parameters argument");
        throw gcnew ArgumentException("not enough parameters in parameters argument");
      }
      int maxFileNumber;
      if (!int::TryParse(params[2], maxFileNumber)) {
        bool deleteRemoteFiles;
        if (bool::TryParse(params[2], deleteRemoteFiles)) {
          maxFileNumber = deleteRemoteFiles ? 1 : int::MaxValue;
        }
        else {
          maxFileNumber = int::MaxValue;
        }
      }
      int skipTime = 10;
      String^ suffix = nullptr;
      if (params->Length >= 3) {
        try {
          skipTime = Int32::Parse(params[3]);
        }
        catch (Exception^ ex) {
          log->ErrorFormat("GetValueFromFileName: "
            "invalid skipTime parameter {0}",
            params[3]);
          log->Error("GetValueFromFileName: "
            "invalid skipTime parameter", ex);
          throw gcnew ArgumentException("Invalid skipTime parameter");
        }
      }
      if (params->Length > 4) {
        suffix = params[4];
      }
      return GetStringValueFromFileName(params[1], maxFileNumber, skipTime, suffix);
    }

    String^ HeidenhainLsv2::GetStringValueFromFileName(String^ directoryPath, int maxFileNumber, int skipTime, String^ suffix)
    {
      String^ result = nullptr;
      if (m_lastGetFromFileNameList->Contains(directoryPath)) {
        // 1. Check the date
        if ((m_lastGetFromFileNameList[directoryPath] != nullptr)
          && ((DateTime::Now - ((DateTime)(m_lastGetFromFileNameList[directoryPath]))).TotalSeconds < skipTime)) {
          log->Debug("GetValueFromFileName: "
            "the stamp file and its associated data is quite recent");

          if (m_lastValueFromFileNameList->Contains(directoryPath)) {
            return (String^)m_lastValueFromFileNameList[directoryPath];
          }
        }
      }

      // 2. get latest file
      System::Collections::Generic::IList<String^>^ remoteFileList = GetFileList(directoryPath);
      int remoteFileListSize = remoteFileList->Count;
      if (0 == remoteFileListSize) {
        log->Info("GetStringValueFromFileName: no file in folder => no value");
        throw gcnew Exception("No file in folder => no value");
      }

      String^ latestFileName = remoteFileList[remoteFileListSize-1];
      if ((latestFileName == nullptr) || (latestFileName->Equals(""))) {
        log->ErrorFormat("GetValueFromFileName: "
          "invalid latest file name {0} in directory {1}", latestFileName, directoryPath);
        throw gcnew Exception("Invalid latest file name");
      }

      // remove suffix
      if (suffix != nullptr && suffix->Length > 0 && latestFileName->Contains(suffix)) {
        result = latestFileName->Remove(latestFileName->Length - suffix->Length);
      }
      else
      {
        result = latestFileName;
      }

      // NO LONGER NUMERIC CHECK result = Int32::Parse(latestFileName);
      m_lastGetFromFileNameList[directoryPath] = DateTime::Now;;
      m_lastValueFromFileNameList[directoryPath] = result;

      try {
        if (maxFileNumber < remoteFileListSize) {
          DeleteRemoteFiles(directoryPath, remoteFileList, latestFileName);
        }
        else {
          String^ REMOVE_REMOTE_FILES_MAX_FILE_NUMBER_KEY = "HeidenhainDNC.RemoveRemoteFiles.MaxFileNumber";
          int REMOVE_REMOTE_FILES_MAX_FILE_NUMBER_DEFAULT = 100; // Switch to about 5 in the future ?
          int maxFileNumberConfig = Lemoine::Info::ConfigSet::LoadAndGet(REMOVE_REMOTE_FILES_MAX_FILE_NUMBER_KEY, REMOVE_REMOTE_FILES_MAX_FILE_NUMBER_DEFAULT);
          if (maxFileNumberConfig < remoteFileListSize) {
            if (log->IsDebugEnabled) {
              log->DebugFormat("GetStringValueFromFileName: remove remote files since max file number in config {0} is reached", maxFileNumberConfig);
            }
            DeleteRemoteFiles(directoryPath, remoteFileList, latestFileName);
          }
        }
      }
      catch (Exception^ ex) {
        log->Error("GetStringValueFromFileName: delete remote files failed", ex);
      }

      log->DebugFormat("GetValueFromFileName value={0}", result);
      return result;
    }

    Int32 HeidenhainLsv2::GetValueFromDownloadedTable(String^ tableName, String^ valueName, int skipTime)
    {
      if (false == DownloadReadTable(tableName, skipTime)) {
        log->Error("GetValueFromDownloadedTable: "
          "DownloadReadTable failed");
        throw gcnew Exception("DownloadReadTable failed");
      }

      if (!m_downloadTableValues->Contains(valueName)) {
        log->WarnFormat("GetValueFromDownloadedTable: "
          "value {0} has not been defined",
          valueName);
        throw gcnew Exception("Value is undefined");
      }

      long result = (long)Math::Round(((double)m_downloadTableValues[valueName]) * m_multiplier);
      log->DebugFormat("GetValueFromDownloadedTable: "
        "Got {0} is {1}",
        valueName, result);
      return result;
    }

    Int32 HeidenhainLsv2::GetValueFromDownloadedTable(String^ parameters)
    {
      if ((parameters == nullptr) || (parameters->Equals(""))) {
        log->ErrorFormat("GetValueFromDownloadedTable: "
          "invalid value, no parameters");
        throw gcnew ArgumentException("empty parameters argument");
      }

      array<String^>^ params = parameters->Split(gcnew array<Char> {',', ';'});
      if (params->Length < 2) {
        log->ErrorFormat("GetValueFromDownloadedTable: "
          "invalid number of parameters in parameters argument");
        throw gcnew ArgumentException("not enough parameters in parameters argument");
      }
      int skipTime = 15;
      if (params->Length >= 3) {
        try {
          skipTime = Int32::Parse(params[2]);
        }
        catch (Exception^ ex) {
          log->ErrorFormat("GetValueFromDownloadedTable: "
            "invalid skipTime parameter {0}",
            params[2]);
          log->Error("GetValueFromDownloadedTable: "
            "invalid skipTime parameter",
            ex);
          throw gcnew ArgumentException("Invalid skipTime parameter", ex);
        }
      }
      return GetValueFromDownloadedTable(params[0], params[1], skipTime);
    }

    bool HeidenhainLsv2::ConnectionError::get()
    {
      bool result = CheckConnection();
      log->DebugFormat("ConnectionError::get: "
        "connection result is {0}",
        result);
      return !result;
    }

    bool HeidenhainLsv2::DNCConnectionError::get()
    {
      bool result = CheckDNCConnection();
      log->DebugFormat("DNCConnectionError::get: "
        "DNC connection result is {0}",
        result);
      return !result;
    }

    bool HeidenhainLsv2::FileConnectionError::get()
    {
      bool result = CheckFileConnection();
      log->DebugFormat("FileConnectionError::get: "
        "File connection result is {0}",
        result);
      return !result;
    }

    bool HeidenhainLsv2::PLCConnectionError::get()
    {
      bool result = CheckPLCConnection();
      log->DebugFormat("PLCConnectionError::get: "
        "PLC connection result is {0}",
        result);
      return !result;
    }

    bool HeidenhainLsv2::DataConnectionError::get()
    {
      bool result = CheckDataConnection();
      log->DebugFormat("DataConnectionError::get: "
        "Data connection result is {0}",
        result);
      return !result;
    }

    Lemoine::Cnc::Position HeidenhainLsv2::Position::get()
    {
      if (false == CheckConnection()) {
        log->ErrorFormat("Position::get: "
          "connection to the CNC failed");
        throw gcnew Exception("No CNC connection");
      }

      if (m_model < Model::HEID_530) {
        log->ErrorFormat("Position::get: "
          "model {0} is less than HEID_530 "
          "=> reading the position is not supported",
          m_model);
        throw gcnew Exception("Position not supported");
      }

      log->DebugFormat("Position::get /B");
      try {
        LSV2RUNINFO runInfo;
        if (false == LSV2ReceiveRunInfo(*m_hPort, LSV2_RUNINFO_AXES_POSITION, &runInfo)) {
          long errorCode = LogLsv2Error("LSV2ReceiveRunInfo");
          log->Error("Position::get: "
            "LSV2ReceiveRunInfo failed !");
          throw gcnew Exception("LSV2ReceiveRunInfo failed");
        }
        else {
          log->DebugFormat("Position::get: "
            "got results");
          bool metric = !runInfo.ri.AxesPosition.IsInch;
          log->DebugFormat("Position::get: "
            "got metric parameter {0}",
            metric);
          double x = 0.0; double y = 0.0; double z = 0.0;
          double u = 0.0; double v = 0.0; double w = 0.0;
          double a = 0.0; double b = 0.0; double c = 0.0;
          unsigned char count = runInfo.ri.AxesPosition.Count;
          char* axisPosition = (char*)(&runInfo.ri.AxesPosition.AxisId
            + count);
          char* p = axisPosition;
          for (int i = 0; i < count; ++i) {
            p += std::strlen(p) + 1;
          }
          char* axisName = p;
          std::string xAxisName = ConvertToStdString(m_xAxisName);
          std::string yAxisName = ConvertToStdString(m_yAxisName);
          std::string zAxisName = ConvertToStdString(m_zAxisName);
          std::string uAxisName = ConvertToStdString(m_uAxisName);
          std::string vAxisName = ConvertToStdString(m_vAxisName);
          std::string wAxisName = ConvertToStdString(m_wAxisName);
          std::string aAxisName = ConvertToStdString(m_aAxisName);
          std::string bAxisName = ConvertToStdString(m_bAxisName);
          std::string cAxisName = ConvertToStdString(m_cAxisName);
          for (unsigned int i = 0; i < count; ++i) {
            if (0 == std::strncmp(axisName, xAxisName.c_str(), xAxisName.size())) {
              x = atof(axisPosition);
              log->DebugFormat("Position::get: "
                "got position {0} for axis X",
                x);
            }
            else if (0 == std::strncmp(axisName, yAxisName.c_str(), yAxisName.size())) {
              y = atof(axisPosition);
              log->DebugFormat("Position::get: "
                "got position {0} for axis Y",
                y);
            }
            else if (0 == std::strncmp(axisName, zAxisName.c_str(), zAxisName.size())) {
              z = atof(axisPosition);
              log->DebugFormat("Position::get: "
                "got position {0} for axis Z",
                z);
            }
            else if (0 == std::strncmp(axisName, uAxisName.c_str(), uAxisName.size())) {
              u = atof(axisPosition);
              log->DebugFormat("Position::get: "
                "got position {0} for axis U",
                u);
            }
            else if (0 == std::strncmp(axisName, vAxisName.c_str(), vAxisName.size())) {
              v = atof(axisPosition);
              log->DebugFormat("Position::get: "
                "got position {0} for axis V",
                v);
            }
            else if (0 == std::strncmp(axisName, wAxisName.c_str(), wAxisName.size())) {
              w = atof(axisPosition);
              log->DebugFormat("Position::get: "
                "got position {0} for axis W",
                w);
            }
            else if (0 == std::strncmp(axisName, aAxisName.c_str(), aAxisName.size())) {
              a = atof(axisPosition);
              log->DebugFormat("Position::get: "
                "got position {0} for axis A",
                a);
            }
            else if (0 == std::strncmp(axisName, bAxisName.c_str(), bAxisName.size())) {
              b = atof(axisPosition);
              log->DebugFormat("Position::get: "
                "got position {0} for axis B",
                b);
            }
            else if (0 == std::strncmp(axisName, cAxisName.c_str(), cAxisName.size())) {
              c = atof(axisPosition);
              log->DebugFormat("Position::get: "
                "got position {0} for axis C",
                c);
            }
            axisPosition += std::strlen(axisPosition) + 1;
            axisName += std::strlen(axisName) + 1;
          }
          Lemoine::Cnc::Position position;
          position.X = Lemoine::Conversion::ConvertToMetric(x, metric);
          position.Y = Lemoine::Conversion::ConvertToMetric(y, metric);
          position.Z = Lemoine::Conversion::ConvertToMetric(z, metric);
          position.U = Lemoine::Conversion::ConvertToMetric(u, metric);
          position.V = Lemoine::Conversion::ConvertToMetric(v, metric);
          position.W = Lemoine::Conversion::ConvertToMetric(w, metric);
          position.A = a;
          position.B = b;
          position.C = c;
          position.Time = DateTime::Now;
          return position;
        }
      }
      catch (Exception^ ex) {
        log->ErrorFormat("Position::get: "
          "exception {0}",
          ex);
        Disconnect();
        throw;
      }
    }

    double HeidenhainLsv2::Feedrate::get()
    {
      try {
        double result = GetPLCValue("D388");
        log->DebugFormat("Feedrate::get: "
          "got feed {0} from PLC value D388",
          result);
        return result;
      }
      catch (Exception^ ex) {
        log->Error("Feedrate::get: exception", ex);
        throw;
      }
    }

    double HeidenhainLsv2::SpindleLoad::get()
    {
      if ((nullptr == m_spindleLoadPLCAddress) || (0 == m_spindleLoadPLCAddress->Length)) {
        log->ErrorFormat("SpindleLoad::get: "
          "no spindle load PLC address is given");
        throw gcnew Exception("No spindle load PLC address");
      }
      try {
        double result = GetPLCValue(m_spindleLoadPLCAddress);
        log->DebugFormat("SpindleLoad::get: "
          "got spindle load {0} from PLC value {1}",
          result, m_spindleLoadPLCAddress);
        return result;
      }
      catch (Exception^ ex) {
        log->ErrorFormat("SpindleLoad::get: "
          "exception {0}, PLC address {1}",
          ex, m_spindleLoadPLCAddress);
        throw ex;
      }
    }

    double HeidenhainLsv2::SpindleSpeed::get()
    {
      try {
        double result = GetPLCValue("D368");
        log->DebugFormat("SpindleSpeed::get: "
          "got spindle speed {0} from PLC value D368",
          result);
        return result;
      }
      catch (Exception^ ex) {
        log->Error("SpindleSpeed::get: exception", ex);
        throw ex;
      }
    }

    bool HeidenhainLsv2::Manual::get()
    {
      GetExecutionMode();
      bool manual = (m_executionMode == LSV2_EXEC_MANUAL) || (m_executionMode == LSV2_EXEC_MDI) || (m_executionMode == LSV2_EXEC_SINGLESTEP);
      log->DebugFormat("Manual::get: "
        "return {0} from m_executionMode={1}",
        manual, (int)m_executionMode);
      return manual;
    }

    bool HeidenhainLsv2::MDI::get()
    {
      GetExecutionMode();
      bool mdi = (m_executionMode == LSV2_EXEC_MDI);
      log->DebugFormat("MDI::get: "
        "return {0} from m_executionMode={1}",
        mdi, (int)m_executionMode);
      return mdi;
    }

    bool HeidenhainLsv2::SingleBlock::get()
    {
      GetExecutionMode();
      bool singleBlock = (m_executionMode == LSV2_EXEC_SINGLESTEP);
      log->DebugFormat("SingleBlock::get: "
        "return {0} from m_executionMode={1}",
        singleBlock, (int)m_executionMode);
      return singleBlock;
    }

    long HeidenhainLsv2::FeedrateOverride::get()
    {
      GetOverrideValues();
      log->DebugFormat("FeedrateOverride::get: "
        "return {0}",
        feedrateOverride);
      return feedrateOverride;
    }

    long HeidenhainLsv2::SpindleSpeedOverride::get()
    {
      GetOverrideValues();
      log->DebugFormat("SpindleSpeedOverride::get: "
        "return {0}",
        spindleSpeedOverride);
      return spindleSpeedOverride;
    }

    unsigned long HeidenhainLsv2::StartEnd::get()
    {
      // TODO
      return 0;
    }

    String^ HeidenhainLsv2::FullProgramName::get()
    {
      GetProgramValues();
      log->DebugFormat("FullProgramName::get: "
        "return {0}",
        m_programName);
      return m_programName;
    }

    String^ HeidenhainLsv2::ProgramName::get()
    {
      GetProgramValues();
      String^ m_programName = Path::GetFileName(this->m_programName);
      log->DebugFormat("ProgramName::get: "
        "return {0}",
        m_programName);
      return m_programName;
    }

    long HeidenhainLsv2::BlockNumber::get()
    {
      GetProgramValues();
      log->DebugFormat("BlockNumber::get: "
        "return {0}",
        m_blockNumber);
      return m_blockNumber;
    }

    int HeidenhainLsv2::ProgramStatusValue::get()
    {
      GetProgramStatus();
      log->DebugFormat("ProgramStatusValue::get: "
        "return {0}",
        (int)m_programStatus);
      return (int)m_programStatus;
    }

    String^ HeidenhainLsv2::ProgramStatus::get()
    {
      GetProgramStatus();
      log->DebugFormat("ProgramStatus::get: "
        "return {0}",
        (int)m_programStatus);
      switch (m_programStatus) {
      case LSV2_PROGRAM_STATUS_STARTED:
        return "Started";
      case LSV2_PROGRAM_STATUS_STOPPED:
        return "Stopped";
      case LSV2_PROGRAM_STATUS_FINISHED:
        return "Finished";
      case LSV2_PROGRAM_STATUS_CANCELED:
        return "Canceled";
      case LSV2_PROGRAM_STATUS_INTERRUPTED:
        return "Interrupted";
      case LSV2_PROGRAM_STATUS_ERROR:
        return "Error";
      case LSV2_PROGRAM_STATUS_ERROR_CLEARED:
        return "ErrorCleared";
      case LSV2_PROGRAM_STATUS_IDLE:
        return "Idle";
      default:
        log->ErrorFormat("ProgramStatus::get: "
          "unknown program status {0}",
          (int)m_programStatus);
        return "Unknown";
      }
    }

    int HeidenhainLsv2::ExecutionModeValue::get()
    {
      GetExecutionMode();
      log->DebugFormat("ExecutionModeValue::get: "
        "return {0}",
        (int)m_executionMode);
      return (int)m_executionMode;
    }

    String^ HeidenhainLsv2::ExecutionMode::get()
    {
      GetExecutionMode();
      log->DebugFormat("ExecutionMode::get: "
        "return {0}",
        (int)m_executionMode);
      switch (m_executionMode) {
      case LSV2_EXEC_MANUAL: // 0
        return "Manual";
      case LSV2_EXEC_MDI: // 1
        return "MDI";
      case LSV2_EXEC_HWHEEL: // 2
        return "HWheel";
      case LSV2_EXEC_SINGLESTEP: // 3
        return "SingleStep";
      case LSV2_EXEC_AUTOMATIC: // 4
        return "Automatic";
      case LSV2_EXEC_OTHER: // 5
        return "Other";
      case LSV2_EXEC_SMART: // 6
        return "Smart";
      case LSV2_EXEC_RPF: // 7
        return "RPF";
      default:
        log->ErrorFormat("ExecutionMode::get: "
          "unknown execution mode {0}",
          (int)m_executionMode);
        return "Unknown";
      }
    }

    bool HeidenhainLsv2::IsDisconnectError(long errorCode)
    {
      // 530
      if (errorCode == LSV2_TCP_ERROR(LSV2_TCP_CONNECT)) {
        log->InfoFormat("IsDisconnectError errorCode={0}: "
          "Disconnect error code TCP/CONNECT detected",
          errorCode);
        return true;
      }
      if (errorCode == LSV2_TCP_ERROR(LSV2_TCP_CLOSED)) {
        log->InfoFormat("IsDisconnectError errorCode={0}: "
          "Disconnect error code TCP/CLOSED detected",
          errorCode);
        return true;
      }
      // 426
      if (errorCode == LSV2_SER_ERROR(LSV2_SER_NOQUITT)) {
        log->InfoFormat("IsDisconnectError errorCode={0}: "
          "Disconnect error code SER/NOQUITT detected",
          errorCode);
        return true;
      }
      if (errorCode == LSV2_SER_ERROR(WSAECONNRESET)) {
        log->InfoFormat("IsDisconnectError errorCode={0}: "
          "Disconnect error code WSAECONNRESET detected",
          errorCode);
        return true;
      }

      return false;
    }

    bool HeidenhainLsv2::ReceiveFile(String^ distantFile, String^ localFile, bool binary)
    {
      log->DebugFormat("ReceiveFile distantFile={0} localFile={1} binary={2} /B",
        distantFile, localFile, binary);

      // 0. Must be logged in
      if (false == CheckFileConnection()) {
        log->ErrorFormat("ReceiveFile: "
          "connection to FILE failed");
        return false;
      }

      // Check distant file exists
      _finddata_t fileInfo;
      if (false == LSV2ReceiveFileInfo(*m_hPort, Lemoine::Conversion::ConvertToStdString(distantFile).c_str(), &fileInfo)) {
        log->ErrorFormat("ReceiveFile: "
          "Distant file not present");
        return false;
      }


      // 1. Binary/Text mode
      DWORD mode;
      if (true == binary) {
        mode = LSV2_TRANSFER_MODE_BIN;
      }
      else {
        mode = LSV2_TRANSFER_MODE_TEXT;
      }

      // 2. Transmission
      if (false == LSV2ReceiveFile(*m_hPort, Lemoine::Conversion::ConvertToStdString(distantFile).c_str(),
        Lemoine::Conversion::ConvertToStdString(localFile).c_str(), true, mode)) {
        long errorCode = LogLsv2Error("LSV2ReceiveFile");
        log->Error("ReceiveFile: "
          "LSV2ReceiveFile failed !");
        return false;
      }
      return true;
    }

    bool HeidenhainLsv2::ReceiveFile(String^ distantFile, String^ localFile)
    {
      return ReceiveFile(distantFile, localFile, false);
    }

    void HeidenhainLsv2::DeleteRemoteFiles(String^ directoryPath, System::Collections::Generic::IEnumerable <String^>^ fileList, String^ fileToKeep)
    {
      if (log->IsDebugEnabled) {
        log->DebugFormat("DeleteRemoteFiles: path={0} fileToKeep={1}", directoryPath, fileToKeep);
      }
      // delete all files, but not the one as parameter
      for each (String^ fileItem in fileList)
      {
        if (!fileItem->Contains(fileToKeep)) {
          log->DebugFormat("DeleteRemoteFiles: file={0}", fileItem);
          DeleteFile(directoryPath, fileItem);
        }
      }
    }

    void HeidenhainLsv2::DeleteFile(String^ directoryPath, String^ fileName)
    {
      DIRDATA saveDir;
      log->DebugFormat("DeleteFile fileName={0}", fileName);

      // 0. Save current directory
      if (!LSV2ReceiveDirInfo(*m_hPort, &saveDir)) {
        long errorCode = LogLsv2Error("LSV2ReceiveDirInfo");
        log->Error("DeleteFile: "
          "LSV2ReceiveDirInfo failed !");
        return;
      }

      // 1. Change working directory
      if (false == LSV2ChangeDir(*m_hPort, Lemoine::Conversion::ConvertToStdString(directoryPath).c_str())) {
        long errorCode = LogLsv2Error("LSV2ChangeDir");
        log->Error("DeleteFile: "
          "LSV2ChangeDir failed !");
        return;
      }

      // 2. delete file
      if (false == LSV2DeleteFile(*m_hPort, Lemoine::Conversion::ConvertToStdString(fileName).c_str())) {
        long errorCode = LogLsv2Error("LSV2DeleteFile");
        log->Error("DeleteFile: "
          "LSV2DeleteFile failed !");
      }
      // 3. Restore current directory
      if (!LSV2ChangeDir(*m_hPort, saveDir.DirPath))
      {
        log->DebugFormat("DeleteFile: unable to restore save dir");
      }
    }

    bool compareFileWriteTime(const _finddata32_t* firstFile, const _finddata32_t* secondFile) {
      return firstFile->time_write < secondFile->time_write;
    }

    System::Collections::Generic::IList<String^>^ HeidenhainLsv2::GetFileList(String^ distantDir)
    {
      DIRDATA saveDir;
      log->DebugFormat("GetFileList distantDir={0}", distantDir);
      // 0. Must be logged in
      if (false == CheckFileConnection()) {
        log->ErrorFormat("GetFileList: "
          "connection failed");
        throw gcnew Exception("File connection failed");
      }

      // 0. Save current directory
      if (!LSV2ReceiveDirInfo(*m_hPort, &saveDir)) {
        long errorCode = LogLsv2Error("LSV2ReceiveDirInfo");
        log->Error("GetFileList: "
          "LSV2ReceiveDirInfo failed !");
        throw gcnew Exception("LSV2ReceiveDirInfo failed");
      }

      // 1. Change working directory
      if (false == LSV2ChangeDir(*m_hPort, Lemoine::Conversion::ConvertToStdString(distantDir).c_str())) {
        long errorCode = LogLsv2Error("LSV2ChangeDir");
        log->Error("GetFileList: "
          "LSV2ChangeDir failed !");
        throw gcnew Exception("LSV2ChangeDir failed");
      }

      // 2. List directory, files matching prefix
      DWORD dirSize, dirCount;
      int dirResult = LSV2ReceiveDir(*m_hPort, &dirSize, &dirCount);
      log->DebugFormat("GetFileList LSV2ReceiveDir DirCount={0}", dirCount.ToString());

      std::list<_finddata32_t*> listFiles;
      struct _finddata32_t* pFile;

      for (pFile = (_finddata32_t*)LSV2GetDirEntry(*m_hPort, LSV2_ACCESS_FIRST); pFile; pFile = (_finddata32_t*)LSV2GetDirEntry(*m_hPort, LSV2_ACCESS_NEXT))
      {

        if (std::regex_match(pFile->name, std::regex("[0-9]+(\.[0-9]*)?"))) {
          String^ fileName = Lemoine::Conversion::ConvertToManagedString(pFile->name);
          String^ fileDateTime = pFile->time_write.ToString();
          log->DebugFormat("GetFileList: filename={0} date={1}", fileName, fileDateTime);
          listFiles.push_back(pFile);
        }
        else
        {
          log->DebugFormat("GetFileList: not numeric filename={0}", Lemoine::Conversion::ConvertToManagedString(pFile->name));
        }
      }

      // sort by datetime
      listFiles.sort(compareFileWriteTime);

      System::Collections::Generic::IList<String^>^ result = gcnew List<String^>();
      log->DebugFormat("GetFileList: ------ sorted list ---------");
      for (struct _finddata32_t* fileItem : listFiles) {
        String^ fileName = Lemoine::Conversion::ConvertToManagedString(fileItem->name);
        String^ fileDateTime = fileItem->time_write.ToString();
        log->DebugFormat("GetLatestFileFromFolder: filename={0} date={1}", fileName, fileDateTime);
        result->Add(fileName);
      }
      log->DebugFormat("GetFileList: latestFileItem={0}", result);

      // Restore current directory
      if (!LSV2ChangeDir(*m_hPort, saveDir.DirPath))
      {
        log->DebugFormat("GetFileList: unable to restore save dir");
      }
      return result;
    }

    void HeidenhainLsv2::GetOverrideValues()
    {
      if (true == m_overrideValues) {
        log->InfoFormat("GetOverrideValues: "
          "the override values are already known, "
          "FeedrateOverride={0} SpindleSpeedOverride={1}",
          feedrateOverride, spindleSpeedOverride);
        return;
      }

      if (false == CheckDNCConnection()) {
        log->ErrorFormat("GetOverrideValues: "
          "connection to DNC failed");
        throw gcnew Exception("No DNC connection");
      }

      if (m_model < Model::HEID_530) {
        log->ErrorFormat("GetOverrideValues: "
          "model {0} is less than HEID_530 "
          "=> getting the override values is not supported",
          m_model);
        throw gcnew Exception("Override not supported");
      }

      log->DebugFormat("GetOverrideValues /B");
      try {
        LSV2RUNINFO runInfo;
        if (false == LSV2ReceiveRunInfo(*m_hPort, LSV2_RUNINFO_OVERRIDES_INFO, &runInfo)) {
          long errorCode = LogLsv2Error("LSV2ReceiveRunInfo");
          log->Error("GetOverrideValues: "
            "LSV2ReceiveRunInfo failed !");
          throw gcnew Exception("LSV2ReceiveRunInfo failed");
        }
        else {
          feedrateOverride = runInfo.ri.OverrideValue[0] / 100;
          spindleSpeedOverride = runInfo.ri.OverrideValue[1] / 100;
          log->DebugFormat("GetOverrideValues: "
            "got feedrateOverride={0} spindleSpeedOverride={1}",
            feedrateOverride, spindleSpeedOverride);
          m_overrideValues = true;
          return;
        }
      }
      catch (Exception^ ex) {
        log->ErrorFormat("GetOverrideValues: "
          "exception {0}",
          ex);
        Disconnect();
        throw ex;
      }
    }

    void HeidenhainLsv2::GetProgramValues()
    {
      if (true == m_programValues) {
        log->InfoFormat("GetProgramValues: "
          "the program values are already known, "
          "ProgramName={0} Block#={1}",
          m_programName, m_blockNumber);
        return;
      }

      if (false == CheckConnection()) {
        log->ErrorFormat("GetProgramValues: "
          "connection to CNC failed");
        throw gcnew Exception("No CNC connection");
      }

      if (m_model < Model::HEID_530) {
        log->ErrorFormat("GetProgramValues: "
          "model {0} is less than HEID_530 "
          "=> getting the program values is not supported",
          m_model);
        throw gcnew Exception("ProgramValues not supported");
      }

      log->DebugFormat("GetProgramValues /B");
      try {
        LSV2RUNINFO runInfo;
        if (false == LSV2ReceiveRunInfo(*m_hPort, LSV2_RUNINFO_EXECUTION_POINT, &runInfo)) {
          long errorCode = LogLsv2Error("LSV2ReceiveRunInfo");
          log->Error("GetProgramValues: "
            "LSV2ReceiveRunInfo failed !");
          throw gcnew Exception("LSV2ReceiveRunInfo failed");
        }
        else {
          char* p = runInfo.ri.ExecutionPoint.NameSelectedProgram;
          p += std::strlen(p) + 1;
          m_programName = Lemoine::Conversion::ConvertToManagedString(p);
          m_blockNumber = runInfo.ri.ExecutionPoint.BlockNr;
          log->DebugFormat("GetProgramValues: "
            "got programName={0} block#={1}",
            m_programName, m_blockNumber);
          m_programValues = true;
          return;
        }
      }
      catch (Exception^ ex) {
        log->ErrorFormat("GetProgramValues: "
          "exception {0}",
          ex);
        Disconnect();
        throw ex;
      }
    }

    void HeidenhainLsv2::GetProgramStatus()
    {
      if (true == m_programStatusOk) {
        log->InfoFormat("GetProgramStatus: "
          "the program status is already known, "
          "m_programStatus={0}",
          (int)m_programStatus);
        return;
      }

      if (false == CheckConnection()) {
        log->ErrorFormat("GetProgramStatus: "
          "connection to CNC failed");
        throw gcnew Exception("No CNC connection");
      }

      log->DebugFormat("GetProgramStatus /B");

      if (m_model < Model::HEID_530) {
        log->ErrorFormat("GetProgramStatus: "
          "model {0} is less than HEID_530 "
          "=> getting the program status is not supported",
          m_model);
        throw gcnew Exception("ProgramStatus not supported");
      }

      try {
        LSV2RUNINFO runInfo;
        if (false == LSV2ReceiveRunInfo(*m_hPort, LSV2_RUNINFO_PROGRAM_STATUS, &runInfo)) {
          long errorCode = LogLsv2Error("LSV2ReceiveRunInfo");
          log->Error("GetProgramStatus: "
            "LSV2ReceiveRunInfo failed !");
          throw gcnew Exception("LSV2ReceiveRunInfo failed");
        }
        else {
          log->DebugFormat("GetProgramStatus: "
            "status is {0}",
            runInfo.ri.ProgramStatus);
          m_programStatus = (LSV2_PROGRAM_STATUS_TYPE)runInfo.ri.ProgramStatus;
          log->DebugFormat("GetProgramStatus: "
            "got m_programStatus={0}",
            (int)m_programStatus);
          m_programStatusOk = true;
          return;
        }
      }
      catch (Exception^ ex) {
        log->ErrorFormat("GetProgramStatus: "
          "exception {0}",
          ex);
        Disconnect();
        throw ex;
      }
    }

    void HeidenhainLsv2::GetExecutionMode()
    {
      if (true == m_executionModeOk) {
        log->InfoFormat("GetExecutionMode: "
          "the execution mode is already known, "
          "m_executionMode={0}",
          (int)m_executionMode);
        return;
      }

      if (false == CheckConnection()) {
        log->ErrorFormat("GetExecutionMode: "
          "connection to CNC failed");
        throw gcnew Exception("No CNC connection");
      }

      log->DebugFormat("GetExecutionMode /B");

      if (m_model < Model::HEID_530) {
        log->ErrorFormat("GetExecutionMode: "
          "model {0} is less than HEID_530 "
          "=> getting the execution mode is not supported",
          m_model);
        throw gcnew Exception("ExecutionMode not supported");
      }

      try {
        LSV2RUNINFO runInfo;
        if (false == LSV2ReceiveRunInfo(*m_hPort, LSV2_RUNINFO_EXECUTION_MODE, &runInfo)) {
          long errorCode = LogLsv2Error("LSV2ReceiveRunInfo");
          log->Error("GetExecutionMode: "
            "LSV2ReceiveRunInfo failed !");
          throw gcnew Exception("LSV2ReceiveRunInfo failed");
        }
        else {
          log->DebugFormat("GetExecutionMode: "
            "status is {0}",
            runInfo.ri.ExecutionMode);
          m_executionMode = (LSV2_EXEC_MODE)runInfo.ri.ExecutionMode;
          log->DebugFormat("GetExecutionMode: "
            "got m_executionMode={0}",
            (int)m_executionMode);
          m_executionModeOk = true;
          return;
        }
      }
      catch (Exception^ ex) {
        log->ErrorFormat("GetExecutionMode: "
          "exception {0}",
          ex);
        Disconnect();
        throw ex;
      }
    }

    bool HeidenhainLsv2::DownloadReadTable(String^ tableName, long skipTime)
    {
      // 1. Check the date
      if ((m_downloadDateTime != nullptr)
        && ((DateTime::Now - *m_downloadDateTime).TotalSeconds < skipTime)) {
        log->Debug("DownloadReadLemoineTable: "
          "the downloaded file and its associated data is quite recent");
        return true;
      }

      // 2. Reset the associated data
      m_downloadTableValues->Clear();

      // 3. Download the file
      String^ localFile = String::Format("{0}-{1}.TAB",
        Path::GetFileNameWithoutExtension(tableName),
        this->CncAcquisitionId);
      if (false == ReceiveFile(tableName,
        localFile)) {
        log->Error("DownloadReadLemoineTable: "
          "receiving file failed");
        return false;
      }
      m_downloadDateTime = DateTime::Now;

      // 4. Read the file
      {
        StreamReader streamReader(localFile);
        bool found = false;
        while (false == streamReader.EndOfStream) {
          String^ line = streamReader.ReadLine();
          if (line->StartsWith("NR")) { // Begin of the interesting part, process it
            // Header
            array<String^>^ names =
              line->Split((array<Char>^) nullptr,
                StringSplitOptions::RemoveEmptyEntries);
            // First values
            line = streamReader.ReadLine();
            if (true == streamReader.EndOfStream) {
              break;
            }
            array<String^>^ values =
              line->Split((array<Char>^) nullptr,
                StringSplitOptions::RemoveEmptyEntries);
            // Fill downloadTableValues
            CultureInfo^ usCultureInfo = gcnew CultureInfo("en-US");
            for (int i = 0;
              (i < names->Length) && (i < values->Length);
              ++i) {
              double doubleValue;
              try {
                doubleValue = Double::Parse(values[i], usCultureInfo);
              }
              catch (Exception^ ex) {
                log->ErrorFormat("DownloadReadLemoineTable: "
                  "{0} is not a double, {1}",
                  values[i], ex);
                throw ex;
              }
              m_downloadTableValues[names[i]] = doubleValue;
            }
            found = true;
            break;
          }
        }
        if (false == found) {
          log->Error("DownloadReadLemoineTable: "
            "no values found in the table");
          m_downloadTableValues->Clear();
          return false;
        }
      }

      return true;
    }

    LSV2DATA HeidenhainLsv2::GetData(String^ entryName)
    {
      LSV2DATA data;

      log->DebugFormat("GetData /B");

      if (m_model < Model::HEID_530) {
        log->ErrorFormat("GetValue: "
          "GetData is not supported in 4xx series");
        throw gcnew Exception("Model not supported for GetData");
      }

      if (false == CheckDataConnection()) {
        log->ErrorFormat("GetData: "
          "connection to Data failed");
        throw gcnew Exception("No Data connection");
      }

      std::string entry = Conversion::ConvertToStdString(entryName);
      if (false == LSV2ReceiveDataProperty(*m_hPort, entry.c_str(), LSV2PROPKIND_DATA, (LSV2DATA*)&data)) {
        long errorCode = LogLsv2Error("LSV2ReceiveDataProperty");
        log->ErrorFormat("GetData: "
          "LSV2ReceiveDataProperty failed with entry {0}!",
          entryName);
        throw gcnew Exception("LSV2ReceiveDataProperty failed");
      }

      log->Debug("GetData: "
        "LSV2ReceiveDataProperty was successful");

      return data;
    }

    String^ HeidenhainLsv2::GetStringData(String^ entryName)
    {
      LSV2DATA data = GetData(entryName);

      log->DebugFormat("GetStringData /B "
        "entryName={0}",
        entryName);

      String^ result;
      switch (data.DataType) {
      case GVT_STRING:
        result =
          Conversion::ConvertToManagedString(data.d.DataString);
        log->DebugFormat("GetStringData: "
          "got string value {0}",
          result);
        return result;
      case GVT_I2:
      case GVT_UI2:// short
        log->WarnFormat("GetStringData: "
          "got a short integer {0} for a string",
          data.d.DataWord);
        return data.d.DataWord.ToString();
      case GVT_I4:
      case GVT_UI4:// long
        log->WarnFormat("GetStringData: "
          "got a long integer {0} for a string",
          data.d.DataLong);
        return data.d.DataLong.ToString();
      case GVT_R4:
      case GVT_R8:
        log->WarnFormat("GetStringData: "
          "got a double {0} for a string",
          data.d.DataDouble);
        return data.d.DataDouble.ToString();
      case GVT_BOOL:
        log->WarnFormat("GetStringData: "
          "got a boolean {0} for a string",
          data.d.DataWord);
        return (0 != data.d.DataWord) ? L"True" : L"False";
      default:
        log->ErrorFormat("GetStringData: "
          "data type {0} is not implemented for entryName={1}",
          (long)data.DataType, entryName);
        throw gcnew Exception("Unsupported data type");
      }
    }

    Int32 HeidenhainLsv2::GetIntData(String^ entryName)
    {
      LSV2DATA data = GetData(entryName);

      log->DebugFormat("GetIntData /B "
        "entryName={0}",
        entryName);

      CultureInfo^ usCultureInfo = gcnew CultureInfo("en-US");
      String^ result;
      switch (data.DataType) {
      case GVT_STRING:
        result =
          Conversion::ConvertToManagedString(data.d.DataString);
        log->DebugFormat("GetIntData: "
          "got string value {0} for Int32",
          result);
        return Int32::Parse(result, usCultureInfo);
      case GVT_I2:
      case GVT_UI2:// short
        log->DebugFormat("GetIntData: "
          "got a short {0}",
          data.d.DataWord);
        return (Int32)data.d.DataWord;
      case GVT_I4:
      case GVT_UI4:// long
        log->DebugFormat("GetIntData: "
          "got a long {0}",
          data.d.DataLong);
        return (Int32)data.d.DataLong;
      case GVT_R4:
      case GVT_R8:
        log->DebugFormat("GetIntData: "
          "got a double {0} for an Int32",
          data.d.DataDouble);
        return (Int32)data.d.DataDouble;
      case GVT_BOOL:
        log->ErrorFormat("GetIntData: "
          "got a bool {0} for an Int32 for entry {1}",
          data.d.DataLong,
          entryName);
        // go to the default section
      default:
        log->ErrorFormat("GetIntData: "
          "data type {0} is not implemented for entry {1}",
          (long)data.DataType,
          entryName);
        throw gcnew Exception("Unsupported data type");
      }
    }

    double HeidenhainLsv2::GetDoubleData(String^ entryName)
    {
      LSV2DATA data = GetData(entryName);

      log->DebugFormat("GetDoubleData /B "
        "entryName={0}",
        entryName);

      CultureInfo^ usCultureInfo = gcnew CultureInfo("en-US");
      String^ result;
      switch (data.DataType) {
      case GVT_STRING:
        result =
          Conversion::ConvertToManagedString(data.d.DataString);
        log->DebugFormat("GetDoubleData: "
          "got string value {0} for double",
          result);
        return double::Parse(result, usCultureInfo);
      case GVT_I2:
      case GVT_UI2:// short
        log->DebugFormat("GetDoubleData: "
          "got a short integer {0} for a double",
          data.d.DataWord);
        return (double)data.d.DataLong;
      case GVT_I4:
      case GVT_UI4:// long
        log->DebugFormat("GetDoubleData: "
          "got a long integer {0} for a double",
          data.d.DataLong);
        return (double)data.d.DataLong;
      case GVT_R4:
      case GVT_R8:
        log->DebugFormat("GetDoubleData: "
          "got double {0}",
          data.d.DataDouble);
        return data.d.DataDouble;
      default:
        log->ErrorFormat("GetDoubleData: "
          "data type {0} is not implemented for entry {1}",
          (long)data.DataType,
          entryName);
        throw gcnew Exception("Unsupported data type");
      }
    }

    bool HeidenhainLsv2::GetBoolData(String^ entryName)
    {
      LSV2DATA data = GetData(entryName);

      log->DebugFormat("GetBoolData /B "
        "entryName={0}",
        entryName);

      CultureInfo^ usCultureInfo = gcnew CultureInfo("en-US");
      String^ result;
      switch (data.DataType) {
      case GVT_BOOL:
        log->DebugFormat("GetBoolData: "
          "got a bool {0} for bool",
          data.d.DataWord);
        return (0 != (Int32)data.d.DataWord);
      case GVT_STRING:
        result =
          Conversion::ConvertToManagedString(data.d.DataString);
        log->DebugFormat("GetBoolData: "
          "got string value {0} for bool",
          result);
        return (!result->Equals("0"));
      case GVT_I2:
      case GVT_UI2:// short
        log->DebugFormat("GetBoolData: "
          "got a short {0} for bool",
          data.d.DataWord);
        return (0 != (Int32)data.d.DataWord);
      case GVT_I4:
      case GVT_UI4:// long
        log->DebugFormat("GetBoolData: "
          "got a long {0} for bool",
          data.d.DataLong);
        return (0 != (Int32)data.d.DataLong);
      case GVT_R4:
      case GVT_R8:
        log->DebugFormat("GetBoolData: "
          "got a double {0} for an bool",
          data.d.DataDouble);
        return (0 != (Int32)data.d.DataDouble);
      default:
        log->ErrorFormat("GetBoolData: "
          "data type {0} is not implemented for entry {1}",
          (long)data.DataType,
          entryName);
        throw gcnew Exception("Unsupported data type");
      }
    }

    Int32 HeidenhainLsv2::GetDataWithMultiplier(String^ entryName)
    {
      double rawResult = GetDoubleData(entryName);
      Int32 result = (Int32)Math::Round(rawResult * m_multiplier);
      log->DebugFormat("GetDataWithMultiplier: "
        "got data {0} for {1} "
        "from {1} with multiplier {2}",
        result, entryName,
        rawResult, m_multiplier);
      return result;
    }

    String^ HeidenhainLsv2::GetParameter(String^ entryName)
    {
      log->DebugFormat("GetParameter /B entryName={0}",
        entryName);

      if (m_model < Model::HEID_530) {
        log->ErrorFormat("GetParameter: "
          "GetParameter is not supported in 4xx series");
        throw gcnew Exception("Model not supported for GetParameter");
      }

      if (false == CheckDataConnection()) {
        log->ErrorFormat("GetParameter: "
          "connection to Data failed");
        throw gcnew Exception("No Data connection");
      }

      std::string entry = Conversion::ConvertToStdString(entryName);
      const int bufferSize = 256;
      char buffer[bufferSize + 1];
      if (false == LSV2ReceiveMachineConstant(*m_hPort, entry.c_str(), buffer, bufferSize)) {
        long errorCode = LogLsv2Error("LSV2ReceiveDataProperty");
        log->ErrorFormat("GetParameter: "
          "LSV2ReceiveMachineConstant failed with entry {0}!",
          entryName);
        throw gcnew Exception("LSV2ReceiveMachineConstant failed");
      }

      String^ result = Conversion::ConvertToManagedString(buffer);
      log->DebugFormat("GetParameter: "
        "got {0}={1}",
        entryName, result);
      return result;
    }

    String^ HeidenhainLsv2::GetStringParameter(String^ entryName)
    {
      String^ v;
      if (m_parameterCache->TryGetValue(entryName, v)) {
        log->DebugFormat("GetStringParameter: "
          "get {0}={1} from cache",
          entryName, v);
        return v;
      }

      v = GetParameter(entryName);
      m_parameterCache[entryName] = v;
      return v;
    }

    Int32 HeidenhainLsv2::GetIntParameter(String^ entryName)
    {
      CultureInfo^ usCultureInfo = gcnew CultureInfo("en-US");
      return Int32::Parse(GetStringParameter(entryName), usCultureInfo);
    }

    double HeidenhainLsv2::GetDoubleParameter(String^ entryName)
    {
      CultureInfo^ usCultureInfo = gcnew CultureInfo("en-US");
      return double::Parse(GetStringParameter(entryName), usCultureInfo);
    }

    String^ HeidenhainLsv2::GetTableLine(String^ tableName, String^ condition)
    {
      log->DebugFormat("GetTableLine tableName={0} condition={1} /B",
        tableName, condition);

      if (false == CheckConnection()) {
        log->ErrorFormat("GetTableLine: "
          "connection failed");
        throw gcnew Exception("No connection");
      }

      if (String::Compare(this->version, "280476 20") < 0) {
        log->ErrorFormat("GetTableLine: "
          "version {0} is less than {1} "
          "=> LSV2ReceiveTableLine is not supported and make the control crash",
          version);
        throw gcnew Exception("LSV2ReceiveTable not supported on this control");
      }

      std::string tableName2 = Conversion::ConvertToStdString(tableName);
      std::string condition2 = Conversion::ConvertToStdString(condition);
      char rawResult[TABLE_LINE_SIZE];
      std::memset(rawResult, 0, TABLE_LINE_SIZE);
      if (false == LSV2ReceiveTableLine(*m_hPort,
        tableName2.c_str(),
        condition2.c_str(),
        rawResult,
        TABLE_LINE_SIZE,
        0)) {
        long errorCode = LogLsv2Error("LSV2ReceiveTableLineEx");
        log->ErrorFormat("GetTableLine: "
          "LSV2ReceiveTableLineEx failed with entry {0} {1}!",
          tableName, condition);
        throw gcnew Exception("LSV2ReceiveTableLineEx failed");
      }

      String^ result =
        Conversion::ConvertToManagedString(rawResult);
      log->DebugFormat("GetTableLine: "
        "LSV2ReceiveTableLineEx was successful and returned {0}",
        result);
      return result;
    }

    String^ HeidenhainLsv2::GetTableLineValue(String^ tableName, String^ condition, int column)
    {
      String^ line = GetTableLine(tableName, condition);
      array<String^>^ values =
        line->Split((array<Char>^) nullptr,
          StringSplitOptions::RemoveEmptyEntries);

      log->DebugFormat("GetTableLineValue: "
        "get value {0} from table {1} condition {2}, column {3}",
        values[column], tableName, condition, column);

      return values[column];
    }

    String^ HeidenhainLsv2::GetTableLineValue(String^ parameters)
    {
      if ((parameters == nullptr) || (parameters->Equals(""))) {
        log->ErrorFormat("GetTableLineValue: "
          "invalid value, no parameters");
        throw gcnew ArgumentException("empty parameters argument");
      }

      array<String^>^ params = parameters->Split(gcnew array<Char> {',', ';'});
      if (params->Length < 2) {
        log->ErrorFormat("GetTableLineValue: "
          "invalid number of parameters in parameters argument");
        throw gcnew ArgumentException("not enough parameters in parameters argument");
      }
      int column = 22;
      if (params->Length >= 3) {
        try {
          column = Int32::Parse(params[2]);
        }
        catch (Exception^ ex) {
          log->ErrorFormat("GetTableLineValue: "
            "invalid column parameter {0}",
            params[2]);
          log->ErrorFormat("GetTableLineValue: "
            "invalid column parameter", ex);
          throw gcnew ArgumentException("Invalid column parameter", ex);
        }
      }
      return GetTableLineValue(params[0], params[1], column);
    }

    double HeidenhainLsv2::GetTableLineDoubleValue(String^ tableName, String^ condition, int column)
    {
      CultureInfo^ usCultureInfo = gcnew CultureInfo("en-US");
      return double::Parse(GetTableLineValue(tableName, condition, column),
        usCultureInfo);
    }

    double HeidenhainLsv2::GetTableLineDoubleValue(String^ parameters)
    {
      if ((parameters == nullptr) || (parameters->Equals(""))) {
        log->ErrorFormat("GetTableLineDoubleValue: "
          "invalid value, no parameters");
        throw gcnew ArgumentException("empty parameters argument");
      }

      array<String^>^ params = parameters->Split(gcnew array<Char> {',', ';'});
      if (params->Length < 2) {
        log->ErrorFormat("GetTableLineDoubleValue: "
          "invalid number of parameters in parameters argument");
        throw gcnew ArgumentException("not enough parameters in parameters argument");
      }
      int column = 22;
      if (params->Length >= 3) {
        try {
          column = Int32::Parse(params[2]);
        }
        catch (Exception^ ex) {
          log->ErrorFormat("GetTableLineDoubleValue: "
            "invalid column parameter {0}",
            params[2]);
          log->Error("GetTableLineDoubleValue: "
            "invalid column parameter", ex);
          throw gcnew ArgumentException("Invalid column parameter", ex);
        }
      }
      return GetTableLineDoubleValue(params[0], params[1], column);
    }

    Int32 HeidenhainLsv2::GetTableLineValueWithMultiplier(String^ tableName, String^ condition, int column)
    {
      double rawResult = GetTableLineDoubleValue(tableName, condition, column);
      Int32 result = (Int32)Math::Round(rawResult * m_multiplier);
      log->DebugFormat("GetTableLineWithMultiplier: "
        "got data {0} for {1};{2};{3} "
        "from {4} with multiplier {5}",
        result, tableName, condition, column,
        rawResult, m_multiplier);
      return result;
    }

    Int32 HeidenhainLsv2::GetTableLineValueWithMultiplier(String^ parameters)
    {
      if ((parameters == nullptr) || (parameters->Equals(""))) {
        log->ErrorFormat("GetTableLineValueWithMultiplier: "
          "invalid value, no parameters");
        throw gcnew ArgumentException("empty parameters argument");
      }

      array<String^>^ params = parameters->Split(gcnew array<Char> {',', ';'});
      if (params->Length < 2) {
        log->ErrorFormat("GetTableLineValueWithMultiplier: "
          "invalid number of parameters in parameters argument");
        throw gcnew ArgumentException("not enough parameters in parameters argument");
      }
      int column = 22;
      if (params->Length >= 3) {
        try {
          column = Int32::Parse(params[2]);
        }
        catch (Exception^ ex) {
          log->ErrorFormat("GetTableLineValueWithMultiplier: "
            "invalid column parameter {0}, {1}",
            params[2], ex);
          throw gcnew ArgumentException("Invalid column parameter");
        }
      }
      return GetTableLineValueWithMultiplier(params[0], params[1], column);
    }

    double HeidenhainLsv2::GetTableLineNotNullValue(String^ tableName, String^ condition)
    {
      CultureInfo^ usCultureInfo = gcnew CultureInfo("en-US");
      String^ line = GetTableLine(tableName, condition);
      array<String^>^ values =
        line->Split((array<Char>^) nullptr,
          StringSplitOptions::RemoveEmptyEntries);

      log->DebugFormat("GetTableLineNotNullValue tableName={0} condition={1}: "
        "analyze line {2}",
        tableName, condition,
        line);

      bool skip = true; // To skip the first value
      for each (String ^ v in values) {
        if (true == skip) {
          skip = false;
          continue;
        }
        double doubleValue;
        if (double::TryParse(v,
          static_cast<NumberStyles>(NumberStyles::Float),
          usCultureInfo,
          doubleValue) && (doubleValue != 0.0)) {
          log->DebugFormat("GetTableLineNotNullValue: "
            "got positive value {1}",
            doubleValue);
          return doubleValue;
        }
      }

      log->DebugFormat("GetTableLineNotNullValue tableName={0} condition={1}: "
        "no not null value found in line {2}, "
        "return 0.0",
        tableName, condition,
        line);
      return 0.0;
    }

    double HeidenhainLsv2::GetTableLineNotNullValue(String^ parameters)
    {
      if ((parameters == nullptr) || (parameters->Equals(""))) {
        log->ErrorFormat("GetTableLineNotNullValue: "
          "invalid value, no parameters");
        throw gcnew ArgumentException("empty parameters argument");
      }

      array<String^>^ params = parameters->Split(gcnew array<Char> {',', ';'});
      if (params->Length < 2) {
        log->ErrorFormat("GetTableLineNotNullValue: "
          "invalid number of parameters in parameters argument");
        throw gcnew ArgumentException("not enough parameters in parameters argument");
      }
      return GetTableLineNotNullValue(params[0], params[1]);
    }

    Int32 HeidenhainLsv2::GetTableLineNotNullValueWithMultiplier(String^ tableName, String^ condition)
    {
      double rawResult = GetTableLineNotNullValue(tableName, condition);
      Int32 result = (Int32)Math::Round(rawResult * m_multiplier);
      log->DebugFormat("GetTableLineNotNullValueWithMultiplier: "
        "got data {0} for {1};{2} "
        "from {3} with multiplier {4}",
        result, tableName, condition,
        rawResult, m_multiplier);
      return result;
    }

    Int32 HeidenhainLsv2::GetTableLineNotNullValueWithMultiplier(String^ parameters)
    {
      if ((parameters == nullptr) || (parameters->Equals(""))) {
        log->ErrorFormat("GetTableLineNotNullValueWithMultiplier: "
          "invalid value, no parameters");
        throw gcnew ArgumentException("empty parameters argument");
      }

      array<String^>^ params = parameters->Split(gcnew array<Char> {',', ';'});
      if (params->Length < 2) {
        log->ErrorFormat("GetTableLineValueNotNullWithMultiplier: "
          "invalid number of parameters in parameters argument");
        throw gcnew ArgumentException("not enough parameters in parameters argument");
      }
      return GetTableLineNotNullValueWithMultiplier(params[0], params[1]);
    }


    // Tool data management
    Lemoine::Cnc::ToolLifeData^ HeidenhainLsv2::ReadToolLifeData()
    {
      if (m_toolAvailableVariables->Count > 0 && m_toolMissingVariables->Count > 0) {
        String^ missing = "";
        for (int i = 0; i < m_toolMissingVariables->Count; i++)
          missing += " " + m_toolMissingVariables[i];
        String^ available = "";
        for (int i = 0; i < m_toolAvailableVariables->Count; i++)
          available += " " + m_toolAvailableVariables[i];
        String^ msg = String::Format("HeidenhainDNC: missing variable(s) {0} for reading tool life data. Available variables are {1}.",
          missing, available);
        log->Error(msg);
        throw gcnew Exception(msg);
      }
      Lemoine::Cnc::ToolLifeData^ tld = gcnew Lemoine::Cnc::ToolLifeData();

      try {
        int toolLinesCount = GetToolNumber();
        CultureInfo^ usCultureInfo = gcnew CultureInfo("en-US");
        int toolNumber = 0;
        List<String^>^ variableList = gcnew List<String^>();
        for (int i = 0; i < toolLinesCount; i++)
        {
          // Browse all data from the tool
          String^ line = GetTableLine("\\TABLE\\TOOL\\T", GetToolCondition(i));
          array<String^>^ values =
            line->Split((array<Char>^) nullptr,
              StringSplitOptions::RemoveEmptyEntries);

          ToolData^ toolDataTmp = gcnew Lemoine::Cnc::ToolData();
          int indexAttribute = GetToolAttributeColumn("T");
          if (indexAttribute != -1) {
            toolDataTmp->SetNumber(int::Parse(values[indexAttribute]));
          }
          indexAttribute = GetToolAttributeColumn("NAME");
          if (indexAttribute != -1) {
            toolDataTmp->SetName(values[indexAttribute]);
          }
          indexAttribute = GetToolAttributeColumn("L");
          if (indexAttribute != -1) {
            toolDataTmp->m_compensationH = double::Parse(values[indexAttribute], usCultureInfo);
          }
          indexAttribute = GetToolAttributeColumn("R");
          if (indexAttribute != -1) {
            toolDataTmp->m_compensationD = double::Parse(values[indexAttribute], usCultureInfo);
          }
          indexAttribute = GetToolAttributeColumn("TIME1");
          if (indexAttribute != -1) {
            toolDataTmp->m_limit = double::Parse(values[indexAttribute], usCultureInfo);
          }
          indexAttribute = GetToolAttributeColumn("TIME2");
          if (indexAttribute != -1) {
            toolDataTmp->m_warning = double::Parse(values[indexAttribute], usCultureInfo);
          }
          indexAttribute = GetToolAttributeColumn("CUR_TIME");
          if (indexAttribute != -1) {
            toolDataTmp->SetCurrent(double::Parse(values[indexAttribute], usCultureInfo));
          }

          // Valid tool?
          if (toolDataTmp->IsValid()) {
            tld->AddTool();
            Lemoine::Cnc::ToolLifeData::ToolLifeDataItem^ tldi = tld[toolNumber];
            tldi->PotNumber = i + 1;
            tldi->ToolId = toolDataTmp->GetName();
            tldi->ToolNumber = toolDataTmp->GetNumber().ToString();
            tldi->SetProperty("CutterCompensation", toolDataTmp->m_compensationD);
            tldi->SetProperty("LengthCompensation", toolDataTmp->m_compensationH);
            tldi->ToolState = Lemoine::Core::SharedData::ToolState::Available;

            tldi->AddLifeDescription();
            tldi[0]->LifeValue = toolDataTmp->GetCurrent() * 60; // Convert to seconds
            tldi[0]->LifeDirection = Lemoine::Core::SharedData::ToolLifeDirection::Up;
            if (toolDataTmp->m_limit.HasValue) {
              tldi[0]->LifeLimit = toolDataTmp->m_limit.Value * 60;
              if (toolDataTmp->m_warning.HasValue) {
                tldi[0]->LifeWarningOffset = toolDataTmp->m_limit.Value * 60 -
                  toolDataTmp->m_warning.Value * 60;
              }
            }

            tldi[0]->LifeType = Lemoine::Core::SharedData::ToolUnit::TimeSeconds;
            toolNumber++;
          }
        }
      }
      catch (Exception^ ex) {
        log->Error("GetToolLifeData: error", ex);
        throw gcnew ArgumentException("Invalid column parameter", ex);
      }
      return tld;
    }

    int HeidenhainLsv2::GetToolNumber()
    {
      // TODO: get the number of tools
      return 0;
    }

    String^ HeidenhainLsv2::GetToolCondition(int toolNumber)
    {
      // TODO: get the condition for a specific tool
      return "";
    }

    int HeidenhainLsv2::GetToolAttributeColumn(String^ attribute)
    {
      // TODO: get the number of the column corresponding to an attribute
      return 0;
    }
    // End of tool data management

    // Alarm management
    List<CncAlarm^>^ HeidenhainLsv2::Alarms::get()
    {
      if (!CheckConnection()) {
        log->ErrorFormat("Alarms::get: connection to CNC failed");
        throw gcnew Exception("No CNC connection");
      }

      log->DebugFormat("Alarms::get /B");
      List<CncAlarm^>^ list = gcnew List<CncAlarm^>();
      try {
        LSV2RUNINFO runInfo;
        bool result = (LSV2ReceiveRunInfo(*m_hPort, LSV2_RUNINFO_FIRST_ERROR, &runInfo) != 0);
        while (result && runInfo.ri.ErrorInfo.ErrorGroup != LSV2_EG_NONE) {
          // Store a new alarm
          CncAlarm^ alarm = GetAlarm(&runInfo);
          list->Add(alarm);
          log->InfoFormat("Alarms::get: received the alarm {0}: {1}", alarm->Number, alarm->Message);

          // Get the next alarm
          result = (LSV2ReceiveRunInfo(*m_hPort, LSV2_RUNINFO_NEXT_ERROR, &runInfo) != 0);
        }

        if (!result)
          log->InfoFormat("Alarms::get - LSV2ReceiveRunInfo returned false, number of alarms: {0}", list->Count);
      }
      catch (Exception^ ex) {
        log->Error("Alarms::get: exception", ex);
        Disconnect();
        throw ex;
      }

      log->InfoFormat("Alarms::get: received {0} alarm(s)", list->Count);

      return list;
    }

    CncAlarm^ HeidenhainLsv2::GetAlarm(LSV2RUNINFO* runInfo)
    {
      // Number, message
      unsigned long errorNumber = runInfo->ri.ErrorInfo.ErrorNumber;
      String^ errorMessage = ConvertToManagedString(runInfo->ri.ErrorInfo.ErrorText);

      // Class
      String^ errorClass = "unknown";
      switch (runInfo->ri.ErrorInfo.ErrorClass)
      {
      case LSV2_EC_NONE:
        errorClass = "none";
        break;
      case LSV2_EC_WARNING:
        errorClass = "warning, no stop";
        break;
      case LSV2_EC_FEEDHOLD:
        errorClass = "error with feed hold";
        break;
      case LSV2_EC_PROGRAMHOLD:
        errorClass = "error with program hold";
        break;
      case LSV2_EC_PROGRAMABORT:
        errorClass = "error with program abort";
        break;
      case LSV2_EC_EMERGENCYSTOP:
        errorClass = "error with emergency stop";
        break;
      case LSV2_EC_RESET:
        errorClass = "error with emergency stop & control reset";
        break;
      }

      // Group
      String^ errorGroup = "unknown";
      switch (runInfo->ri.ErrorInfo.ErrorGroup)
      {
      case LSV2_EG_NONE:
        errorGroup = "none";
        break;
      case LSV2_EG_OPERATING:
        errorGroup = "operating error";
        break;
      case LSV2_EG_PROGRAMMING:
        errorGroup = "programming error";
        break;
      case LSV2_EG_PLC:
        errorGroup = "PLC error";
        break;
      case LSV2_EG_GENERAL:
        errorGroup = "general error";
        break;
      }

      // Convert the alarm number if the error group is "PLC" (otherwise negative values)
      if (runInfo->ri.ErrorInfo.ErrorGroup == LSV2_EG_PLC)
        errorNumber &= ~(0x81000000);

      // Create the alarm
      CncAlarm^ alarm = gcnew CncAlarm("HeidenhainLSV2", errorGroup, errorNumber.ToString());
      alarm->Message = errorMessage;
      alarm->Properties->Add("severity", errorClass);

      return alarm;
    }
  }
}
