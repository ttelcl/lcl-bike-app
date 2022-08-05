using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

using LclBikeApp.Database.Models;
using LclBikeApp.Database;

namespace CitybikeApp.Services
{
  /// <summary>
  /// Provides a cache for station-day-ridecount information
  /// (both departure based and return based). This is used as a
  /// singleton service (not a scoped service).
  /// </summary>
  public class RideStatsCacheService
  {
    private StationDateCount[]? _departureStats = null;
    private StationDateCount[]? _returnStats = null;

    /// <summary>
    /// Create a new RideStatsCacheService
    /// </summary>
    public RideStatsCacheService()
    {
    }

    /// <summary>
    /// Loads or reloads the departure stats
    /// </summary>
    /// <param name="db">
    /// The database API interface
    /// </param>
    /// <param name="reload">
    /// When true: load the data from the DB even if already present.
    /// </param>
    /// <returns>
    /// True when (re-)loaded, false if the data was present already.
    /// </returns>
    public bool LoadDepartures(ICitybikeDb db, bool reload = false)
    {
      var api = db.GetQueryApi();
      if(reload || _departureStats == null)
      {
        _departureStats = api.GetDepartureStats();
        return true;
      }
      else
      {
        return false;
      }
    }

    /// <summary>
    /// Loads or reloads the return stats
    /// </summary>
    /// <param name="db">
    /// The database API interface
    /// </param>
    /// <param name="reload">
    /// When true: load the data from the DB even if already present.
    /// </param>
    /// <returns>
    /// True when (re-)loaded, false if the data was present already.
    /// </returns>
    public bool LoadReturns(ICitybikeDb db, bool reload = false)
    {
      var api = db.GetQueryApi();
      if(reload || _returnStats == null)
      {
        _returnStats = api.GetReturnStats();
        return true;
      }
      else
      {
        return false;
      }
    }

    /// <summary>
    /// Return the cached departure stats.
    /// This throws an exception if the cache was never loaded
    /// </summary>
    public IReadOnlyList<StationDateCount> GetCachedDepartureStats()
    {
      if(_departureStats == null)
      {
        throw new InvalidOperationException(
          "Attempt to use the departure stats cache without loading it first");
      }
      return _departureStats;
    }

    /// <summary>
    /// Return the cached return stats.
    /// This throws an exception if the cache was never loaded
    /// </summary>
    public IReadOnlyList<StationDateCount> GetCachedReturnStats()
    {
      if(_returnStats == null)
      {
        throw new InvalidOperationException(
          "Attempt to use the return stats cache without loading it first");
      }
      return _returnStats;
    }

  }
}
