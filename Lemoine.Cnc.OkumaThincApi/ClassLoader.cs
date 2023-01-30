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
  public sealed class ClassLoader
  {
    readonly ILog log = LogManager.GetLogger ("Lemoine.Cnc.In.OkumaThincApi.ClassLoader");

    readonly Lemoine.Core.Plugin.TypeLoader m_typeLoader;
    readonly OkumaMachineType m_okumaMachineType;

    #region Getters / Setters
    /// <summary>
    /// Okuma machine type
    /// </summary>
    public OkumaMachineType OkumaMachineType => m_okumaMachineType;
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ClassLoader ()
    {
      var assemblyLoader = new Lemoine.Core.Plugin.DefaultAssemblyLoader ();
      m_typeLoader = new Lemoine.Core.Plugin.TypeLoader (assemblyLoader);

      Scout.LogSystemInfo (log);
      m_okumaMachineType = Scout.GetOkumaMachineType ();
    }
    #endregion // Constructors

    /// <summary>
    /// Load a class
    /// </summary>
    /// <param name="className"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public object LoadClass (string className, params object[] args)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"LoadClass: name={className}");
      }

      foreach (var qualifiedName in GetPossibleQualifiedNames (className)) {
        try {
          return m_typeLoader.Load<object> (qualifiedName, args);
        }
        catch (Exception ex) {
          if (log.IsInfoEnabled) {
            log.Info ($"LoadClass: load of {qualifiedName} failed", ex);
          }
        }
      }

      log.Error ($"LoadClass: {className} could not be loaded");
      throw new Exception ($"{className} could not be loaded");
    }

    IEnumerable<string> GetPossibleQualifiedNames (string className)
    {
      if (className.Contains (",")) { // Already qualified
        if (log.IsDebugEnabled) {
          log.Debug ($"GetPossibleQualifiedNames: {className} already qualified, return it");
        }
        yield return className;
      }

      foreach (var template in GetQualifiedNameTemplates ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetQualitifedNameTemplates: try with template {template}");
        }
        yield return string.Format (template, className);
      }
    }

    IEnumerable<string> GetQualifiedNameTemplates ()
    {
      switch (m_okumaMachineType) {
      case OkumaMachineType.Mill:
        return GetQualifiedNameTemplatesMill ();
      case OkumaMachineType.Lathe:
        return GetQualifiedNameTemplatesLathe ();
      case OkumaMachineType.Grinder:
        return GetQualifiedNameTemplatesGrinder ();
      default:
        log.Error ($"GetQualifiedNameTemplates: not supported okuma machine type");
        return new string[] { };
      }
    }

    IEnumerable<string> GetQualifiedNameTemplatesMill ()
    {
      return new string[] {
        "Okuma.CMDATAPI.DataAPI.{0}, Okuma.CMDATAPI"
      };
    }

    IEnumerable<string> GetQualifiedNameTemplatesLathe ()
    {
      return new string[] {
        "Okuma.CLDATAPI.DataAPI.{0}, Okuma.CLDATAPI"
      };
    }

    IEnumerable<string> GetQualifiedNameTemplatesGrinder ()
    {
      return new string[] {
        "Okuma.CGDATAPI.DataApi.{0}, Okuma.CGDATAPI"
      };
    }
  }
}
