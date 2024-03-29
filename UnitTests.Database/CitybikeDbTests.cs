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
using LclBikeApp.DataWrangling.DataLocation;
using LclBikeApp.DataWrangling.RawModel;
using LclBikeApp.Database.Models;
using XsvLib;
using LclBikeApp.DataWrangling.Validation;
using System.Data;

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
    public void CanResetDb()
    {
      _output.WriteLine("Deleting and rebuilding database!!!!!!!!!!!!!!!!");

      var connstring = _configuration["TestDb:ConnectionString"];
      Assert.NotNull(connstring);

      using(var db = new CitybikeDbSqlServer(connstring))
      {
        var count = db.InitDb(true);
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

    [Fact]
    public void CanAddStations()
    {
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
    }

    [Fact]
    public void CanLoadValidationParameters()
    {
      var df = DataFolder.LocateAsAncestorSibling("sampledata");

      // Note: read a configuration file with all values different from the default
      var cfgname = "validation-parameters-FOR-TESTING-ONLY.json";
      Assert.NotNull(df);
      Assert.Contains("/bin/", df.Root.Replace('\\', '/'));
      Assert.True(df.HasFile(cfgname));

      var json = df.ReadAllText(cfgname);
      var cfg = ValidationConfiguration.FromJson(json);

      Assert.NotNull(cfg);
      Assert.Equal(420, cfg.MinDistance);
      Assert.Equal(8888, cfg.MaxDistance);
      Assert.Equal(121, cfg.MinDuration);
      Assert.Equal(14401, cfg.MaxDuration);
      Assert.Equal(21, cfg.TimeTolerance);
      Assert.False(cfg.RequireNonAscendingDepartures);
    }

    [Fact]
    public void CanInsertRideSamples()
    {
      const string stationsFile = "stations-subset.csv";
      const string ridesFile = "rides-subset-validated.csv";

      var connstring = _configuration["TestDb:ConnectionString"];
      Assert.NotNull(connstring);

      var df = DataFolder.LocateAsAncestorSibling("sampledata");
      Assert.NotNull(df);
      Assert.Contains("/bin/", df.Root.Replace('\\', '/'));
      Assert.True(df.HasFile(stationsFile));
      Assert.True(df.HasFile(ridesFile));

      var cfg = new ValidationConfiguration(); // use defaults

      using(var db = new CitybikeDbSqlServer(connstring))
      {
        // Just to make sure: ensure the DB exists and tables exist
        var n = db.InitDb();
        _output.WriteLine($"DB Init added {n} objects");

        // Just in case: insert missing stations first
        var stationCursor = new StationCursor();
        var stations = new List<Station>();
        using(var xsv = Xsv.ReadXsv(df.OpenReadText(stationsFile), stationsFile).AsXsvReader())
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
        n = db.AddStations(stations);
        _output.WriteLine($"Inserted {n} of {stations.Count} station records");

        // Now to load, validate, and insert rides
        var stationIds = stations.Select(x => x.Id).ToList();
        var validator = new RideValidator(cfg, stationIds);
        var rideCursor = new RideCursor();
        int inserted;
        using(var xsv = Xsv.ReadXsv(df.OpenReadText(ridesFile), ridesFile).AsXsvReader())
        {
          // This approach assumes the number of rides in the data file is relatively small
          // and can be processed without batching
          var rides =
            validator
            .Validate(xsv.ReadCursor(rideCursor))
            .Select(cur => RideBase.FromCursor(cur));
          inserted = db.AddBaseRides(rides);
          _output.WriteLine($"Inserted {inserted} rides into the DB");
        }

        _output.WriteLine(
          $"{validator.CandidateCount} rides loaded, {validator.AcceptedCount} accepted, " +
          $"{validator.CandidateCount - validator.AcceptedCount} rejected, " +
          $"{inserted} inserted, {validator.AcceptedCount - inserted} skipped.");

      }

    }

    [Fact]
    public void CanGetCities()
    {
      var connstring = _configuration["TestDb:ConnectionString"];
      Assert.NotNull(connstring);

      using(var db = new CitybikeDbSqlServer(connstring))
      {
        db.InitDb();
        var icq = db as ICitybikeQueries;
        Assert.NotNull(icq);
        var cities = icq.GetCities();
        Assert.NotNull(cities);
        Assert.NotEmpty(cities);
        Assert.Equal(2, cities.Count);
        Assert.Equal(0, cities[0].Id);
        Assert.Equal(1, cities[1].Id);
      }
    }

    [Fact]
    public void CanGetStations()
    {
      var connstring = _configuration["TestDb:ConnectionString"];
      Assert.NotNull(connstring);

      using(var db = new CitybikeDbSqlServer(connstring))
      {
        db.InitDb();
        var icq = db as ICitybikeQueries;
        Assert.NotNull(icq);
        var stations = icq.GetStations();
        Assert.NotNull(stations);
        Assert.NotEmpty(stations);

        _output.WriteLine($"Read {stations.Count} stations");
      }
    }

    [Fact]
    public void CanGetRidesCount()
    {
      var connstring = _configuration["TestDb:ConnectionString"];
      Assert.NotNull(connstring);

      using(var db = new CitybikeDbSqlServer(connstring))
      {
        db.InitDb();
        var icq = db as ICitybikeQueries;
        Assert.NotNull(icq);
        var ridesCount = icq.GetRidesCount2(null, null);
        Assert.True(ridesCount > 0);

        _output.WriteLine($"There are {ridesCount} rides in this test db");

        var t0 = new DateTime(2021, 5, 31, 23, 50, 00);
        var t1 = new DateTime(2021, 5, 31, 23, 55, 00);

        var rides2 = icq.GetRidesCount2(t0, t1);
        Assert.True(rides2 > 0);
        _output.WriteLine($"Of those {ridesCount}, there are {rides2} rides from {t0:s} to {t1:s}");

        var depid = 4;
        var rides3 = icq.GetRidesCount2(depId: depid);
        Assert.True(rides3 > 0);
        _output.WriteLine($"Of those {ridesCount}, there are {rides3} rides starting from station {depid}");

        var retid = 62;
        var rides4 = icq.GetRidesCount2(retId: retid);
        Assert.True(rides4 > 0);
        _output.WriteLine($"Of those {ridesCount}, there are {rides4} rides ending at station {retid}");

      }
    }

    [Fact]
    public void CanGetTimeRange()
    {
      var connstring = _configuration["TestDb:ConnectionString"];
      Assert.NotNull(connstring);

      using(var db = new CitybikeDbSqlServer(connstring))
      {
        db.InitDb();
        var icq = db as ICitybikeQueries;
        Assert.NotNull(icq);
        var range = icq.GetTimeRange();
        if(range == null)
        {
          _output.WriteLine($"The rides table is Empty!!");
        }
        else
        {
          _output.WriteLine($"Ride time range is {range.StartTime:s} to {range.EndTime:s}");
        }

      }
    }

    [Fact]
    public void CanGetStats()
    {
      var connstring = _configuration["TestDb:ConnectionString"];
      Assert.NotNull(connstring);

      using(var db = new CitybikeDbSqlServer(connstring))
      {
        db.InitDb();
        var icq = db as ICitybikeQueries;
        Assert.NotNull(icq);

        var depstats = icq.GetDepartureStats();
        Assert.NotNull(depstats);
        _output.WriteLine($"There are {depstats.Length} departure statistics rows (in the unit test DB)");
        Assert.NotEmpty(depstats);
        foreach(var dep in depstats.Take(5))
        {
          _output.WriteLine($" DEP ({dep.StationId}, {dep.Day:yyyy-MM-dd}) => {dep.Count}");
        }

        var retstats = icq.GetReturnStats();
        Assert.NotNull(retstats);
        _output.WriteLine($"There are {retstats.Length} return statistics rows (in the unit test DB)");
        Assert.NotEmpty(retstats);
        foreach(var ret in retstats.Take(5))
        {
          _output.WriteLine($" RET ({ret.StationId}, {ret.Day:yyyy-MM-dd}) => {ret.Count}");
        }
      }
    }
  }
}
