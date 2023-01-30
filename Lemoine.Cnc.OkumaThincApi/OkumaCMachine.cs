// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lemoine.Core.Log;

namespace Lemoine.Cnc.Module.OkumaThincApi
{
  /// <summary>
  /// OkumaCMachine (singleton)
  /// </summary>
  public sealed class OkumaCMachine
  {
    ILog log = LogManager.GetLogger (typeof (OkumaCMachine).FullName);
    static readonly ILog slog = LogManager.GetLogger (typeof (OkumaCMachine).FullName);

#if STATIC_OKUMA_LOAD
    static readonly string DYNAMIC_LOAD_KEY = "Cnc.Okuma.DynamicLoad";
    static readonly bool DYNAMIC_LOAD_DEFAULT = true;

    readonly bool m_dynamicLoad;
#endif // STATIC_OKUMA_LOAD
    object m_cmachine = null;
    ClassLoader m_classLoader = null;

    #region Getters / Setters
#if STATIC_OKUMA_LOAD
    /// <summary>
    /// Is dynamic load used ?
    /// </summary>
    public static bool DynamicLoad => Instance.m_dynamicLoad;
#endif // STATIC_OKUMA_LOAD

    /// <summary>
    /// Used class loader if any or null
    /// </summary>
    public static ClassLoader ClassLoader => Instance.m_classLoader;

    /// <summary>
    /// CMachine
    /// 
    /// null is returned if it has not been initialized yet.
    /// Please call <see cref="Load"/> in the main thread first
    /// </summary>
    public static object CMachine => Instance.m_cmachine;
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor (private because singleton)
    /// </summary>
    OkumaCMachine ()
    {
#if STATIC_OKUMA_LOAD
      m_dynamicLoad = Lemoine.Info.ConfigSet.LoadAndGet (DYNAMIC_LOAD_KEY, DYNAMIC_LOAD_DEFAULT);
#endif // STATIC_OKUMA_LOAD
    }
#endregion // Constructors

    /// <summary>
    /// Load the CMachine
    /// 
    /// This must be done in the main thread
    /// </summary>
    /// <param name="cancellationToken"></param>
    public static void Load (CancellationToken cancellationToken)
    {
      Instance.LoadAuto (cancellationToken);
    }

    void LoadAuto (CancellationToken cancellationToken)
    {
#if STATIC_OKUMA_LOAD
      if (m_dynamicLoad) {
        LoadDynamically (cancellationToken);
      }
      else {
        LoadStatically (cancellationToken);
      }
    }

    void LoadDynamically (CancellationToken cancellationToken)
    {
#endif // STATIC_OKUMA_LOAD
      if (m_classLoader is null) {
        m_classLoader = new ClassLoader ();
      }
      var cmachine = m_classLoader.LoadClass ("CMachine", "Lemoine");
      if (cmachine is null) {
        log.Error ("LoadDynamically: no CMachine class could be loaded, null was returned");
        throw new Exception ("No CMachine class can be loaded");
      }
      while (!cancellationToken.IsCancellationRequested) {
        try {
          var cmachineType = cmachine.GetType ();
          var initMethodInfo = cmachineType.GetMethod ("Init", new Type[] { });
          initMethodInfo.Invoke (cmachine, null);
          m_cmachine = cmachine;
          return;
        }
        catch (ApplicationException ex) {
          log.Error ("LoadDynamically: ApplicationException, retry in 10s", ex);
          cancellationToken.WaitHandle.WaitOne (TimeSpan.FromSeconds (10));
        }
        catch (Exception ex) {
          log.Fatal ($"LoadDynamically: not supported exception", ex);
          throw;
        }
      }
    }

#if STATIC_OKUMA_LOAD
    void LoadStatically (CancellationToken cancellationToken)
    {
      var machineType = Scout.GetOkumaMachineType ();
      switch (machineType) {
      case OkumaMachineType.Mill:
        LoadMill (cancellationToken);
        break;
      case OkumaMachineType.Lathe:
        LoadLathe (cancellationToken);
        break;
      case OkumaMachineType.Grinder:
        LoadGrinder (cancellationToken);
        break;
      default:
        slog.Fatal ($"Load: not supported or implemented type {machineType}");
        throw new NotSupportedException ($"Unsupported okume type {machineType}");
      }
    }

    void LoadMill (CancellationToken cancellationToken)
    {
      var cmachine = new Okuma.CMDATAPI.DataAPI.CMachine ("Lemoine");
      while (!cancellationToken.IsCancellationRequested) {
        try {
          cmachine.Init ();
          m_cmachine = cmachine;
          return;
        }
        catch (ApplicationException ex) {
          log.Error ("LoadMill: ApplicationException, retry in 2s", ex);
          cancellationToken.WaitHandle.WaitOne (TimeSpan.FromSeconds (2));
        }
        catch (Exception ex) {
          log.Fatal ($"LoadMill: not supported exception", ex);
          throw;
        }
      }
    }

    void LoadLathe (CancellationToken cancellationToken)
    {
      var cmachine = new Okuma.CLDATAPI.DataAPI.CMachine ("Lemoine");
      while (!cancellationToken.IsCancellationRequested) {
        try {
          cmachine.Init ();
          m_cmachine = cmachine;
          return;
        }
        catch (ApplicationException ex) {
          log.Error ("LoadLathe: ApplicationException, retry in 2s", ex);
          cancellationToken.WaitHandle.WaitOne (TimeSpan.FromSeconds (2));
        }
        catch (Exception ex) {
          log.Fatal ($"LoadLathe: not supported exception", ex);
          throw;
        }
      }
    }

    void LoadGrinder (CancellationToken cancellationToken)
    {
      var cmachine = new Okuma.CGDATAPI.DataApi.CMachine ("Lemoine");
      while (!cancellationToken.IsCancellationRequested) {
        try {
          cmachine.Init ();
          m_cmachine = cmachine;
          return;
        }
        catch (ApplicationException ex) {
          log.Error ("LoadGrinder: ApplicationException, retry in 2s", ex);
          cancellationToken.WaitHandle.WaitOne (TimeSpan.FromSeconds (2));
        }
        catch (Exception ex) {
          log.Fatal ($"LoadGrinder: not supported exception", ex);
          throw;
        }
      }
    }
#endif // STATIC_OKUMA_LOAD

#region Instance
    static OkumaCMachine Instance
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

      internal static readonly OkumaCMachine instance = new OkumaCMachine ();
    }
#endregion // Instance
  }
}
