using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CitybikeApp
{
  [Route("api/[controller]")]
  [ApiController]
  public class ScratchController: ControllerBase
  {

    [Route("dummy")]
    public Dictionary<string, string> GetDummy()
    {
      var d = new Dictionary<string, string>();
      d["ServerTime"] = DateTimeOffset.Now.ToString();
      d["RandomGuid"] = Guid.NewGuid().ToString();
      return d;
    }

    // ((Works, but even without values this is a security hole.))
    //[Route("cfgkeys")]
    //public IReadOnlyList<string> GetCfgKeys([FromServices] IConfiguration cfg)
    //{
    //  return cfg.AsEnumerable().Select(kvp => kvp.Key).ToList();
    //}

  }



}
