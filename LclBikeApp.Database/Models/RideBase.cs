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
  /// Describes a citybike ride excluding its DB-generated identifier.
  /// The subclass Ride includes that and fully describes the content of
  /// a row in table "Rides".
  /// </summary>
  public class RideBase
  {
    /// <summary>
    /// Create a new RideBase
    /// </summary>
    public RideBase(
      DateTime depTime,
      DateTime retTime,
      int depStationId,
      int retStationId,
      int distance,
      int duration)
    {
      DepTime = depTime;
      RetTime = retTime;
      DepStationId = depStationId;
      RetStationId = retStationId;
      Distance = distance;
      Duration = duration;
    }

    /// <summary>
    /// The departure time, in Helsinki local time
    /// </summary>
    public DateTime DepTime { get; }

    /// <summary>
    /// The return time, in Helsinki local time
    /// </summary>
    public DateTime RetTime { get; }

    /// <summary>
    /// The ID of the departure station (foreign key to Station)
    /// </summary>
    public int DepStationId { get; }

    /// <summary>
    /// The ID of the return station (foreign key to Station)
    /// </summary>
    public int RetStationId { get; }

    /// <summary>
    /// The distance in meters
    /// </summary>
    public int Distance { get; }

    /// <summary>
    /// The specified duration of the ride in seconds. This is usually
    /// different by a few seconds from the difference between start
    /// time and end time.
    /// </summary>
    public int Duration { get; }

  }
}
