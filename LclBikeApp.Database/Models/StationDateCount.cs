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
  /// A record reporting a count of rides for one station and one day.
  /// This is used both for (DepartureStation, DepartureDay) and for
  /// (ReturnStation, ReturnDay) keys.
  /// </summary>
  public struct StationDateCount
  {
    /// <summary>
    /// Create a new StationDateCount
    /// </summary>
    public StationDateCount(int stationId, DateTime day, int count)
    {
      StationId = stationId;
      Day = day;
      Count = count;
    }

    /// <summary>
    /// The ride count
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// The Station ID (Departure or Return)
    /// </summary>
    public int StationId { get; }

    // public DateOnly Day { get; }
    // We'd like to use the new "DateOnly" (.net 6) type here, but not all
    // external libraries support it without hacks.

    /// <summary>
    /// The date part of the departure or return time.
    /// (Typed as DateTime since DateOnly is not fully supported by external
    /// libraries yet)
    /// </summary>
    public DateTime Day { get; }

  }
}