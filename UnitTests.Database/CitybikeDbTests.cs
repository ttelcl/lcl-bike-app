/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Dapper;

using Xunit;
using Xunit.Abstractions;

using LclBikeApp.Database;
using LclBikeApp.Database.ImplementationSqlServer;

namespace UnitTests.Database
{
  public class CitybikeDbTests
  {
    private readonly ITestOutputHelper _output;
    private readonly IConfiguration _configuration;

    public CitybikeDbTests(ITestOutputHelper output)
    {
      _output=output;
      _configuration = new ConfigurationBuilder()
        .AddUserSecrets<SecretsInUnitTestsTests2>()
        .Build();
    }

    [Fact]
    public void CanInitDb()
    {
      var connstring = _configuration["TestDb:ConnectionString"];
      Assert.NotNull(connstring);

      using(var db = new CitybikeDbSqlServer(connstring))
      {
        db.InitDb(false);
      }
    }

  }
}
