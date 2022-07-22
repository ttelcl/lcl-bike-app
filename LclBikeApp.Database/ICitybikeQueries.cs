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
    /// Return a page from the Rides table, sorted in the order
    /// DepTime, RetTime, DepStation, RetStation, Distance, Duration,
    /// in the specified departure time range
    /// </summary>
    /// <param name="pageSize">
    /// The page size
    /// </param>
    /// <param name="pageOffset">
    /// The page offset
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
    IReadOnlyList<RideBase> GetRidesPage(
      int pageSize,
      int pageOffset,
      DateTime? fromTime=null,
      DateTime? toTime=null);

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
    /// Return a page from the Rides table for a specific departure station,
    /// sorted in the order
    /// DepTime, RetTime, DepStation, RetStation, Distance, Duration,
    /// in the specified departure time range
    /// </summary>
    /// <param name="pageSize">
    /// The page size
    /// </param>
    /// <param name="pageOffset">
    /// The page offset
    /// </param>
    /// <param name="depStationId">
    /// The identifier of the departure station to look for
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
    IReadOnlyList<RideBase> GetDepartingRidesPage(
      int pageSize,
      int pageOffset,
      int depStationId,
      DateTime? fromTime = null,
      DateTime? toTime = null);

    /// <summary>
    /// Return the total number of rides in the given departure time range
    /// for the given departure station
    /// (suitable for calculating the number of pages available from
    /// GetDepartingRidesPage())
    /// </summary>
    /// <param name="depStationId">
    /// The identifier of the departure station to look for
    /// </param>
    /// <param name="fromTime">
    /// The earliest departure time to report (or null to not restrict)
    /// </param>
    /// <param name="toTime">
    /// The final departure time to report (or null to not restrict)
    /// </param>
    /// <returns>
    /// The number of rides in the specified time range
    /// </returns>
    int GetDepartingRidesCount(
      int depStationId,
      DateTime? fromTime = null,
      DateTime? toTime = null);

    /// <summary>
    /// Return a page from the Rides table for a specific return station,
    /// sorted in the order
    /// DepTime, RetTime, DepStation, RetStation, Distance, Duration,
    /// in the specified RETURN time range
    /// </summary>
    /// <param name="pageSize">
    /// The page size
    /// </param>
    /// <param name="pageOffset">
    /// The page offset
    /// </param>
    /// <param name="retStationId">
    /// The identifier of the return station to look for
    /// </param>
    /// <param name="fromTime">
    /// The earliest RETURN time to report (or null to not restrict)
    /// </param>
    /// <param name="toTime">
    /// The final RETURN time to report (or null to not restrict)
    /// </param>
    /// <returns>
    /// The requested page of rides
    /// </returns>
    IReadOnlyList<RideBase> GetReturningRidesPage(
      int pageSize,
      int pageOffset,
      int retStationId,
      DateTime? fromTime = null,
      DateTime? toTime = null);

    /// <summary>
    /// Return the total number of rides in the given RETURN time range
    /// for the given return station
    /// (suitable for calculating the number of pages available from
    /// GetReturningRidesPage())
    /// </summary>
    /// <param name="retStationId">
    /// The identifier of the RETURN station to look for
    /// </param>
    /// <param name="fromTime">
    /// The earliest return time to report (or null to not restrict)
    /// </param>
    /// <param name="toTime">
    /// The final return time to report (or null to not restrict)
    /// </param>
    /// <returns>
    /// The number of rides in the specified time range
    /// </returns>
    int GetReturningRidesCount(
      int retStationId,
      DateTime? fromTime = null,
      DateTime? toTime = null);

  }
}

