using System;
using System.Collections.Generic;
using System.Linq;

using LclBikeApp.Database.Models;
using LclBikeApp.Database;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace CitybikeApp.Services
{
  /// <summary>
  /// Provides a service that exposes cached stations with "Scoped" lifetime.
  /// This is a thin wrapper around StationCacheService
  /// </summary>
  public class StationListService
  {
    private readonly StationCacheService _stationCacheService;

    /// <summary>
    /// Create a new StationListService instance and load the cache
    /// </summary>
    public StationListService(
      StationCacheService stationCacheService,
      ICitybikeDb db)
    {
      _stationCacheService=stationCacheService;
      _stationCacheService.LoadCache(db); // that's a no-op if already loaded
    }

    /// <summary>
    /// Access the cached stations
    /// </summary>
    public IReadOnlyDictionary<int, Station> Stations => _stationCacheService.GetCachedStations();

    /// <summary>
    /// Access the cached cities
    /// </summary>
    public IReadOnlyDictionary<int, City> Cities => _stationCacheService.GetCachedCities();

  }
}
