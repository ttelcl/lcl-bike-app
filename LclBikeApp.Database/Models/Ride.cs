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
  /// Describes a citybike ride including its DB-generated identifier.
  /// </summary>
  public class Ride: RideBase
  {
    /// <summary>
    /// Create a new Ride
    /// </summary>
    public Ride(
      Guid id,
      DateTime depTime,
      DateTime retTime,
      int depStationId,
      int retStationId,
      int distance,
      int duration)
      : base(depTime, retTime, depStationId, retStationId, distance, duration)
    {
      Id = id;
    }

    /// <summary>
    /// The ride identifier, generated upon insertion by the database
    /// </summary>
    public Guid Id { get; }

  }
}