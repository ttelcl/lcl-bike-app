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
using LclBikeApp.DataWrangling.DataLocation;
using LclBikeApp.DataWrangling.RawModel;
using LclBikeApp.Database.Models;
using XsvLib;

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

    [SkippableFact]
    public void CanAddStations()
    {
      // Note that Xunit's "Skip" functionality is a bit misused in this test.
      // The test being flagged as skipped actually means: it was run, but everything was inserted already.

      var connstring = _configuration["TestDb:ConnectionString"];
      Assert.NotNull(connstring);

      var df = DataFolder.LocateAsAncestorSibling("sampledata");
      Assert.NotNull(df);
      Assert.Contains("/bin/", df.Root.Replace('\\', '/'));
      Assert.True(df.HasFile("stations-subset.csv"));

      var stationCursor = new StationCursor();
      var stations = new List<Station>();
      using(var xsv = Xsv.ReadXsv(df.OpenReadText("stations-subset.csv"), ".csv").AsXsvReader())
      {
        foreach(var cursor in xsv.ReadCursor(stationCursor))
        {
          var station = Station.TryFromCursor(cursor);
          if(station != null)
          {
            stations.Add(station);
          }
        }
      }
      Assert.NotEmpty(stations);
      _output.WriteLine($"Read {stations.Count} stations from the data file");

      var postInsertionKnownIds = new HashSet<int>();
      var inserted = 0;
      
      using(var db = new CitybikeDbSqlServer(connstring))
      {
        var knownStationIds = new HashSet<int>(db.GetStationIds());
        _output.WriteLine($"There were already {knownStationIds.Count} stations in the DB");
        var newStations = stations.Where(s => !knownStationIds.Contains(s.Id)).ToList();
        _output.WriteLine($"Number of new stations to insert: {newStations.Count}");

        inserted = db.AddStations(stations);
        _output.WriteLine($"Inserted {inserted} stations");

        postInsertionKnownIds.UnionWith(db.GetStationIds());
      }

      Assert.True(stations.All(s => postInsertionKnownIds.Contains(s.Id)));

      Skip.If(inserted == 0);
    }

  }
}
