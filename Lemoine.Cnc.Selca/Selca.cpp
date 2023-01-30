// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

#include <vcclr.h>

#include "Selca.h"

#include "StringConversion.h"
#include "NumberConversion.h"

#include "SeLGeCStruct.h"

#undef GetCurrentDirectory

using namespace System::Globalization;
using namespace System::IO;
using namespace System::Threading;
using namespace Lemoine::Conversion;

#define DLL_NAME "SeLGeC"
#define CONNECTION_INIT_SLEEP_MS (2000)
#define NB_CONNECTION_ATTEMPT (5)
#define RECONNECT_SLEEP_MS (1000)
#define CONNECTION_ERROR_SLEEP_MS (30000)

namespace Lemoine
{
  namespace Cnc
  {
    // By default, do not use the messages
    // because this is not supported in not graphical
    // interfaces
    Selca::Selca ()
      : Lemoine::Cnc::BaseCncModule ("Lemoine.Cnc.In.Selca")
      , m_SeLGeCLibrary (NULL)
      , m_parametersInitialization (true)
      , m_parameters (gcnew List<String^> ())
      , m_values (gcnew Dictionary<String^,String^> ())
      , m_sloDBInitialized (false)
      , m_sloDBSize (0)
      , m_cookie (0)
      , m_connected (false)
      , m_ready (false)
      , m_useMessages (false)
      , m_processTcpMsgCallback (nullptr)
      , m_pendingMessages (0)
      , m_disconnectRequested (false)
      , m_lastConnectionErrorDateTime (DateTime::MinValue)
      , m_errorRead (false)
      , m_errorWrite (false)
      , m_errorWdog (false)
      , m_errorConnection (false)
    {
      if (m_useMessages) {
        m_processTcpMsgCallback = gcnew ProcessTcpMsgCallback (this, &Lemoine::Cnc::Selca::ProcessTcpMsg);
        m_processTcpMsgGCHandle = GCHandle::Alloc (m_processTcpMsgCallback);
      }
    }

    Selca::Selca (bool useMessages)
      : Lemoine::Cnc::BaseCncModule (Selca::typeid->FullName)
      , m_SeLGeCLibrary (NULL)
      , m_parametersInitialization (true)
      , m_parameters (gcnew List<String^> ())
      , m_values (gcnew Dictionary<String^,String^> ())
      , m_sloDBInitialized (false)
      , m_sloDBSize (0)
      , m_cookie (0)
      , m_connected (false)
      , m_ready (false)
      , m_useMessages (useMessages)
      , m_processTcpMsgCallback (nullptr)
      , m_pendingMessages (0)
      , m_disconnectRequested (false)
      , m_lastConnectionErrorDateTime (DateTime::MinValue)
      , m_errorRead (false)
      , m_errorWrite (false)
      , m_errorWdog (false)
      , m_errorConnection (false)
    {
      if (m_useMessages) {
        m_processTcpMsgCallback = gcnew ProcessTcpMsgCallback (this, &Lemoine::Cnc::Selca::ProcessTcpMsg);
        m_processTcpMsgGCHandle = GCHandle::Alloc (m_processTcpMsgCallback);
      }
    }

    Selca::!Selca ()
    {
      DisconnectAndFreeLibrary ();
      if (m_useMessages) {
        m_processTcpMsgGCHandle.Free ();
      }
    }

    void Selca::LoadSeLGeCLibrary ()
    {
      DirectoryInfo^ assemblyDirectory =
        Directory::GetParent (Lemoine::Info::AssemblyInfo::AbsolutePath);
      Directory::SetCurrentDirectory (assemblyDirectory->FullName);
      String^ currentDirectory = Directory::GetCurrentDirectory ();
      log->InfoFormat ("Selca: "
                       "Current directory is {0}",
                       currentDirectory);
      String^ dllName = String::Format ("{0}.dll",
                                        DLL_NAME);
      pin_ptr<const wchar_t> dllName2 = PtrToStringChars (dllName);
      m_SeLGeCLibrary = LoadLibraryW (dllName2);
      if (NULL == m_SeLGeCLibrary) {
        log->FatalFormat ("LoadSeLGeCLibrary: "
                          "Failed to load dll {0} !",
                          dllName);
        throw gcnew Exception ("Could not load " + dllName);
      }
      // Initialize the SeLGeC function pointers
#undef API
#define API(RTYPE, METHOD, ARGS) \
      { \
      if ( (METHOD = (RTYPE (__stdcall *) ARGS)GetProcAddress (m_SeLGeCLibrary, #METHOD)) == NULL) { \
      log->FatalFormat ("LoadSeLGeCLibrary: GetProcAddress of method {0} failed", #METHOD);\
      throw gcnew Exception ("GetProcAddress failed");\
      } \
      }
#include "SeLGeCApi.h"
    }

    bool Selca::CheckConnection ()
    {
      if (true == m_disconnectRequested) {
        log->InfoFormat ("CheckConnection: "
                         "disconnecting is requested");
        Disconnect (false);
      }

      if (m_connected) {
        System::Diagnostics::Debug::Assert (0 != m_cookie);
        if (!SOLisConnected (m_cookie)) {
          log->Error ("CheckConnection: "
                      "SOLisConnected returned false "
                      "=> disconnect");
          Disconnect (false);
        }
      }

      if (false == this->m_connected) {
        log->InfoFormat ("CheckConnection: "
          "the CNC is not connected: try to connect");

        // . Check the connection parameters
        if (this->m_ipAddress == nullptr) {
          log->Error ("CheckConnection: "
            "no IP Address is given");
          return false;
        }

        // . If the last attempt is too recent, postpone the connection
        if (DateTime::UtcNow.Subtract (m_lastConnectionErrorDateTime)
            < TimeSpan::FromMilliseconds (CONNECTION_ERROR_SLEEP_MS)) {
          log->WarnFormat ("CheckConnection: "
                           "the last connection attempt at {0} is too recent "
                           "=> postpone the connection",
                           m_lastConnectionErrorDateTime);
          return false;
        }

        // . Check the LSV2 library is loaded, else load it
        if (NULL == m_SeLGeCLibrary) {
          LoadSeLGeCLibrary ();
        }

        // . SOLCreate
        if (0 != m_cookie) {
          log->InfoFormat ("CheckConnection: "
                           "a cookie {0} already exists, "
                           "use it",
                           m_cookie);
        }
        else {
          m_cookie = SOLCreate ();
          log->DebugFormat ("CheckConnection: "
                            "cookie is {0}",
                            m_cookie);
        }
        if (0 == m_cookie) {
          log->ErrorFormat ("CheckConnection: "
                            "the returned cookie is 0, give up");
          return false;
        }

        // . SOLSetConnectionMode
        SOLSetConnectionMode (m_cookie, NULL, NULL);
        if (m_useMessages) {
          IntPtr callbackPointer = Marshal::GetFunctionPointerForDelegate (m_processTcpMsgCallback);
          SLPROC_PROCESSTCPMSG callback =
            static_cast<SLPROC_PROCESSTCPMSG>(callbackPointer.ToPointer());
          SOLSetConnectionMode (m_cookie, NULL, (LONG)callback);
        }

        // . SOLGoodTcpAddr
        System::Diagnostics::Debug::Assert (this->m_ipAddress != nullptr);
        std::string ipAddress = ConvertToStdString (this->m_ipAddress);
        if (false == SOLGoodTcpAddr (m_cookie, ipAddress.c_str ())) {
          log->ErrorFormat ("CheckConnection: "
                            "IP address is not valid");
          return false;
        }

        // . SOLTryConnection
        for (int attempt = 0; ; ++attempt) {
          short tryConnectionResult = SOLTryConnection (m_cookie, ipAddress.c_str ());
          // Warning: the code that is returned by SOLTryConnection is not reliable
          if (0 == tryConnectionResult) {
            log->DebugFormat ("CheckConnection: "
                              "SOLTryConnection is ok with IP Address {0}",
                              this->m_ipAddress);
            break;
          }
          else if (ERROR_CLASS_ALREADY_EXISTS == tryConnectionResult) {
            log->WarnFormat ("CheckConnection: "
                             "SOLTryConnection return ERROR_CLASS_ALREADY_EXISTS with IP Address {0} "
                             "but give it a chance to work",
                             this->m_ipAddress);
            // Unreliable returned code: run SOLisConnected after a few milliseconds
            System::Threading::Thread::Sleep (CONNECTION_INIT_SLEEP_MS);
            if (SOLisConnected (m_cookie)) {
              log->DebugFormat ("CheckConnection: "
                                "connected after ERROR_CLASS_ALREADY_EXISTS, great");
              SetActive ();
            }
            else {
              log->ErrorFormat ("CheckConnection: "
                                "not connected after ERROR_CLASS_ALREADY_EXISTS, "
                                "sleep infintely until the parent thread/process kills it");
              DisconnectAndFreeLibrary ();
              System::Threading::Thread::Sleep (System::Threading::Timeout::Infinite);
              return false;
            }
            break;
          }
          else {
            log->WarnFormat ("CheckConnection: "
                             "SOLTryConnection returned {1} with IP address {0} "
                             "=> disconnect",
                             this->m_ipAddress, tryConnectionResult);
            SOLCloseConnection (m_cookie);
            if (attempt < NB_CONNECTION_ATTEMPT) {
              log->DebugFormat ("CheckConnection: "
                                "attempt is {0} => try again "
                                "after {1}ms",
                                attempt, RECONNECT_SLEEP_MS);
              SetActive ();
              System::Threading::Thread::Sleep (RECONNECT_SLEEP_MS);
              SetActive ();
              continue;
            }
            else {
              log->DebugFormat ("CheckConnection: "
                                "the maximum number of attempts is reached, "
                                "give up, disconnect and re-connect after {0}ms",
                                CONNECTION_ERROR_SLEEP_MS);
              Disconnect (false);
              SetActive ();
              m_lastConnectionErrorDateTime = DateTime::UtcNow;
              return false;
            }
          }
        }

        // . SOLSendPPInfo
        SOLSendPPInfo (m_cookie, m_sloDB, m_sloDBSize);

        this->m_disconnectRequested = false;
        this->m_connected = true;
      }

      return true;
    }

    void Selca::Disconnect (bool deleteConnection)
    {
      log->Debug ("Disconnect /B");

      if (0 == m_cookie) {
        log->DebugFormat ("Disconnect: "
                          "no existing connection (no cookie)");
        return;
      }

      if (m_connected) {
        SOLSetConnectionMode (m_cookie, NULL, NULL);
        SOLCloseConnection (m_cookie);
        m_connected = false;
        m_ready = false;
      }

      if (deleteConnection) {
        log->DebugFormat ("Disconnect: "
                          "delete the connection {0}",
                          m_cookie);
        SOLDelete (m_cookie);
        this->m_cookie = 0;
      }

      m_values->Clear ();
    }

    void Selca::DisconnectAndFreeLibrary ()
    {
      Disconnect (true);
      if (!FreeLibrary (m_SeLGeCLibrary)) {
        int errorCode = GetLastError ();
        log->ErrorFormat ("DisconnectAndFreeLibrary: "
                          "FreeLibrary failed with error {0}",
                          errorCode);
        if (!UnmapViewOfFile (m_SeLGeCLibrary)) {
          int unmapErrorCode = GetLastError ();
          log->ErrorFormat ("DisconnectAndFreeLibrary: "
                            "UnmapViewOfFile failed with error {0}",
                            unmapErrorCode);
        }
      }
      m_SeLGeCLibrary = NULL;      
    }

    bool Selca::Start ()
    {
      SetActive ();

      m_errorRead = false;
      m_errorWrite = false;
      m_errorWdog = false;
      m_errorConnection = false;

      if (m_parametersInitialization) {
        return true;
      }

      try {
        if (!m_sloDBInitialized) {
          m_sloDB = new Slo_t [m_parameters->Count];
          for (int i = 0; i < m_parameters->Count; ) {
            try {
              ParseParameter (m_sloDB[i], m_parameters [i]);
            }
            catch (Exception^) {
              log->ErrorFormat ("Start: "
                                "invalid parameter {0}",
                                m_parameters [i]);
              m_parameters->RemoveAt (i);
              continue;
            }
            ++i;
          }
          m_sloDBSize = m_parameters->Count;
        
          m_sloDBInitialized = true;
        }
      
        if (false == CheckConnection ()) {
          log->ErrorFormat ("Start: "
                            "CheckConnection failed");
          m_errorConnection = true;
          return true;
        }
        
        if (m_useMessages
            && (0 == Interlocked::CompareExchange (m_pendingMessages, 0, 1))) {
          log->DebugFormat ("Start: "
                            "no pending message, do nothing");
          return true;
        }
      
        // At least one message to process:
        // read the messages
        do {
          if (false == CheckConnection ()) {
            log->ErrorFormat ("Start: "
                              "CheckConnection failed "
                              "while the messages were being processed");
            // Because it was interrupted before all the messages were processed
            // set m_pendingMessages to 1
            m_pendingMessages = 1;
            m_errorConnection = true;
            // Return true because some messages may have been already processed
            return true;
          }

          SLMsg_t *message;
          SOLGetMessage (m_cookie, &message);
          if (-1 == message->sloid) { // For the terminal, skip it
            log->DebugFormat ("Start: "
                              "skip the data with sloid -1");
          }
          else if (-2 == message->sloid) { // To SOI
            if (0 == strcmp(message->dd.datas, (const char*)"BUSY")) {
              // SKM is busy and cannot accept any connection any more
              // => disconnect
              log->WarnFormat ("Start: "
                               "BUSY message => disconnect");
              Disconnect (false);
            }
            else if (0 == strcmp(message->dd.datas, (const char*)"IDENTIFY")) {
              log->DebugFormat ("Start: "
                                "IDENTIFY message");
              SLMsg_t buffer;
              buffer.dest = DEV_SKM_USER;
              strcpy_s (buffer.dd.datas, (const char*)""); // Not implemented yet
              buffer.sloid = 0;
              SOLSendMessage (m_cookie, &buffer, 1);
            }
            else if (0 == strcmp(message->dd.datas, (const char*)"READY")) {
              log->WarnFormat ("Start: "
                               "READY message should not be received here");
              m_ready = true;
            }
            else if (0 == strcmp(message->dd.datas, (const char*)"PLCREADY")) {
              log->DebugFormat ("Start: "
                                "PLCREADY message");
              m_ready = true;
            }
            else if (0 == strcmp(message->dd.datas, (const char*)"KILLED")) {
              // communication was killed by an extern process
              // => disconnect
              log->WarnFormat ("Start: "
                               "KILLED message => disconnect");
              Disconnect (false);
            }
            else {
              log->ErrorFormat ("Start: "
                                "Invalid message {0} with sloid=-2",
                                ConvertToManagedString (message->dd.datas));
            }
          }
          else { // sloid != -2 && sloid != -1
            m_ready = true;
            String^ v = ConvertToManagedString (message->dd.datas);
            switch (message->dest) {
            case DEV_SOI:
              {
                System::Diagnostics::Debug::Assert (message->sloid < m_parameters->Count);
                String^ parameter = m_parameters [message->sloid];
                log->DebugFormat ("Start: "
                                  "received {0} for sloid {1} "
                                  "parameter {2}",
                                  v, message->sloid, parameter);
                m_values [parameter] = v;
              }
              break;
            case DEV_SOI_ERR:
              {
                log->DebugFormat ("Start: "
                                  "got error code {0}",
                                  v);
                Int16 error = 0;
                if (Int16::TryParse (v, error)) {
                  bool processed = false;
                  if (error & (int)ERR_READ) {
                    log->ErrorFormat ("Start: "
                                      "got a READ error");
                    m_errorRead = true;
                    processed = true;
                  }
                  if (error & (int)ERR_WRITE) {
                    log->ErrorFormat ("Start: "
                                      "got a WRITE error");
                    m_errorWrite = true;
                    processed = true;
                  }
                  if (error & (int)ERR_WDOG) {
                    log->ErrorFormat ("Start: "
                                      "got a WDOG error");
                    m_errorWdog = true;
                    processed = true;
                  }
                  if (!processed) {
                    log->ErrorFormat ("Start: "
                                      "unknown error code {0}",
                                      v);
                  }
                }
                else {
                  log->ErrorFormat ("Start: "
                                    "invalid error code {0}",
                                    v);
                }
              }
              break;
            default:
              log->ErrorFormat ("Start: "
                                "unknown dest value {0} "
                                "=> ignore SOI_ERR",
                                message->dest);
              break;
            }
          }
        }
        while (!SOLisReceiveBufferEmpty (m_cookie));
      }
      catch (Exception^ ex) {
        log->ErrorFormat ("Start: "
                          "Exception ocurred, disconnect {0}",
                          ex);
        Disconnect (false);
        // Note there may be some remaining messages to process
        // because of the exception
        m_pendingMessages = 1;
        m_errorConnection = true;
        return true;
      }
      
      return true;
    }

    void Selca::Finish ()
    {
      m_parametersInitialization = false;
    }
    
    String^ Selca::GetString (String^ parameter)
    {
      if (m_parametersInitialization) {
        // Not absolutely necessary
        if (CheckParameter (parameter)) {
          m_parameters->Add (parameter);          
        }
        else {
          log->ErrorFormat ("GetString: "
                            "invalid parameter {0}",
                            parameter);
        }
        throw gcnew Exception ("Initialization");
      }

      if (!m_ready) {
        log->InfoFormat ("GetString: "
                         "skip the data parameter {0} because the connection is not ready",
                         parameter);
        throw gcnew Exception ("Connection not ready");
      }

      if (m_values->ContainsKey (parameter)) {
        String^ v = m_values [parameter];
        log->DebugFormat ("GetString: "
                          "got {0} for parameter {1}",
                          v, parameter);
        return v;
      }
      else {
        log->DebugFormat ("GetString: "
                          "no data for parameter {0}",
                          parameter);
        throw gcnew Exception ("No data");
      }
    }

    int Selca::GetInt (String^ parameter)
    {
      return Int32::Parse (this->GetString (parameter));
    }

    Int64 Selca::GetLong (String^ parameter)
    {
      return Int64::Parse (this->GetString (parameter));
    }

    double Selca::GetDouble (String^ parameter)
    {
      CultureInfo^ usCultureInfo = gcnew CultureInfo ("en-US");
      return Double::Parse (this->GetString (parameter), usCultureInfo);
    }

    bool Selca::GetBool (String^ parameter)
    {
      return (1.0 == this->GetDouble (parameter));
    }

    void Selca::ParseParameter (Slo_t &slo, String^ parameter)
    {
      try {
        log->DebugFormat ("ParseParameter: "
                          "parameter={0}",
                          parameter);

        array<String^>^ params = parameter->Split (gcnew array<Char> {':'}, 3);

        slo.flag = SLO_ENABLE|SLO_OUTPUT|SLO_INPUT;
        if (params [2]->Equals ("NONE")) {
          slo.nature = DATA_NONE;
        }
        else if (params [2]->Equals ("BOOL")) {
          slo.nature = DATA_BOOL;
        }
        else if (params [2]->Equals ("WORD")) {
          slo.nature = DATA_WORD;
        }
        else if (params [2]->Equals ("DWORD")) {
          slo.nature = DATA_DWORD;
        }
        else if (params [2]->Equals ("FLOAT")) {
          slo.nature = DATA_FLOAT;
        }
        else if (params [2]->Equals ("STRING")) {
          slo.nature = DATA_STRING;
        }
        else {
          log->ErrorFormat ("ParseParameter: "
                            "invalid data type {0} in parameter {1}",
                            params [2], parameter);
          throw gcnew FormatException ("Invalid data type");
        }        
        slo.device = 0; // S4000
        slo.addr1 = Int16::Parse (params [1]); // Variable to monitor. See varaddr.txt
        // addr2 - 0: PLC, 1: DISPL, 2: GENERAL
        if (params [0]->Equals ("PLC")) {
          slo.addr2 = 0;
        }
        else if (params [0]->Equals ("DISPL")) {
          slo.addr2 = 1;
        }
        else if (params [0]->Equals ("GENERAL")) {
          slo.addr2 = 2;
        }
        else {
          log->ErrorFormat ("ParseParameter: "
                            "invalid domain {0} in parameter {1}",
                            params [0], parameter);
          throw gcnew FormatException ("Invalid domain");
        }
        slo.addr2 = 0; // 
        slo.addr3 = 0; // Always 0
        slo.mask = 1; // Always 1
        slo.dfo = -1; // Always -1
        strcpy_s (slo.name, (const char*)""); // Not used, only to make it clearer 
      }
      catch (Exception^ ex) {
        log->ErrorFormat ("ParseParameter: "
                          "parameter {0} is invalid "
                          "Exception {1}",
                          parameter,
                          ex);
        throw;
      }
    }

    bool Selca::CheckParameter (String^ parameter)
    {
      try {
        Slo_t slo;
        ParseParameter (slo, parameter);
        return true;
      }
      catch (Exception^) {
        return false;
      }
    }

    void Selca::ProcessTcpMsg (LPARAM param, int code)
    {
      log->DebugFormat ("ProcessTcpMsg: "
                        "got code {0}",
                        code);

      switch (code) {
      case SLC_DISCONNECT:
        m_disconnectRequested = true;
        return;
      case SLC_MSG:        
        Interlocked::CompareExchange (m_pendingMessages, 1, 0);
        return;
      default:
        log->ErrorFormat ("ProcessTcpMsg: "
                          "invalid code {0}",
                          code);
        return;
      }
    }
  }
}

