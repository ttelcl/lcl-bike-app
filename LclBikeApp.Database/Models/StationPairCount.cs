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
  /// return station id, count) triplet, for reporting total ride
  /// counts.
  /// </summary>
  public struct StationPairCount
  {
    /// <summary>
    /// Create a new StationPairCount
    /// </summary>
    public StationPairCount(
      int depId,
      int retId,
      int count)
    {
      DepId = depId;
      RetId = retId;
      Count = count;
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

  }
}