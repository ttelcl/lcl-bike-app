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
  /// Extension methods for aggregating
  /// </summary>
  public static class AggregatingExtensions
  {
    /// <summary>
    /// Calculate an aggregated list of (station, rideCount) statistics
    /// over a sequence of <see cref="StationDateCount"/> records.
    /// </summary>
    /// <param name="stats">
    /// The <see cref="StationDateCount"/> records to aggregate
    /// </param>
    /// <param name="firstDay">
    /// Optional first day to take into account
    /// </param>
    /// <param name="lastDay">
    /// Optional last day to take into account
    /// </param>
    /// <returns>
    /// A list of <see cref="StationCount"/> records containing the
    /// Station Id and the total number of rides in the specified day interval.
    /// The list is sorted by station ID
    /// </returns>
    public static List<StationCount> AggregateForStations(
      this IEnumerable<StationDateCount> stats,
      DateOnly? firstDay = null,
      DateOnly? lastDay = null)
    {
      var filtered =
        firstDay.HasValue || lastDay.HasValue
        ? from stat in stats
          where (firstDay==null || DateOnly.FromDateTime(stat.Day) >= firstDay.Value)
             && (lastDay==null || DateOnly.FromDateTime(stat.Day) <= lastDay.Value)
          select stat
        : stats;
      var stationCounts =
        from stat in filtered
        group stat by stat.StationId into g
        orderby g.Key
        select new StationCount(g.Key, g.Sum(stat => stat.Count));
      return stationCounts.ToList();
    }

    /// <summary>
    /// Calculate an aggregated list of <see cref="DayCount"/> records
    /// over the sequence of <see cref="StationDateCount"/> records
    /// returning the total number of rides per day.
    /// </summary>
    /// <param name="stats">
    /// The <see cref="StationDateCount"/> records to aggregate
    /// </param>
    /// <param name="stationId">
    /// Optional. When not null nor 0: only look at input records matching
    /// the given station.
    /// </param>
    /// <returns>
    /// A list of <see cref="DayCount"/> records containing the number
    /// of rides on each day, either for all stations or for the selected station
    /// </returns>
    public static List<DayCount> AggregateForDays(
      this IEnumerable<StationDateCount> stats,
      int? stationId = null)
    {
      var sid = stationId ?? 0;
      var filtered =
        sid > 0
        ? from stat in stats
          where (sid<=0 || stat.StationId == sid)
          select stat
        : stats;
      var dayCounts =
        from stat in filtered
        group stat by stat.Day.Date into g
        orderby g.Key
        select new DayCount(g.Key, g.Sum(stat => stat.Count));
      return dayCounts.ToList();
    }

  }
}
