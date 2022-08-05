using System;
using System.Collections.Generic;
using System.Diagnostics;

using LclBikeApp.Database.Models;
using LclBikeApp.Database;

namespace CitybikeApp.Services
{
  /// <summary>
  /// Provides a longer-term cache for the station and city lists. This service
  /// is not used directly, but indirect via the StationListService.
  /// This cache service has a longer lifetime (singleton), and indeed a longer
  /// lifetime than the database service it needs (scoped), hence the separation
  /// </summary>
  public class StationCacheService
  {
    private readonly Dictionary<int, Station> _stations;
    private readonly Dictionary<int, City> _cities;
    private bool _loaded;

    /// <summary>
    /// Create a new StationCacheService instance
    /// </summary>
    public StationCacheService()
    {
      _stations = new Dictionary<int, Station>();
      _cities = new Dictionary<int, City>();
      _loaded = false;
    }

    /// <summary>
    /// Load the cache from the database if not already done so
    /// </summary>
    /// <param name="db">
    /// The database service
    /// </param>
    /// <param name="reload">
    /// When true, clear and re-load this cache
    /// </param>
    /// <returns>
    /// True if (re)loaded, false if it was already loaded
    /// </returns>
    public bool LoadCache(ICitybikeDb db, bool reload=false)
    {
      if(reload || !_loaded)
      {
        Trace.TraceInformation("Loading station / city cache from DB");
        var icq = db.GetQueryApi();
        var stations = icq.GetStations();
        var cities = icq.GetCities();
        _cities.Clear();
        foreach(var city in cities)
        {
          _cities.Add(city.Id, city);
        }
        _stations.Clear();
        foreach(var station in stations)
        {
          _stations.Add(station.Id, station);
        }
        _loaded = true;
        return true;
      }
      else
      {
        Trace.TraceInformation("Station / City cache was already in memory");
        return false;
      }
    }

    /// <summary>
    /// Return the cached stations. This throws an exception if the cache
    /// was never loaded
    /// </summary>
    public IReadOnlyDictionary<int, Station> GetCachedStations()
    {
      if(!_loaded)
      {
        throw new InvalidOperationException(
          "Attempt to use the station cache without loading it first");
      }
      return _stations;
    }

    /// <summary>
    /// Return the cached cities. This throws an exception if the cache
    /// was never loaded
    /// </summary>
    public IReadOnlyDictionary<int, City> GetCachedCities()
    {
      if(!_loaded)
      {
        throw new InvalidOperationException(
          "Attempt to use the city cache without loading it first");
      }
      return _cities;
    }


  }
}
