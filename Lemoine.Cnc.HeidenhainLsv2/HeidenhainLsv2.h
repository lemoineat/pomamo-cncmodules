// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

#pragma once

#include <Windows.h>
#include <corecrt_io.h>
#include "lsv2/lsv2_dnc_def.h"
#include "lsv2/lsv2_data_def.h"
#include "lsv2/lsv2_data.h"
#include "lsv2/lsv2_ctrl.h"
#include "lsv2/lsv2_def.h"
#include "lsv2/lsv2_file.h"

using namespace System;
using namespace System::Collections;
using namespace System::Collections::Generic;
using namespace Lemoine::Core::Log;

namespace Lemoine
{
  namespace Cnc
  {
    /// <summary>
    /// Heidenhain LSV2 input module for CNC v2
    /// </summary>
    public ref class HeidenhainLsv2
      : public Lemoine::Cnc::BaseCncModule
      , public Lemoine::Cnc::ICncModule
      , public IDisposable
    {
    public: // Types
      enum class Model { HEID_UNKNOWN, HEID_426, HEID_430, HEID_530, HEID_640 };

    private: // Constants

    private: // Members
      String^ ipAddress;
      String^ m_spindleLoadPLCAddress;
      int m_multiplier;
      bool m_keepPlcConnection;
      bool m_keepDncConnection;

      bool m_connected;
      HINSTANCE m_lsv2Library;
      HANDLE *m_hPort;
      LSV2PARA *m_lsv2Para;
      bool m_isLoggedDnc;
      bool m_isLoggedFile;
      bool m_isLoggedPlc;
      bool m_isLoggedData;

      String^ modelString;
      Model m_model;
      String^ version;

      // GetStringParameter
      System::Collections::Generic::IDictionary<String^, String^>^ m_parameterCache;

      // GetOverrideValues
      bool m_overrideValues;
      long feedrateOverride;
      long spindleSpeedOverride;

      // GetProgramValues
      bool m_programValues;
      String^ m_programName;
      long m_blockNumber;

      // GetProgramStatus
      bool m_programStatusOk;
      LSV2_PROGRAM_STATUS_TYPE m_programStatus;

      // GetExecutionMode
      bool m_executionModeOk;
      LSV2_EXEC_MODE m_executionMode;

      // DownloadReadLemoineTable
      DateTime^ m_downloadDateTime;
      Hashtable^ m_downloadTableValues;

      // GetValueFromFileName
      Hashtable^ m_lastValueFromFileNameList;
      Hashtable^ m_lastGetFromFileNameList;

      // Axis names
      String^ m_xAxisName;
      String^ m_yAxisName;
      String^ m_zAxisName;
      String^ m_uAxisName;
      String^ m_vAxisName;
      String^ m_wAxisName;
      String^ m_aAxisName;
      String^ m_bAxisName;
      String^ m_cAxisName;

    public: // Getters / Setters
      /// <summary>
      /// IP Address
      /// </summary>
      property String^ IPAddress
      {
        String^ get() { return ipAddress; }
        void set(String^ value) { ipAddress = value; }
      }

      /// <summary>
      /// Model string as returned by HeidenhainLsv2
      /// </summary>
      property String^ ModelString
      {
        String^ get() { return modelString; }
      }

      /// <summary>
      /// Version as returned by HeidenhainLsv2
      /// </summary>
      property String^ Version
      {
        String^ get() { return version; }
      }

      /// <summary>
      /// Spindle load PLC address
      /// </summary>
      property String^ SpindleLoadPLCAddress
      {
        String^ get() { return m_spindleLoadPLCAddress; }
        void set(String^ value) { m_spindleLoadPLCAddress = value; }
      }

      /// <summary>
      /// Multiplier to use in the data retrieved in the tables
      /// 
      /// This value is used in:
      /// <item>GetValueFromDownloadedTable</item>
      /// </summary>
      property int Multiplier
      {
        int get() { return m_multiplier; }
        void set(int value) { m_multiplier = value; }
      }

      /// <summary>
      /// Keep the PLC Connection option
      /// 
      /// Default: False
      /// </summary>
      property bool KeepPlcConnection
      {
        bool get() { return m_keepPlcConnection; }
        void set(bool value) { m_keepPlcConnection = value; }
      }

      /// <summary>
      /// Keep the DNC Connection option
      /// 
      /// Default: False
      /// </summary>
      property bool KeepDncConnection
      {
        bool get() { return m_keepDncConnection; }
        void set(bool value) { m_keepDncConnection = value; }
      }

      /// <summary>
      /// Is this module connected to the CNC ?
      /// </summary>
      property bool Connected
      {
        bool get() { return m_connected; }
      }

      /// <summary>
      /// Does a connection to the control end in error ?
      /// </summary>
      property bool ConnectionError
      {
        bool get();
      }

      /// <summary>
      /// Does a DNC connection to the control end in error ?
      /// </summary>
      property bool DNCConnectionError
      {
        bool get();
      }

      /// <summary>
      /// Does a File connection to the control end in error ?
      /// </summary>
      property bool FileConnectionError
      {
        bool get();
      }

      /// <summary>
      /// Does a PLC connection to the control end in error ?
      /// </summary>
      property bool PLCConnectionError
      {
        bool get();
      }

      /// <summary>
      /// Does a Data connection to the control end in error ?
      /// </summary>
      property bool DataConnectionError
      {
        bool get();
      }

      /// <summary>
      /// Position
      /// </summary>
      property Lemoine::Cnc::Position Position
      {
        Lemoine::Cnc::Position get();
      }

      /// <summary>
      /// Feedrate
      /// </summary>
      property double Feedrate
      {
        double get();
      }

      /// <summary>
      /// Spindle load
      /// </summary>
      property double SpindleLoad
      {
        double get();
      }

      /// <summary>
      /// Spindle speed
      /// </summary>
      property double SpindleSpeed
      {
        double get();
      }

      /// <summary>
      /// Manual status
      /// </summary>
      property bool Manual
      {
        bool get();
      }

      /// <summary>
      /// MDI (Manual Data Input) mode ?
      /// </summary>
      property bool MDI
      {
        bool get();
      }

      /// <summary>
      /// Single block mode ?
      /// </summary>
      property bool SingleBlock
      {
        bool get();
      }

      /// <summary>
      /// Feedrate override
      /// </summary>
      property long FeedrateOverride
      {
        long get();
      }

      /// <summary>
      /// Spindle speed override
      /// </summary>
      property long SpindleSpeedOverride
      {
        long get();
      }

      /// <summary>
      /// Start/end value
      /// </summary>
      property unsigned long StartEnd
      {
        unsigned long get();
      }

      /// <summary>
      /// Full program name with TNC:\
			/// </summary>
      property String^ FullProgramName
      {
        String^ get();
      }

      /// <summary>
      /// Program name without the directory
      /// </summary>
      property String^ ProgramName
      {
        String^ get();
      }

      /// <summary>
      /// Block number
      /// </summary>
      property long BlockNumber
      {
        long get();
      }

      /// <summary>
      /// Program status
      /// 
      /// <item>0: Started</item>
      /// <item>1: Stopped</item>
      /// <item>2: Finished</item>
      /// <item>3: Canceled</item>
      /// <item>4: Interrupted</item>
      /// <item>5: Error</item>
      /// <item>6: Error cleared</item>
      /// <item>7: Idle</item>
      /// </summary>
      property int ProgramStatusValue
      {
        int get();
      }

      /// <summary>
      /// Program status string
      /// 
      /// <item>0: Started</item>
      /// <item>1: Stopped</item>
      /// <item>2: Finished</item>
      /// <item>3: Canceled</item>
      /// <item>4: Interrupted</item>
      /// <item>5: Error</item>
      /// <item>6: Error cleared</item>
      /// <item>7: Idle</item>
      /// </summary>
      property String^ ProgramStatus
      {
        String^ get();
      }

      /// <summary>
      /// Execution mode
      /// 
      /// <item>0: Manual</item>
      /// <item>1: MDI</item>
      /// <item>2: RPF</item>
      /// <item>3: SingleStep</item>
      /// <item>4: Automatic</item>
      /// <item>5: Other</item>
      /// <item>6: Smart</item>
        /// </summary>
      property int ExecutionModeValue
      {
        int get();
      }

      /// <summary>
      /// Execution mode in string
      /// 
      /// <item>0: Manual</item>
      /// <item>1: MDI</item>
      /// <item>2: RPF</item>
      /// <item>3: SingleStep</item>
      /// <item>4: Automatic</item>
      /// <item>5: Other</item>
      /// <item>6: Smart</item>
        /// </summary>
      property String^ ExecutionMode
      {
        String^ get();
      }

      /// <summary>
      /// X axis name
      /// </summary>
      property String^ XAxisName
      {
        String^ get() { return m_xAxisName; }
        void set(String^ value) { m_xAxisName = value; }
      }

      /// <summary>
      /// Y axis name
      /// </summary>
      property String^ YAxisName
      {
        String^ get() { return m_yAxisName; }
        void set(String^ value) { m_yAxisName = value; }
      }

      /// <summary>
      /// Z axis name
      /// </summary>
      property String^ ZAxisName
      {
        String^ get() { return m_zAxisName; }
        void set(String^ value) { m_zAxisName = value; }
      }

      /// <summary>
      /// U axis name
      /// </summary>
      property String^ UAxisName
      {
        String^ get() { return m_uAxisName; }
        void set(String^ value) { m_uAxisName = value; }
      }

      /// <summary>
      /// V axis name
      /// </summary>
      property String^ VAxisName
      {
        String^ get() { return m_vAxisName; }
        void set(String^ value) { m_vAxisName = value; }
      }

      /// <summary>
      /// W axis name
      /// </summary>
      property String^ WAxisName
      {
        String^ get() { return m_wAxisName; }
        void set(String^ value) { m_wAxisName = value; }
      }

      /// <summary>
      /// A axis name
      /// </summary>
      property String^ AAxisName
      {
        String^ get() { return m_aAxisName; }
        void set(String^ value) { m_aAxisName = value; }
      }

      /// <summary>
      /// B axis name
      /// </summary>
      property String^ BAxisName
      {
        String^ get() { return m_bAxisName; }
        void set(String^ value) { m_bAxisName = value; }
      }

      /// <summary>
      /// C axis name
      /// </summary>
      property String^ CAxisName
      {
        String^ get() { return m_cAxisName; }
        void set(String^ value) { m_cAxisName = value; }
      }

    public: // Constructors / Destructors / ToString methods
      /// <summary>
      /// Constructor
      /// </summary>
      HeidenhainLsv2();
      /// <summary>
      /// Destructor: cleans up all resources
      /// </summary>
      virtual ~HeidenhainLsv2() { this->!HeidenhainLsv2(); }
      /// <summary>
      /// Finalizer: cleans up unmanaged resources
      /// </summary>
      !HeidenhainLsv2();

    public: // Public methods
      /// <summary>
      /// Start method of the HeidenhainLsv2.
      /// </summary>
      /// <returns>Success</returns>
      bool Start();
      /// <summary>
      /// End method of the HeidenhainLsv2.
      /// </summary>
      void Finish();

      /// <summary>
      /// Get a PLC value
      ///
      /// Raise an exception in case getting the PLC value fails
      /// </summary>
      /// <param name="type"></param>
      /// <param name="address"></param>
      /// <returns>PLC value</returns>
      long GetPLCValue(char type, long address);

      /// <summary>
      /// Get a PLC value
      /// </summary>
      /// <param name="parameter">The parameter is made of the type followed by the address, e.g. D284 or D\\284</param>
      long GetPLCValue(String^ parameter);

      /// <summary>
      /// Get a PLC value and convert it to a bool
      /// </summary>
      /// <param name="parameter">The parameter is made of the type followed by the address, e.g. D284 or D\\284</param>
      bool GetPLCBoolValue(String^ parameter);

      /// <summary>
      /// Get a value from the dowloaded table.
      ///
      /// The returned value is multiplied by the property Multiplier
      ///
      /// Note: this class supports the use of only one table for the moment,
      ///       because a same attribute downloadTableValues is used whichever tableName.
      /// </summary>
      /// <param name="tableName">Name of the table, e.g. TNC:\LEMOINE\OPID.TAB</param>
      /// <param name="valueName">Name of the value to read, e.g. OPID</param>
      /// <param name="skipTime">Duration in seconds while the file is not downloaded again. Default is 15s.</param>
      /// <returns>Value read in the downloaded table</returns>
      Int32 GetValueFromDownloadedTable(String^ tableName, String^ valueName, int skipTime);

      /// <summary>
      /// Read stamp value from a file written using F-PRINT on NC side.
      /// Value is written in files "STAMP-xx" with xx incremented on each stamp.
      /// In addition, "xx" value is written in a main "STAMP" file.
      /// If there are several "STAMP-xx", the latest one is read, determined from the latest line of "STAMP" file.
      /// After reading, "STAMP-xx" file(s) and "STAMP" files are deleted.
      ///
      /// Parameters is made of several values that are separated by ',' or ';':
      /// <item>Path of folder containing stamp files, e.g. TNC:\LEMOINE</item>
      /// <item>Name of file containing stamp files list, e.g STAMP</item>
      /// <item>Prefix name of the stamp file, e.g. STAMP</item>
      /// <item>Separator bewtween prefix and xx, e.g. -</item>
      /// <item>Duration in seconds while the file is not downloaded again. Default is 15s.</item>
      ///
      /// Here is an example of parameters:
      /// <item>TNC:\LEMOINE;STAMP;STAMP;-</item>
      /// </summary>
      /// <param name="parameters">Parameters</param>
      /// <returns>Value read in the downloaded table</returns>
      [Obsolete("Remove the implementation since there was some weakness in the implementation", true)]
      double GetValueFromStampFile(String^ parameters);

      /// <summary>
      /// Read stamp value from a file written using F-PRINT on NC side.
      /// Value is written in files "STAMP-xx" with xx incremented on each stamp.
      /// In addition, "xx" value is written in a main "STAMP" file.
      /// If there are several "STAMP-xx", the latest one is read, determined from the latest line of "STAMP" file.
      /// After reading, "STAMP-xx" file(s) and "STAMP" files are deleted.      ///
      /// </summary>
      /// <param name="pathName">Path of folder containing stamp files, e.g. TNC:\LEMOINE</param>
      /// <param name="mainStampFile">Name of file containing stamp files list, e.g STAMP</param>
      /// <param name="stampFilePrefix">Prefix name of the stamp file, e.g. STAMP</param>
      /// <param name="separator">Separator bewtween prefix and xx, e.g. -</param>
      /// <param name="skipTime">Duration in seconds while the file is not downloaded again. Default is 15s.</param>
      /// <returns>Value read in the stamp file</returns>
      double GetValueFromStampFile(String^ pathName, String^ mainStampFile, String^ stampFilePrefix, Char separator, int skipTime);

      /// <summary>
      /// Read parameter value from a file name written using F-PRINT on NC side.
      /// Parameter name is written in file name prefix
      /// Value is written in file name suffix, e.g. MODE-0, MODE-100...
      ///
      /// Parameters is made of several values that are separated by ',' or ';':
      /// <item>Path of folder containing stamp files, e.g. TNC:\LEMOINE</item>
      /// <item>Name of parameter, e.g MODE</item>
      /// <item>Separator bewtween prefix and value, e.g. -</item>
      /// <item>Duration in seconds while the file is not downloaded again. Default is 15s.</item>
      ///
      /// Here is an example of parameters:
      /// <item>TNC:\LEMOINE;MODE;-</item>
      /// </summary>
      /// <param name="parameters">Parameters</param>
      /// <returns>Value read from the stamp file name</returns>
      double GetValueFromFileName(String^ parameters);
      String^ GetStringValueFromFileName(String^ parameters);

      /// <summary>
      /// Read parameter value from a file name written using F-PRINT on NC side.
      /// Parameter name is written in file name prefix
      /// Value is written in file name suffix, e.g. MODE_0, MODE_100...
      /// </summary>
      /// <param name="directoryName">Path of folder containing stamp files, e.g. TNC:\LEMOINE</param>
      /// <param name="maxFileNumber">delete remote files when the max file number is reached</param>
      /// <param name="skipTime">Duration in seconds while the file is not downloaded again. Default is 15s.</param>
      /// <param name="suffix">optional string to remove at end of filename</param>
      /// <returns>Value read from the stamp file name</returns>
      String^ GetStringValueFromFileName(String^ directoryName, int maxFileNumber, int skipTime, String^ suffix);

      /// <summary>
      /// delete a remote file in currently selected woking dir
      /// </summary>
      /// <param name="fileName">Name of file, e.g. STAMP-xx</param>
      /// <returns></returns>
      void DeleteFile(String^ pathName, String^ fileName);

      /// <summary>
      /// delete remote files in list, except the one as paramter
      /// </summary>
      /// <param name="pathName">Path of files</param>
      /// <param name="fileList">List of files</param>
      /// <param name="fileToKeep">File to keep</param>
      /// <returns></returns>
      void HeidenhainLsv2::DeleteRemoteFiles(String^ directoryPath, System::Collections::Generic::IEnumerable <String^>^ fileList, String^ fileToKeep);

      /// <summary>
      /// Get a value from the dowloaded table.
      ///
      /// The returned value is multiplied by the property Multiplier.
      ///
      /// Parameters is made of several values that are separated by ',' or ';':
      /// <item>the table name, e.g. TNC:\LEMOINE\OPID.TAB</item>
      /// <item>the nane of the value to read, e.g. OPID</item>
      /// <item>Duration in seconds while the file is not downloaded again. Default is 15s.</item>
      ///
      /// Here are some example of parameters:
      /// <item>TNC:\LEMOINE\OPID.TAB;OPID</item>
      /// <item>TNC:\LEMOINE\OPID.TAB;CPTID</item>
      /// <item>TNC:\LEMOINE\OPID.TAB;OPID;15</item>
      ///
      /// Note: this class supports the use of only one table for the moment,
      ///       because a same attribute downloadTableValues is used whichever tableName.
      /// </summary>
      /// <param name="parameters">Parameters</param>
      /// <returns>Value read in the downloaded table</returns>
      Int32 GetValueFromDownloadedTable(String^ parameters);

      /// <summary>
      /// Get directly a string data property.
      ///
      /// This is not supported on all controls.
      /// The control must be at least a 530 control.
      //
      //  entryName may be for example:
      //  <item>\PLC\memory\D\388</item>
      //  <item>\TABLE\'TNC:\LEMOINE\OPID.TAB'\NR\0\OPID</item>
      /// </summary>
      /// <param name="entryName">Name of the data property to read</param>
      /// <returns>Got string value</returns>
      String^ GetStringData(String^ entryName);

      /// <summary>
      /// Get directly an integer data property.
      ///
      /// This is not supported on all controls.
      /// The control must be at least a 530 control.
      //
      //  entryName may be for example:
      //  <item>\PLC\memory\D\388</item>
      //  <item>\TABLE\'TNC:\LEMOINE\OPID.TAB'\NR\0\OPID</item>
      /// </summary>
      /// <param name="entryName">Name of the data property to read</param>
      /// <returns>Got an integer value</returns>
      Int32 GetIntData(String^ entryName);

      /// <summary>
      /// Get directly a double data property.
      ///
      /// This is not supported on all controls.
      /// The control must be at least a 530 control.
      //
      //  entryName may be for example:
      //  <item>\PLC\memory\D\388</item>
      //  <item>\TABLE\'TNC:\LEMOINE\OPID.TAB'\NR\0\OPID</item>
      /// </summary>
      /// <param name="entryName">Name of the data property to read</param>
      /// <returns>Got double value</returns>
      double GetDoubleData(String^ entryName);

      /// <summary>
      /// Get directly a bool data property.
      ///
      /// This is not supported on all controls.
      /// The control must be at least a 530 control.
      //
      //  entryName may be for example:
      //  <item>\PLC\memory\D\388</item>
      //  <item>\TABLE\'TNC:\LEMOINE\OPID.TAB'\NR\0\OPID</item>
      /// </summary>
      /// <param name="entryName">Name of the data property to read</param>
      /// <returns>Got an integer value</returns>
      bool GetBoolData(String^ entryName);

      /// <summary>
      /// Get a string parameter.
      ///
      /// This requires to have a Data connection.
      /// This is not supported on all controls.
      /// The control must be at least a 530 control.
      ///
      /// Note: the data is cached
      /// </summary>
      /// <param name="entryName">Name of the parameter to read</param>
      /// <returns>Parameter value</returns>      
      String^ GetStringParameter(String^ entryName);

      /// <summary>
      /// Get directly an integer parameter.
      ///
      /// This requires to have a Data connection.
      /// This is not supported on all controls.
      /// The control must be at least a 530 control.
      ///
      /// Note: the data is cached
      /// </summary>
      /// <param name="entryName">Name of the parameter to read</param>
      /// <returns>Parameter value</returns>      
      Int32 GetIntParameter(String^ entryName);

      /// <summary>
      /// Get directly an integer parameter.
      ///
      /// This requires to have a Data connection.
      /// This is not supported on all controls.
      /// The control must be at least a 530 control.
      ///
      /// Note: the data is cached
      /// </summary>
      /// <param name="entryName">Name of the parameter to read</param>
      /// <returns>Parameter value</returns>      
      double GetDoubleParameter(String^ entryName);

      /// <summary>
      /// Get directly a data property multiplied by the property Multiplier.
      ///
      /// This is not supported on all controls.
      /// The control must be at least a 530 control.
      /// </summary>
      /// <param name="entryName">Name of the data property to read</param>
      /// <returns>Got double value</returns>
      Int32 GetDataWithMultiplier(String^ entryName);

      /// <summary>
      /// Get a table line.
      /// </summary>
      /// <param name="tableName">Name of the table, e.g. TNC:\TOOL.T</param>
      /// <param name="condition">Condition, e.g. WHERE NAME LIKE 'RESERVEDPULSE1'</param>
      /// <returns>Read table line</returns>
      String^ GetTableLine(String^ tableName, String^ condition);

      /// <summary>
      /// Get a string value from a given column in a table line.
      /// </summary>
      /// <param name="tableName">Name of the table, e.g. TNC:\TOOL.T</param>
      /// <param name="condition">Condition, e.g. WHERE NAME LIKE 'RESERVEDPULSE1'</param>
      /// <param name="column">Column number</param>
      /// <returns>Read column value</returns>
      String^ GetTableLineValue(String^ tableName, String^ condition, int column);

      /// <summary>
      /// Get a string value from a given column in a table line.
      ///
      /// Parameters is made of several values that are separated by ',' or ';':
      /// <item>the table name, e.g. TNC:\TOOL.T</item>
      /// <item>the condition, e.g. WHERE NAME LIKE 'RESERVEDPULSE1'</item>
      /// <item>the column number. Default is 22.</item>
      ///
      /// Here are some example of parameters:
      /// <item>TNC:\TOOL.T,WHERE NAME LIKE 'RESERVEDPULSE1'</item>
      /// <item>TNC:\TOOL.T,WHERE NAME LIKE 'RESERVEDPULSE1';2</item>
      /// </summary>
      /// <param name="parameters">Parameters</param>
      /// <returns>Read column value</returns>
      String^ GetTableLineValue(String^ parameters);

      /// <summary>
      /// Get a double value from a given column in a table line.
      /// </summary>
      /// <param name="tableName">Name of the table, e.g. TNC:\TOOL.T</param>
      /// <param name="condition">Condition, e.g. WHERE NAME LIKE 'RESERVEDPULSE1'</param>
      /// <param name="column">Column number</param>
      /// <returns>Read column value</returns>
      double GetTableLineDoubleValue(String^ tableName, String^ condition, int column);

      /// <summary>
      /// Get a double value from a given column in a table line.
      ///
      /// Parameters is made of several values that are separated by ',' or ';':
      /// <item>the table name, e.g. TNC:\TOOL.T</item>
      /// <item>the condition, e.g. WHERE NAME LIKE 'RESERVEDPULSE1'</item>
      /// <item>the column number. Default is 22.</item>
      ///
      /// Here are some example of parameters:
      /// <item>TNC:\TOOL.T,WHERE NAME LIKE 'RESERVEDPULSE1'</item>
      /// <item>TNC:\TOOL.T;WHERE NAME LIKE 'RESERVEDPULSE1';2</item>
      /// </summary>
      /// <param name="parameters">Parameters</param>
      /// <returns>Read column value</returns>
      double GetTableLineDoubleValue(String^ parameters);

      /// <summary>
      /// Get a value from a given column in a table line.
      ///
      /// The returned value is multiplied by the property Multiplier.
      /// </summary>
      /// <param name="tableName">Name of the table, e.g. TNC:\TOOL.T</param>
      /// <param name="condition">Condition, e.g. WHERE NAME LIKE 'RESERVEDPULSE1'</param>
      /// <param name="column">Column number</param>
      /// <returns>Read column value</returns>
      Int32 GetTableLineValueWithMultiplier(String^ tableName, String^ condition, int column);

      /// <summary>
      /// Get a value from a given column in a table line.
      ///
      /// The returned value is multiplied by the property Multiplier.
      ///
      /// Parameters is made of several values that are separated by ',' or ';':
      /// <item>the table name, e.g. TNC:\TOOL.T</item>
      /// <item>the condition, e.g. WHERE NAME LIKE 'RESERVEDPULSE1'</item>
      /// <item>the column number. Default is 22.</item>
      ///
      /// Here are some example of parameters:
      /// <item>TNC:\TOOL.T,WHERE NAME LIKE 'RESERVEDPULSE1'</item>
      /// <item>TNC:\TOOL.T;WHERE NAME LIKE 'RESERVEDPULSE1';2</item>
      /// </summary>
      /// <param name="parameters">Parameters</param>
      /// <returns>Read column value</returns>
      Int32 GetTableLineValueWithMultiplier(String^ parameters);

      /// <summary>
      /// Get the first not null double value in a given column in a table line.
      /// In case no not null value is found in the table line, 0.0 is returned.
      /// </summary>
      /// <param name="tableName">Name of the table, e.g. TNC:\TOOL.T</param>
      /// <param name="condition">Condition, e.g. WHERE NAME LIKE 'RESERVEDPULSE1'</param>
      /// <returns>First not null double value</returns>
      double GetTableLineNotNullValue(String^ tableName, String^ condition);

      /// <summary>
      /// Get the first not null double value in a given column in a table line.
      /// In case no not null value is found in the table line, 0.0 is returned.
      ///
      /// Parameters is made of several values that are separated by ',' or ';':
      /// <item>the table name, e.g. TNC:\TOOL.T</item>
      /// <item>the condition, e.g. WHERE NAME LIKE 'RESERVEDPULSE1'</item>
      ///
      /// Here are some examples of parameters:
      /// <item>TNC:\TOOL.T,WHERE NAME LIKE 'RESERVEDPULSE1'</item>
      /// <item>TNC:\TOOL.T;WHERE NAME LIKE 'RESERVEDPULSE1'</item>
      /// </summary>
      /// <param name="parameters">Parameters</param>
      /// <returns>First not null double value</returns>
      double GetTableLineNotNullValue(String^ parameters);

      /// <summary>
      /// Get the first not null double value in a given column in a table line.
      ///
      /// The returned value is multiplied by the property Multiplier.
      /// In case no not null value is found in the table line, 0 is returned.
      /// </summary>
      /// <param name="tableName">Name of the table, e.g. TNC:\TOOL.T</param>
      /// <param name="condition">Condition, e.g. WHERE NAME LIKE 'RESERVEDPULSE1'</param>
      /// <returns>Read not null value with m_multiplier</returns>
      Int32 GetTableLineNotNullValueWithMultiplier(String^ tableName, String^ condition);

      /// <summary>
      /// Get the first not null double value in a given column in a table line.
      ///
      /// The returned value is multiplied by the property Multiplier.
      /// In case no not null value is found in the table line, 0 is returned.
      ///
      /// Parameters is made of several values that are separated by ',' or ';':
      /// <item>the table name, e.g. TNC:\TOOL.T</item>
      /// <item>the condition, e.g. WHERE NAME LIKE 'RESERVEDPULSE1'</item>
      ///
      /// Here are some example of parameters:
      /// <item>TNC:\TOOL.T,WHERE NAME LIKE 'RESERVEDPULSE1'</item>
      /// <item>TNC:\TOOL.T;WHERE NAME LIKE 'RESERVEDPULSE1'</item>
      /// </summary>
      /// <param name="parameters">Parameters</param>
      /// <returns>Read not null value with m_multiplier</returns>
      Int32 GetTableLineNotNullValueWithMultiplier(String^ parameters);

    private: // Private methods
      /// <summary>
      /// Check if the error is an error that requires a disconnection
      /// </summary>
      /// <param name="errorCode">Error code</param>
      /// <returns></returns>
      bool IsDisconnectError(long errorCode);

      /// <summary>
      /// Load the LSV2 library
      /// </summary>
      /// <exception cref="Exception">An exception is raised if the LSV2 library could not be loaded</exception>
      void LoadLsv2Library();

      /// <summary>
      /// Check if the connection with the CNC is up. If not, connect to it
      /// </summary>
      /// <returns>The connection was successful</returns>
      bool CheckConnection();

      /// <summary>
      /// Check the DNC connection.
      /// If the system is not logged in, do it
      /// </summary>
      /// <returns>CNC connection and DNC Login were successful</returns>
      bool CheckDNCConnection();

      /// <summary>
      /// Check the FILE connection.
      /// If the system is not logged in, do it
      /// </summary>
      /// <returns>CNC connection and FILE Login were successful</returns>
      bool CheckFileConnection();

      /// <summary>
      /// Check the PLC connection.
      /// If the system is not logged in, do it
      /// </summary>
      /// <returns>CNC connection and PLC Login were successful</returns>
      bool CheckPLCConnection();

      /// <summary>
      /// Check the DATA connection.
      /// If the system is not logged in, do it
      /// </summary>
      /// <returns>CNC connection and DATA Login were successful</returns>
      bool CheckDataConnection();

      /// <summary>
      /// Log out from the PLC connection
      /// </summary>
      void LogoutPlc();

      /// <summary>
      /// Log out from the DNC connection
      /// </summary>
      void LogoutDnc();

      /// <summary>
      /// Disconnect the CNC (for example in case of error)
      /// </summary>
      /// <returns>Success</returns>
      void Disconnect();

      /// <summary>
      /// Log the Lsv2 error of the given method
      /// </summary>
      /// <param name="method"></param>
      /// <returns>Error code</returns>
      DWORD LogLsv2Error(String^ method);

      /// <summary>
      /// Get the PLC type info
      /// </summary>
      /// <param name="type"></param>
      /// <param name="address"></param>
      /// <param name="length"></param>
      /// <param name="rawAddress"></param>
      /// <returns>success</returns>
      bool GetPLCTypeInfo(char type, long address, int* length, long* rawAddress);

      /// <summary>
      /// Download a file using Heidenhain TNCremote
      /// </summary>
      /// <param name="distantFile">Distant file path</param>
      /// <param name="localFile">Local file path</param>
      /// <param name="binary">Is the file binary ? Default is false</param>
      /// <returns>success</returns>
      bool ReceiveFile(String^ distantFile, String^ localFile, bool binary);
      bool ReceiveFile(String^ distantFile, String^ localFile);

      /// <summary>
      /// Get the list of files in a folder ordered by ascending write time
      /// </summary>
      /// <param name="distantDir">Distant directory path</param>
      /// <returns>files list</returns>
      /// 
      System::Collections::Generic::IList<String^>^ HeidenhainLsv2::GetFileList(String^ distantDir);

      /// <summary>
      /// Get the override values (feedrate and spindle speed)
      /// if the values have not been taken yet
      /// </summary>
      void GetOverrideValues();

      /// <summary>
      /// Get the program values (program values, block number)
      /// if the values have not been taken yet
      /// </summary>
      void GetProgramValues();

      /// <summary>
      /// Get the program status
      /// if the values have not been taken yet
      /// </summary>
      void GetProgramStatus();

      /// <summary>
      /// Get the execution mode
      /// if the values have not been taken yet
      /// </summary>
      void GetExecutionMode();

      /// <summary>
      /// Download and read a table
      /// if it has not been done yet
      ///
      /// This method fills the attribute downloadTableValues.
      ///
      /// Note: this class supports the use of only one table for the moment,
      ///       because a same attribute downloadTableValues is used whichever tableName.
      /// </summary>
      /// <param name="tableName">Name of the table, e.g. TNC:\LEMOINE\OPID.TAB</param>
      /// <param name="skipTime">Duration in seconds while the file is not downloaded again.</param>
      /// <returns>Success</returns>
      bool DownloadReadTable(String^ tableName, long skipTime);

      /// <summary>
      /// Get a parameter.
      ///
      /// The data is not cached here
      /// </summary>
      /// <param name="entryName">Name of the parameter to read</param>
      /// <returns>Parameter value</returns>      
      String^ GetParameter(String^ entryName);

      /// <summary>
      /// Get directly a data property.
      ///
      /// This is not supported on all controls.
      /// The control must be at least a 530 control.
      /// </summary>
      /// <param name="entryName">Name of the data property to read</param>
      /// <returns>Got LSV2DATA value</returns>
      LSV2DATA GetData(String^ entryName);

    private: // LSV2 function pointers
#define API(RTYPE, METHOD, ARGS) RTYPE (__stdcall *METHOD) ARGS
#include "Lsv2Api.h"

      // Tool data management
    private:
      Lemoine::Cnc::ToolLifeData^ m_toolLifeData;
      System::Collections::Generic::IList<String^>^ m_toolMissingVariables;
      System::Collections::Generic::IList<String^>^ m_toolAvailableVariables;

    public:
      /// <summary>
      /// IP Address
      /// </summary>
      property Lemoine::Cnc::ToolLifeData^ ToolLifeData
      {
        Lemoine::Cnc::ToolLifeData^ get() {
          if (m_toolLifeData == nullptr)
            ReadToolLifeData();
          return m_toolLifeData;
        }
      }

    private:
      Lemoine::Cnc::ToolLifeData^ ReadToolLifeData();
      int GetToolNumber();
      String^ GetToolCondition(int toolNumber);
      int GetToolAttributeColumn(String^ attribute);
      // End of tool data management


      // Alarms
    public:
      /// <summary>
      /// Get all alarms currently raised
      /// </summary>
      property List<Lemoine::Cnc::CncAlarm^>^ Alarms
      {
        List<Lemoine::Cnc::CncAlarm^>^ get();
      }

    private:
      CncAlarm^ GetAlarm(LSV2RUNINFO* runInfo);
      // End of alarms
    };
  }
}

