// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Lemoine.Net;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Fanuc_CNC_program.
  /// </summary>
  public partial class Fanuc
  {
    int m_mainProgramNumberFromProgramNumbers = -1;
    int m_runningProgramNumberFromProgramNumbers = -1;
    int m_programNumberFromExecutionPointer = -1;
    int m_blockNumberFromExecutionPointer = -1; // -1: not read yet
    string m_programNameFromExeprgname = null;
    int m_programNumberFromExeprgname = -1;
    int m_currentProgramNumber = -1;
    string m_currentProgramComment = null;
    int m_currentSubProgramNumber = -1;
    string m_currentSubProgramComment = null;
    string m_currentProgramFirstTool = null;
    Dictionary<string, string> m_subprogramList = new Dictionary<string, string> ();
    string m_sharedProgramFolderUser = null;
    string m_sharedProgramFolderPassword = null;
    string m_sharedProgramFolderDomain = null;
    bool m_sharedProgramFolderLoginDone = false;

    static readonly string DEFAULT_PROGRAMFILE_PREFIX = "";
    static readonly string DEFAULT_PROGRAMFILE_EXTENSION = "";
    static readonly string DEFAULT_COMMENT_SEARCH_STRING = "(POMAMO PART=";
    static readonly string DEFAULT_COMMENT_SEARCH_NEXT_LINE_STRING = "";
    static readonly string DEFAULT_COMMENT_STRING = "(";
    static readonly string PROGRAM_FIRST_TOOL_REGEXP = "T(?<toolnumber>[0-9]+)";
    static readonly string DEFAULT_CNC_UPLOAD_SIZE = "2048";
    static readonly string DEFAULT_SHARED_FOLDER_USER = "makino";
    static readonly string DEFAULT_SHARED_FOLDER_PASSWORD = "MAKINO-ACU";
    static readonly string DEFAULT_SHARED_FOLDER_DOMAIN = "GLOBAL";

    void ResetCncProgramCache ()
    {
      m_mainProgramNumberFromProgramNumbers = -1;
      m_runningProgramNumberFromProgramNumbers = -1;
      m_programNumberFromExecutionPointer = -1;
      m_blockNumberFromExecutionPointer = -1;
      m_programNameFromExeprgname = null;
      m_programNumberFromExeprgname = -1;
    }

    /// <summary>
    /// Path to shared programs folder, w/o trailing separator
    /// 
    /// For example:
    /// \\192.168.1.11\MC_Direct Mode Programs
    /// </summary>
    public string SharedProgramFolder { get; set; }

    /// <summary>
    /// User to connect to shared programs folder
    /// </summary>
    public string SharedProgramFolderUser
    {
      get {
        return (!string.IsNullOrEmpty (m_sharedProgramFolderUser) ? m_sharedProgramFolderUser : DEFAULT_SHARED_FOLDER_USER);
      }
      set {
        m_sharedProgramFolderUser = value;
      }
    }

    /// <summary>
    /// password to connect to shared programs folder
    /// </summary>
    public string SharedProgramFolderPassword
    {
      get {
        return (!string.IsNullOrEmpty (m_sharedProgramFolderPassword) ? m_sharedProgramFolderPassword : DEFAULT_SHARED_FOLDER_PASSWORD);
      }
      set {
        m_sharedProgramFolderPassword = value;
      }
    }

    /// <summary>
    /// domain to connect to shared programs folder
    /// </summary>
    public string SharedProgramFolderDomain
    {
      get {
        return (!string.IsNullOrEmpty (m_sharedProgramFolderDomain) ? m_sharedProgramFolderDomain : DEFAULT_SHARED_FOLDER_DOMAIN);
      }
      set {
        m_sharedProgramFolderDomain = value;
      }
    }

    /// <summary>
    /// Force program content FTP read even if no program number change
    /// 
    /// </summary>
    public string ForceFTPProgramContentRead { get; set; }

    /// <summary>
    /// string to identify part comment in program content
    /// 
    /// </summary>
    public string CommentSearchString { get; set; }

    /// <summary>
    /// string to get line after a comment in program content
    /// 
    /// </summary>
    public string CommentSearchStringNextLine { get; set; }

    /// <summary>
    /// string to hold the folder location of subprograms
    /// 
    /// </summary>
    public string SubProgramFolder { get; set; }

    /// <summary>
    /// Main program number
    /// </summary>
    public int ProgramNumber
    {
      get {
        GetProgramNumbers ();
        return m_mainProgramNumberFromProgramNumbers;
      }
    }

    /// <summary>
    /// Main program name
    /// </summary>
    public string ProgramName
    {
      get { return "O" + ProgramNumber; }
    }

    /// <summary>
    /// Sub-program number
    /// 
    /// -1 is return if no sub-program is running
    /// </summary>
    public int SubProgramNumber
    {
      get {
        GetProgramNumbers ();

        if (m_mainProgramNumberFromProgramNumbers != m_runningProgramNumberFromProgramNumbers) {
          log.DebugFormat ("SubProgramNumber: return {0}", m_runningProgramNumberFromProgramNumbers);
          return m_runningProgramNumberFromProgramNumbers;
        }
        else {
          log.InfoFormat ("SubProgramNumber: no sub-program running");
          return -1;
        }
      }
    }

    /// <summary>
    /// Sub-program name
    /// 
    /// If no sub-program is running, an empty string is returned
    /// </summary>
    public string SubProgramName
    {
      get {
        int subProgramNumber = this.SubProgramNumber;
        if (-1 == subProgramNumber) {
          return "";
        }
        else {
          return "O" + SubProgramNumber;
        }
      }
    }

    /// <summary>
    /// Get the running program number from GetProgramNumbers ()
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public int GetRunningProgramNumberFromProgramNumbers (string param)
    {
      return GetRunningProgramNumberFromProgramNumbers ();
    }

    int GetRunningProgramNumberFromProgramNumbers ()
    {
      GetProgramNumbers ();
      return m_runningProgramNumberFromProgramNumbers;
    }

    /// <summary>
    /// Running program number
    /// </summary>
    public int RunningProgramNumber
    {
      get {
        try {
          return GetRunningProgramNumberFromProgramNumbers ();
        }
        catch (Exception) {
        }

        try {
          return GetProgramNumberFromExeprgname ();
        }
        catch (Exception) {
        }

        return GetProgramNumberFromExecutionPointer ();
      }
    }

    /// <summary>
    /// Running program name
    /// </summary>
    public string RunningProgramName
    {
      get {
        try {
          return GetProgramNameFromExeprgname ();
        }
        catch (Exception) {
        }

        return "O" + RunningProgramNumber;
      }
    }

    /// <summary>
    /// Executed program number from GetExecutionPointer ()
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public int GetProgramNumberFromExecutionPointer (string param)
    {
      return GetProgramNumberFromExecutionPointer ();
    }

    int GetProgramNumberFromExecutionPointer ()
    {
      GetExecutionPointer ();
      return m_programNumberFromExecutionPointer;
    }

    /// <summary>
    /// Executed block number (available on very free CNCs)
    /// </summary>
    public int BlockNumber
    {
      get {
        GetExecutionPointer ();
        return m_blockNumberFromExecutionPointer;
      }
    }

    /// <summary>
    /// Get the program number and the sub-program number
    /// </summary>
    void GetProgramNumbers ()
    {
      if (-1 != m_mainProgramNumberFromProgramNumbers) {
        log.DebugFormat ("GetProgramNumbers: " +
                        "the program numbers are already known, " +
                        "main={0} running={1}",
                        m_mainProgramNumberFromProgramNumbers, m_runningProgramNumberFromProgramNumbers);
        return;
      }

      CheckAvailability ("rdprgnumo8");
      CheckConnection ();

      Import.FwLib.ODBPROO8 prgnum;
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdprgnumo8 (m_handle, out prgnum);
      ManageErrorWithException ("rdprgnumo8", result);
      m_mainProgramNumberFromProgramNumbers = prgnum.mdata;
      m_runningProgramNumberFromProgramNumbers = prgnum.data;
      log.DebugFormat ("GetProgramNumbers: got main={0} and running={1}",
                       m_mainProgramNumberFromProgramNumbers, m_runningProgramNumberFromProgramNumbers);
    }

    /// <summary>
    /// Get the program comment from a program number
    /// 
    /// It does not look not working when the program is running from memory
    /// </summary>
    /// <param name="programNumber"></param>
    /// <returns></returns>
    string GetProgramCommentFromProgramNumber (int programNumber)
    {
      CheckAvailability ("rdprogdir2");
      CheckConnection ();

      Import.FwLib.PRGDIR2 buf;
      short top_prog = (short)programNumber;
      short num_prog = 1;
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdprogdir2 (m_handle, 1, ref top_prog, ref num_prog, out buf);
      ManageErrorWithException ("rdprogdir2", result);
      if (1 != num_prog) {
        log.ErrorFormat ("GetProgramCommentFromProgramNumber: program {0} not found", programNumber);
        throw new Exception ("GetProgramCommentFromProgramNumber: program number not found");
      }
      log.DebugFormat ("GetProgramCommentFromProgramNumber: comment={0} for {1}", buf.dir.comment, programNumber);
      return buf.dir.comment;
    }

    /// <summary>
    /// Get the program modification date from a program number
    /// 
    /// It does not look not working when the program is running from memory
    /// </summary>
    /// <param name="programNumber"></param>
    /// <returns></returns>
    string GetProgramModificationDate (int programNumber)
    {
      CheckAvailability ("rdprogdir3");
      CheckConnection ();

      Import.FwLib.PRGDIR3 buf;
      int top_prog = programNumber;
      short num_prog = 1;
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdprogdir3 (m_handle, 1, ref top_prog, ref num_prog, out buf);
      ManageErrorWithException ("rdprogdir3", result);
      if (1 != num_prog) {
        log.ErrorFormat ("GetProgramModificationDate: program {0} not found", programNumber);
        throw new Exception ("GetProgramModificationDate: program number not found");
      }
      log.DebugFormat ("GetProgramModificationDate: comment={0} for {1}", buf.dir1.mdate.day, programNumber);
      return buf.dir1.mdate.day.ToString ();
    }

    /// <summary>
    /// Program comment of the main program
    /// 
    /// Not sure in which condition it is working
    /// </summary>
    public string ProgramComment
    {
      get {
        var programNumber = this.ProgramNumber;
        return GetProgramCommentFromProgramNumber (programNumber);
      }
    }

    /// <summary>
    /// Program comment of the sub program
    /// 
    /// Not sure in which condition it is working
    /// </summary>
    public string SubProgramComment
    {
      get {
        var programNumber = this.SubProgramNumber;
        if (-1 == programNumber) {
          log.Debug ("SubProgramComment.get: no active sub-program");
          throw new Exception ("SubProgramComment: no active sub-program");
        }
        return GetProgramCommentFromProgramNumber (programNumber);
      }
    }

    void GetExecutionPointer ()
    {
      if (-1 != m_blockNumberFromExecutionPointer) {
        log.InfoFormat ("GetExecutionPointer: " +
                        "the execution pointer is already known, " +
                        "ProgramNumber={0} BlockNumber={1}",
                        m_programNumberFromExecutionPointer, m_blockNumberFromExecutionPointer);
        return;
      }

      CheckAvailability ("rdexecpt");
      CheckConnection ();

      Import.FwLib.PRGPNT pact, pnext;
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdexecpt (m_handle, out pact, out pnext);
      ManageErrorWithException ("rdexecpt", result);
      m_programNumberFromExecutionPointer = pact.prog_no;
      m_blockNumberFromExecutionPointer = pact.blk_no;
      log.DebugFormat ("GetExecutionPointer: got ProgramNumber={0} and BlockNumber={1}",
                       m_programNumberFromExecutionPointer, m_blockNumberFromExecutionPointer);
    }

    /// <summary>
    /// Program name from GetExeprgname
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public string GetProgramNameFromExeprgname (string param)
    {
      return GetProgramNameFromExeprgname ();
    }

    string GetProgramNameFromExeprgname ()
    {
      GetExeprgname ();
      return m_programNameFromExeprgname;
    }

    /// <summary>
    /// Program number from GetExeprgname
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public int GetProgramNumberFromExeprgname (string param)
    {
      return GetProgramNumberFromExeprgname ();
    }

    int GetProgramNumberFromExeprgname ()
    {
      GetExeprgname ();
      return m_programNumberFromExeprgname;
    }

    void GetExeprgname ()
    {
      if (null != m_programNameFromExeprgname) {
        log.InfoFormat ("GetExeprgname: " +
                        "the data is already known, " +
                        "name={0} number={1}",
                        m_programNameFromExeprgname, m_programNumberFromExeprgname);
        return;
      }

      CheckAvailability ("exeprgname");
      CheckConnection ();

      Import.FwLib.ODBEXEPRG exeprg;
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.exeprgname (m_handle, out exeprg);
      ManageErrorWithException ("exeprgname", result);
      m_programNameFromExeprgname = exeprg.name;
      m_programNumberFromExeprgname = exeprg.o_num;
      log.DebugFormat ("GetExeprgname: got Name={0} and Number={1}",
                       m_programNameFromExeprgname, m_programNumberFromExeprgname);
    }

    /// <summary>
    /// Program number from GetDtailErr
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public string GetDtailErr (string param)
    {
      log.DebugFormat ("GetDtailErr");
      string errString = null;

      Import.FwLib.ODBERR odberr;
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.getdtailerr (m_handle, out odberr);

      if (result != Import.FwLib.EW.OK) {
        log.DebugFormat ("GetDtailErr: result =", result.ToString ());
      }
      else {
        log.DebugFormat ("GetDtailErr OK");
      }

      if (null != odberr) {
        log.DebugFormat ("GetDtailErr: err_no={0}, err_dtno={1}", odberr.err_no.ToString (), odberr.err_dtno.ToString ());
        errString = odberr.err_no.ToString ();
      }
      else {
        log.DebugFormat ("GetDtailErr: odberr is null");
      }

      return errString;
    }

    /// <summary>
    /// Full program name
    /// 
    /// For example: //CNC_MEM/USER/LIBRARY/O4903
    /// </summary>
    public string FullProgramName
    {
      get {
        CheckAvailability ("exeprgname2");
        CheckConnection ();

        var path_name_builder = new System.Text.StringBuilder (256);
        var result = (Import.FwLib.EW)Import.FwLib.Cnc.exeprgname2 (m_handle, path_name_builder);
        ManageErrorWithException ("exeprgname2", result);
        var fullProgramName = path_name_builder.ToString ();
        log.DebugFormat ("FullProgramName.get: got Name={0}", fullProgramName);
        return fullProgramName;
      }
    }

    /// <summary>
    /// Full main program name
    /// 
    /// For example: //CNC_MEM/USER/LIBRARY/O4903
    /// </summary>
    public string PdfProgramName
    {
      get {
        CheckAvailability ("pdf_rdmain");
        CheckConnection ();

        var path_name_builder = new System.Text.StringBuilder (256);
        var result = (Import.FwLib.EW)Import.FwLib.Cnc.pdf_rdmain (m_handle, path_name_builder);
        ManageErrorWithException ("pdf_rdmain", result);
        var pdfProgramName = path_name_builder.ToString ();
        log.DebugFormat ("PdfProgramName.get: got Name={0}", pdfProgramName);
        return pdfProgramName;
      }
    }

    /// <summary>
    /// Reads full path name of the program which is being currently executed in CNC.
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public string GetFullProgramName (string param)
    {
      return FullProgramName;
    }

    /// <summary>
    /// Reads full path name of the main program which is being currently executed in CNC.
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public string GetPdfProgramName (string param)
    {
      return PdfProgramName;
    }

    /// <summary>
    /// Block counter
    /// </summary>
    public int BlockCounter
    {
      get {
        CheckAvailability ("rdblkcount");
        CheckConnection ();

        int prog_bc;
        var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdblkcount (m_handle, out prog_bc);
        ManageErrorWithException ("rdblkcount", result);
        if (log.IsDebugEnabled) {
          log.DebugFormat ("BlockCounter: got {0}", prog_bc);
        }
        return prog_bc;
      }
    }


    /// <summary>
    /// Get the program content
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    public string GetDirCount (string param)
    {
      log.DebugFormat ("GetDirCount: dir={0}", param);
      string countResult = null;
      CheckAvailability ("rdpdf_subdirn");
      CheckConnection ();

      var dir_name_builder = new System.Text.StringBuilder (param);
      Import.FwLib.ODBPDFNFIL data;
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdpdf_subdirn (m_handle, dir_name_builder, out data);
      log.DebugFormat ("GetDirCount: rdpdf_subdirn result={0}", result);
      ManageErrorWithException ("rdpdf_subdirn", result);

      if (null != data) {
        var programContent = data.ToString ();
        log.DebugFormat ("GetDirCount: got dir={0} dir_num={1} file_num={2}",
          dir_name_builder, data.dir_num, data.file_num);
        countResult = data.file_num.ToString ();
      }
      return countResult;
      ;
    }

    /// <summary>
    /// Get the current executed block program content
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    public string GetProgramContent (string param)
    {
      CheckAvailability ("rdexecprog");
      CheckConnection ();

      const int DEFAULT_LENGTH = 50;

      // Length needed
      ushort length = DEFAULT_LENGTH;
      if (!string.IsNullOrEmpty (param)) {
        if (!ushort.TryParse (param, out length)) {
          log.ErrorFormat ("GetProgramContent: invalid length parameter {0}, consider the default length {1}",
            param, length);
        }
        if (length <= 0) {
          length = DEFAULT_LENGTH;
          log.ErrorFormat ("GetProgramContent: negative length in {0} => switch to a default one {1}",
            param, length);
        }
      }

      var data = new System.Text.StringBuilder (length);
      short blknum;
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdexecprog (m_handle, ref length, out blknum, data);
      ManageErrorWithException ("rdexecprog", result);
      var programContent = data.ToString ();
      if (log.IsDebugEnabled) {
        log.DebugFormat ("GetProgramContent: got blknum={0} data={1}", blknum, programContent);
      }
      return programContent;
    }

    /// <summary>
    /// Get the CNC program content from main program number
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    public IList<String> GetProgramUpload (string param)
    {
      log.DebugFormat ("GetProgramUpload: start program={0}", this.ProgramNumber);
      return GetAnyProgramUpload (param, this.ProgramNumber);
    }

    /// <summary>
    /// Get the CNC program content from subprogram number
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    public IList<String> GetSubProgramUpload (string param)
    {
      log.DebugFormat ("GetSubProgramUpload: start subprogram={0}", this.SubProgramNumber);
      return GetAnyProgramUpload (param, this.SubProgramNumber);
    }

    /// <summary>
    /// Get the CNC program or subprogram content from program number
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    /// <param name="programNumber">program number</param>
    IList<String> GetAnyProgramUpload (string param, int programNumber)
    {
      IList<string> programContent = new List<string> ();

      //var programNumber = this.ProgramNumber;

      log.DebugFormat ("GetAnyProgramUpload: start");
      CheckAvailability ("upstart3");
      CheckConnection ();

      const int DEFAULT_LENGTH = 256;   // 256 multiples
      const ushort UPLOAD_TYPE = 0; // CNC data

      // Length needed
      ushort length = DEFAULT_LENGTH;
      if (!string.IsNullOrEmpty (param)) {
        if (!ushort.TryParse (param, out length)) {
          log.ErrorFormat ("GetAnyProgramUpload: invalid length parameter {0}, consider the default length {1}",
            param, length);
        }
        if (length <= 0) {
          length = DEFAULT_LENGTH;
          log.ErrorFormat ("GetAnyProgramUpload: negative length in {0} => switch to a default one {1}",
            param, length);
        }
      }

      // notify end upload
      log.DebugFormat ("GetAnyProgramUpload: force notify end upload");
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.upend3 (m_handle);
      //ManageErrorWithException ("upend3", result);
      log.DebugFormat ("GetAnyProgramUpload: force notify end upload, result={0}", result);

      // notify start upload
      log.DebugFormat ("GetAnyProgramUpload: notify start upload Length={0}, ProgramNumber={1}", length, programNumber);
      result = (Import.FwLib.EW)Import.FwLib.Cnc.upstart3 (m_handle, UPLOAD_TYPE, (ushort)programNumber, (ushort)programNumber);
      /*if (result == Import.FwLib.EW.DATA) {
        string result1 = GetDtailErr (null);
        log.DebugFormat ("GetAnyProgramUpload: upstart3, detailerr={0}", result1);
      }
      */
      ManageErrorWithException ("upstart3", result);

      // read data
      log.DebugFormat ("GetAnyProgramUpload: upload data");
      var data = new System.Text.StringBuilder (length + 1);

      int nbRead = 0;
      ushort actualLength = 0;
      const int NB_MAX_READ = 5;

      do {
        actualLength = length;
        log.DebugFormat ("GetAnyProgramUpload: nbRead={0}", nbRead);
        result = (Import.FwLib.EW)Import.FwLib.Cnc.upload3 (m_handle, ref actualLength, data);
        nbRead++;
        log.DebugFormat ("GetAnyProgramUpload: upload data result={0}, length={1}, nbRead={2}", result, actualLength, nbRead);

        if (result == Import.FwLib.EW.OK) {
          break;
        }
        else {
          if (result == Import.FwLib.EW.BUFFER) {
            log.DebugFormat ("GetAnyProgramUpload: upload data result=BUFFER, continue");
            Thread.Sleep (100);
          }
          else {
            Thread.Sleep (100);
          }
        }
      } while (nbRead <= NB_MAX_READ);

      if (result == Import.FwLib.EW.OK) {
        // convert result to String list
        log.DebugFormat ("GetAnyProgramUpload: convert result to String list");
        var parts = data.ToString ().Split ('\n');
        log.DebugFormat ("GetAnyProgramUpload: convert result to String list, length={0}", parts.Length);
        for (int i = 0; i < parts.Length; i++) {
          log.DebugFormat ("GetAnyProgramUpload: line {0}=, {1}", i, parts[i]);
          programContent.Add (parts[i]);
        }
      }

      // notify end upload
      log.DebugFormat ("GetAnyProgramUpload: notify end upload");
      result = (Import.FwLib.EW)Import.FwLib.Cnc.upend3 (m_handle);
      ManageErrorWithException ("upend3", result);

      return programContent;
    }

    /// <summary>
    /// Get the CNC program content from program number
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    public IList<String> GetProgramUpload4 (string param)
    {
      log.DebugFormat ("GetProgramUpload4: start program={0}", this.ProgramNumber);
      return GetAnyProgramUpload4 (param, FullProgramName);
    }

    /// <summary>
    /// Get the CNC program content from subprogram full path
    /// Use SubProgramFolder config parameter to build full path with Subprogram number
    /// E.g.: SubProgramFolder=//CNC_MEM/MTB1/
    ///       SubProgramNumber=O9001
    ///       fullpath=//CNC_MEM/MTB1/O9001
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    public IList<String> GetSubProgramUpload4 (string param)
    {
      log.DebugFormat ("GetSubProgramUpload4: start subprogram={0}", this.SubProgramNumber);
      String fullSubProgramName = null;
      // get content only if main is running, not subprogram
      if (!string.IsNullOrEmpty (SubProgramFolder)) {
        // original code
        fullSubProgramName = SubProgramFolder + FullProgramName;

        // TODO: for test purpose only
        //fullSubProgramName = SubProgramFolder;
        //fullSubProgramName = "//172.20.48.201/SGI_MEM/O6";
      }
      else {
        if (FullProgramName.EndsWith (this.SubProgramNumber.ToString ())) {
          fullSubProgramName = FullProgramName;
        }
      }
      log.DebugFormat ("GetSubProgramOperationComment4: fullSubProgramName={0}", fullSubProgramName);
      if (!string.IsNullOrEmpty (fullSubProgramName)) {
        return GetAnyProgramUpload4 (param, fullSubProgramName);
      }
      else {
        return null;
      }
    }

    /// <summary>
    /// Get the CNC program or subprogram content from program number
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    /// <param name="fullProgramName">program number</param>
    IList<String> GetAnyProgramUpload4 (string param, string fullProgramName)
    {
      IList<string> programContent = new List<string> ();

      //var programNumber = this.ProgramNumber;

      log.DebugFormat ("GetAnyProgramUpload4: start");
      CheckAvailability ("upstart4");
      CheckConnection ();

      const int DEFAULT_LENGTH = 256;   // 256 multiples
      const ushort UPLOAD_TYPE = 0; // CNC data

      // Length needed
      ushort length = DEFAULT_LENGTH;
      if (!string.IsNullOrEmpty (param)) {
        if (!ushort.TryParse (param, out length)) {
          log.ErrorFormat ("GetAnyProgramUpload4: invalid length parameter {0}, consider the default length {1}",
            param, length);
        }
        if (length <= 0) {
          length = DEFAULT_LENGTH;
          log.ErrorFormat ("GetAnyProgramUpload4: negative length in {0} => switch to a default one {1}",
            param, length);
        }
      }

      // notify end upload
      log.DebugFormat ("GetAnyProgramUpload4: force notify end upload");
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.upend4 (m_handle);
      //ManageErrorWithException ("upend3", result);
      log.DebugFormat ("GetAnyProgramUpload4: force notify end upload, result={0}", result);

      // notify start upload
      log.DebugFormat ("GetAnyProgramUpload4: notify start upload Length={0}, fullProgramName={1}", length, fullProgramName);
      result = (Import.FwLib.EW)Import.FwLib.Cnc.upstart4 (m_handle, UPLOAD_TYPE, new System.Text.StringBuilder (fullProgramName));
      log.DebugFormat ("GetAnyProgramUpload4: upstart4 result={0}", result);
      /*
      if (result == Import.FwLib.EW.DATA) {
           string result1 = GetDtailErr (null);
           log.DebugFormat ("GetAnyProgramUpload4: upstart4, detailerr={0}", result1);
         }
      */
      ManageErrorWithException ("upstart4", result);

      // read data
      log.DebugFormat ("GetAnyProgramUpload4: upload data");
      var data = new System.Text.StringBuilder (length + 1);

      int nbRead = 0;
      ushort actualLength = 0;
      const int NB_MAX_READ = 5;

      do {
        actualLength = length;
        log.DebugFormat ("GetAnyProgramUpload4: nbRead={0}", nbRead);
        result = (Import.FwLib.EW)Import.FwLib.Cnc.upload4 (m_handle, ref actualLength, data);
        nbRead++;
        log.DebugFormat ("GetAnyProgramUpload4: upload data result={0}, length={1}, nbRead={2}", result, actualLength, nbRead);

        if (result == Import.FwLib.EW.OK) {
          break;
        }
        else {
          if (result == Import.FwLib.EW.BUFFER) {
            log.DebugFormat ("GetAnyProgramUpload4: upload data result=BUFFER, continue");
            Thread.Sleep (100);
          }
          else {
            Thread.Sleep (100);
          }
        }
      } while (nbRead <= NB_MAX_READ);

      if (result == Import.FwLib.EW.OK) {
        // convert result to String list
        // add \0 at end of string (not done by upload4)
        //data[actualLength] = '\0';
        log.DebugFormat ("GetAnyProgramUpload4: convert result to String list");
        var parts = data.ToString ().Split ('\n');
        log.DebugFormat ("GetAnyProgramUpload4: convert result to String list, length={0}", parts.Length);
        for (int i = 0; i < parts.Length; i++) {
          log.DebugFormat ("GetAnyProgramUpload4: line {0}=, {1}", i, parts[i]);
          programContent.Add (parts[i]);
        }
      }

      // notify end upload
      log.DebugFormat ("GetAnyProgramUpload4: notify end upload");
      result = (Import.FwLib.EW)Import.FwLib.Cnc.upend4 (m_handle);
      ManageErrorWithException ("upend4", result);

      return programContent;
    }

    /// <summary>
    /// Get the CNC program content from program number
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    public IList<String> GetProgramUpload1 (string param)
    {
      log.DebugFormat ("GetProgramUpload1: start program={0}", this.ProgramNumber);
      return GetAnyProgramUpload1 (param, this.ProgramNumber);
    }

    /// <summary>
    /// Get the CNC program content from program number
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    public IList<String> GetSubProgramUpload1 (string param)
    {
      log.DebugFormat ("GetSubProgramUpload1: start subprogram={0}", this.SubProgramNumber);
      return GetAnyProgramUpload1 (param, this.SubProgramNumber);
    }
    /// <summary>
    /// Get the CNC program or subprogram content from program number
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    /// <param name="programNumber">program number</param>
    IList<String> GetAnyProgramUpload1 (string param, int programNumber)
    {
      IList<string> programContent = new List<string> ();

      //var programNumber = this.ProgramNumber;

      log.DebugFormat ("GetAnyProgramUpload1: start");
      CheckAvailability ("upstart");
      CheckConnection ();

      const int DEFAULT_LENGTH = 256;   // 256 multiples

      // Length needed
      ushort length = DEFAULT_LENGTH;
      if (!string.IsNullOrEmpty (param)) {
        if (!ushort.TryParse (param, out length)) {
          log.ErrorFormat ("GetAnyProgramUpload1: invalid length parameter {0}, consider the default length {1}",
            param, length);
        }
        if (length <= 0) {
          length = DEFAULT_LENGTH;
          log.ErrorFormat ("GetAnyProgramUpload1: negative length in {0} => switch to a default one {1}",
            param, length);
        }
      }

      // notify end upload
      log.DebugFormat ("GetAnyProgramUpload1: force notify end upload");
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.upend (m_handle);
      //ManageErrorWithException ("upend3", result);
      log.DebugFormat ("GetAnyProgramUpload1: force notify end upload, result={0}", result);

      // notify start upload
      log.DebugFormat ("GetAnyProgramUpload1: notify start upload Length={0}, ProgramNumber={1}", length, programNumber);
      result = (Import.FwLib.EW)Import.FwLib.Cnc.upstart (m_handle, (ushort)programNumber);
      ManageErrorWithException ("upstart", result);

      // read data
      log.DebugFormat ("GetAnyProgramUpload1: upload data");
      var data = new System.Text.StringBuilder (length + 1);

      ushort actualLength = length;

      Import.FwLib.ODBUP uploadData;
      result = (Import.FwLib.EW)Import.FwLib.Cnc.upload (m_handle, out uploadData, ref actualLength);
      log.DebugFormat ("GetAnyProgramUpload1: upload data result={0}, length={1}", result, actualLength);
      ManageErrorWithException ("upload", result);

      if (result == Import.FwLib.EW.OK) {
        // convert result to String list
        log.DebugFormat ("GetAnyProgramUpload1: convert result to String list");
        var parts = uploadData.data.ToString ().Split ('\n');
        log.DebugFormat ("GetAnyProgramUpload1: convert result to String list, length={0}", parts.Length);
        for (int i = 0; i < parts.Length; i++) {
          log.DebugFormat ("GetAnyProgramUpload1: line {0}=, {1}", i, parts[i]);
          programContent.Add (parts[i]);
        }
      }

      // notify end upload
      log.DebugFormat ("GetAnyProgramUpload: notify end upload");
      result = (Import.FwLib.EW)Import.FwLib.Cnc.upend (m_handle);
      ManageErrorWithException ("upend", result);

      return programContent;
    }

    /// <summary>
    /// Get the program comment from current program in FTP network share
    /// </summary>
    /// <param name="param">optional. list: program name prefix, programname extension, comment string to search in program</param>
    public string GetProgramOperationCommentFromFile (string param)
    {
      string result = null;
      // if force reload is set to true, get comment from file each time
      if (!string.IsNullOrEmpty (ForceFTPProgramContentRead) && string.Equals (ForceFTPProgramContentRead, "true")) {
        result = GetAnyProgramOperationCommentFromFile (param, this.ProgramNumber);
      }
      else {
        // read content only if programNumber change
        if (this.ProgramNumber == m_currentProgramNumber) {
          result = m_currentProgramComment;
        }
        else {
          m_currentProgramNumber = this.ProgramNumber;
          result = GetAnyProgramOperationCommentFromFile (param, this.ProgramNumber);
          m_currentProgramComment = result;
        }
      }
      return result;
    }

    /// <summary>
    /// Get the subprogram comment from current program in FTP network share
    /// </summary>
    /// <param name="param">optional. list: program name prefix, programname extension, comment string to search in program</param>
    public string GetSubProgramOperationCommentFromFile (string param)
    {
      string result = null;
      // if force reload is set to true, get comment from file each time
      if (!string.IsNullOrEmpty (ForceFTPProgramContentRead) && string.Equals (ForceFTPProgramContentRead, "true")) {
        result = GetAnyProgramOperationCommentFromFile (param, this.SubProgramNumber);
      }
      else {
        // read content only if programNumber change
        if (this.SubProgramNumber == m_currentSubProgramNumber) {
          result = m_currentSubProgramComment;
        }
        else {
          m_currentSubProgramNumber = this.SubProgramNumber;
          result = GetAnyProgramOperationCommentFromFile (param, this.SubProgramNumber);
          m_currentSubProgramComment = result;
        }
      }
      return result;
    }

    /// <summary>
    /// Get the program comment from current program in FTP network share
    /// Search program in SharedProgramFolder variable, including trailing separator
    /// 
    /// </summary>
    /// <param name="param">list: program name prefix, programname extension, comment string to search in program</param>
    /// <param name="programNumber">program or subprogram number to read</param>
    public string GetAnyProgramOperationCommentFromFile (string param, int programNumber)
    {
      string programName = "";
      string searchString = "";
      if (string.IsNullOrEmpty (param)) {
        // no parameter, use default prefix, extension and search string
        programName = DEFAULT_PROGRAMFILE_PREFIX + programNumber.ToString () + DEFAULT_PROGRAMFILE_EXTENSION;
        searchString = DEFAULT_COMMENT_SEARCH_STRING;
      }
      else {
        var paramItems = Lemoine.Collections.EnumerableString.ParseListString (param);
        if (paramItems.Length == 3) {
          string programNamePrefix = paramItems[0];
          string programNamesuffix = paramItems[1];
          programName = programNamePrefix + programNumber.ToString () + programNamesuffix;
          searchString = paramItems[2];
        }
        else {
          log.Error ("GetProgramOperationCommentFromFile: bad param format: " + param);
          return null;
        }
      }

      string comment = "";
      string programFilePath = "";

      if (!string.IsNullOrEmpty (SharedProgramFolder)) {
        if (!string.IsNullOrEmpty (programName)) {
          programFilePath = Path.Combine (SharedProgramFolder, programName);
          log.DebugFormat ("GetProgramOperationCommentFromFile: program name path={0}", programFilePath);
        }
        else {
          log.DebugFormat ("GetProgramOperationCommentFromFile: no program name");
        }
      }
      else {
        log.ErrorFormat ("GetProgramOperationCommentFromFile: SharedProgramFolder is not set");
      }
      log.DebugFormat ("GetProgramOperationCommentFromFile: path: {0}", programFilePath);
      comment = GetCommentFromFTPFile (programFilePath, searchString);
      return comment;
    }

    /// <summary>
    /// parse the program file and extract first line containing part comment
    /// The path to the program file folder is passed as parameter
    /// </summary>
    /// <param name="programFilePath">FTP path to ptogram file</param>
    /// <param name="searchString">sting to search in file</param>
    /// <returns></returns>
    public string GetCommentFromFTPFile (string programFilePath, string searchString)
    {
      string comment = "";
      string commentSearchString = "";
      bool commentFound = false;
      string fileString = "";
      // TODO ??? test file date for change ????

      if (!string.IsNullOrEmpty (searchString)) {
        commentSearchString = searchString;
      }
      else {
        commentSearchString = DEFAULT_COMMENT_SEARCH_STRING;
      }
      log.DebugFormat ("GetCommentFromFTPFile: file: {0} searchString: {1}", programFilePath, commentSearchString);

      var request = new System.Net.WebClient ();
      request.Credentials = new NetworkCredential ("", "");

      try {

        byte[] newFileData = request.DownloadData (programFilePath);
        fileString = System.Text.Encoding.UTF8.GetString (newFileData);
        log.DebugFormat ("GetCommentFromFTPFile: get string, length={0}", fileString.Length);

        var parts = fileString.Split ('\n');
        log.DebugFormat ("GetCommentFromFTPFile: convert result to String list, length={0}", parts.Length);
        for (int i = 0; i < parts.Length; i++) {
          if (parts[i].IndexOf (commentSearchString, 0) != -1) {
            log.DebugFormat ("GetCommentFromFTPFile: line found: {0}", parts[i]);
            comment = parts[i];
            commentFound = true;
            break;
          }
        }
        if (!commentFound) {
          log.DebugFormat ("GetCommentFromFTPFile: no comment line found in: {0}", programFilePath);
        }
        else {
          log.DebugFormat ("GetCommentFromFTPFile: comment: {0}", comment);
        }
        request.Dispose ();
      }
      catch (WebException e) {
        log.ErrorFormat ("GetCommentFromFTPFile: failed to read file: {0}, {1}", programFilePath, e);
        return null;
      }
      return comment;
    }

    /// <summary>
    /// parse the program file and extract
    /// -header line with program number
    /// -first line starting with subComment set in CommentSearchString
    /// The program is in a network share provided in SharedProgramFolder properties
    /// The network credentials user/domain/password must be provided as properties
    /// </summary>
    /// <param name="programFileName"></param>
    /// <param name="subProgramName"></param>
    /// <param name="subComment"></param>
    /// <returns></returns>
    public bool GetCommentFromNetworkShareFile (string programFileName, out string subProgramName, out string subComment)
    {
      bool result = false;
      string searchString = DEFAULT_COMMENT_SEARCH_STRING;
      if (!string.IsNullOrEmpty (CommentSearchString)) {
        searchString = CommentSearchString;
      }
      bool commentFound = false;
      bool subNameFound = false;
      subProgramName = null;
      subComment = null;
      log.DebugFormat ("GetCommentFromNetworkShareFile: file: {0}", programFileName);

      try {
        var filePath = Path.Combine (SharedProgramFolder, programFileName);
        if (!m_sharedProgramFolderLoginDone) {
          // establish login for the network share
          UNCAccess unc = new UNCAccess ();
          int uncResult = unc.Login (SharedProgramFolder, SharedProgramFolderUser, SharedProgramFolderDomain, SharedProgramFolderPassword);
          m_sharedProgramFolderLoginDone = (0 == uncResult);
          log.DebugFormat ("GetCommentFromNetworkShareFile: UNC login to {0}, result={1}", SharedProgramFolder, uncResult);
        }

        log.DebugFormat ("GetCommentFromNetworkShareFile: filePath: {0}", filePath);
        bool firstLineRead = false;
        using (StreamReader reader = new StreamReader (new FileStream (filePath, FileMode.Open, FileAccess.Read))) {
          while (!commentFound && !reader.EndOfStream) {
            string line = reader.ReadLine ();
            //log.DebugFormat ("GetCommentFromNetworkShareFile: line: {0}", line);
            if (!firstLineRead) {
              if (5 <= line.Length) {
                subProgramName = line.Substring (0, 5);
                log.DebugFormat ("GetCommentFromNetworkShareFile: subProgramName found: {0}", subProgramName);
                subNameFound = true;
              }
              firstLineRead = true;
            }
            else {
              if (line.IndexOf (searchString, 0) != -1) {
                subComment = line;
                log.DebugFormat ("GetCommentFromNetworkShareFile: subComment found: {0}", subComment);
                commentFound = true;
              }
            }
          }
        }

        if (!commentFound) {
          log.DebugFormat ("GetCommentFromNetworkShareFile: no comment line found in: {0}", programFileName);
        }
        result = subNameFound && commentFound;
      }
      catch (Exception e) {
        log.ErrorFormat ("GetCommentFromNetworkShareFile: failed to read file: {0}, {1}", programFileName, e);
        // reinit login to network share
        m_sharedProgramFolderLoginDone = false;
      }
      return result;
    }


    /// <summary>
    /// Get the program comment from current program
    /// Don't read content again if no program change.
    /// ProgramName must already be read
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    public string GetProgramOperationComment (string param)
    {
      string result = null;
      // read content from main only if programNumber change
      if (this.ProgramNumber != m_currentProgramNumber || null == m_currentProgramComment) {
        m_currentProgramNumber = this.ProgramNumber;
        var lines = GetProgramUpload (param);
        result = GetAnyProgramOperationComment (lines);
        m_currentProgramComment = result;
      }
      else {
        log.DebugFormat ("GetProgramOperationComment: no program change, current comment={0}", m_currentProgramComment);
        result = m_currentProgramComment;
      }
      return result;
    }

    /// <summary>
    /// Get the program comment from current program and the revision date
    /// Don't read content again if no program change.
    /// ProgramName must already be read
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    public string GetProgramOperationCommentAndNextLine (string param)
    {
      string result = null;
      // read content from main only if programNumber change
      if (this.ProgramNumber != m_currentProgramNumber || null == m_currentProgramComment) {
        m_currentProgramNumber = this.ProgramNumber;
        var lines = GetProgramUpload (param);
        result = GetAnyProgramOperationCommentAndNextLine (lines);
        m_currentProgramComment = result;
      }
      else {
        log.DebugFormat ("GetProgramOperationCommentAndNextLine: no program change, current comment={0}", m_currentProgramComment);
        result = m_currentProgramComment;
      }
      return result;
    }

    /// <summary>
    /// Get the program comment from main program if present, or sub if not
    /// don't read content again if no program change.
    /// ProgramName and SubProgramName must already be read
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    public string GetProgramOrSubProgramOperationComment (string param)
    {
      IList<String> lines = new List<string> ();
      string result = null;

      // read content from main only if programNumber change
      if (this.ProgramNumber == m_currentProgramNumber) {
        log.DebugFormat ("GetProgramOrSubProgramOperationComment: no program change, current comment={0}", m_currentProgramComment);
        result = m_currentProgramComment;
        // if current program comment AND current sub program comment are empty, force reload (maybe service start)
        if (null == m_currentProgramComment && null == m_currentSubProgramComment) {
          log.DebugFormat ("GetProgramOrSubProgramOperationComment: current comments null, force reload");
          lines = GetProgramUpload (param);
          result = GetAnyProgramOperationComment (lines);
          m_currentProgramComment = result;
        }
      }
      else {
        m_currentProgramNumber = this.ProgramNumber;
        lines = GetProgramUpload (param);
        result = GetAnyProgramOperationComment (lines);
        m_currentProgramComment = result;
      }

      if (null == result) {
        // if no comment in main check in subprogram, but only if it changes
        if (this.ProgramNumber == m_currentProgramNumber && this.SubProgramNumber == m_currentSubProgramNumber) {
          log.DebugFormat ("GetProgramOrSubProgramOperationComment: no subprogram change, current sub comment={0}", m_currentSubProgramComment);
          result = m_currentSubProgramComment;
        }
        else {
          m_currentSubProgramNumber = this.SubProgramNumber;
          lines = GetSubProgramUpload (param);
          result = GetAnyProgramOperationComment (lines);
          m_currentSubProgramComment = result;
        }
      }
      return result;
    }

    /// <summary>
    /// Get the program comment from current program
    /// only if subprogram is not running 
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    public string GetProgramOperationCommentWithTest (string param)
    {
      log.DebugFormat ("GetProgramOperationCommentWithTest: param: {0}", param);
      if (SubProgramNumber != RunningProgramNumber) {
        long cycleVariable = GetMacroLong (param);
        if (0 == cycleVariable || 1 == cycleVariable) {
          var lines = GetProgramUpload (null);
          return GetAnyProgramOperationComment (lines);
        }
      }
      return null;
    }

    /// <summary>
    /// Get the program comment from current program
    /// Don't read content again if no program change.
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    public string GetProgramOperationComment4 (string param)
    {
      string result = null;
      // read content from main only if programNumber change
      if (this.ProgramNumber != m_currentProgramNumber || null == m_currentProgramComment) {
        // get content only if main is running, not subprogram
        if (FullProgramName.EndsWith (this.ProgramNumber.ToString ())) {
          m_currentProgramNumber = this.ProgramNumber;
          var lines = GetProgramUpload4 (param);
          result = GetAnyProgramOperationComment (lines);
          m_currentProgramComment = result;
        }
      }
      else {
        log.DebugFormat ("GetProgramOperationComment4: no program change, current comment={0}", m_currentProgramComment);
        result = m_currentProgramComment;
      }
      return result;
    }

    /// <summary>
    /// Read content of program given as parameter
    /// Only the second line is returned by the function.
    /// Other lines are available in debug log, as "line xx=..."
    /// </summary>
    /// <param name="param">program full path</param>
    public string GetProgramContentFromFileName (string param)
    {
      string resultLine = null;
      log.DebugFormat ("GetProgramContentFromFileName: FileName={0}", param);
      if (null != param) {
        IList<String> fileContent = GetAnyProgramUpload4 (DEFAULT_CNC_UPLOAD_SIZE, param);
        if (null != fileContent) {
          if (fileContent.Count >= 2) {
            resultLine = fileContent[1];
          }
        }
      }
      return resultLine;
    }

    /// <summary>
    /// Read content of program given as parameter
    /// Only the second line is returned by the function.
    /// Other lines are available in debug log, as "line xx=..."
    /// </summary>
    /// <param name="param">program full path</param>
    public string GetProgramContentFromProgramNumber (string param)
    {
      string resultLine = null;
      log.DebugFormat ("GetProgramContentFromProgramNumber: ProgramNumber={0}", param);
      if (null != param) {
        int programNumber = int.Parse (param);
        IList<String> fileContent = GetAnyProgramUpload (DEFAULT_CNC_UPLOAD_SIZE, programNumber);
        if (null != fileContent) {
          if (fileContent.Count >= 2) {
            resultLine = fileContent[1];
          }
        }
      }
      return resultLine;
    }

    /// <summary>
    /// Get the program comment from current subprogram
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    public string GetSubProgramOperationComment4 (string param)
    {
      // get content only if main is running, not subprogram
      if (FullProgramName.EndsWith (this.SubProgramNumber.ToString ())) {
        var lines = GetSubProgramUpload4 (param);
        return GetAnyProgramOperationComment (lines);
      }
      else {
        return null;
      }
    }

    /// <summary>
    /// Get the program comment from current program or sub program
    /// Don't read content again if no program change.
    /// ProgramName must already be read
    /// </summary>
    /// <param name="param">main program full path</param>
    public string GetProgramOperationCommentFromDynamicFileName (string param)
    {
      string result = null;
      // read content from main only if programNumber change
      if (ProgramNumber != m_currentProgramNumber) {
        log.DebugFormat ("GetProgramOperationCommentFromDynamicFileName: program change, old={0}, new={1}", m_currentProgramNumber, ProgramNumber);
        // if main program change, check content, or subprogram if 07996
        m_currentProgramNumber = this.ProgramNumber;
        if (param.EndsWith (ProgramName)) {
          if (!InitSubProgramContentListFromDynamicFileName (param)) {
            log.DebugFormat ("GetProgramOperationCommentFromDynamicFileName: failed to initialize subprogram list");
          }
        }
        else {
          var lines = GetProgramUpload (DEFAULT_CNC_UPLOAD_SIZE);
          result = GetAnyProgramOperationComment (lines);
          if (!string.IsNullOrEmpty (result)) {
            m_currentProgramComment = result;
          }
        }
      }
      else {
        log.DebugFormat ("GetProgramOperationCommentFromDynamicFileName: no program change, current comment={0}", m_currentProgramComment);
        result = m_currentProgramComment;
      }

      // check in subprograms for dynamic main in case of sub change
      if (param.EndsWith (ProgramName)) {
        if (SubProgramNumber != m_currentSubProgramNumber) {
          string subName = 'O' + SubProgramNumber.ToString ().PadLeft (4, '0');
          string comment = m_subprogramList[subName];
          if (!string.IsNullOrEmpty (comment)) {
            m_currentProgramComment = result;
            result = comment;
          }
        }
      }
      return result;
    }


    /// <summary>
    /// Main program (O7996) contains a list of calls to subprograms, M198Pxxx
    /// The Pxxx programs are on a network share. The program name is always "O" + 4 digits
    ///     P812 -> O0812
    ///     The network share path is in "SharedProgramFolder" property
    /// The subprogram number is not the Pxxx, but index of Pxxx in main program
    /// O7996:
    ///     ...
    ///     M198P812(...
    ///     M198P813(...
    ///     M198P814(...
    ///     ....
    /// Subprogram name from subprogram number 
    ///     01 -> P812
    ///     02 -> P813
    ///     03 -> P814
    ///     ....
    /// Read content of program given as parameter
    /// Only the second line is returned by the function.
    /// Other lines are available in debug log, as "line xx=..."
    /// </summary>
    /// <param name="param">program full path</param>
    public bool InitSubProgramContentListFromDynamicFileName (string param)
    {
      log.DebugFormat ("InitSubProgramContentListFromDynamicFileName: FileName={0}", param);
      bool result = false;
      if (null != param) {
        // check if 
        // -main program content change or first run TODO
        // -running program = subprogram and subprogram = Oxx (Ox)
        if (param.EndsWith (ProgramName)) {
          // read main (O7996) to get list of subprogram and build a subprogram number/subprogram programname tag table
          IList<String> fileContent = GetAnyProgramUpload4 (DEFAULT_CNC_UPLOAD_SIZE, param);
          if (null != fileContent) {
            // reset subprogram list
            m_subprogramList.Clear ();
            foreach (string currentLine in fileContent) {
              if (currentLine.StartsWith ("M198")) {
                string subProgramName = null;
                Regex progRegexp = new Regex ("^M198P(?<prog>\\d+)\\(.*", RegexOptions.Compiled);
                var match = progRegexp.Match (currentLine);
                if (match.Success) {
                  if (match.Groups["prog"].Success) {
                    subProgramName = 'O' + match.Groups["prog"].Value.PadLeft (4, '0');
                    log.DebugFormat ("InitSubProgramContentListFromDynamicFileName: subName={0}", subProgramName);
                    // get subprogram index and programname tag from Subprogram 
                    string subProgramComment = null;
                    string subProgramNumber = null;
                    bool subResult = GetCommentFromNetworkShareFile (subProgramName, out subProgramNumber, out subProgramComment);
                    if (subResult) {
                      m_subprogramList.Add (subProgramNumber, subProgramComment);
                      result = true;
                    }
                  }
                }
              }
            }
          }
        }
      }
      return result;
    }

    /// <summary>
    /// Get the program comment from current program and the first tool
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    public string GetProgramOperationCommentAndFirstTool (string param)
    {

      string result = null;
      // read content from main only if programNumber change
      if (this.ProgramNumber != m_currentProgramNumber || null == m_currentProgramComment) {
        m_currentProgramNumber = this.ProgramNumber;
        var lines = GetProgramUpload (param);
        result = GetAnyProgramOperationComment (lines);
        m_currentProgramComment = result;
        // get tool only if comment is not null
        if (null != result) {
          m_currentProgramFirstTool = GetProgramFirstToolFromContent (lines);
        }
      }
      else {
        log.DebugFormat ("GetProgramOperationComment: no program change, current comment={0}", m_currentProgramComment);
        result = m_currentProgramComment;
      }
      return result;
    }

    /// <summary>
    /// Get the program first tool from current program
    ///
    /// </summary>
    public string GetProgramFirstTool (string param)
    {
      string result = null;

      // read content from main only if programNumber change
      if (this.ProgramNumber != m_currentProgramNumber || null == m_currentProgramFirstTool) {
        m_currentProgramNumber = this.ProgramNumber;
        var lines = GetProgramUpload (param);
        result = GetProgramFirstToolFromContent (lines);
        m_currentProgramFirstTool = result;
      }
      else {
        log.DebugFormat ("GetProgramFirstTool: no program change, current first tool={0}", m_currentProgramFirstTool);
        result = m_currentProgramFirstTool;
      }
      return result;
    }

    /// <summary>
    /// Get the subprogram comment from current program
    /// Don't read content again if no program change.
    /// ProgramName must already be read    /// </summary>
    /// <param name="param">Number of characters to read</param>
    public string GetSubProgramOperationComment (string param)
    {
      string result = null;
      // read content from main only if programNumber change
      if (this.SubProgramNumber != m_currentSubProgramNumber || null == m_currentSubProgramComment) {
        m_currentSubProgramNumber = this.SubProgramNumber;
        var lines = GetSubProgramUpload (param);
        result = GetAnyProgramOperationComment (lines);
        m_currentProgramComment = result;
      }
      else {
        log.DebugFormat ("GetSubProgramOperationComment: no program change, current comment={0}", m_currentProgramComment);
        result = m_currentProgramComment;
      }
      return result;
    }

    /// <summary>
    /// Get the program comment from current program
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    public string GetProgramOperationComment1 (string param)
    {
      var lines = GetProgramUpload1 (param);
      return GetAnyProgramOperationComment (lines);
    }

    /// <summary>
    /// Get the subprogram comment from current program
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    public string GetSubProgramOperationComment1 (string param)
    {
      var lines = GetSubProgramUpload1 (param);
      return GetAnyProgramOperationComment (lines);
    }

    /// <summary>
    /// Get the program ot subprogram comment from current program
    /// </summary>
    /// <param name="lines">program content</param>
    public string GetAnyProgramOperationComment (IList<string> lines)
    {
      log.DebugFormat ("GetAnyProgramOperationComment: start");
      string searchString = DEFAULT_COMMENT_SEARCH_STRING;
      if (!string.IsNullOrEmpty (CommentSearchString)) {
        searchString = CommentSearchString;
      }

      string result = null;

      if (lines == null) {
        log.DebugFormat ("GetAnyProgramOperationComment - null response");
      }
      else {
        // Extract the single result
        if (lines.Count == 0) {
          log.DebugFormat ("GetAnyProgramOperationComment - empty response");
        }
        else {
          foreach (var line in lines) {
            if (line.IndexOf (searchString, 0) != -1) {
              log.DebugFormat ("GetAnyProgramOperationComment '{0}' match", searchString);
              result = line.Trim ();
              break;
            }
          }
        }
        if (string.IsNullOrEmpty (result)) {
          log.DebugFormat ("GetAnyProgramOperationComment - cannot get line containing {0}", searchString);
          return null;
        }
      }

      log.DebugFormat ("GetAnyProgramOperationComment: comment={0}", result);
      return result;
    }

    /// <summary>
    /// Get the program ot subprogram comment and revision date from current program
    /// return line containing DEFAULT_COMMENT_SEARCH_STRING
    /// plus line 
    /// </summary>
    /// <param name="lines">program content</param>
    public string GetAnyProgramOperationCommentAndNextLine (IList<string> lines)
    {
      log.DebugFormat ("GetAnyProgramOperationCommentAndNextLine: start");
      string searchString = DEFAULT_COMMENT_SEARCH_STRING;
      string searchNextLineString = DEFAULT_COMMENT_SEARCH_NEXT_LINE_STRING;
      if (!string.IsNullOrEmpty (CommentSearchString)) {
        searchString = CommentSearchString;
      }
      if (!string.IsNullOrEmpty (CommentSearchStringNextLine)) {
        searchNextLineString = CommentSearchStringNextLine;
      }

      string comment = null;
      string commentNextLine = null;
      bool commentFound = false;
      bool commentNextLineFound = false;

      if (lines == null) {
        log.DebugFormat ("GetAnyProgramOperationCommentAndNextLine - null response");
      }
      else {
        // Extract the single result
        if (lines.Count == 0) {
          log.DebugFormat ("GetAnyProgramOperationCommentAndNextLine - empty response");
        }
        else {
          string line = null;
          for (int i = 0; i < lines.Count; i++) {
            line = lines[i];
            log.DebugFormat ("GetAnyProgramOperationCommentAndNextLine line ='{0}'", line);
            // search first comment
            if (line.IndexOf (searchString, 0) != -1) {
              log.DebugFormat ("GetAnyProgramOperationCommentAndNextLine '{0}' match", searchString);
              comment = line.Trim ();
              commentFound = true;
            }
            // search second comment
            if (!string.IsNullOrEmpty (searchNextLineString)) {
              if (line.IndexOf (searchNextLineString, 0) != -1) {
                log.DebugFormat ("GetAnyProgramOperationCommentAndNextLine '{0}' match", searchNextLineString);
                if (i < lines.Count - 1) {
                  commentNextLine = lines[i + 1];
                  log.DebugFormat ("GetAnyProgramOperationCommentAndNextLine got '{0}'", commentNextLine);
                }
                commentNextLineFound = true;
              }
            }
            if (commentFound && commentNextLineFound) {
              break;
            }
          }
        }
        if (string.IsNullOrEmpty (comment)) {
          log.DebugFormat ("GetAnyProgramOperationCommentAndNextLine - cannot get line containing {0}", searchString);
          return null;
        }
      }

      string result = null;
      if (!string.IsNullOrEmpty (searchNextLineString)) {
        result = ";" + comment + ";" + commentNextLine;
      }
      else {
        result = comment;
      }

      log.DebugFormat ("GetAnyProgramOperationCommentAndNextLine: result={0}", result);
      return result;
    }

    /// <summary>
    /// Get the program first tool from current program
    /// </summary>
    /// <param name="lines">program content</param>
    public string GetProgramFirstToolFromContent (IList<string> lines)
    {
      //log.DebugFormat ("GetProgramFirstToolFromContent: start");

      string searchString = DEFAULT_COMMENT_STRING;
      string result = null;

      if (lines == null) {
        log.DebugFormat ("GetProgramFirstToolFromContent - null response");
      }
      else {
        // Extract the single result
        if (lines.Count == 0) {
          log.DebugFormat ("GetProgramFirstToolFromContent - empty response");
        }
        else {
          foreach (var line in lines) {
            if (null != line) {
              if (line.IndexOf (searchString, 0) == -1) {
                // ignore comment lines, search for toolnumber
                Regex toolNumberRegexp = new Regex (PROGRAM_FIRST_TOOL_REGEXP, RegexOptions.Compiled);
                var match = toolNumberRegexp.Match (line);
                if (match.Success) {
                  log.DebugFormat ("GetProgramFirstToolFromContent '{0}' match", line);
                  if (match.Groups["toolnumber"].Success) {
                    int number;
                    if (!int.TryParse (match.Groups["toolnumber"].Value.Trim (), out number)) {
                      log.DebugFormat ("GetProgramFirstToolFromContent bad tool format");
                    }
                    else {
                      result = match.Groups["toolnumber"].Value.Trim ();
                    }
                  }
                  break;
                }
              }
            }
          }
        }
        if (null == result) {
          log.DebugFormat ("GetProgramFirstToolFromContent - cannot get line containing first tool");
        }
      }
      log.DebugFormat ("GetProgramFirstToolFromContent: tool={0}", result);
      return result;
    }


    /// <summary>
    /// Get the program content byt lines
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    public string GetProgramLines (string param)
    {
      CheckAvailability ("rdprogline");
      CheckConnection ();
      var programNumber = this.ProgramNumber;
      var programContent = "";

      const int DEFAULT_LENGTH = 256;
      const int DEFAULT_LINES = 5;

      // Length needed
      ushort length = DEFAULT_LENGTH;
      ushort lines = DEFAULT_LINES;
      if (!string.IsNullOrEmpty (param)) {
        if (!ushort.TryParse (param, out length)) {
          log.ErrorFormat ("GetProgramLines: invalid length parameter {0}, consider the default length {1}",
            param, length);
        }
        if (length <= 0) {
          length = DEFAULT_LENGTH;
          log.ErrorFormat ("GetProgramLines: negative length in {0} => switch to a default one {1}",
            param, length);
        }
      }

      var data = new System.Text.StringBuilder (length);
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdprogline (m_handle, (ushort)programNumber, 0, data, ref lines, ref length);
      log.DebugFormat ("GetProgramLines: result={0}, lines={1}, length={2}", result, lines, length);

      ManageErrorWithException ("rdprogline", result);
      if (result == Import.FwLib.EW.OK) {
        programContent = data.ToString ();
        log.DebugFormat ("GetProgramLines: content={0}", result);
      }
      return programContent;
    }

    /// <summary>
    /// Get the program content byt lines
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    public string GetProgramLines2 (string param)
    {
      CheckAvailability ("rdprogline2");
      CheckConnection ();
      var programNumber = this.ProgramNumber;
      var programContent = "";

      const int DEFAULT_LENGTH = 256;
      const int DEFAULT_LINES = 5;

      // Length needed
      ushort length = DEFAULT_LENGTH;
      ushort lines = DEFAULT_LINES;
      if (!string.IsNullOrEmpty (param)) {
        if (!ushort.TryParse (param, out length)) {
          log.ErrorFormat ("GetProgramLines2: invalid length parameter {0}, consider the default length {1}",
            param, length);
        }
        if (length <= 0) {
          length = DEFAULT_LENGTH;
          log.ErrorFormat ("GetProgramLines2: negative length in {0} => switch to a default one {1}",
            param, length);
        }
      }

      var data = new System.Text.StringBuilder (length);
      var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdprogline2 (m_handle, (ushort)programNumber, 0, data, ref lines, ref length);
      log.DebugFormat ("GetProgramLines: result={0}, lines={1}, length={2}", result, lines, length);

      ManageErrorWithException ("rdprogline2", result);
      if (result == Import.FwLib.EW.OK) {
        programContent = data.ToString ();
        log.DebugFormat ("GetProgramLines2: content={0}", result);
      }
      return programContent;
    }
    /// <summary>
    /// Cnc sequence number
    /// </summary>
    public int CncSequenceNumber
    {
      get {
        CheckAvailability ("rdseqnum");
        CheckConnection ();

        Import.FwLib.ODBSEQ seqnum;
        var result = (Import.FwLib.EW)Import.FwLib.Cnc.rdseqnum (m_handle, out seqnum);
        ManageErrorWithException ("rdseqnum", result);
        if (log.IsDebugEnabled) {
          log.DebugFormat ("CncSequenceNumber.get: got {0}", seqnum.data);
        }
        return seqnum.data;
      }
    }
  }
}
