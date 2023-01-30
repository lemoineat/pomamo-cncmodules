// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Lemoine.Core.Plugin;
using Lemoine.Core.Log;

namespace Lemoine.Cnc.Module.OkumaThincApi
{
  /// <summary>
  /// Method caller
  /// </summary>
  internal class MethodCaller
  {
    readonly ILog log = LogManager.GetLogger ("Lemoine.Cnc.In.OkumaThincApi.MethodCaller");

    Lemoine.Conversion.IAutoConverter m_autoConverter = new AutoConverter ();

    /// <summary>
    /// Constructor
    /// </summary>
    public MethodCaller ()
    {
    }

    /// <summary>
    /// Call a method of the class
    /// Exceptions can be thrown
    /// </summary>
    /// <param name="o">Object instance, not null</param>
    /// <param name="methodName">Name of the method to call</param>
    /// <param name="parameters">parameters sent to the function</param>
    /// <returns>result of the call</returns>
    public object Call (object o, string methodName, params object[] parameters)
    {
      // First check that an object of the class is instantiated
      if (o is null) {
        log.Error ($"Call: null object");
        throw new ArgumentNullException ("o");
      }

      try {
        var method = GetMethod (o, methodName, parameters);
        if (method is null) {
          log.Error ($"Call: no method with name {methodName} was found");
          throw new InvalidOperationException ("No method found");
        }
        else {
          return method.InvokeAutoConvert (m_autoConverter, o, parameters);
        }
      }
      catch (Exception ex) {
        log.Error ($"Call: Exception when calling method {methodName}", ex);
        throw;
      }
    }

    MethodInfo GetMethod (object o, string methodName, params object[] parameters)
    {
      try {
        var type = o.GetType ();
        var methods = type.GetMethods ()
          .Where (m => m.Name.Equals (methodName, StringComparison.InvariantCultureIgnoreCase))
          .Where (m => m.IsParameterMatch (m_autoConverter, parameters));
        if (!methods.Any ()) {
          log.Error ($"GetMethod: no method with name {methodName} on type {type} matches the parameters");
          return null;
        }
        var count = methods.Count ();
        if (1 < count) {
          log.Warn ($"GetMethod: more than one matching method with name {methodName} on type {type} matches the parameter => consider the first one");
        }
        return methods.First ();
      }
      catch (Exception ex) {
        log.Error ($"GetMethod: method={methodName} number of parameters={parameters.Length}, exception", ex);
        throw;
      }
    }
  }
}
