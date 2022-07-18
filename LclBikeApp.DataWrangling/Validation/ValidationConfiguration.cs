/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace LclBikeApp.DataWrangling.Validation
{
  /// <summary>
  /// Parameters for the ride validation rules
  /// </summary>
  public class ValidationConfiguration
  {
    /// <summary>
    /// Create a new ValidationConfiguration
    /// </summary>
    public ValidationConfiguration()
    {
    }

    /// <summary>
    /// Convert a JSON string to a validation configuration
    /// </summary>
    public static ValidationConfiguration FromJson(string json)
    {
      return JsonConvert.DeserializeObject<ValidationConfiguration>(json)!;
    }

    /// <summary>
    /// Convert to JSON
    /// </summary>
    public string ToJson()
    {
      return JsonConvert.SerializeObject(this, Formatting.Indented);
    }

    /// <summary>
    /// The minimum distance (in meters). Default 400 m.
    /// </summary>
    public int MinDistance { get; set; } = 400;

    /// <summary>
    /// The maximum distance (in meters). Default 8000 m.
    /// </summary>
    public int MaxDistance { get; set; } = 8000;

    /// <summary>
    /// The minimum ride duration in seconds. Default 120 seconds
    /// </summary>
    public int MinDuration { get; set; } = 120;

    /// <summary>
    /// The maximum ride duration in seconds. Default 14400 seconds (4 hours)
    /// </summary>
    public int MaxDuration { get; set; } = 14400;

    /// <summary>
    /// The number of seconds the specified ride duration
    /// is allowed to deviate from the calculated duration.
    /// Default 20 seconds
    /// </summary>
    public int TimeTolerance { get; set; } = 20;

    /// <summary>
    /// When true, each successive data record must refer to a departure time
    /// that is no later than the previous one.
    /// This is a "hacky" workaround to the raw data containing all data rows
    /// twice, with the second half of the file equal to the first, and both
    /// in non-ascending order (last record first).
    /// Default true.
    /// </summary>
    public bool RequireNonAscendingDepartures { get; set; } = true;
  }
}
