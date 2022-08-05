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
  /// Provides a service that exposes cached ride stats with "Scoped" lifetime.
  /// This is a thin wrapper around RideStatsCacheService, moving the
  /// loading of data to the service constructor.
  /// </summary>
  public class RideStatsService
  {
    private readonly RideStatsCacheService _rideStatsCacheService;

    /// <summary>
    /// Create a new RideStatsService instance and load the cache
    /// </summary>
    public RideStatsService(
      RideStatsCacheService rideStatsCacheService,
      ICitybikeDb db)
    {
      _rideStatsCacheService=rideStatsCacheService;
      _rideStatsCacheService.LoadDepartures(db); // that's a no-op if already loaded
      _rideStatsCacheService.LoadReturns(db); // that's a no-op if already loaded
    }

    /// <summary>
    /// Retrieve the list of departure station-day-count stats (unordered)
    /// </summary>
    public IReadOnlyList<StationDateCount> DepartureStats => _rideStatsCacheService.GetCachedDepartureStats();

    /// <summary>
    /// Retrieve the list of return station-day-count stats (unordered)
    /// </summary>
    public IReadOnlyList<StationDateCount> ReturnStats => _rideStatsCacheService.GetCachedReturnStats();

    /// <summary>
    /// Calculate an aggregated list of (departure station, rideCount) statistics
    /// Note that the aggregated stats are not cached.
    /// </summary>
    /// <param name="firstDay">
    /// Optional first departure day to take into account
    /// </param>
    /// <param name="lastDay">
    /// Optional last departure day to take into account
    /// </param>
    /// <returns>
    /// A list of StationCount records containing the Departure Station Id and the
    /// total number of rides departing in the specified day interval.
    /// The list is sorted by station ID
    /// </returns>
    public List<StationCount> GetDeparturesForStations(
      DateOnly? firstDay = null, DateOnly? lastDay = null)
    {
      return DepartureStats.AggregateForStations(firstDay, lastDay);
    }

    /// <summary>
    /// Calculate an aggregated list of (return station, rideCount) statistics
    /// Note that the aggregated stats are not cached.
    /// </summary>
    /// <param name="firstDay">
    /// Optional first return day to take into account
    /// </param>
    /// <param name="lastDay">
    /// Optional last return day to take into account
    /// </param>
    /// <returns>
    /// A list of StationCount records containing the Return Station Id and the
    /// total number of rides arriving in the specified day interval.
    /// The list is sorted by station ID
    /// </returns>
    public List<StationCount> GetReturnsForStations(
      DateOnly? firstDay = null, DateOnly? lastDay = null)
    {
      return ReturnStats.AggregateForStations(firstDay, lastDay);
    }

    /// <summary>
    /// Calculate an aggregated list of <see cref="DayCount"/> records
    /// over the cached <see cref="DepartureStats"/>,
    /// returning the total number of rides departing per day.
    /// </summary>
    /// <param name="depId">
    /// Optional. When not null nor 0: only look at input records matching
    /// the given departure station.
    /// </param>
    /// <returns>
    /// A list of <see cref="DayCount"/> records containing the number
    /// of rides departing on each day, either for all stations or for the
    /// selected departure station
    /// </returns>
    public List<DayCount> GetDeparturesForDays(
      int? depId)
    {
      return DepartureStats.AggregateForDays(depId);
    }

    /// <summary>
    /// Calculate an aggregated list of <see cref="DayCount"/> records
    /// over the cached <see cref="ReturnStats"/>
    /// returning the total number of rides arriving per day.
    /// </summary>
    /// <param name="retId">
    /// Optional. When not null nor 0: only look at input records matching
    /// the given return station.
    /// </param>
    /// <returns>
    /// A list of <see cref="DayCount"/> records containing the number
    /// of rides arriving on each day, either for all stations or for the
    /// selected return station
    /// </returns>
    public List<DayCount> GetReturnsForDays(
      int? retId)
    {
      return ReturnStats.AggregateForDays(retId);
    }

  }
}
