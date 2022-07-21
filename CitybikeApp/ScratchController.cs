using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
  }
}
