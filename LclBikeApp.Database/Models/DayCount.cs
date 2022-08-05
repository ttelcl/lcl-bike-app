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
  /// Combines a Day and some (unspecified) count
  /// </summary>
  public struct DayCount
  {
    /// <summary>
    /// Create a new DayCount
    /// </summary>
    public DayCount(
      DateTime day,
      int count)
    {
      Day = day;
      Count = count;
    }

    /// <summary>
    /// The day (cast as DateTime because DateOnly isn't well supported)
    /// </summary>
    public DateTime Day { get; }

    /// <summary>
    /// The count (what exactly is counted depends on context)
    /// </summary>
    public int Count { get; }
  }
}
