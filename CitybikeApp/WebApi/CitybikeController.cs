using System;
using System.Collections.Generic;
using System.Linq;

using LclBikeApp.Database.Models;
using LclBikeApp.Database;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;


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
  }
}
