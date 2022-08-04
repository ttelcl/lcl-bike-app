/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LclBikeApp.Database.Models;

namespace LclBikeApp.Database
{
  /// <summary>
  /// Models the query API that will be exposed to clients
  /// </summary>
  public interface ICitybikeQueries
  {
    /// <summary>
    /// Returns the list of all involved cities.
    /// (That's just Helsinki &amp; Espoo, no need for detailed
    /// subset queries)
    /// </summary>
    /// <returns>
    /// A list of City instances, in ID order.
    /// </returns>
    IReadOnlyList<City> GetCities();

    /// <summary>
    /// Return the full list of all stations. 
    /// It seems this list is small enough (457 entries)
    /// to return in full, no server side paging required.
    /// </summary>
    IReadOnlyList<Station> GetStations();

    /// <summary>
    /// Get a single station record
    /// </summary>
    /// <param name="id">
    /// The station ID to find
    /// </param>
    /// <returns>
    /// The station if found, or null if not found
    /// </returns>
    Station? GetStation(int id);

    /// <summary>
    /// Get the range of Departure times in the rides table, or null if there
    /// are no rides.
    /// </summary>
    TimeRange? GetTimeRange();

    /// <summary>
    /// Return a page from the Rides table, sorted in the order
    /// DepTime, RetTime, DepStation, RetStation, Distance, Duration,
    /// in the specified departure time range
    /// </summary>
    /// <param name="pageSize">
    /// The page size
    /// </param>
    /// <param name="pageOffset">
    /// The row offset of the page to return
    /// </param>
    /// <param name="fromTime">
    /// The earliest departure time to report (or null to not restrict)
    /// </param>
    /// <param name="toTime">
    /// The final departure time to report (or null to not restrict)
    /// </param>
    /// <returns>
    /// The requested page of rides
    /// </returns>
    List<Ride> GetRidesPage(
      int pageSize,
      int pageOffset,
      DateTime? fromTime = null,
      DateTime? toTime = null);

    /// <summary>
    /// Return the total number of rides in the given departure time range
    /// (suitable for calculating the number of pages available from
    /// GetRidesPage())
    /// </summary>
    /// <param name="fromTime">
    /// The earliest departure time to report (or null to not restrict)
    /// </param>
    /// <param name="toTime">
    /// The final departure time to report (or null to not restrict)
    /// </param>
    /// <returns>
    /// The number of rides in the specified time range
    /// </returns>
    int GetRidesCount(
      DateTime? fromTime = null,
      DateTime? toTime = null);

    /// <summary>
    /// Generic Rides Query, returning one page of query results
    /// </summary>
    /// <param name="pageSize">
    /// The (maximum) number of returned results
    /// </param>
    /// <param name="pageOffset">
    /// The row offset of the page to return
    /// </param>
    /// <param name="fromTime">
    /// Only return rides at or after this time, or null not to constrain.
    /// Depending on other parameters, this may apply to a departure or arrival time
    /// </param>
    /// <param name="toTime">
    /// Only return rides at or before this time, or null not to constrain.
    /// Depending on other parameters, this may apply to a departure or arrival time
    /// </param>
    /// <param name="depId">
    /// Only return rides starting at this station, or 0 to return all
    /// </param>
    /// <param name="retId">
    /// Only return rides ending at this station, or 0 to return all.
    /// If not 0, and <paramref name="depId"/> is 0, the time bounds will apply
    /// to the return time instead of the departure time.
    /// </param>
    /// <param name="distMin">
    /// Minimum ride distance in meters (or 0 to ignore)
    /// </param>
    /// <param name="distMax">
    /// Maximum ride distance in meters (or Int32.MaxValue to ignore)
    /// </param>
    /// <param name="durMin">
    /// Minimum ride duration in seconds (or 0 to ignore)
    /// </param>
    /// <param name="durMax">
    /// Minimum ride duration in seconds (or Int32.MaxValue to ignore)
    /// </param>
    /// <param name="sort">
    /// Placeholder for future sorting support. Currently must be blank.
    /// Sort order is automagically implied by other parameters.
    /// </param>
    /// <returns>
    /// The requested list of rides
    /// </returns>
    List<Ride> GetRidesPage2(
      int pageSize,
      int pageOffset,
      DateTime? fromTime = null,
      DateTime? toTime = null,
      int depId = 0,
      int retId = 0,
      int distMin = 0,
      int distMax = Int32.MaxValue,
      int durMin = 0,
      int durMax = Int32.MaxValue,
      string sort = "");

    /// <summary>
    /// Get the total number of results for the GetRidesPage2 query
    /// with the specified parameters
    /// </summary>
    /// <param name="fromTime">
    /// Only return rides at or after this time, or null not to constrain.
    /// Depending on other parameters, this may apply to a departure or arrival time
    /// </param>
    /// <param name="toTime">
    /// Only return rides at or before this time, or null not to constrain.
    /// Depending on other parameters, this may apply to a departure or arrival time
    /// </param>
    /// <param name="depId">
    /// Only return rides starting at this station, or 0 to return all
    /// </param>
    /// <param name="retId">
    /// Only return rides ending at this station, or 0 to return all.
    /// If not 0, and <paramref name="depId"/> is 0, the time bounds will apply
    /// to the return time instead of the departure time.
    /// </param>
    /// <param name="distMin">
    /// Minimum ride distance in meters (or 0 to ignore)
    /// </param>
    /// <param name="distMax">
    /// Maximum ride distance in meters (or Int32.MaxValue to ignore)
    /// </param>
    /// <param name="durMin">
    /// Minimum ride duration in seconds (or 0 to ignore)
    /// </param>
    /// <param name="durMax">
    /// Minimum ride duration in seconds (or Int32.MaxValue to ignore)
    /// </param>
    /// <returns>
    /// The total number of rides matching the query
    /// </returns>
    int GetRidesCount2(
      DateTime? fromTime = null,
      DateTime? toTime = null,
      int depId = 0,
      int retId = 0,
      int distMin = 0,
      int distMax = Int32.MaxValue,
      int durMin = 0,
      int durMax = Int32.MaxValue);

    ///// <summary>
    ///// Return a page from the Rides table for a specific departure station,
    ///// sorted in the order
    ///// DepTime, RetTime, DepStation, RetStation, Distance, Duration,
    ///// in the specified departure time range
    ///// </summary>
    ///// <param name="pageSize">
    ///// The page size
    ///// </param>
    ///// <param name="pageOffset">
    ///// The page offset
    ///// </param>
    ///// <param name="depStationId">
    ///// The identifier of the departure station to look for
    ///// </param>
    ///// <param name="fromTime">
    ///// The earliest departure time to report (or null to not restrict)
    ///// </param>
    ///// <param name="toTime">
    ///// The final departure time to report (or null to not restrict)
    ///// </param>
    ///// <returns>
    ///// The requested page of rides
    ///// </returns>
    //IReadOnlyList<Ride> GetDepartingRidesPage(
    //  int pageSize,
    //  int pageOffset,
    //  int depStationId,
    //  DateTime? fromTime = null,
    //  DateTime? toTime = null);

    ///// <summary>
    ///// Return the total number of rides in the given departure time range
    ///// for the given departure station
    ///// (suitable for calculating the number of pages available from
    ///// GetDepartingRidesPage())
    ///// </summary>
    ///// <param name="depStationId">
    ///// The identifier of the departure station to look for
    ///// </param>
    ///// <param name="fromTime">
    ///// The earliest departure time to report (or null to not restrict)
    ///// </param>
    ///// <param name="toTime">
    ///// The final departure time to report (or null to not restrict)
    ///// </param>
    ///// <returns>
    ///// The number of rides in the specified time range
    ///// </returns>
    //int GetDepartingRidesCount(
    //  int depStationId,
    //  DateTime? fromTime = null,
    //  DateTime? toTime = null);

    ///// <summary>
    ///// Return a page from the Rides table for a specific return station,
    ///// sorted in the order
    ///// DepTime, RetTime, DepStation, RetStation, Distance, Duration,
    ///// in the specified RETURN time range
    ///// </summary>
    ///// <param name="pageSize">
    ///// The page size
    ///// </param>
    ///// <param name="pageOffset">
    ///// The page offset
    ///// </param>
    ///// <param name="retStationId">
    ///// The identifier of the return station to look for
    ///// </param>
    ///// <param name="fromTime">
    ///// The earliest RETURN time to report (or null to not restrict)
    ///// </param>
    ///// <param name="toTime">
    ///// The final RETURN time to report (or null to not restrict)
    ///// </param>
    ///// <returns>
    ///// The requested page of rides
    ///// </returns>
    //IReadOnlyList<Ride> GetReturningRidesPage(
    //  int pageSize,
    //  int pageOffset,
    //  int retStationId,
    //  DateTime? fromTime = null,
    //  DateTime? toTime = null);

    ///// <summary>
    ///// Return the total number of rides in the given RETURN time range
    ///// for the given return station
    ///// (suitable for calculating the number of pages available from
    ///// GetReturningRidesPage())
    ///// </summary>
    ///// <param name="retStationId">
    ///// The identifier of the RETURN station to look for
    ///// </param>
    ///// <param name="fromTime">
    ///// The earliest return time to report (or null to not restrict)
    ///// </param>
    ///// <param name="toTime">
    ///// The final return time to report (or null to not restrict)
    ///// </param>
    ///// <returns>
    ///// The number of rides in the specified time range
    ///// </returns>
    //int GetReturningRidesCount(
    //  int retStationId,
    //  DateTime? fromTime = null,
    //  DateTime? toTime = null);

  }
}

