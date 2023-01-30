// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using Lemoine.Core.Log;
using Okuma.Scout;
using Okuma.Scout.Enums;

namespace Lemoine.Cnc.Module.OkumaThincApi
{
  /// <summary>
  /// Okuma CNC type
  /// </summary>
  public enum OkumaMachineType
  {
    /// <summary>
    /// Machining center
    /// 
    /// Use the CM* assemblies
    /// </summary>
    Mill,
    /// <summary>
    /// Lathe
    /// 
    /// Use the CL* assemblies
    /// </summary>
    Lathe,
    /// <summary>
    /// Grinder
    /// 
    /// Use the CG* assemblies
    /// </summary>
    Grinder,
  }

  /// <summary>
  /// Utilitary class to get system info, using dll
  /// </summary>
  internal static class Scout
  {
    static readonly ILog slog = LogManager.GetLogger (typeof (Scout).FullName);

    /// <summary>
    /// Get the machine type
    /// </summary>
    /// <returns></returns>
    public static OkumaMachineType GetOkumaMachineType ()
    {
      var baseMachineType = Okuma.Scout.Platform.BaseMachineType;
      switch (baseMachineType) {
      case MachineType.L:
      case MachineType.NCM_L:
      case MachineType.PCNCM_L:
        return OkumaMachineType.Lathe;
      case MachineType.M:
      case MachineType.NCM_M:
      case MachineType.PCNCM_M:
        return OkumaMachineType.Mill;
      case MachineType.G:
      case MachineType.NCM_G:
      case MachineType.PCNCM_G:
        return OkumaMachineType.Grinder;
      case MachineType.Unknown:
      case MachineType.PC:
      default:
        throw new NotSupportedException ($"This machine type {baseMachineType} is not supported");
      }
    }

    /// <summary>
    /// Get system info in logs, helping for debugging
    /// </summary>
    /// <param name="log"></param>
    public static void LogSystemInfo (ILog log)
    {
      if (log.IsInfoEnabled) {
        var installVersion = ThincApi.InstallVersion;
        log.Info ($"LogSystemInfo: Installed={ThincApi.Installed}, ApiAvailable={ThincApi.ApiAvailable}, MachineType={Platform.BaseMachineType}, ApiVersion={installVersion.ApiVersion}, ApiNotifierVersion={installVersion.ApiNotifierVersion}, CustomApiVersion={installVersion.CustomApiVersion}, DataApiVersion={installVersion.DataApiVersion}");
      }
    }

    /// <summary>
    /// Name of the dll to use on the system, depending on the machine type
    /// </summary>
    /// <returns>CLDATAPI / CMDATAPI / CGDATAPI / ""</returns>
    public static string DllToUse ()
    {
      switch (Platform.BaseMachineType) {
      case MachineType.L:
      case MachineType.NCM_L:
      case MachineType.PCNCM_L:
        return "CLDATAPI";
      case MachineType.M:
      case MachineType.NCM_M:
      case MachineType.PCNCM_M:
        return "CMDATAPI";
      case MachineType.G:
      case MachineType.NCM_G:
      case MachineType.PCNCM_G:
        return "CGDATAPI";
      case MachineType.Unknown:
      case MachineType.PC:
      default:
        return "";
      }
    }

    public static bool IsThincApiVersionValid (string version)
    {
      if (Version.TryParse (version, out var ver)) {
        return IsThincApiVersionValid (ver);
      }
      else {
        slog.Error ($"IsTincApiVersionValid: invalid version {version} => return false");
        return false;
      }
    }

    /// <summary>
    /// Is a thinc api version valid for this control ?
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public static bool IsThincApiVersionValid (Version version)
    {
      return ThincApi.DoesMachineSupportThincApiVersion (version);
    }
  }
}
