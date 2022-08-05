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
  /// Combines a StationId and some (unspecified) count
  /// </summary>
  public struct StationCount
  {
    /// <summary>
    /// Create a new StationCount
    /// </summary>
    public StationCount(
      int stationId,
      int count)
    {
      StationId = stationId;
      Count = count;
    }

    /// <summary>
    /// The station ID
    /// </summary>
    public int StationId { get; }

    /// <summary>
    /// The count (what exactly is counted depends on context)
    /// </summary>
    public int Count { get; }
  }
}
