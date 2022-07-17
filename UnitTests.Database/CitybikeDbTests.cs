﻿/*
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
        var count = db.InitDb(false);
        _output.WriteLine($"Number of DB objects created: {count}");
      }
    }

    [Fact]
    public void CanLoadCities()
    {
      var connstring = _configuration["TestDb:ConnectionString"];
      Assert.NotNull(connstring);

      using(ICitybikeDb db = new CitybikeDbSqlServer(connstring))
      {
        var cities = db.LoadCities();
        Assert.NotNull(cities);
        _output.WriteLine($"Loaded {cities.All.Count} city records:");
        Assert.Equal(2, cities.All.Count);
        foreach(var city in cities.All)
        {
          _output.WriteLine($"{city.Id}: {city.CityFi} ({city.CitySe})");
        }
      }
    }

  }
}
