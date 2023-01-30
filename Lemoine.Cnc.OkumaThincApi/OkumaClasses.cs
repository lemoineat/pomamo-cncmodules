// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Lemoine.Core.Log;

namespace Lemoine.Cnc.Module.OkumaThincApi
{
  /// <summary>
  /// ThincApi
  /// </summary>
  public class OkumaClasses
  {
    readonly ILog log = LogManager.GetLogger ("Lemoine.Cnc.In.OkumaThincApi.OkumaClasses");

    readonly ClassLoader m_classLoader;
#if STATIC_OKUMA_LOAD
    readonly bool m_dynamicLoad;
#endif // STATIC_OKUMA_LOAD
    readonly IDictionary<string, object> m_okumaClasses = new Dictionary<string, object> ();
    bool m_preLoadCompleted = false;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    OkumaClasses ()
      : this (OkumaCMachine.ClassLoader ?? new ClassLoader ())
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="classLoader">not null</param>
    OkumaClasses (ClassLoader classLoader)
    {
      Debug.Assert (null != classLoader);

      m_classLoader = classLoader;

      var cmachine = OkumaCMachine.CMachine;
      if (cmachine is null) {
        log.Fatal ($"OkumaClasses: please load OkumaCMachine first in the main thread");
      }
#if STATIC_OKUMA_LOAD
      m_dynamicLoad = OkumaCMachine.DynamicLoad;
#endif // STATIC_OKUMA_LOAD
      m_okumaClasses["CMachine"] = cmachine;
    }
    #endregion // Constructors

    /// <summary>
    /// Pre-load the okuma classes
    /// </summary>
    public static void PreLoad ()
    {
      Instance.PreLoadClasses ();
    }

    /// <summary>
    /// Get an okuma class specifying a class name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static object Get (string name)
    {
      return Instance[name];
    }

    /// <summary>
    /// Pre-load the okuma classes
    /// </summary>
    void PreLoadClasses ()
    {
      if (m_preLoadCompleted) {
        return;
      }

      try {
        switch (m_classLoader.OkumaMachineType) {
        case OkumaMachineType.Mill:
          PreLoadMill ();
          break;
        case OkumaMachineType.Lathe:
          PreLoadLathe ();
          break;
        case OkumaMachineType.Grinder:
          PreLoadGrinder ();
          break;
        default:
          log.Error ($"PreLoadClasses: not supported okuma machine type for pre-load");
          break;
        }
        m_preLoadCompleted = true;
      }
      catch (Exception ex) {
        log.Error ($"PreLoadClasses: exception", ex);
      }
    }


    /// <summary>
    /// Get/set an okuma class specifying a class name
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    object this[string s]
    {
      get {
        if (m_okumaClasses.ContainsKey (s)) {
          return m_okumaClasses[s];
        }
        else {
          var o = LoadClassWithPrefix (s);
          m_okumaClasses[s] = o;
          return o;
        }
      }
      set => m_okumaClasses[s] = value;
    }

    object LoadClassWithPrefix (string s)
    {
      if (s.Contains (":")) {
        var split = s.Split (new char[] { ':' }, 2);
        return m_classLoader.LoadClass (split[1]);
      }
      else {
        return m_classLoader.LoadClass (s);
      }
    }

    bool TryLoadClass (string className)
    {
      try {
        m_okumaClasses[className] = m_classLoader.LoadClass (className);
        if (log.IsDebugEnabled) {
          log.Debug ($"TryLoadClass: {className} successfully loaded");
        }
        return true;
      }
      catch (System.NotSupportedException ex) {
        log.Info ($"TryLoadClass: {className} not supported", ex);
        return false;
      }
      catch (System.ApplicationException ex) {
        log.Error ($"TryLoadClass: {className} could not be loaded at current NC condition", ex);
        return false;
      }
      catch (Exception ex) {
        log.Error ($"TryLoadClass: {className} could not be loaded", ex);
        return false;
      }
    }

    /// <summary>
    /// Load classes
    /// </summary>
    /// <param name="classNames"></param>
    void TryLoadClasses (IEnumerable<string> classNames)
    {
      foreach (var className in classNames) {
        TryLoadClass (className);
      }
    }

    void PreLoadMill ()
    {
#if STATIC_OKUMA_LOAD
      if (m_dynamicLoad) {
        PreLoadMillDynamically ();
      }
      else {
        PreLoadMillStatically ();
      }
    }

    void PreLoadMillDynamically ()
    {
#endif // STATIC_OKUMA_LOAD
      var classNames = new List<string> { "CATC", "CAxis", "CProgram", "CSpec", "CSpindle", "CTools", "CVariables", "CIO" };
      TryLoadClasses (classNames);
    }

#if STATIC_OKUMA_LOAD
    void PreLoadMillStatically ()
    {
      try {
        m_okumaClasses["CATC"] = new Okuma.CMDATAPI.DataAPI.CATC ();
      }
      catch (System.NotSupportedException ex) {
        log.Info ($"PreLoadMillStatically: CATC not supported", ex);
      }
      catch (System.ApplicationException ex) {
        log.Error ($"PreLoadMillStatically: CATC could not be loaded at current NC condition", ex);
      }
      m_okumaClasses["CAxis"] = new Okuma.CMDATAPI.DataAPI.CAxis ();
      m_okumaClasses["CCoolant"] = new Okuma.CMDATAPI.DataAPI.CCoolant ();
      m_okumaClasses["CProgram"] = new Okuma.CMDATAPI.DataAPI.CProgram ();
      m_okumaClasses["CSpec"] = new Okuma.CMDATAPI.DataAPI.CSpec ();
      m_okumaClasses["CSpindle"] = new Okuma.CMDATAPI.DataAPI.CSpindle ();
      m_okumaClasses["CTools"] = new Okuma.CMDATAPI.DataAPI.CTools ();
      m_okumaClasses["CTools2"] = new Okuma.CMDATAPI.DataAPI.CTools2 ();
      m_okumaClasses["CVariables"] = new Okuma.CMDATAPI.DataAPI.CVariables ();
      m_okumaClasses["CWorkpiece"] = new Okuma.CMDATAPI.DataAPI.CWorkpiece ();
      m_okumaClasses["COptionalParameter"] = new Okuma.CMDATAPI.DataAPI.COptionalParameter ();
      m_okumaClasses["CIO"] = new Okuma.CMDATAPI.DataAPI.CIO ();
    }
#endif // STATIC_OKUMA_LOAD

    void PreLoadLathe ()
    {
#if STATIC_OKUMA_LOAD
      if (m_dynamicLoad) {
        PreLoadLatheDynamically ();
      }
      else {
        PreLoadLatheStatically ();
      }
    }

    void PreLoadLatheDynamically ()
    {
#endif // STATIC_OKUMA_LOAD
      var classNames = new List<string> { "CATC", "CAxis", "CProgram", "CSpec", "CSpindle", "CTools", "CVariables", "CIO" };
      TryLoadClasses (classNames);
    }

#if STATIC_OKUMA_LOAD
    void PreLoadLatheStatically ()
    {
      try {
        m_okumaClasses["CATC"] = new Okuma.CLDATAPI.DataAPI.CATC ();
      }
      catch (System.NotSupportedException ex) {
        log.Info ($"PreLoadLatheStatically: CATC not supported", ex);
      }
      catch (System.ApplicationException ex) {
        log.Error ($"PreLoadLatheStatically: CATC could not be loaded at current NC condition", ex);
      }
      m_okumaClasses["CAxis"] = new Okuma.CLDATAPI.DataAPI.CAxis ();
      m_okumaClasses["CBallScrew"] = new Okuma.CLDATAPI.DataAPI.CBallScrew ();
      m_okumaClasses["CChuck"] = new Okuma.CLDATAPI.DataAPI.CChuck ();
      m_okumaClasses["CMSpindle"] = new Okuma.CLDATAPI.DataAPI.CMSpindle ();
      m_okumaClasses["CProgram"] = new Okuma.CLDATAPI.DataAPI.CProgram ();
      m_okumaClasses["CSpec"] = new Okuma.CLDATAPI.DataAPI.CSpec ();
      m_okumaClasses["CSpindle"] = new Okuma.CLDATAPI.DataAPI.CSpindle ();
      m_okumaClasses["CTailStock"] = new Okuma.CLDATAPI.DataAPI.CTailStock ();
      m_okumaClasses["CTools"] = new Okuma.CLDATAPI.DataAPI.CTools ();
      m_okumaClasses["CTurret"] = new Okuma.CLDATAPI.DataAPI.CTurret ();
      m_okumaClasses["CVariables"] = new Okuma.CLDATAPI.DataAPI.CVariables ();
      m_okumaClasses["CWorkpiece"] = new Okuma.CLDATAPI.DataAPI.CWorkpiece ();
      m_okumaClasses["COptionalParameter"] = new Okuma.CLDATAPI.DataAPI.COptionalParameter ();
      m_okumaClasses["CIO"] = new Okuma.CLDATAPI.DataAPI.CIO ();
    }
#endif // STATIC_OKUMA_LOAD

    void PreLoadGrinder ()
    {
#if STATIC_OKUMA_LOAD
      if (m_dynamicLoad) {
        PreLoadGrinderDynamically ();
      }
      else {
        PreLoadLatheStatically ();
      }
    }

    void PreLoadGrinderDynamically ()
    {
#endif // STATIC_OKUMA_LOAD
      var classNames = new List<string> { "CAxis", "CProgram", "CSpec", "CSpindle", "CTools", "CVariables", "CIO" };
      TryLoadClasses (classNames);
    }

#if STATIC_OKUMA_LOAD
    void PreLoadGrinderStatically ()
    {
      m_okumaClasses["CAxis"] = new Okuma.CGDATAPI.DataApi.CAxis ();
      m_okumaClasses["CProgram"] = new Okuma.CGDATAPI.DataApi.CProgram ();
      m_okumaClasses["CSpec"] = new Okuma.CGDATAPI.DataApi.CSpec ();
      m_okumaClasses["CSpindle"] = new Okuma.CGDATAPI.DataApi.CSpindle ();
      m_okumaClasses["CTools"] = new Okuma.CGDATAPI.DataApi.CTools ();
      m_okumaClasses["CVariables"] = new Okuma.CGDATAPI.DataApi.CVariables ();
      m_okumaClasses["CWorkpiece"] = new Okuma.CGDATAPI.DataApi.CWorkpiece ();
      m_okumaClasses["CIO"] = new Okuma.CGDATAPI.DataApi.CIO ();
      m_okumaClasses["CWheel"] = new Okuma.CGDATAPI.DataApi.CWheel ();
    }
#endif // STATIC_OKUMA_LOAD

    #region Instance
    static public OkumaClasses Instance
    {
      get { return Nested.instance; }
    }

    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested ()
      {
      }

      internal static readonly OkumaClasses instance = new OkumaClasses ();
    }
    #endregion // Instance
  }
}
