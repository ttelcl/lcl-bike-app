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
  /// Data transfer object representing a start time and an end time
  /// </summary>
  public class TimeRange
  {
    /// <summary>
    /// Create a new TimeRange
    /// </summary>
    public TimeRange(DateTime startTime, DateTime endTime)
    {
      StartTime = startTime;
      EndTime = endTime;
    }

    /// <summary>
    /// The start time
    /// </summary>
    public DateTime StartTime { get; }

    /// <summary>
    /// The end time
    /// </summary>
    public DateTime EndTime { get; }

  }
}
