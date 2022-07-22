using System;
using System.Collections.Generic;
using System.Linq;

using LclBikeApp.Database.Models;
using LclBikeApp.Database;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Globalization;
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

    /*
     * NOTE on the XML-comment formatting: the format is slightly unusual, because
     * the comments are pickked up by Swagger, which uses Markdown-like formatting
     * instead of the usual .net XML-doc style
     */

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
    /// Return the number of rides in the given time interval.
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
    /// Get a page of the rides table
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
    public ActionResult<List<RideBase>> GetRidesPage(
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
  
  }

}
