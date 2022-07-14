/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LclBikeApp.DataWrangling.RawModel
{
  /// <summary>
  /// Static methods related to City Names
  /// </summary>
  public static class CityName
  {
    /// <summary>
    /// Convert a string describing one of the supported cities to 
    /// a city code.
    /// </summary>
    public static CityCode ParseCity(string? cityName)
    {
      if(String.IsNullOrEmpty(cityName))
      {
        return CityCode.Helsinki;
      }
      switch(cityName.ToLower())
      {
        case "":
        case "helsinki":
        case "helsingfors":
          return CityCode.Helsinki;
        case "espoo":
        case "esbo":
          return CityCode.Espoo;
        default:
          throw new ArgumentOutOfRangeException(
            nameof(cityName),
            $"Unsupported city name: {cityName}");
      }
    }
  }
}
