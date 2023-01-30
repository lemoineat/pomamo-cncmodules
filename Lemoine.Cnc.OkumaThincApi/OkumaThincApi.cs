// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lemoine.Cnc.Module.OkumaThincApi;
using Lemoine.Collections;
using Okuma.Scout.Enums;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Okuma input module
  /// </summary>
  public sealed partial class OkumaThincApi
    : Lemoine.Cnc.BaseCncModule
    , Lemoine.Cnc.ICncModule
    , IDisposable
  {
    /// <summary>
    /// Pre-load the classes in the initializeation phase
    /// </summary>
    static readonly string INIT_PRE_LOAD_KEY = "OkumaThincApi.Initialize.PreLoad";
    static readonly bool INIT_PRE_LOAD_DEFAULT = true;

    readonly MethodCaller m_methodCaller = new MethodCaller ();

    #region Getters / Setters
    /// <summary>
    /// Pre-load the classes
    /// </summary>
    public bool PreLoad { get; set; }
#if STATIC_OKUMA_LOAD
      = true;
#else // !STATIC_OKUMA_LOAD
      = false;
#endif // !STATIC_OKUMA_LOAD
    #endregion // Getters / Setters

    #region Constructor
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public OkumaThincApi () : base ("Lemoine.Cnc.In.OkumaThincApi")
    {
    }
    #endregion Constructor

    /// <summary>
    /// Start method
    /// </summary>
    public bool Start ()
    {
      if (this.PreLoad) {
        OkumaClasses.PreLoad ();
      }

      return true;
    }

    /// <summary>
    /// Run an Okuma method
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="objectName"></param>
    /// <param name="methodName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    T Call<T> (string objectName, string methodName, params object[] parameters)
    {
      var result = Call (objectName, methodName, parameters);

      try {
        return (T)Convert.ChangeType (result, typeof (T));
      }
      catch (Exception ex) {
        log.Error ($"Call: Couldn't cast {result} into type {typeof (T)}", ex);
        throw;
      }
    }

    /// <summary>
    /// Set a value with the distant Okuma module
    /// </summary>
    /// <param name="objectName"></param>
    /// <param name="methodName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    object Call (string objectName, string methodName, params object[] parameters)
    {
      try {
        var o = OkumaClasses.Get (objectName);
        return m_methodCaller.Call (o, methodName, parameters);
      }
      catch (Exception ex) {
        log.Error ($"Call: exception for object {objectName} method {methodName}", ex);
        throw;
      }
    }

    /// <summary>
    /// Get a data
    /// </summary>
    /// <param name="param">;objectName;methodName;param1;param2;...</param>
    /// <returns></returns>
    public object GetData (string param)
    {
      var listString = EnumerableString.ParseListString (param);
      if (listString.Length < 2) {
        log.Error ($"GetData: {param} does not contain enough parameters");
        throw new ArgumentException ("Invalid number of parameters", "param");
      }
      var objectName = listString[0];
      var methodName = listString[1];
      var parameters = listString.Skip (2).ToArray ();
      return Call (objectName, methodName, parameters);
    }

    /// <summary>
    /// Set a data
    /// 
    /// Replace the parameter you want to set by @
    /// </summary>
    /// <param name="param">;objectName;methodName;@;param2;...</param>
    /// <param name="x"></param>
    public void SetData (string param, object x)
    {
      var listString = EnumerableString.ParseListString (param);
      if (listString.Length < 3) {
        log.Error ($"SetData: {param} does not contain enough parameters");
        throw new ArgumentException ("Invalid number of parameters", "param");
      }
      var objectName = listString[0];
      var methodName = listString[1];
      var remaining = listString.Skip (2);
      if (!remaining.Any (p => p.Equals ("@"))) {
        log.Error ($"SetData: @ is missing in param");
        throw new ArgumentException ("Missing @ in param", "param");
      }
      var parameters = remaining
        .Select (p => p.Equals ("@") ? x : p)
        .ToArray ();
      Call (objectName, methodName, parameters);
    }

    /// <summary>
    /// Like <see cref="GetData(string)"/> but convert to a string afterwards
    /// 
    /// Useful when the result is an Enum
    /// </summary>
    /// <param name="param">;objectName;methodName;param1;param2;...</param>
    /// <returns></returns>
    public object GetString (string param)
    {
      return GetData (param).ToString ();
    }


    #region IDisposable implementation
    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    public void Dispose ()
    {
      Call ("CMachine", "Close");
    }
    #endregion // IDisposable

    /// <summary>
    /// Static method for early initialization
    /// </summary>
    public static void Initialize (System.Threading.CancellationToken cancellationToken)
    {
      OkumaAssemblies.CopyOkumaAssemblies (cancellationToken);
      OkumaCMachine.Load (cancellationToken);

      bool preLoadInInitialization = Lemoine.Info.ConfigSet
        .LoadAndGet (INIT_PRE_LOAD_KEY, INIT_PRE_LOAD_DEFAULT);
      if (preLoadInInitialization) {
        OkumaClasses.PreLoad ();
      }
    }
  }
}
