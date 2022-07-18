/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using XsvLib;
using XsvLib.Tables.Cursor;

namespace LclBikeApp.DataWrangling.RawModel
{
  /// <summary>
  /// Describes a ride, as found in the raw CSV data files, as
  /// a cursor view on row in that table.
  /// Note that the cursor may expose data that is not validated.
  /// </summary>
  public class RideCursor: XsvCursor
  {
    private readonly MappedColumn _colDepTime;
    private readonly MappedColumn _colRetTime;
    private readonly MappedColumn _colDepStation;
    private readonly MappedColumn _colDepStationName;
    private readonly MappedColumn _colRetStation;
    private readonly MappedColumn _colRetStationName;
    private readonly MappedColumn _colDistance;
    private readonly MappedColumn _colDuration;

    private static string[] _timeFormats = new[] {
      "yyyy-MM-dd'T'HH:mm:ss",
      "yyyy-MM-dd"
    };

    /// <summary>
    /// Create a new RideCursor
    /// </summary>
    public RideCursor()
      : base(null)
    {
      var map = ColumnMapping;
      _colDepTime = map.Declare("Departure");
      _colRetTime = map.Declare("Return");
      _colDepStation = map.Declare("Departure station id");
      _colDepStationName = map.Declare("Departure station name");
      _colRetStation = map.Declare("Return station id");
      _colRetStationName = map.Declare("Return station name");
      _colDistance = map.Declare("Covered distance (m)");
      _colDuration = map.Declare("Duration (sec.)");
    }

    /// <summary>
    /// True when a potentially valid record is loaded. When false,
    /// accessing other properties may throw an exception.
    /// Specifically: this ensures that there is any data at all and 
    /// the Distance column is not blank
    /// </summary>
    public bool MayBeValid => HasData && !String.IsNullOrEmpty(this[_colDistance]);

    /// <summary>
    /// The departure time
    /// </summary>
    public DateTime DepTime => ParseCitybikeTime(GetString(_colDepTime));

    /// <summary>
    /// The return time
    /// </summary>
    public DateTime RetTime => ParseCitybikeTime(GetString(_colRetTime));

    /// <summary>
    /// The identifier of the departure station
    /// </summary>
    public int DepStation => GetInt32(_colDepStation);

    /// <summary>
    /// The name of the departure station as specified in the data file
    /// </summary>
    public string DepStationName => GetString(_colDepStationName);

    /// <summary>
    /// The identifier of the return station
    /// </summary>
    public int RetStation => GetInt32(_colRetStation);

    /// <summary>
    /// The name of the return station as specified in the data file
    /// </summary>
    public string RetStationName => GetString(_colRetStationName);

    /// <summary>
    /// The distance as specified in the data file (in meters).
    /// Alert! Only access this field after checking MayBeValid.
    /// Accessing this field when the distance column is blank will throw
    /// an exception.
    /// Also note this field is almost always an integer. "almost" ...
    /// </summary>
    public double Distance => Double.Parse(GetString(_colDistance), CultureInfo.InvariantCulture);

    /// <summary>
    /// The duration of the ride in seconds, as specified explicitly in the data
    /// file. This will usually be a few seconds off from the difference
    /// between start and end time.
    /// </summary>
    public int Duration => GetInt32(_colDuration);

    private static DateTime ParseCitybikeTime(string t)
    {
      return DateTime.ParseExact(
        t, _timeFormats, CultureInfo.InvariantCulture, 
        DateTimeStyles.None);
    }

  }
}
