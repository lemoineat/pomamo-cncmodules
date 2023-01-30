// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemoine.Cnc
{
  public partial class OkumaThincApi
  {
    /// <summary>
    /// Control type, among: P300G, P300M, P300L, P300SLP, P300SMP, P200, P100, None
    /// </summary>
    public string ControlType
    {
      get {
        string result;
        int controlTypeIndex = Call<int> ("CSpec", "GetControlType");
        switch (controlTypeIndex) {
        case 0:
          result = "P300G";
          break;
        case 1:
          result = "P300M";
          break;
        case 2:
          result = "P300L";
          break;
        case 3:
          result = "P300SLP";
          break;
        case 4:
          result = "P200";
          break;
        case 5:
          result = "P100";
          break;
        case 6:
          result = "None";
          break;
        case 7:
          result = "P300SMP";
          break;
        default:
          result = "unknown control type " + controlTypeIndex.ToString ();
          log.Error ($"ControlType.get: unknown control type {controlTypeIndex}");
          break;
        }

        return result;
      }
    }

    /// <summary>
    /// Position
    /// </summary>
    public JPosition Position
    {
      get {
        var position = new JPosition ();
        try {
          var x = Call<double> ("CAxis", "GetActualPositionMachineCoord", "X_Axis");
          position.X = x;
        }
        catch (System.Reflection.TargetInvocationException ex) {
          if (ex.InnerException is NotSupportedException) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Position.get: X_Axis not supported");
            }
          }
          else {
            throw;
          }
        }
        catch (NotSupportedException) { }

        try {
          var y = Call<double> ("CAxis", "GetActualPositionMachineCoord", "Y_Axis");
          position.Y = y;
        }
        catch (System.Reflection.TargetInvocationException ex) {
          if (ex.InnerException is NotSupportedException) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Position.get: Y_Axis not supported");
            }
          }
          else {
            throw;
          }
        }
        catch (NotSupportedException) { }

        try {
          var z = Call<double> ("CAxis", "GetActualPositionMachineCoord", "Z_Axis");
          position.Z = z;
        }
        catch (System.Reflection.TargetInvocationException ex) {
          if (ex.InnerException is NotSupportedException) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Position.get: Z_Axis not supported");
            }
          }
          else {
            throw;
          }
        }
        catch (NotSupportedException) { }

        AddAxisValue (ref position, "Fourth_Axis");
        AddAxisValue (ref position, "Fifth_Axis");
        AddAxisValue (ref position, "Sixth_Axis");

        return position;
      }
    }

    string GetAxisName (string axisIndex)
    {
      try {
        return Call<string> ("CAxis", "GetAxisName", axisIndex);
      }
      catch (System.Reflection.TargetInvocationException ex) {
        if (ex.InnerException is NotSupportedException) {
          return "";
        }
        else {
          log.Error ($"GetAxisName: TargetInvocationException", ex.InnerException);
          throw;
        }
      }
      catch (NotSupportedException) {
        return "";
      }
    }

    void AddAxisValue (ref JPosition position, string axisIndex)
    {
      string name;
      try {
        name = GetAxisName (axisIndex);
      }
      catch (System.Reflection.TargetInvocationException ex) {
        log.Error ($"AddAxisValue: TargetInvocationException in GetAxisName for index {axisIndex}", ex.InnerException);
        return;
      }
      catch (NotSupportedException ex) {
        log.Error ($"AddAxisValue: not supported in GetAxisName for index {axisIndex}", ex);
        return;
      }
      catch (Exception ex) {
        log.Error ($"AddAxisValue: exception in GetAxisName for index {axisIndex}", ex);
        return;
      }
      if (!string.IsNullOrEmpty (name)) {
        double v;
        try {
          v = Call<double> ("CAxis", "GetActualPositionMachineCoord", axisIndex);
        }
        catch (System.Reflection.TargetInvocationException ex) {
          log.Error ($"AddAxisValue: TargetInvocationException inner exception for name {name}", ex.InnerException);
          return;
        }
        catch (NotSupportedException ex) {
          log.Error ($"AddAxisValue: not supported for name {name}", ex);
          return;
        }
        catch (Exception ex) {
          log.Error ($"AddAxisValue: exception for name {name}", ex);
          return;
        }
        position.SetAxisValue (v, name);
      }
    }

    /// <summary>
    /// Get a set of cnc variables.
    /// </summary>
    /// <param name="param">ListString (first character is the separator)</param>
    /// <returns></returns>
    public IDictionary<string, double> GetCncVariableSet (string param)
    {
      var macroVariableNumbers = Lemoine.Collections.EnumerableString.ParseListString (param)
        .Select (k => int.Parse (k))
        .Distinct ();
      if (!macroVariableNumbers.Any ()) {
        return new Dictionary<string, double> ();
      }
      if (1 == macroVariableNumbers.Count ()) {
        var n = macroVariableNumbers.Single ();
        var v = Call<double> ("CVariables", "GetCommonVariableValue", n);
        if (log.IsDebugEnabled) {
          log.Debug ($"GetCncVariableSet: single value {n}={v}");
        }
        var dictionary = new Dictionary<string, double> ();
        dictionary[n.ToString ()] = v;
        return dictionary;
      }
      var min = macroVariableNumbers.Min ();
      var max = macroVariableNumbers.Max ();
      if (macroVariableNumbers.Max () - macroVariableNumbers.Min () + 1 <= macroVariableNumbers.Count () + 10) {
        // Maximum 10 additional variables to read: use GetCommonVariableValues
        if (log.IsDebugEnabled) {
          log.Debug ($"GetCncVariableSet: maximum 10 extra values, use GetCommonVariableValues {min}-{max}");
        }
        double[] values;
        try {
          values = Call<double[]> ("CVariables", "GetCommonVariableValues", min, max);
        }
        catch (System.Reflection.TargetInvocationException ex) {
          log.Error ($"GetCncVariableSet: {min}-{max} TargetInvocationException with inner exception", ex.InnerException);
          throw;
        }
        catch (Exception ex) {
          log.Error ($"GetCncVariableSet: {min}-{max} exception", ex);
          throw;
        }
        var dictionary = new Dictionary<string, double> ();
        for (int i = 0; i < values.Length; ++i) {
          var n = i + min;
          if (macroVariableNumbers.Contains (n)) {
            dictionary[n.ToString ()] = values[i];
          }
        }
        return dictionary;
      }
      else { // Individual values 
        var dictionary = new Dictionary<string, double> ();
        foreach (var n in macroVariableNumbers) {
          double v;
          try {
            v = Call<double> ("CVariables", "GetCommonVariableValue", n);
          }
          catch (System.Reflection.TargetInvocationException ex) {
            log.Error ($"GetCncVariableSet: variable={n} TargetInvocationException with inner exception", ex.InnerException);
            continue;
          }
          catch (Exception ex) {
            log.Error ($"GetCncVariableSet: variable={n} exception", ex);
            continue;
          }
          if (log.IsDebugEnabled) {
            log.Debug ($"GetCncVariableSet: add {n}={v}");
          }
          dictionary[n.ToString ()] = v;
        }
        return dictionary;
      }
    }

    /// <summary>
    /// Get the value of a variable
    /// 
    /// In case of problem, this method does not set the error flag
    /// </summary>
    /// <param name="param">Name of the variable (integer)</param>
    /// <returns></returns>
    public double GetVariable (string param)
    {
      if (0 == param.Length) {
        log.ErrorFormat ("GetVariable: " +
                         "no variable name given");
        throw new ArgumentException ("No param variable name");
      }

      int variableIndex;
      try {
        variableIndex = int.Parse (param);
      }
      catch (Exception ex) {
        log.Error ($"GetVariable: invalid param {param}", ex);
        throw;
      }

      double variableValue = Call<double> ("CVariables", "GetCommonVariableValue", variableIndex);
      if (double.IsInfinity (variableValue)) {
        log.ErrorFormat ("GetVariable: got variable is infinity");
        throw new Exception ();
      }
      else {
        log.DebugFormat ("GetVariable: " +
                         "got variable {0}",
                         variableValue);
        return variableValue;
      }
    }

    /// <summary>
    /// Get a range of variables from X to Y included having a gap of Z between each variable,
    /// param being in the format X-Y-Z
    /// For example: 1-9-2 will read 1, 3, 5, 7, 9
    /// </summary>
    /// <param name="param"></param>
    /// <returns>dictionary of doubles</returns>
    public IDictionary<int, double> GetVariables (string param)
    {
      var splitParam = param.Split ('-');

      if (splitParam.Length != 3) {
        string txt = string.Format ("GetVariables: parameter {0} is not in the format X-Y", param);
        log.ErrorFormat (txt);
        throw new ArgumentException (txt);
      }

      int valInf, valSup, gap;
      try {
        valInf = int.Parse (splitParam[0]);
      }
      catch (Exception) {
        string txt = string.Format ("GetVariables: couldn't parse X as an int in parameter {0}", param);
        log.ErrorFormat (txt);
        throw new ArgumentException (txt);
      }

      try {
        valSup = int.Parse (splitParam[1]);
      }
      catch (Exception) {
        string txt = string.Format ("GetVariables: couldn't parse Y as an int in parameter {0}", param);
        log.ErrorFormat (txt);
        throw new ArgumentException (txt);
      }

      try {
        gap = int.Parse (splitParam[2]);
      }
      catch (Exception) {
        string txt = string.Format ("GetVariables: couldn't parse Z as an int in parameter {0}", param);
        log.ErrorFormat (txt);
        throw new ArgumentException (txt);
      }

      var variableValues = Call<double[]> ("CVariables", "GetCommonVariableValues", valInf, valSup);
      IDictionary<int, double> ret = new Dictionary<int, double> ();
      for (int i = 0; i <= valSup - valInf; i += gap) {
        ret[i + valInf] = variableValues[i];
      }
      return ret;
    }
  }
}
