using System;
using System.Collections.Generic;
using System.Linq;

using LclBikeApp.Database;
using LclBikeApp.Database.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CitybikeApp.WebApi
{
  /// <summary>
  /// Container for exploring all the new stuff in ASP.NET Core 6 Web APIs.
  /// Nothing to see here :)
  /// </summary>
  [Route("api/[controller]")]
  [ApiController]
  [ApiExplorerSettings(IgnoreApi = true)]
  public class ScratchController: ControllerBase
  {

    /// <summary>
    /// Experiment for getting back into Asp.Net Web API design after some years...
    /// </summary>
    [HttpGet("dummy")]
    public Dictionary<string, string> GetDummy()
    {
      var d = new Dictionary<string, string>();
      d["ServerTime"] = DateTimeOffset.Now.ToString();
      d["RandomGuid"] = Guid.NewGuid().ToString();
      return d;
    }

    //// ((Works, but even without values this is a security hole.))
    //[HttpGet("cfgkeys")]
    //public IReadOnlyList<string> GetCfgKeys([FromServices] IConfiguration cfg)
    //{
    //  return cfg.AsEnumerable().Select(kvp => kvp.Key).ToList();
    //}

    /// <summary>
    /// Connection string debug aid
    /// </summary>
    [HttpGet("conndbg")]
    public IReadOnlyList<string> GetConnectionStringNames(
      [FromServices] IConfiguration cfg)
    {
      var section = cfg.GetSection("ConnectionStrings");
      return section.AsEnumerable(true).Where(kvp => kvp.Value!=null).Select(kvp => kvp.Key).ToList();
    }

    /// <summary>
    /// Connection string debug aid
    /// </summary>
    [HttpGet("conndbg2")]
    public IReadOnlyList<string> GetConnectionStringNames2(
      [FromServices] IServiceProvider services)
    {
      var cfg = services.GetService<IConfiguration>();
      if(cfg == null)
      {
        throw new InvalidOperationException(
          $"Configuration error: cannot access config service");
      }
      var section = cfg.GetSection("ConnectionStrings");
      return section.AsEnumerable(true).Where(kvp => kvp.Value!=null).Select(kvp => kvp.Key).ToList();
    }

    /// <summary>
    /// Causes an exception to be thrown, to test how that is handled
    /// </summary>
    [HttpGet("crash")]
    public string GetCrash()
    {
      throw new InvalidOperationException("Just testing how exceptions are handled...");
    }

  }



}
