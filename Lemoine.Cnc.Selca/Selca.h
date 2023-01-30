// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

#pragma once

#include <Windows.h>

#include "SeLGeCStruct.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;
using namespace Lemoine::Core::Log;

namespace Lemoine
{
  namespace Cnc
  {
    /// <summary>
    /// Selca input module for CNC v2
    /// </summary>
    public ref class Selca
      : public Lemoine::Cnc::BaseCncModule
      , public Lemoine::Cnc::ICncModule
      , public IDisposable
    {
    private: // Types
      delegate void ProcessTcpMsgCallback (LPARAM lParam, int code);

    private: // Constants

    private: // Members
      String^ m_ipAddress;
      HINSTANCE m_SeLGeCLibrary;
      bool m_parametersInitialization;
      IList<String^> ^m_parameters;
      IDictionary<String^, String^> ^m_values;
      bool m_sloDBInitialized;
      Slo_t* m_sloDB;
      int m_sloDBSize;
      int m_cookie;
      bool m_connected;
      bool m_ready;
      bool m_useMessages; // Messages only work in a graphical application !
      ProcessTcpMsgCallback ^m_processTcpMsgCallback;
      GCHandle m_processTcpMsgGCHandle;
      int m_pendingMessages;
      volatile bool m_disconnectRequested;
      DateTime m_lastConnectionErrorDateTime;
      bool m_errorRead;
      bool m_errorWrite;
      bool m_errorWdog;
      bool m_errorConnection;

    public: // Getters / Setters
      /// <summary>
      /// IP Address
      /// </summary>
      property String^ IPAddress
      {
        String^ get () { return m_ipAddress; }
        void set (String^ value) { m_ipAddress = value; }
      }

      /// <summary>
      /// Connection error
      /// </summary>
      property bool ConnectionError
      {
        bool get () { return m_errorConnection; }
      }

      /// <summary>
      /// The connection is not ready
      /// </summary>
      property bool Error
      {
        bool get () { return m_errorConnection || !m_ready; }
      }

      /// <summary>
      /// Read error (not blocking)
      /// </summary>
      property bool ReadError
      {
        bool get () { return m_errorRead; }
      }

      /// <summary>
      /// Write error (not blocking)
      /// </summary>
      property bool WriteError
      {
        bool get () { return m_errorWrite; }
      }

      /// <summary>
      /// Wdog error (not blocking)
      /// </summary>
      property bool WdogError
      {
        bool get () { return m_errorWdog; }
      }

    public: // Constructors / Destructors / ToString methods
      /// <summary>
      /// Constructor
      /// </summary>
      Selca ();
      /// <summary>
      /// Destructor: cleans up all resources
      /// </summary>
      virtual ~Selca () { this->!Selca (); }
      /// <summary>
      /// Finalizer: cleans up unmanaged resources
      /// </summary>
      !Selca ();

    private: // Private constructors
      /// <summary>
      /// Constructor with the useMessages parameter
      /// 
      /// Messages can only be used in graphical interfaces
      /// </summary>
      /// <param name="useMessages"></param>
      Selca (bool useMessages);

    public: // Public methods
      /// <summary>
      /// Start method of the Selca.
      /// </summary>
      /// <returns>Success</returns>
      bool Start ();
      /// <summary>
      /// End method of the Selca.
      /// </summary>
      void Finish ();
      
      /// <summary>
      /// Get a string value
      /// </summary>
      /// <param name="parameter">The parameter is made of the type of data: PLC, DISPL or GENERAL followed by ':' and an address number</param>
      /// <returns></returns>
      String^ GetString (String^ parameter);

      /// <summary>
      /// Get a int value
      /// </summary>
      /// <param name="parameter">The parameter is made of the type of data: PLC, DISPL or GENERAL followed by ':' and an address number</param>
      /// <returns></returns>
      int GetInt (String^ parameter);

      /// <summary>
      /// Get a long value
      /// </summary>
      /// <param name="parameter">The parameter is made of the type of data: PLC, DISPL or GENERAL followed by ':' and an address number</param>
      /// <returns></returns>
      Int64 GetLong (String^ parameter);

      /// <summary>
      /// Get a double value
      /// </summary>
      /// <param name="parameter">The parameter is made of the type of data: PLC, DISPL or GENERAL followed by ':' and an address number</param>
      /// <returns></returns>
      double GetDouble (String^ parameter);

      /// <summary>
      /// Get a bool value
      /// </summary>
      /// <param name="parameter">The parameter is made of the type of data: PLC, DISPL or GENERAL followed by ':' and an address number</param>
      /// <returns></returns>
      bool GetBool (String^ parameter);

    private: // Private methods
      /// <summary>
      /// Parse a parameter (PLC|DISPL|GENERAL):[0-9]*:(NONE|BOOL|WORD|DWORD|FLOAT|STRING)
      /// </summary>
      /// <param name="slo"></param>
      /// <param name="parameter">The parameter is made of the type of data: PLC, DISPL or GENERAL followed by ':' and an address number</param>
      void ParseParameter (Slo_t &slo, String^ parameter);

      /// <summary>
      /// Check a parameter has the syntax (PLC|DISPL|GENERAL):[0-9]*:(NONE|BOOL|WORD|DWORD|FLOAT|STRING)
      /// </summary>
      /// <param name="parameter">The parameter is made of the type of data: PLC, DISPL or GENERAL followed by ':' and an address number</param>
      /// <returns></returns>
      bool CheckParameter (String^ parameter);

      /// <summary>
      /// Load the SeLGeC library
      /// </summary>
      /// <exception cref="Exception">An exception is raised if the SELGeC library could not be loaded</exception>
      void LoadSeLGeCLibrary ();

      /// <summary>
      /// Check if the connection with the CNC is up. If not, connect to it
      /// </summary>
      /// <returns>The connection was successful</returns>
      bool CheckConnection ();

      /// <summary>
      /// Disconnect the CNC (for example in case of error)
      /// </summary>
      /// <param name="deleteConnection"></param>
      void Disconnect (bool deleteConnection);
      
      /// <summary>
      /// Disconnect the CNC (for example in case of error) and free the library
      /// </summary>
      void DisconnectAndFreeLibrary ();

      /// <summary>
      /// Selca callback
      /// </summary>
      /// <param name="param"></param>
      /// <param name="code"></param>
      void ProcessTcpMsg (LPARAM param, int code);

    private: // Selca function pointers
#define API(RTYPE, METHOD, ARGS) RTYPE (__stdcall *METHOD) ARGS
#include "SeLGeCApi.h"
    };
  }
}

