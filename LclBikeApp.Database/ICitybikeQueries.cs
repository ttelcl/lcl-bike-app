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
  /// Models the database query API to be exposed to clients
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

    /// <summary>
    /// Returns a list of (DepartureStation, DepartureDay, RideCount) triplets.
    /// </summary>
    /// <returns>
    /// A list of StationDateCount records with ride departure information. 
    /// The order is random.
    /// </returns>
    /// <remarks>
    /// The return value is intended to be cached by the backend and reshaped
    /// to (station, count) or (day, count) pairs for the client. With the
    /// initial data there are just below 40000 records in the result.
    /// </remarks>
    StationDateCount[] GetDepartureStats();

    /// <summary>
    /// Returns a list of (ReturnStation, ReturnDay, RideCount) triplets.
    /// </summary>
    /// <returns>
    /// A list of StationDateCount records with ride return information
    /// The order is random.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The return value is intended to be cached by the backend and reshaped
    /// to (station, count) or (day, count) pairs for the client. With the
    /// initial data there are just below 40000 records in the result.
    /// </para>
    /// <para>
    /// Beware: the days in these records are the day of return. Most other
    /// timestamps / days in the API are about ride departure.
    /// </para>
    /// </remarks>
    StationDateCount[] GetReturnStats();

    /// <summary>
    /// Return the total number of rides between all pairs of stations,
    /// optionally constrained to a specific time interval.
    /// </summary>
    /// <param name="fromTime">
    /// If not null: the oldest ride departure time to include.
    /// </param>
    /// <param name="toTime">
    /// If not null: the newest ride _departure_ time to include.
    /// Yes, "departure time", not "return time", for the sake of database
    /// efficiency.
    /// </param>
    /// <returns>
    /// A list of departure station - return station - count triplets, in no
    /// particular order.
    /// </returns>
    StationPairCount[] GetStationPairCounts(
      DateTime? fromTime = null,
      DateTime? toTime = null);
  }
}

