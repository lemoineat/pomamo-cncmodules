// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.Linq;
using AxprjZ32ComLib;
using Lemoine.Core.Log;
using prjZ32ComLib;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of DElectronCommands.
  /// </summary>
  public class DElectronCommands
  {
    #region Events
    /// <summary>
    /// Event emitted when the commands are not received anymore: a reset a required
    /// </summary>
    public event Action ResetConnection;
    #endregion // Events
    
    #region Members
    readonly IList<ParameteredCommand> m_commands = new List<ParameteredCommand>();
    readonly AxZ32FastCodos m_FC = null;
    bool m_commandsInitialized = false;
    long m_version = 0;
    int m_notReadyCount = 0;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (DElectronCommands).FullName);
    const int MAX_NOT_READY_NB = 5;

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public DElectronCommands(AxZ32FastCodos FC) { m_FC = FC; }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Called when start is asked
    /// The DElectron machine is supposed to be connected here
    /// </summary>
    public void Start()
    {
      m_version++;
      if (m_commandsInitialized) {
        // Commands are already initialized, we send them (no matter the result has been received or not)
        m_FC.SendCommands();
        
        // We count the number of time the commands are asked
        m_notReadyCount++; // reset to 0 when commands are ready
        if (m_notReadyCount > MAX_NOT_READY_NB) {
          ResetConnection();
          m_notReadyCount = 0;
        }
      } else {
        // Commands are not initialized yet. If m_commands is not empty,
        // this means that this is not the first start and that we can initialize them
        if (m_commands.Count > 0) {
          InitializeCommands ();
        }
      }
    }
    
    void InitializeCommands()
    {
      try {
        m_FC.ClearCommandsList();
        
        // Add all commands
        object [] objects;
        foreach (ParameteredCommand command in m_commands) {
          command.Version = m_version - 1;
          objects = new object[command.InputObjects.Count() + 1];
          objects[0] = command.SubCommand;
          for (int i = 0; i < command.InputObjects.Count(); i++) {
            objects[i+1] = command.InputObjects[i];
          }

          m_FC.AddCommandToList(command.Command, ref objects);
        }
        
        // End
        objects = null;
        m_FC.AddCommandToList(Z32FastCodosCommands.FCODOS_END_BLOCK, ref objects);
        m_commandsInitialized = true;
        
        // Ask for an answer
        m_FC.SendCommands();
      } catch (Exception e) {
        const string text = "Couldn't initialize commands";
        log.Error(text, e);
        throw new Exception(text, e);
      }
    }
    
    /// <summary>
    /// Get a value stored
    /// Throw an exception if it's not stored yet
    /// 
    /// At the beginning, each new command is stored. Then the ActiveX is configured
    /// and the values will be stored.
    /// </summary>
    /// <param name="param"></param>
    /// <param name="isConnected"></param>
    /// <returns></returns>
    public object[] GetValue(string param, bool isConnected)
    {
      if (m_commandsInitialized) {
        // Commands are already initialized
        if (isConnected) {
          foreach (ParameteredCommand paramCommand in m_commands) {
            if (paramCommand.Equals(param)) {
              // We read only one time the result
              if (paramCommand.Version < m_version - 1 || paramCommand.Result == null) {
                throw new Exception("Value not updated yet");
              }
              else {
                return paramCommand.Result;
              }
            }
          }
          throw new Exception("Value not found!");
        } else {
          // Back to the not-initialized state
          m_commandsInitialized = false;
          throw new Exception("DElectron machine not connected");
        }
      } else {
        // Commands are not initialized yet, we store them
        StoreCommand(param);
        throw new Exception("Commands not initialized yet");
      }
    }
    
    void StoreCommand(string param)
    {
      bool isAlreadyStored = false;
      foreach (ParameteredCommand paramCommand in m_commands) {
        if (paramCommand.Equals(param)) {
          isAlreadyStored = true;
          break;
        }
      }
      log.InfoFormat("Store {0}: {1}", param, !isAlreadyStored);
      if (!isAlreadyStored) {
        m_commands.Add(new ParameteredCommand(param));
      }
    }
    
    /// <summary>
    /// Store the values retrieved from the DElectron machine
    /// </summary>
    public void CommandsReady()
    {
      if (m_commandsInitialized) {
        for (short i = 1; i <= m_commands.Count; i++) {
          ParameteredCommand command = m_commands[i-1];
          command.Version = m_version;
          var output = new object[command.OutputNumber];
          for (int j = 0; j < command.OutputNumber; j++) {
            output[j] = 0;
          }

          try {
            m_FC.GetCommandResult(i, ref output);
          } catch (Exception e) {
            string text = "Couldn't retrieve a value for \"" + command.SubCommand + "\"";
            log.Error(text, e);
            throw new Exception(text, e);
          }
          
          command.Result = output;
        }
        
        m_notReadyCount = 0;
      }
    }
    #endregion // Methods
  }
}