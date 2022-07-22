using System;
using System.Collections.Generic;
using System.Linq;

using LclBikeApp.Database.Models;
using LclBikeApp.Database;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace CitybikeApp.WebApi
{
  /// <summary>
  /// Implements the citybike Web API
  /// </summary>
  [Route("api/[controller]")]
  [ApiController]
  public class CitybikeController: ControllerBase
  {

    /// <summary>
    /// GET api/citybike/cities
    /// </summary>
    [Route("cities")]
    public IReadOnlyList<City> GetCities(
      [FromServices] ICitybikeDb db // injected by DI
    )
    {
      var icq = db.GetQueryApi();
      var cities = icq.GetCities();
      return cities;
    }

    /// <summary>
    /// GET api/citybike/station/123
    /// </summary>
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
    /// Return the number of rides in the given time interval.
    /// example: GET api/citybike/ridescount?t0=2021-07-30&amp;t1=2021-07-31
    /// </summary>
    /// <param name="db">
    /// The DB service
    /// </param>
    /// <param name="t0">
    /// The start time. Date-only values are interpreted as time 00:00:00.
    /// Formats: blank/omitted: use minimum;
    /// yyyy-MM-dd; yyyyMMdd; yyyyMMdd-HHmmss
    /// </param>
    /// <param name="t1">
    /// The end time. Date-only values are interpreted as time 23:59:59
    /// Formats: blank/omitted: use minimum;
    /// yyyy-MM-dd; yyyyMMdd; yyyyMMdd-HHmmss
    /// </param>
    /// <returns>
    /// The total number of rides in the given time interval
    /// </returns>
    /// <remarks>
    /// <para>
    /// Some examples:
    /// </para>
    /// <list type="bullet">
    /// <item>GET api/citybike/ridescount</item>
    /// <item>GET api/citybike/ridescount?t0=2021-07-30&amp;t1=2021-07-31</item>
    /// <item>GET api/citybike/ridescount?t0=20210730&amp;t1=20210731</item>
    /// <item>GET api/citybike/ridescount?t0=20210730-120000&amp;t1=20210730-180000</item>
    /// </list>
    /// </remarks>
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

    private DateTime? ParseTime(string? txt, bool isEnd)
    {
      if(String.IsNullOrEmpty(txt))
      {
        return null;
      }
      else if(DateTime.TryParseExact(
        txt,
        new[] { "yyyy-MM-dd", "yyyyMMdd" },
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
        new[] { "yyyyMMdd-HHmmss", "yyyyMMdd-HHmm" },
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
