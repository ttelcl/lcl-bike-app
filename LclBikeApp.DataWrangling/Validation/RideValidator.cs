/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LclBikeApp.DataWrangling.RawModel;

namespace LclBikeApp.DataWrangling.Validation
{
  /// <summary>
  /// Encapsulates the logic of validating ride records
  /// </summary>
  public class RideValidator
  {
    private readonly Dictionary<string, int> _statistics;
    private DateTime _maxDepartureTime;
    private bool _hadOrderRejection;

    /// <summary>
    /// Create a new RideValidator
    /// </summary>
    /// <param name="validationParameters">
    /// The validation rule parameters
    /// </param>
    /// <param name="knownStationIds">
    /// The collection of known Citybike Station IDs
    /// </param>
    public RideValidator(
      ValidationConfiguration validationParameters,
      IEnumerable<int> knownStationIds)
    {
      _statistics = new Dictionary<string, int>();
      ValidationParameters = validationParameters;
      KnownStationIds = new HashSet<int>(knownStationIds);
      Reset();
    }

    /// <summary>
    /// Reset the rejection statistics and departure time check state
    /// </summary>
    public void Reset()
    {
      _statistics.Clear();
      _statistics[AcceptedKey] = 0;
      _maxDepartureTime = DateTime.MaxValue;
      _hadOrderRejection = false;
    }

    /// <summary>
    /// The validation rule parameters
    /// </summary>
    public ValidationConfiguration ValidationParameters { get; }

    /// <summary>
    /// The collection of known Citybike Station IDs
    /// </summary>
    public HashSet<int> KnownStationIds { get; }

    /// <summary>
    /// Check the record currently in the given cursor. If validation
    /// rejects it, a rejection reason string is returned. If it is not
    /// rejected, null is returned. This method cannot check for duplicates,
    /// since it has no access to those. As a side effect, rejection statistics
    /// are updated.
    /// </summary>
    /// <param name="cursor">
    /// The cursor pointing to the current CSV file row.
    /// </param>
    /// <returns>
    /// Null if the record is valid, a rejection string otherwise.
    /// </returns>
    public string? CheckAndTrack(RideCursor cursor)
    {
      var rejection = ExplainInvalid(ValidationParameters, KnownStationIds, cursor);
      if(rejection == null && ValidationParameters.RequireNonAscendingDepartures)
      {
        var dep = cursor.DepTime;
        if(dep > _maxDepartureTime || (_hadOrderRejection && dep >= _maxDepartureTime))
        {
          rejection = "Departure timestamp is ascending (duplicate data suspected)";
          if(!_hadOrderRejection)
          {
            // Once a record is rejected as out-of-order, make sure not to accept
            // records with exactly that same maximum departure time anymore, but require
            // a strictly earlier time.
            _hadOrderRejection = true;
          }
        }
        else
        {
          _maxDepartureTime = dep;
          _hadOrderRejection = false;
        }
      }
      var reason = rejection == null ? AcceptedKey : rejection;
      if(_statistics.TryGetValue(reason, out _))
      {
        _statistics[reason]++;
      }
      else
      {
        _statistics[reason] = 1;
      }

      return rejection;
    }

    /// <summary>
    /// Validate all records presented through the cursor sequence, returning only those
    /// that are accepted, and as side effect keeping statistics on the rejection reasons
    /// (retrievable via the "Statistics" property aferward)
    /// </summary>
    /// <param name="cursors">
    /// The sequence of a cursor object bound to consecutive CSV data rows.
    /// </param>
    /// <returns>
    /// The sequence of the inputs that were not rejected.
    /// </returns>
    public IEnumerable<RideCursor> Validate(IEnumerable<RideCursor> cursors)
    {
      foreach(var cursor in cursors)
      {
        var reason = CheckAndTrack(cursor);
        if(reason == null)
        {
          yield return cursor;
        }
      }
    }

    /// <summary>
    /// The constant key used in the statistics to indicate accepted records
    /// </summary>
    public const string AcceptedKey = "ACCEPTED";

    /// <summary>
    /// Statistics for each rejection reason plus "ACCEPTED" (AcceptedKey).
    /// </summary>
    public IReadOnlyDictionary<string, int> Statistics => _statistics;

    /// <summary>
    /// The number of candidate records offered to CheckAndTrack() since
    /// the last Reset()
    /// </summary>
    public int CandidateCount => Statistics.Values.Sum();

    /// <summary>
    /// The number of candidate records offered to CheckAndTrack() since
    /// the last Reset() that were accepted
    /// </summary>
    public int AcceptedCount => Statistics[AcceptedKey];

    /// <summary>
    /// Returns a fixed string containing the explanation on why the
    /// ride is rejected, or returns null to indicate that the ride
    /// is accepted.
    /// Note that this method cannot check for duplicates, since it
    /// has access to a single record only.
    /// </summary>
    /// <param name="cfg">
    /// The parameters for the validation rules.
    /// </param>
    /// <param name="knownStations">
    /// The set of known station IDs. Rides with departure or return
    /// station IDs not in this list are rejected.
    /// </param>
    /// <param name="cursor">
    /// The cursor holding the raw ride data recordto validate (loaded from CSV)
    /// </param>
    /// <returns>
    /// A string explaining the rejection reason, or null to indicate the
    /// record is accepted.
    /// </returns>
    internal static string? ExplainInvalid(
      ValidationConfiguration cfg,
      HashSet<int> knownStations,
      RideCursor cursor)
    {
      if(!cursor.HasData)
      {
        return "No data";
      }
      if(!cursor.MayBeValid)
      {
        return "Incomplete data (distance field blank)";
      }
      var distance = (int)Math.Round(cursor.Distance);
      if(distance < cfg.MinDistance)
      {
        return "Distance too short";
      }
      if(distance > cfg.MaxDistance)
      {
        return "Distance too far";
      }
      var duration = cursor.Duration;
      if(duration < cfg.MinDuration)
      {
        return "Duration too short";
      }
      if(duration > cfg.MaxDuration)
      {
        return "Duration too long";
      }
      var t0 = cursor.DepTime;
      var t1 = cursor.RetTime;
      var dt = t1 - t0;
      if(dt < TimeSpan.Zero)
      {
        return "Return time before departure time";
      }
      var ddt = dt.TotalSeconds - duration;
      if(ddt < -cfg.TimeTolerance || ddt > cfg.TimeTolerance)
      {
        return "Difference between given and calculated duration outside tolerance interval";
      }
      if(!knownStations.Contains(cursor.DepStation))
      {
        return "Unknown departure station";
      }
      if(!knownStations.Contains(cursor.RetStation))
      {
        return "Unknown return station";
      }
      return null;
    }
  }
}
