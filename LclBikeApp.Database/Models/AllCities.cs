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
  /// An in-memory representation of all supported cities.
  /// </summary>
  public class AllCities
  {
    private readonly Dictionary<int, City> _cities;

    /// <summary>
    /// Create a new EMPTY AllCities
    /// </summary>
    public AllCities()
    {
      _cities = new Dictionary<int, City>();
    }

    internal AllCities(IEnumerable<City> cities)
      : this()
    {
      foreach(var city in cities)
      {
        Insert(city);
      }
    }

    /// <summary>
    /// The hardcoded default list of cities (0=Helsinki, 1=Espoo)
    /// </summary>
    public static AllCities Default { get; } =
      new AllCities(new[] {
         new City(0, "Helsinki", "Helsingfors"),
         new City(1, "Espoo", "Esbo")
        });

    /// <summary>
    /// Enumerate all cities
    /// </summary>
    public IEnumerable<City> All => _cities.Values;


    private void Insert(City city)
    {
      _cities[city.Id] = city;
    }

  }
}
