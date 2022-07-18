/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LclBikeApp.Database.Models
{
  /// <summary>
  /// Describes one of the cities for which data is available (content of table
  /// Cities)
  /// Note that Helsinki acts as default value (ID 0)
  /// </summary>
  public class City
  {
    /// <summary>
    /// Create a new City
    /// </summary>
    public City(
      int id, string cityFi, string citySe)
    {
      Id=id;
      CityFi=cityFi;
      CitySe=citySe;
    }

    /// <summary>
    /// The identifier code of the city
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// The finnish name for the city
    /// </summary>
    public string CityFi { get; }

    /// <summary>
    /// The swedish name for the city
    /// </summary>
    public string CitySe { get; }

  }
}
