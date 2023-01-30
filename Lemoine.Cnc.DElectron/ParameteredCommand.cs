// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Linq;
using prjZ32ComLib;
using Lemoine.Core.Log;

namespace Lemoine.Cnc
{
  /// <summary>
  /// This class represents a DElectron command associated with its parameters
  /// The result can also attached once it is available
  /// 
  /// Example: if the input is "FCODOS16_4_AXIS_NAMES:0:0"
  /// The command is the enum value corresponding to FCODOS16_4_AXIS_NAMES
  /// The input objects will be "0" and "0"
  /// (no subcommand here)
  /// </summary>
  public class ParameteredCommand
  {
    #region Members
    readonly string m_strCommand;
    #endregion // Members
    
    static readonly ILog log = LogManager.GetLogger(typeof (ParameteredCommand).FullName);
    
    #region Getters / Setters
    /// <summary>
    /// DElectron command (FCODOS_READ_DOUBLEWORD_SPECIFIC, FCODOS_READ_QUAD_WORD_SPECIFIC)
    /// Filled based on the input string
    /// </summary>
    public Z32FastCodosCommands Command { get; private set; }
    
    /// <summary>
    /// DElectron sub command, can be a value of several enum types
    /// (Z32FastCodos16Subtypes or Z32FastCodos17Subtypes, depending on the command)
    /// Filled based on the input string
    /// </summary>
    public object SubCommand { get; private set; }
    
    /// <summary>
    /// Object associated to the command, as input
    /// Filled based on the input string (elements separated by ":")
    /// </summary>
    public object[] InputObjects { get; private set; }
    
    /// <summary>
    /// Number of outputs expected by the command
    /// Filled based on the input string
    /// </summary>
    public int OutputNumber { get; private set; }
    
    /// <summary>
    /// Result associated to the command (convenient storage here)
    /// </summary>
    public object[] Result { get; set; }
    
    /// <summary>
    /// Return true if the result has already been read
    /// </summary>
    public long Version { get; set; }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="param"></param>
    public ParameteredCommand(string param)
    {
      m_strCommand = param;
      
      // Default values
      Command = Z32FastCodosCommands.FCODOS_GENERIC_COMMAND;
      SubCommand = null;
      InputObjects = null;
      OutputNumber = 0;
      Result = null;
      Version = 0;
      
      // Recognize the command and subcommands
      FillCommand(param);
      
      // Add input objects
      FillInputObjects(param);
    }
    #endregion // Constructors
    
    #region Equals and GetHashCode implementation
    /// <summary>
    /// Cf parent
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      // We try as a string
      var otherStrCommand = obj as string;
      if (otherStrCommand != null) {
        return this.m_strCommand == otherStrCommand;
      }
      else {
        // We try as another ParameteredCommand
        var other = obj as ParameteredCommand;
        if (other != null) {
          return (this.m_strCommand == other.m_strCommand);
        }
        else {
          // Nothing else allowed
          return false;
        }
      }
    }

    /// <summary>
    /// Cf parent
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        if (m_strCommand != null) {
          hashCode += 1000000007 * m_strCommand.GetHashCode();
        }
      }
      return hashCode;
    }

    /// <summary>
    /// Cf parent
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static bool operator ==(ParameteredCommand lhs, ParameteredCommand rhs) {
      if (ReferenceEquals(lhs, rhs)) {
        return true;
      }

      if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null)) {
        return false;
      }

      return lhs.Equals(rhs);
    }

    /// <summary>
    /// Cf parent
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static bool operator !=(ParameteredCommand lhs, ParameteredCommand rhs) {
      return !(lhs == rhs);
    }
    #endregion // Equals and GetHashCode implementation
    
    #region Methods
    void FillCommand(string param)
    {
      bool commandRecognized = false;
      string[] splitCommand = param.Split(':');
      
      if (splitCommand.Any()) {
        string txtCommand = splitCommand[0];
        
        // Try to recognize a Z32FastCodos16Subtypes command
        try {
          SubCommand = Enum.Parse(typeof(Z32FastCodos16Subtypes), txtCommand);
          Command = Z32FastCodosCommands.FCODOS_READ_DOUBLEWORD_SPECIFIC;
          OutputNumber = 1;
          commandRecognized = true;
        } catch (ArgumentException) {}
        
        // Try to recognize a Z32FastCodos17Subtypes command
        if (!commandRecognized) {
          try {
            SubCommand = Enum.Parse(typeof(Z32FastCodos17Subtypes), txtCommand);
            Command = Z32FastCodosCommands.FCODOS_READ_QUADWORD_SPECIFIC;
            OutputNumber = 1;
            commandRecognized = true;
          } catch (ArgumentException) {}
        }
      }
      
      if (!commandRecognized) {
        ThrowException (param, null);
      }
    }
    
    void FillInputObjects(string param)
    {
      string[] splitCommand = param.Split(':');
      
      // We skip the first part => this is the command
      int count = splitCommand.Count() - 1;
      
      // We try to convert each argument as an int
      InputObjects = new object[count];
      try {
        for (int i = 0; i < count; i++) {
          InputObjects[i] = int.Parse(splitCommand[i + 1]);
        }
      } catch (Exception e) {
        ThrowException(param, e);
      }
    }
    
    void ThrowException(string param, Exception e)
    {
      string text = "Invalid DElectron command " + param + "\n";
      if (e != null) {
        text += e.ToString();
      }

      log.ErrorFormat (text);
      throw new ArgumentException(text);
    }
    #endregion // Methods
  }
}
