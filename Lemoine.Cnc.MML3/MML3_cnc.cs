// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Diagnostics;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of MML3_position.
  /// </summary>
  public partial class MML3
  {
    bool m_cncExecInitialized = false;
    UInt32 m_cncMainONumber = 0;
    UInt32 m_cncCurrentONumber = 0;
    UInt32 m_cncSequenceNumber = 0;

    bool m_mcodeInCache = false;
    int? m_mcodeValue = null;
    int? m_mcodeRequest = null;

    #region Getters / Setters
    /// <summary>
    /// Cnc sequence number
    /// </summary>
    public int CncSequenceNumber
    {
      get
      {
        if (!m_cncExecInitialized) {
          ReadExecutingNcpInformation ();
        }
        return (int)m_cncSequenceNumber;
      }
    }

    /// <summary>
    /// Main Cnc sequence number only
    /// </summary>
    public int MainCncSequenceNumber
    {
      get
      {
        if (!m_cncExecInitialized) {
          ReadExecutingNcpInformation ();
        }
        if (m_cncMainONumber == m_cncCurrentONumber) {
          return (int)m_cncSequenceNumber;
        }
        else {
          throw new Exception ("The main program is not active to get the cnc sequence number");
        }
      }
    }

    /// <summary>
    /// Sub Cnc sequence number only
    /// </summary>
    public int SubCncSequenceNumber
    {
      get
      {
        if (!m_cncExecInitialized) {
          ReadExecutingNcpInformation ();
        }
        if (m_cncMainONumber != m_cncCurrentONumber) {
          return (int)m_cncSequenceNumber;
        }
        else {
          throw new Exception ("The sub program is not active to get the cnc sequence number");
        }
      }
    }

    /// <summary>
    /// Cnc current O number
    /// </summary>
    public int CncCurrentONumber
    {
      get
      {
        if (!m_cncExecInitialized) {
          ReadExecutingNcpInformation ();
        }
        return (int)m_cncCurrentONumber;
      }
    }

    /// <summary>
    /// Cnc main O number
    /// </summary>
    public int CncMainONumber
    {
      get
      {
        if (!m_cncExecInitialized) {
          ReadExecutingNcpInformation ();
        }
        return (int)m_cncMainONumber;
      }
    }

    /// <summary>
    /// Get the execution block
    /// </summary>
    /// <param name="param">Number of characters to read</param>
    public string GetExecBlock (string param)
    {
      CheckCncConnection ();

      const int DEFAULT_LENGTH = 50;

      // Length needed
      short length = DEFAULT_LENGTH;
      if (!string.IsNullOrEmpty (param)) {
        if (!short.TryParse (param, out length)) {
          log.ErrorFormat ("GetExecBlock: invalid length parameter {0}, consider the default length {1}",
            param, length);
        }
        if (length <= 0) {
          length = DEFAULT_LENGTH;
          log.ErrorFormat ("GetExecBlock: negative length in {0} => switch to a default one {1}",
            param, length);
        }
      }

      // Read the block
      var execBlock = new System.Text.StringBuilder (length);
      var result = Md3Cnc.exec_block (m_cncHandle, execBlock, length);
      ManageCncResult ("exec_block", result);
      return execBlock.ToString ();
    }

    /// <summary>
    /// Get the M code that corresponds to the current executing block
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public int GetCurrentBlockMCode (string param)
    {
      ReadMCode ();
      Debug.Assert (m_mcodeValue.HasValue);
      Debug.Assert (m_mcodeRequest.HasValue);
      if (0 == m_mcodeRequest.Value) {
        log.InfoFormat ("GetCurrentBlockMCode: m code {0} in not requested in the current executing block", 
          m_mcodeValue);
        throw new Exception ("The M code does not correspond to the current executing block");
      }

      log.InfoFormat ("GetCurrentBlockMCode: m code {0} is in the current executing block", m_mcodeValue);
      return m_mcodeValue.Value;
    }

    /// <summary>
    /// Get the last requested M code
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public int GetLastMCode (string param)
    {
      ReadMCode ();
      Debug.Assert (m_mcodeValue.HasValue);
      Debug.Assert (m_mcodeRequest.HasValue);
      log.InfoFormat ("GetLastBlockMCode: m code={0}", m_mcodeValue);
      return m_mcodeValue.Value;
    }

    void ReadMCode ()
    {
      if (!m_mcodeInCache) {
        CheckCncConnection ();

        uint codeVal;
        ushort request;
        var result = Md3Cnc.modal_mcode (m_cncHandle, out codeVal, null, out request);
        ManageCncResult ("modal_mcode", result);
        m_mcodeValue = (int)codeVal;
        m_mcodeRequest = (int)request;
        m_mcodeInCache = true;
      }
    }

    /// <summary>
    /// Get the EOP bit (End Of Program: G52#2)
    /// 
    /// Not sure what it really coresponds to, but it can't be used to know if the M30 has just been executed
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public bool GetEndOfProgram (string param)
    {
      CheckCncConnection ();

      bool eop;
      var result = Md3Cnc.chk_eop (m_cncHandle, out eop);
      ManageCncResult ("chk_eop", result);
      log.InfoFormat ("GetEndOfProgram: return {0}", eop);
      return eop;
    }

    /// <summary>
    /// Program comment
    /// 
    /// This does not work when the program is loaded in memory
    /// </summary>
    public string ProgramComment
    {
      get
      {
        CheckCncConnection ();

        var oNumber = CncMainONumber;
        return GetProgramComment (oNumber);
      }
    }

    /// <summary>
    /// Sub-program comment
    /// 
    /// This does not work when the program is loaded in memory
    /// </summary>
    public string SubProgramComment
    {
      get
      {
        CheckCncConnection ();

        var currentONumber = CncCurrentONumber;
        var mainONumber = CncMainONumber;
        if (currentONumber == mainONumber) {
          log.DebugFormat ("Same O Number (no active sub-program)");
          throw new Exception ("ProgramSubComment: no active sub-program");
        }

        return GetProgramComment (currentONumber);
      }
    }

    /// <summary>
    /// Get the program comment of a specific oNumber
    /// 
    /// This does not work when the program is loaded in memory
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public string GetProgramCommentFromName (string param)
    {
      int oNumber = int.Parse (param);
      return GetProgramComment (oNumber);
    }
    #endregion // Getters / Setters

    #region Private methods
    string GetProgramComment (int oNumber)
    {
      CheckCncConnection ();

      bool exist;
      var comment = new System.Text.StringBuilder (Md3Cnc.CMMT_STR_L);
      int size;
      var result = Md3Cnc.mem_dir (m_cncHandle, oNumber, out exist, comment, out size);
      ManageCncResult ("mem_dir", result);
      if (!exist) {
        log.ErrorFormat ("GetProgramComment: O number {0} does not exist", oNumber);
        throw new Exception ("Requested O number does not exist");
      }
      return comment.ToString ();
    }

    void ReadExecutingNcpInformation ()
    {
      CheckCncConnection ();

      // Read the data
      var result = Md3Cnc.exec_ncp (m_cncHandle, out m_cncMainONumber, out m_cncCurrentONumber, out m_cncSequenceNumber);
      ManageCncResult ("exec_ncp", result);
      m_cncExecInitialized = true;
    }

    void CncStart ()
    {
      m_cncExecInitialized = false;

      { // M code cache
        m_mcodeInCache = false;
        m_mcodeValue = null;
        m_mcodeRequest = null;
      }
    }
    #endregion Private methods
  }
}
