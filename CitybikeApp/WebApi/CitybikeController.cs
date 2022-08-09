using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using LclBikeApp.Database.Models;
using LclBikeApp.Database;

using CitybikeApp.Services;

namespace CitybikeApp.WebApi
{

  /// <summary>
  /// Implements the citybike Web API
  /// </summary>
  [Route("api/citybike")]  // Using explicit "citybike" instead of "[controller]"
  [ApiController]
  public class CitybikeController: ControllerBase
  {
    private readonly ILogger _logger;

    /*
     * NOTE on the XML-comment formatting: the format is slightly unusual, because
     * the comments are picked up by Swagger, which uses Markdown-like formatting
     * instead of the usual .net XML-doc style
     * 
     * The declaration order of API methods matters for that same reason.
     */

    /// <summary>
    /// Create the controller
    /// </summary>
    public CitybikeController(
      ILogger<CitybikeController> logger)
    {
      _logger = logger;
      // _logger.LogInformation("Hello CitybikeController!");
    }

    /// <summary>
    /// Get a full list of all involved cities, loaded from the cache.
    /// (Spoiler: that's just Helsinki and Espoo)
    /// </summary>
    /// <remarks>
    /// Example:
    /// 
    ///     GET api/citybike/cities-raw
    /// </remarks>
    /// <returns>
    /// A list of <see cref="City"/> instances
    /// </returns>
    /// <response code="200">On success</response>
    [HttpGet("cities")]
    public IReadOnlyList<City> GetCities(
      [FromServices] StationListService sls
    )
    {
      var cities = sls.Cities.Values.ToList();
      return cities;
    }

    /// <summary>
    /// Return the list of all known stations from the cache
    /// </summary>
    /// <param name="sls">
    /// The station cache accessor
    /// </param>
    /// <returns>
    /// The list of stations on success
    /// </returns>
    /// <response code="200">On success</response>
    [HttpGet("stations")]
    public IReadOnlyList<Station> GetStations(
      [FromServices] StationListService sls)
    {
      var stations = sls.Stations.Values.ToList();
      return stations;
    }

    /// <summary>
    /// Return an object containing the first and last departure times for all rides
    /// in the database, or returns an empty 204 response in case there are no rides at all
    /// </summary>
    /// <param name="db">
    /// The DB service
    /// </param>
    /// <returns>
    /// An object with "startTime" and "endTime" fields specifying the first and last
    /// available ride departure times. Or a 204 response.
    /// </returns>
    /// <response code="200">On success</response>
    /// <response code="204">When there are no rides in the DB at all.</response>
    [HttpGet("timerange")]
    public ActionResult<TimeRange> GetTimeRange(
      [FromServices] ICitybikeDb db)
    {
      var icq = db.GetQueryApi();
      var result = icq.GetTimeRange();
      if(result == null)
      {
        return NoContent();
      }
      else
      {
        return result;
      }
    }

    /// <summary>
    /// Return the number of rides for the given query parameters
    /// </summary>
    /// <param name="db">
    /// The DB service
    /// </param>
    /// <param name="t0">
    /// The start time. Date-only values are interpreted as time 00:00:00.
    /// </param>
    /// <param name="t1">
    /// The end time. Date-only values are interpreted as time 23:59:59
    /// </param>
    /// <param name="depid">
    /// The departure station id, or 0 for "all"
    /// </param>
    /// <param name="retid">
    /// The return station id, or 0 for "all"
    /// </param>
    /// <param name="distmin">
    /// The minimum distance in meters, or 0 for "any"
    /// </param>
    /// <param name="distmax">
    /// The maximum distance in meters, or 0 for "any"
    /// </param>
    /// <param name="secmin">
    /// The minimum duration in seconds, or 0 for "no minimum"
    /// </param>
    /// <param name="secmax">
    /// The maximum duration in seconds, or 0 for "no maximum"
    /// </param>
    /// <returns>
    /// The total number of rides for the given query parameters
    /// </returns>
    /// <remarks>
    /// The following time formats are supported:
    /// 
    /// * _(blank or omitted)_
    /// * yyyy-MM-dd
    /// * yyyyMMdd
    /// * yyyyMMdd-HHmm
    /// * yyyyMMdd-HHmmss
    /// 
    /// </remarks>
    /// <response code="200">On success</response>
    /// <response code="400">On unrecognized date/time format</response>
    [HttpGet("ridescount2")]
    public ActionResult<int> GetRidesCount2(
      [FromServices] ICitybikeDb db,
      [FromQuery] string? t0 = null,
      [FromQuery] string? t1 = null,
      [FromQuery] int depid = 0,
      [FromQuery] int retid = 0,
      [FromQuery] int distmin = 0,
      [FromQuery] int distmax = 0,
      [FromQuery] int secmin = 0,
      [FromQuery] int secmax = 0)
    {
      DateTime? dt0, dt1;
      try
      {
        dt0 = ParseTime(t0, false);
      }
      catch(ArgumentException aex)
      {
        return BadRequest(aex.Message);
      }
      try
      {
        dt1 = ParseTime(t1, true);
      }
      catch(ArgumentException aex)
      {
        return BadRequest(aex.Message);
      }
      if(depid < 0)
      {
        depid = 0;
      }
      if(retid < 0)
      {
        retid = 0;
      }
      if(secmin < 0)
      {
        secmin = 0;
      }
      if(secmax <= 0 || secmax > 3600*24*31)
      {
        secmax = Int32.MaxValue;
      }
      if(distmin < 0)
      {
        distmin = 0;
      }
      if(distmax <= 0 || distmax > 40000)
      {
        distmax = Int32.MaxValue;
      }
      var icq = db.GetQueryApi();
      try
      {
        return icq.GetRidesCount2(dt0, dt1, depid, retid, distmin, distmax, secmin, secmax);
      }
      catch(NotImplementedException nie)
      {
        return StatusCode(500, $"Not Implemented ({nie.Message})");
      }
    }

    /// <summary>
    /// Get a page of the rides table for the given query parameters
    /// </summary>
    /// <param name="db">
    /// The database accessor
    /// </param>
    /// <param name="offset">
    /// The ride offset where the page starts (default 0)
    /// </param>
    /// <param name="pagesize">
    /// the page size (default 50)
    /// </param>
    /// <param name="t0">
    /// The start time. Date-only values are interpreted as time 00:00:00.
    /// </param>
    /// <param name="t1">
    /// The end time. Date-only values are interpreted as time 23:59:59
    /// </param>
    /// <param name="depid">
    /// The departure station id, or 0 for "all"
    /// </param>
    /// <param name="retid">
    /// The return station id, or 0 for "all"
    /// </param>
    /// <param name="distmin">
    /// The minimum distance in meters, or 0 for "any"
    /// </param>
    /// <param name="distmax">
    /// The maximum distance in meters, or 0 for "any"
    /// </param>
    /// <param name="secmin">
    /// The minimum duration in seconds, or 0 for "no minimum"
    /// </param>
    /// <param name="secmax">
    /// The maximum duration in seconds, or 0 for "no maximum"
    /// </param>
    /// <param name="sort">
    /// Sort order hint. Currently not supported! Must be omitted, blank, or "default"
    /// </param>
    /// <returns>
    /// A list of up to <paramref name="pagesize"/> rides
    /// </returns>
    /// <remarks>
    /// The following time formats are supported:
    /// 
    /// * _(blank or omitted)_
    /// * yyyy-MM-dd
    /// * yyyyMMdd
    /// * yyyyMMdd-HHmm
    /// * yyyyMMdd-HHmmss
    /// </remarks>
    /// <response code="200">On success</response>
    /// <response code="400">On unrecognized date/time format</response>
    [HttpGet("ridespage2")]
    public ActionResult<List<Ride>> GetRidesPage2(
      [FromServices] ICitybikeDb db,
      [FromQuery] int offset = 0,
      [FromQuery] int pagesize = 50,
      [FromQuery] string? t0 = null,
      [FromQuery] string? t1 = null,
      [FromQuery] int depid = 0,
      [FromQuery] int retid = 0,
      [FromQuery] int distmin = 0,
      [FromQuery] int distmax = 0,
      [FromQuery] int secmin = 0,
      [FromQuery] int secmax = 0,
      [FromQuery] string? sort = "")
    {
      DateTime? dt0, dt1;
      try
      {
        dt0 = ParseTime(t0, false);
      }
      catch(ArgumentException aex)
      {
        return BadRequest(aex.Message);
      }
      try
      {
        dt1 = ParseTime(t1, true);
      }
      catch(ArgumentException aex)
      {
        return BadRequest(aex.Message);
      }
      if(pagesize > 1000)
      {
        return BadRequest("Maximum page size is 1000");
      }
      if(depid < 0)
      {
        depid = 0;
      }
      if(retid < 0)
      {
        retid = 0;
      }
      if(secmin < 0)
      {
        secmin = 0;
      }
      if(secmax <= 0 || secmax > 3600*24*31)
      {
        secmax = Int32.MaxValue;
      }
      if(distmin < 0)
      {
        distmin = 0;
      }
      if(distmax <= 0 || distmax > 40000)
      {
        distmax = Int32.MaxValue;
      }
      if(String.IsNullOrEmpty(sort) || sort=="default")
      {
        sort = "";
      }
      else
      {
        return BadRequest("Sorting is not yet supported");
      }
      var icq = db.GetQueryApi();
      try
      {
        var rides = icq.GetRidesPage2(
          pagesize, offset,
          dt0, dt1,
          depid, retid,
          distmin, distmax,
          secmin, secmax,
          sort);
        return rides;
      }
      catch(NotImplementedException nie)
      {
        return StatusCode(500, $"Not Implemented ({nie.Message})");
      }
    }

    /// <summary>
    /// Return a list of (departure station, return station, ride count,
    /// total distance, total duration)
    /// records, optionally constrained to the given time interval.
    /// Beware! Use the "cap" parameter when invoking from Swagger,
    /// or it will choke on the large result!
    /// </summary>
    /// <param name="db">
    /// The database accessor
    /// </param>
    /// <param name="t0">
    /// The start time. Date-only values are interpreted as having time 00:00:00.
    /// </param>
    /// <param name="t1">
    /// The end time. Date-only values are interpreted as having time 23:59:59
    /// </param>
    /// <param name="cap">
    /// When present and above 0: only return the first <paramref name="cap"/>
    /// results. This is a hack to avoid Swagger from choking.
    /// </param>
    /// <returns>
    /// The requested list
    /// </returns>
    /// <remarks>
    /// The following time formats are supported for t0 and t1:
    /// 
    /// * _(blank or omitted)_
    /// * yyyy-MM-dd
    /// * yyyyMMdd
    /// * yyyyMMdd-HHmm
    /// * yyyyMMdd-HHmmss
    /// </remarks>
    /// <response code="200">On success</response>
    /// <response code="400">On unrecognized date/time format</response>
    [HttpGet("stationpairstats")]
    public ActionResult<StationPairStats[]> GetStationPairStats( 
      [FromServices] ICitybikeDb db,
      [FromQuery] string? t0 = null,
      [FromQuery] string? t1 = null,
      [FromQuery] int? cap = null)
    {
      DateTime? dt0, dt1;
      try
      {
        dt0 = ParseTime(t0, false);
        dt1 = ParseTime(t1, true);
      }
      catch(ArgumentException aex)
      {
        return BadRequest(aex.Message);
      }
      var icq = db.GetQueryApi();
      var spcs = icq.GetStationPairStats(dt0, dt1);
      if(cap.HasValue && cap.Value > 0)
      {
        spcs = spcs.Take(cap.Value).ToArray();
      }
      return spcs;
    }

    /// <summary>
    /// Get the full list of departure day-station-ridecount statistics
    /// (expect around 40000 records - Swagger will choke on this unless you use the "cap" parameter!)
    /// </summary>
    /// <param name="rss">
    /// The RideStatsService providing the data and assisting in caching it in the server
    /// </param>
    /// <param name="cap">
    /// The maximum number of records to return. Use this parameter in Swagger to prevent
    /// it from choking...
    /// </param>
    /// <returns>
    /// A list of <see cref="StationDateCount"/> records listing departure day, departure
    /// station and total ride count for that combination.
    /// </returns>
    /// <remarks>
    /// The result is intended to be cached by the client. It can be used for instance
    /// to either project departures-by-day or departures-by-station.
    /// 
    /// Currently not in use by the frontend app, but left in as a demo.
    /// </remarks>
    /// <response code="200">On success</response>
    [HttpGet("stationdaydepstats")]
    public List<StationDateCount> GetStationDayDepartureStats( // Unused, but left as demo
      [FromServices] RideStatsService rss,
      [FromQuery] int cap = 0)
    {
      //var ua = Request.Headers["User-Agent"].ToString();
      //_logger.LogInformation($"UA = {ua}");
      var r =
        cap > 0
        ? rss.DepartureStats.Take(cap).ToList()
        : rss.DepartureStats.ToList();
      return r;
    }

    /// <summary>
    /// Get the full list of return day-station-ridecount statistics
    /// (expect around 40000 records - Swagger will choke on this unless you use the "cap" parameter!)
    /// </summary>
    /// <param name="rss">
    /// The RideStatsService providing the data and assisting in caching it in the server
    /// </param>
    /// <param name="cap">
    /// The maximum number of records to return. Use this parameter in Swagger to prevent
    /// it from choking...
    /// </param>
    /// <returns>
    /// A list of <see cref="StationDateCount"/> records listing return day, return
    /// station and total ride count for that combination.
    /// </returns>
    /// <remarks>
    /// The result is intended to be cached by the client. It can be used for instance
    /// to either project returns-by-day or returns-by-station.
    /// 
    /// Currently not in use by the frontend app, but left in as a demo.
    /// </remarks>
    /// <response code="200">On success</response>
    [HttpGet("stationdayretstats")]
    public List<StationDateCount> GetStationDayReturnStats( // Unused, but left as demo
      [FromServices] RideStatsService rss,
      [FromQuery] int cap = 0)
    {
      var r =
        cap > 0
        ? rss.ReturnStats.Take(cap).ToList()
        : rss.ReturnStats.ToList();
      return r;
    }

#if UNUSED

    /// <summary>
    /// Get the number of rides departing from each station,
    /// optionally constraining what days to look at.
    /// </summary>
    /// <param name="rss">
    /// The RideStatsService providing and caching the data
    /// </param>
    /// <param name="firstDay">
    /// Optional. The first day to look at, in "yyyy-MM-dd" or "yyyyMMdd" format.
    /// </param>
    /// <param name="lastDay">
    /// Optional. The last day to look at, in "yyyy-MM-dd" or "yyyyMMdd" format.
    /// </param>
    /// <returns>
    /// A list of <see cref="StationCount"/> records, one for each station that had any
    /// departures in the time range.
    /// </returns>
    /// <response code="200">On success</response>
    /// <response code="400">When the arguments are not in a recognized format</response>
    [HttpGet("departuresbystation")]
    public ActionResult<List<StationCount>> GetDeparturesByStation(
      [FromServices] RideStatsService rss,
      [FromQuery] string? firstDay = null,
      [FromQuery] string? lastDay = null)
    {
      try
      {
        var d0 = ParseDay(firstDay);
        var d1 = ParseDay(lastDay);
        var results = rss.GetDeparturesForStations(d0, d1);
        return results;
      }
      catch(ArgumentException aex)
      {
        return BadRequest(aex.Message);
      }
    }

    /// <summary>
    /// Get the number of rides returning to each station,
    /// optionally constraining what days to look at.
    /// </summary>
    /// <param name="rss">
    /// The RideStatsService providing and caching the data
    /// </param>
    /// <param name="firstDay">
    /// Optional. The first return day to look at, in "yyyy-MM-dd" or "yyyyMMdd" format.
    /// </param>
    /// <param name="lastDay">
    /// Optional. The last return day to look at, in "yyyy-MM-dd" or "yyyyMMdd" format.
    /// </param>
    /// <returns>
    /// A list of <see cref="StationCount"/> records, one for each station that had any
    /// returns in the time range.
    /// </returns>
    /// <response code="200">On success</response>
    /// <response code="400">When the arguments are not in a recognized format</response>
    [HttpGet("returnsbystation")]
    public ActionResult<List<StationCount>> GetReturnsByStation(
      [FromServices] RideStatsService rss,
      [FromQuery] string? firstDay = null,
      [FromQuery] string? lastDay = null)
    {
      try
      {
        var d0 = ParseDay(firstDay);
        var d1 = ParseDay(lastDay);
        var results = rss.GetReturnsForStations(d0, d1);
        return results;
      }
      catch(ArgumentException aex)
      {
        return BadRequest(aex.Message);
      }
    }

    /// <summary>
    /// Get the number of rides departing on each day,
    /// optionally constraining what station to look at.
    /// </summary>
    /// <param name="rss">
    /// The RideStatsService providing and caching the data
    /// </param>
    /// <param name="station">
    /// Optional. The station to look at. When omitted or 0: sum over all stations
    /// </param>
    /// <returns>
    /// A list of <see cref="DayCount"/> records, one for each day that had any
    /// departures for the station (or all stations)
    /// </returns>
    /// <response code="200">On success</response>
    [HttpGet("departuresbyday")]
    public ActionResult<List<DayCount>> GetDeparturesByDay(
      [FromServices] RideStatsService rss,
      [FromQuery] int station = 0)
    {
      try
      {
        var results = rss.GetDeparturesForDays(station);
        return results;
      }
      catch(ArgumentException aex)
      {
        return BadRequest(aex.Message);
      }
    }

    /// <summary>
    /// Get the number of rides returned each day,
    /// optionally constraining what station to look at.
    /// </summary>
    /// <param name="rss">
    /// The RideStatsService providing and caching the data
    /// </param>
    /// <param name="station">
    /// Optional. The station to look at. When omitted or 0: sum over all stations
    /// </param>
    /// <returns>
    /// A list of <see cref="DayCount"/> records, one for each day that had any
    /// returns for the station (or all stations)
    /// </returns>
    /// <response code="200">On success</response>
    [HttpGet("returnsbyday")]
    public ActionResult<List<DayCount>> GetReturnsByDay(
      [FromServices] RideStatsService rss,
      [FromQuery] int station = 0)
    {
      try
      {
        var results = rss.GetReturnsForDays(station);
        return results;
      }
      catch(ArgumentException aex)
      {
        return BadRequest(aex.Message);
      }
    }

    /// <summary>
    /// Return one station record by station ID
    /// </summary>
    /// <param name="id">
    /// The station ID
    /// </param>
    /// <param name="db">
    /// The database accessor (injected by DI)
    /// </param>
    /// <remarks>
    /// Example:
    /// 
    ///     GET api/citybike/station/123
    /// </remarks>
    /// <response code="200">On success</response>
    /// <response code="404">When not found</response>
    [HttpGet("station/{id}")]
    public ActionResult<Station> GetStation(
      int id,
      [FromServices] ICitybikeDb db // injected by DI
      )
    {
      var icq = db.GetQueryApi();
      var station = icq.GetStation(id);
      if(station == null)
      {
        return NotFound();
      }
      else
      {
        return station;
      }
    }

    /// <summary>
    /// Get a full list of all involved cities, loaded from the database (not the cache).
    /// (Spoiler: that's just Helsinki and Espoo)
    /// </summary>
    /// <remarks>
    /// Example:
    /// 
    ///     GET api/citybike/cities-raw
    /// </remarks>
    /// <returns>
    /// A list of <see cref="City"/> instances
    /// </returns>
    /// <response code="200">On success</response>
    [HttpGet("cities-raw")]
    public IReadOnlyList<City> GetCitiesRaw(
      [FromServices] ICitybikeDb db // injected by DI
    )
    {
      var icq = db.GetQueryApi();
      var cities = icq.GetCities();
      return cities;
    }

    /// <summary>
    /// Return the list of all known stations, loaded directly from the database
    /// (not from the cache)
    /// </summary>
    /// <param name="db">
    /// The database accessor service (injected by DI)
    /// </param>
    /// <returns>
    /// The list of stations on success
    /// </returns>
    /// <response code="200">On success</response>
    [HttpGet("stations-raw")]
    public IReadOnlyList<Station> GetStationsRaw(
      [FromServices] ICitybikeDb db)
    {
      var icq = db.GetQueryApi();
      return icq.GetStations();
    }

    /// <summary>
    /// Return the number of rides in the given time interval.
    /// DEPRECATED: use "ridescount2" instead.
    /// </summary>
    /// <param name="db">
    /// The DB service
    /// </param>
    /// <param name="t0">
    /// The start time. Date-only values are interpreted as time 00:00:00.
    /// </param>
    /// <param name="t1">
    /// The end time. Date-only values are interpreted as time 23:59:59
    /// </param>
    /// <returns>
    /// The total number of rides in the given time interval
    /// </returns>
    /// <remarks>
    /// The following time formats are supported:
    /// 
    /// * _(blank or omitted)_
    /// * yyyy-MM-dd
    /// * yyyyMMdd
    /// * yyyyMMdd-HHmm
    /// * yyyyMMdd-HHmmss
    /// 
    /// Some example calls:
    /// 
    ///     GET api/citybike/ridescount
    ///     GET api/citybike/ridescount?t0=2021-07-30&amp;t1=2021-07-31
    ///     GET api/citybike/ridescount?t0=20210730&amp;t1=20210731
    ///     GET api/citybike/ridescount?t0=20210730-120000&amp;t1=20210730-180000
    /// 
    /// </remarks>
    /// <response code="200">On success</response>
    /// <response code="400">On unrecognized date/time format</response>
    [HttpGet("ridescount")]
    public ActionResult<int> GetRidesCount(
      [FromServices] ICitybikeDb db,
      [FromQuery] string? t0 = null,
      [FromQuery] string? t1 = null)
    {
      DateTime? dt0, dt1;
      try
      {
        dt0 = ParseTime(t0, false);
      }
      catch(ArgumentException aex)
      {
        return BadRequest(aex.Message);
      }
      try
      {
        dt1 = ParseTime(t1, true);
      }
      catch(ArgumentException aex)
      {
        return BadRequest(aex.Message);
      }

      var icq = db.GetQueryApi();
      return icq.GetRidesCount(dt0, dt1);
    }

    /// <summary>
    /// Get a page of the rides table in the given time interval.
    /// DEPRECATED: use "ridespage2" instead.
    /// </summary>
    /// <param name="db">
    /// The database accessor
    /// </param>
    /// <param name="offset">
    /// The ride offset where the page starts (default 0)
    /// </param>
    /// <param name="pagesize">
    /// the page size (default 50)
    /// </param>
    /// <param name="t0">
    /// The start time. Date-only values are interpreted as time 00:00:00.
    /// </param>
    /// <param name="t1">
    /// The end time. Date-only values are interpreted as time 23:59:59
    /// </param>
    /// <returns>
    /// A list of up to <paramref name="pagesize"/> rides
    /// </returns>
    /// <remarks>
    /// The following time formats are supported:
    /// 
    /// * _(blank or omitted)_
    /// * yyyy-MM-dd
    /// * yyyyMMdd
    /// * yyyyMMdd-HHmm
    /// * yyyyMMdd-HHmmss
    /// </remarks>
    /// <response code="200">On success</response>
    /// <response code="400">On unrecognized date/time format</response>
    [HttpGet("ridespage")]
    public ActionResult<List<Ride>> GetRidesPage(
      [FromServices] ICitybikeDb db,
      [FromQuery] int offset = 0,
      [FromQuery] int pagesize = 50,
      [FromQuery] string? t0 = null,
      [FromQuery] string? t1 = null)
    {
      DateTime? dt0, dt1;
      try
      {
        dt0 = ParseTime(t0, false);
      }
      catch(ArgumentException aex)
      {
        return BadRequest(aex.Message);
      }
      try
      {
        dt1 = ParseTime(t1, true);
      }
      catch(ArgumentException aex)
      {
        return BadRequest(aex.Message);
      }
      var icq = db.GetQueryApi();
      var rides = icq.GetRidesPage(pagesize, offset, dt0, dt1);
      return rides;
    }
#endif

    private static readonly string[] __dateOnlyPatterns =
      new[] { "yyyy-MM-dd", "yyyyMMdd" };
    private static readonly string[] __dateTimePatterns =
      new[] { "yyyyMMdd-HHmmss", "yyyyMMdd-HHmm" };

    private static DateTime? ParseTime(string? txt, bool isEnd)
    {
      if(String.IsNullOrEmpty(txt))
      {
        return null;
      }
      else if(DateTime.TryParseExact(
        txt,
        __dateOnlyPatterns,
        CultureInfo.InvariantCulture,
        DateTimeStyles.None,
        out DateTime dt1))
      {
        if(isEnd)
        {
          dt1 = dt1.AddDays(1).AddSeconds(-1);
        }
        return dt1;
      }
      else if(DateTime.TryParseExact(
        txt,
        __dateTimePatterns,
        CultureInfo.InvariantCulture,
        DateTimeStyles.None,
        out DateTime dt2))
      {
        return dt2;
      }
      else
      {
        throw new ArgumentException(
          $"date/time is not in a recognized format: {txt}");
      }
    }

    private static DateOnly? ParseDay(string? txt)
    {
      if(String.IsNullOrEmpty(txt))
      {
        return null;
      }
      else if(DateTime.TryParseExact(
        txt,
        __dateOnlyPatterns,
        CultureInfo.InvariantCulture,
        DateTimeStyles.None,
        out var dt1))
      {
        return DateOnly.FromDateTime(dt1);
      }
      else
      {
        throw new ArgumentException(
          $"date/time is not in a recognized format: {txt}");
      }
    }

  }

}
