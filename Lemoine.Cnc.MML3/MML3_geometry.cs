// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using Lemoine.Collections;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Part of the class MML3 dealing with geometry.
  /// </summary>
  public partial class MML3
  {
    IDictionary<int, double> m_toolCompensationHList = new Dictionary<int, double> ();
    IDictionary<int, double> m_toolCompensationDList = new Dictionary<int, double> ();

    public IDictionary<string, object> GetAllGeometry (string param)
    {
      IDictionary<string, object> result = new Dictionary<string, object> ();
      ;
      var geoListH = m_toolCompensationHList.GetEnumerator ();
      while (geoListH.MoveNext ()) {
        result.Add ("Geo_" + geoListH.Current.Key + "_H", geoListH.Current.Value);
      }
      var geoListD = m_toolCompensationDList.GetEnumerator ();
      while (geoListD.MoveNext ()) {
        result.Add ("Geo_" + geoListD.Current.Key + "_D", geoListD.Current.Value);
      }
      return result;
    }

    public string GetAllGeometryString (string param)
    {
      string result = null;
      var geoList = m_toolCompensationHList.GetEnumerator ();
      while (geoList.MoveNext ()) {
        result += ";Geo_" + geoList.Current.Key + "_H:" + geoList.Current.Value;
      }
      return result;
    }
    #region Private methods

    #endregion Private methods
    }
}
