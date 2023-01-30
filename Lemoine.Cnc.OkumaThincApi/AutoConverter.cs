// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Conversion;
using Lemoine.Core.Log;

namespace Lemoine.Cnc.Module.OkumaThincApi
{
  /// <summary>
  /// AutoConverter
  /// </summary>
  internal class AutoConverter
    : DefaultAutoConverter
    , IAutoConverter
  {
    readonly ILog log = LogManager.GetLogger (typeof (AutoConverter).FullName);

    /// <summary>
    /// Keep only the types that make sense here
    /// </summary>
    /// <param name="x"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public override bool IsCompatible (object x, Type t)
    {
      if (x.GetType () == typeof (string)) {
        // If the input data is a boolean, limit the type to a boolean
        if (t == typeof (bool)) {
          switch ((string)x) {
          case "True":
          case "False":
            return true;
          default:
            return false;
          }
        }

        // If the input data is a number, limit the type to be a number
        if (base.IsCompatible (x, typeof (decimal)) || base.IsCompatible (x, typeof (double))) {
          if (log.IsDebugEnabled) {
            log.Debug ($"IsCompatible: check if numeric {x} is compatible with {t}");
          }
          return t.IsNumeric () && base.IsCompatible (x, t);
        }
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"IsCompatible: check if {x} is compatible with {t}");
      }
      return base.IsCompatible (x, t);
    }
  }
}
