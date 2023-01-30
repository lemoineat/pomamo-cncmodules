// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Lemoine.Core.Log;

namespace Lemoine.Cnc.Module.OkumaThincApi
{
  /// <summary>
  /// OkumaAssemblies
  /// 
  /// Copy the right assemblies
  /// </summary>
  public static class OkumaAssemblies
  {
    static readonly ILog log = LogManager.GetLogger (typeof (OkumaAssemblies).FullName);

    static readonly IEnumerable<string> AVAILABLE_VERSIONS = new List<string> { "1.22.5", "1.21.1", "1.19.0", "1.17.2", "1.12.1", "1.9.1" };

    static bool IsVersionValid (string thincApiVersion)
    {
      return Scout.IsThincApiVersionValid (thincApiVersion);
    }

    /// <summary>
    /// Copy the Okuma assemblies from the right version to the system directory
    /// </summary>
    public static void CopyOkumaAssemblies (CancellationToken cancellationToken)
    {
      var programDirectory = Lemoine.Info.AssemblyInfo.AbsoluteDirectory;
      var version = AVAILABLE_VERSIONS
        .FirstOrDefault (x => Scout.IsThincApiVersionValid (x));
      if (string.IsNullOrEmpty (version)) {
        log.Error ($"CopyOkumaAssemblies: no version is valid");
      }
      var versionDirectory = Path.Combine (programDirectory, version);
      if (!Directory.Exists (versionDirectory)) {
        log.Error ($"CopyOkumaAssemblies: directory {versionDirectory} does not exist");
        return;
      }
      var assemblyPaths = Directory.EnumerateFiles (versionDirectory, "*.dll", SearchOption.TopDirectoryOnly);
      foreach (var assemblyPath in assemblyPaths) {
        cancellationToken.ThrowIfCancellationRequested ();
        var fileName = Path.GetFileName (assemblyPath);
        var destination = Path.Combine (programDirectory, fileName);
        try {
          File.Copy (assemblyPath, destination, true);
        }
        catch (Exception ex) {
          log.Error ($"CopyOkumaAssemblies: copy from {assemblyPath} to {destination} failed", ex);
        }
      }
    }
  }
}
