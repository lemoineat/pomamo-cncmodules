// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Interface_error.
  /// </summary>
  public class Interface_error: GenericInterface<HeidenhainDNCLib.IJHError3>
  {
    #region Members
    bool m_alarmsInitialized = false;
    readonly IDictionary<int, IList<CncAlarm>> m_alarms = new Dictionary<int, IList<CncAlarm>>();
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Interface_error() : base(HeidenhainDNCLib.DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHERROR) {}
    #endregion // Constructors

    #region Protected methods
    /// <summary>
    /// Extra work during the initialization
    /// Already within a try catch
    /// </summary>
    protected override void InitializeInstance(InterfaceParameters parameters)
    {
      // Nothing
    }
    
    /// <summary>
    /// Work to do on "start", before new data will be read
    /// Already within a try catch
    /// </summary>
    protected override void ResetDataInstance()
    {
      m_alarmsInitialized = false;
      m_alarms.Clear();
    }
    #endregion // Protected methods
    
    #region Get methods
    /// <summary>
    /// Get all alarms
    /// </summary>
    /// <param name="param">channel number</param>
    /// <returns></returns>
    public IList<CncAlarm> GetAlarms(string param)
    {
      if (!m_alarmsInitialized) {
        InitializeAlarms ();
      }

      if (!m_alarmsInitialized) {
        throw new Exception("HeidenhainDNC::Alarms - Couldn't initialize alarms");
      }

      int channel = int.Parse(param);
      return m_alarms.ContainsKey(channel) ? m_alarms[channel] : new List<CncAlarm>();
    }
    #endregion // Get methods
    
    #region Private methods
    void InitializeAlarms()
    {
      // Get all errors
      object errorGroup = (Int32)0;
      object errorNumber = (Int32)0;
      object errorClass = (Int32)0;
      object errorStr = "";
      object errorChannel = (Int32)0;
      
      m_interface.GetFirstError(ref errorGroup, ref errorNumber, ref errorClass, ref errorStr, ref errorChannel);
      
      // Create as many alarms that we can
      while (errorGroup != null)
      {
        // Check the variables
        var errorGroup2 = HeidenhainDNCLib.DNC_ERROR_GROUP.DNC_EG_NONE;
        try {
          errorGroup2 = (HeidenhainDNCLib.DNC_ERROR_GROUP)errorGroup;
        } catch (Exception e) {
          Logger.ErrorFormat("HeidenhainDNC.InitializeAlarms - Couldn't get the error group from '{0}': {1}", errorGroup, e);
        }
        if (errorGroup2 == HeidenhainDNCLib.DNC_ERROR_GROUP.DNC_EG_NONE) {
          break; // Stop the loop
        }

        if (errorNumber == null) {
          Logger.WarnFormat("HeidenhainDNC.InitializeAlarms - error number is null");
          errorNumber = "";
        } else if (errorGroup2 == HeidenhainDNCLib.DNC_ERROR_GROUP.DNC_EG_PLC) {
          try {
            // Remove a flag so that values are not negative
            uint number = unchecked((uint)errorNumber);
            if (number >= 0x81000000) {
              // Flag ok (PLC error messages start at offset 0x81000000 according to the documentation)
              errorNumber = number - 0x81000000;
            } else {
              // No flag: log the number
              Logger.ErrorFormat("HeidenhainDNC.InitializeAlarms - a PLC error doesn't start at offset 0x81000000: '{0}'", errorNumber);
            }
          } catch (Exception e) {
            Logger.WarnFormat("HeidenhainDNC.InitializeAlarms - Couldn't remove a flag from '{0}': {1}", errorNumber, e);
          }
        }
        
        int errorChannel2 = 0;
        if (errorChannel == null) {
          Logger.WarnFormat("HeidenhainDNC.InitializeAlarms - error channel is null, keep default value 0");
        } else {
          try {
            errorChannel2 = (int)errorChannel;
          } catch (Exception e) {
            Logger.ErrorFormat("HeidenhainDNC.InitializeAlarms - Couldn't get the error channel from '{0}', keep default value 0: {1}", errorChannel, e);
          }
        }
        
        // Prepare the alarm
        var alarm = new CncAlarm("HeidenhainDNC", GetErrorGroup(errorGroup), errorNumber.ToString());
        alarm.Message = (errorStr == null ? "-" : errorStr.ToString());
        alarm.Properties["severity"] = GetErrorClass(errorClass);
        
        // Store it by channel number
        if (!m_alarms.ContainsKey(errorChannel2)) {
          m_alarms[errorChannel2] = new List<CncAlarm>();
        }

        m_alarms[errorChannel2].Add(alarm);
        
        m_interface.GetNextError(ref errorGroup, ref errorNumber, ref errorClass, ref errorStr, ref errorChannel);
      }
      
      m_alarmsInitialized = true;
    }
    
    string GetErrorGroup(object errorGroup)
    {
      if (errorGroup == null) {
        return "unknown";
      }

      string result = "";
      
      try {
        switch ((HeidenhainDNCLib.DNC_ERROR_GROUP)errorGroup) {
          case HeidenhainDNCLib.DNC_ERROR_GROUP.DNC_EG_NONE:
            result = "none";
            break;
          case HeidenhainDNCLib.DNC_ERROR_GROUP.DNC_EG_OPERATING:
            result = "operating error";
            break;
          case HeidenhainDNCLib.DNC_ERROR_GROUP.DNC_EG_PROGRAMING:
            result = "programming error";
            break;
          case HeidenhainDNCLib.DNC_ERROR_GROUP.DNC_EG_PLC:
            result = "PLC error";
            break;
          case HeidenhainDNCLib.DNC_ERROR_GROUP.DNC_EG_GENERAL:
            result = "general error";
            break;
          case HeidenhainDNCLib.DNC_ERROR_GROUP.DNC_EG_REMOTE:
            result = "remote client error";
            break;
          case HeidenhainDNCLib.DNC_ERROR_GROUP.DNC_EG_PYTHON:
            result = "python script error";
            break;
          default:
            result = errorGroup.ToString();
            break;
        }
      } catch (Exception e) {
        Logger.WarnFormat("HeidenhainDNC.GetErrorGroup - couldn't get the error group from {0}: {1}",
                       errorGroup, e);
      }
      
      return result;
    }
    
    string GetErrorClass(object errorClass)
    {
      if (errorClass == null) {
        return "unknown";
      }

      string result = "";
      
      try {
        switch ((HeidenhainDNCLib.DNC_ERROR_CLASS)errorClass) {
          case HeidenhainDNCLib.DNC_ERROR_CLASS.DNC_EC_NONE:
            result = "none";
            break;
          case HeidenhainDNCLib.DNC_ERROR_CLASS.DNC_EC_WARNING:
            result = "warning, no stop";
            break;
          case HeidenhainDNCLib.DNC_ERROR_CLASS.DNC_EC_FEEDHOLD:
            result = "error with feed hold";
            break;
          case HeidenhainDNCLib.DNC_ERROR_CLASS.DNC_EC_PROGRAMHOLD:
            result = "error with program hold";
            break;
          case HeidenhainDNCLib.DNC_ERROR_CLASS.DNC_EC_PROGRAMABORT:
            result = "error with program abort";
            break;
          case HeidenhainDNCLib.DNC_ERROR_CLASS.DNC_EC_EMERGENCY_STOP:
            result = "error with emergency stop";
            break;
          case HeidenhainDNCLib.DNC_ERROR_CLASS.DNC_EC_RESET:
            result = "error with emergency stop & control reset";
            break;
          case HeidenhainDNCLib.DNC_ERROR_CLASS.DNC_EC_INFO:
            result = "info, no stop";
            break;
          case HeidenhainDNCLib.DNC_ERROR_CLASS.DNC_EC_ERROR:
            result = "error, no stop";
            break;
          case HeidenhainDNCLib.DNC_ERROR_CLASS.DNC_EC_NOTE:
            result = "note, no stop";
            break;
          default:
            result = errorClass.ToString();
            break;
        }
      } catch (Exception e) {
        Logger.WarnFormat("HeidenhainDNC.GetErrorClass - couldn't get the error class from {0}: {1}",
                       errorClass, e);
      }
      
      return result;
    }
    #endregion // Private methods
  }
}
