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
  /// A simple record for transfering a (departure station id, 
  /// return station id, count, totaldistance, totalduration) quintuplet,
  /// a building block for reporting total ride counts and average distances
  /// and durations.
  /// </summary>
  public struct StationPairStats
  {
    /// <summary>
    /// Create a new StationPairCount
    /// </summary>
    public StationPairStats(
      int depId,
      int retId,
      int count,
      int distSum,
      int durSum)
    {
      DepId = depId;
      RetId = retId;
      Count = count;
      DistSum = distSum;
      DurSum = durSum;
    }

    /// <summary>
    /// The departure station ID
    /// </summary>
    public int DepId { get; }

    /// <summary>
    /// The return station ID
    /// </summary>
    public int RetId { get; }

    /// <summary>
    /// The count of rides from DepId to Retid
    /// (for some externally defined time interval)
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// The sum of all distances (in meters)
    /// </summary>
    public int DistSum { get; }

    /// <summary>
    /// The sum of all durations (in seconds)
    /// </summary>
    public int DurSum { get; }
  }
}