/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper;

using LclBikeApp.Database.Models;

namespace LclBikeApp.Database.ImplementationSqlServer
{
  /// <summary>
  /// Database API entrypoint implementation for SQL Server
  /// </summary>
  public class CitybikeDbSqlServer: IDisposable, ICitybikeDb, ICitybikeQueries
  {
    private static readonly DateTime __minDate = new DateTime(2000, 1, 1);
    private static readonly DateTime __maxDate = new DateTime(2100, 1, 1);
    private readonly string _connString;

    /// <summary>
    /// Create a new CitybikeDbSqlServer
    /// </summary>
    public CitybikeDbSqlServer(
      string connString)
    {
      _connString = connString;
      Connection = new SqlConnection(connString);
    }

    /// <summary>
    /// Get the object that implements ICitybikeQueries for this
    /// database accessor. In this case that is just this object itself.
    /// </summary>
    public ICitybikeQueries GetQueryApi()
    {
      return this;
    }

    /// <summary>
    /// True after this object and the connection it wraps have been disposed
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// The database connection
    /// </summary>
    public IDbConnection Connection { get; private set; }

    /// <summary>
    /// Clean up
    /// </summary>
    public void Dispose()
    {
      if(!Disposed)
      {
        Disposed = true;
        Connection.Close();
        Connection.Dispose();
      }
    }

    /// <summary>
    /// Initialize the database, creating missing tables if necessary
    /// and initializing the Cities table to default content
    /// </summary>
    /// <param name="erase">
    /// When true ALL DATABASE CONTENT IS REMOVED first
    /// (the "factory reset" option).
    /// </param>
    /// <returns>
    /// The number of tables and indexes created
    /// </returns>
    public int InitDb(bool erase = false)
    {
      EnsureNotDisposed();

      if(erase)
      {
        Connection.Execute(@"DROP TABLE IF EXISTS [dbo].[Rides]");
        Connection.Execute(@"DROP TABLE IF EXISTS [dbo].[Stations]");
        Connection.Execute(@"DROP TABLE IF EXISTS [dbo].[Cities]");
      }

      var createCount = 0;

      var existingTables = Connection.Query<string>(
        @"
SELECT TABLE_NAME
FROM [INFORMATION_SCHEMA].[TABLES]
WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_SCHEMA = 'dbo'");

      if(!existingTables.Contains("Cities"))
      {
        Trace.TraceInformation("Creating 'Cities' table");
        CreateCitiesTable();
        createCount += 1;
        Trace.TraceInformation("Filling 'Cities' table");
        FillCitiesTable();
      }

      if(!existingTables.Contains("Stations"))
      {
        Trace.TraceInformation("Creating 'Stations' table");
        CreateStationsTable();
        createCount += 1;
      }

      if(!existingTables.Contains("Rides"))
      {
        Trace.TraceInformation("Creating 'Rides' table");
        createCount += CreateRidesTable();
      }

      return createCount;
    }

    /// <summary>
    /// Load the full cities table from the database
    /// </summary>
    public AllCities LoadCities()
    {
      EnsureNotDisposed();
      var cities = Connection.Query<City>(@"
SELECT Id, CityFi, CitySe
FROM [dbo].[Cities]
");
      var allcities = new AllCities(cities);
      return allcities;
    }

    /// <summary>
    /// Enumerate all station records in the DB.
    /// See also GetStationIds().
    /// </summary>
    public IReadOnlyList<Station> GetStations()
    {
      EnsureNotDisposed();
      throw new NotImplementedException();
    }

    /// <summary>
    /// Enumerate a brief summary for each station in the DB
    /// </summary>
    public IReadOnlyList<StationBasics> GetStationBasics()
    {
      EnsureNotDisposed();
      var stationIds = Connection.Query<StationBasics>(@"
SELECT Id, NameFi AS [Label]
FROM [dbo].[Stations]
");
      return stationIds.ToList().AsReadOnly();
    }

    /// <summary>
    /// Enumerate all known station IDs. To load the full
    /// station data use GetStations() instead.
    /// </summary>
    public IReadOnlyList<int> GetStationIds()
    {
      EnsureNotDisposed();
      var stationIds = Connection.Query<int>(@"
SELECT Id
FROM [dbo].[Stations]
");
      return stationIds.ToList().AsReadOnly();
    }

    /// <summary>
    /// Insert the given stations into the DB, unless they already
    /// are present. This method does not update existing stations.
    /// </summary>
    public int AddStations(IEnumerable<Station> stations)
    {
      EnsureNotDisposed();
      var knownStationIds = new HashSet<int>(GetStationIds());
      var newStations = stations.Where(s => !knownStationIds.Contains(s.Id)).ToList();
      var sql = @"
INSERT INTO Stations (Id, NameFi, NameSe, NameEn, AddrFi, AddrSe, City, Capacity, Latitude, Longitude)
VALUES (@Id, @NameFi, @NameSe, @NameEn, @AddrFi, @AddrSe, @CityId, @Capacity, @Latitude, @Longitude)
";
      var count = Connection.Execute(sql, newStations);
      return count;
    }

    /// <summary>
    /// Insert the provided batch of RideBase instances.
    /// No validation is done - that is supposed to have happened
    /// already. The rides are inserted as new instances, with 
    /// the DB generating new IDs for them
    /// </summary>
    /// <param name="rides">
    /// The rides to insert
    /// </param>
    /// <returns>
    /// The number of rides inserted, which may be less than
    /// the number presented rides when duplicates are rejected.
    /// </returns>
    public int AddBaseRides(IEnumerable<RideBase> rides)
    {
      EnsureNotDisposed();
      int count;
      Connection.Open();
      using(var trx = Connection.BeginTransaction())
      {
        // Note that "Id" is *not* set - let the DB generate it
        var sql = @"
INSERT INTO Rides (DepTime, RetTime, DepStation, RetStation, Distance, Duration)
VALUES (@DepTime, @RetTime, @DepStationId, @RetStationId, @Distance, @Duration)";
        count = Connection.Execute(sql, rides, transaction: trx);
        trx.Commit();
      }
      return count;
    }

    TimeRange? ICitybikeQueries.GetTimeRange()
    {
      EnsureNotDisposed();
      Connection.Open();
      var results = Connection.Query<TimeRange>(@"
SELECT MIN(DepTime) AS startTime, MAX(DepTime) AS endTime
FROM Rides").ToList();
      if(results.Any() && results[0].StartTime > DateTime.MinValue)
      {
        return results[0];
      }
      else
      {
        return null;
      }
    }
    IReadOnlyList<City> ICitybikeQueries.GetCities()
    {
      EnsureNotDisposed();
      var cities = Connection.Query<City>(@"
SELECT Id, CityFi, CitySe
FROM [dbo].[Cities]
");
      return cities.ToList();
    }

    IReadOnlyList<Station> ICitybikeQueries.GetStations()
    {
      EnsureNotDisposed();
      var stations = Connection.Query<Station>(@"
SELECT Id, NameFi, NameSe, NameEn, AddrFi, AddrSe, City AS CityId, Capacity, Latitude, Longitude
FROM [dbo].[Stations]
");
      return stations.ToList();
    }

#if UNUSED
    List<Ride> ICitybikeQueries.GetRidesPage(
      int pageSize, int pageOffset, DateTime? fromTime, DateTime? toTime)
    {
      EnsureNotDisposed();
      if(pageSize < 1)
      {
        pageSize = 50;
      }
      if(pageOffset < 0)
      {
        pageOffset = 0;
      }
      // Avoid DateTime.MinValue and DateTime.MaxValue, since they may not be
      // database compatible
      var tFrom = fromTime ?? __minDate;
      var tTo = toTime ?? __maxDate;
      // Optimize the case where both times were null (or out of range)
      if(tFrom<=__minDate && tTo>=__maxDate)
      {
        var rides = Connection.Query<Ride>(@"
SELECT Id, DepTime, RetTime, DepStation AS DepStationId, RetStation AS RetStationId, Distance, Duration
FROM [dbo].[Rides]
ORDER BY DepTime, RetTime, DepStation, RetStation, Distance, Duration
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY
", new { Offset = pageOffset, PageSize = pageSize });
        return rides.ToList();
      }
      else
      {
        var rides = Connection.Query<Ride>(@"
SELECT Id, DepTime, RetTime, DepStation AS DepStationId, RetStation AS RetStationId, Distance, Duration
FROM [dbo].[Rides]
WHERE DepTime >= @TFrom AND DepTime <= @TTo
ORDER BY DepTime, RetTime, DepStation, RetStation, Distance, Duration
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY
", new { Offset = pageOffset, PageSize = pageSize, TFrom = tFrom, TTo = tTo });
        return rides.ToList();
      }
    }

    int ICitybikeQueries.GetRidesCount(
      DateTime? fromTime, DateTime? toTime)
    {
      EnsureNotDisposed();
      // Avoid DateTime.MinValue and DateTime.MaxValue, since they may not be
      // database compatible
      var tFrom = fromTime ?? __minDate;
      var tTo = toTime ?? __maxDate;
      // Optimize the case where both times were null (or out of range)
      if(tFrom<=__minDate && tTo>=__maxDate)
      {
        return Connection.QuerySingle<int>(@"
SELECT COUNT(*)
FROM [dbo].[Rides]
");
      }
      else
      {
        return Connection.QuerySingle<int>(@"
SELECT COUNT(*)
FROM [dbo].[Rides]
WHERE DepTime >= @TFrom AND DepTime <= @TTo
", new { TFrom = tFrom, TTo = tTo });
      }
    }


    /// <summary>
    /// Get a single station record
    /// </summary>
    /// <param name="id">
    /// The station ID to find
    /// </param>
    /// <returns>
    /// The station if found, or null if not found
    /// </returns>
    Station? ICitybikeQueries.GetStation(int id)
    {
      EnsureNotDisposed();
      var station = Connection.QuerySingleOrDefault<Station>(@"
SELECT Id, NameFi, NameSe, NameEn, AddrFi, AddrSe, City AS CityId, Capacity, Latitude, Longitude
FROM [dbo].[Stations]
WHERE Id = @StationId
", new { StationId = id });
      return station;
    }

#endif

    List<Ride> ICitybikeQueries.GetRidesPage2(
      int pageSize,
      int pageOffset,
      DateTime? fromTime, 
      DateTime? toTime,
      int depId,
      int retId,
      int distMin,
      int distMax,
      int durMin,
      int durMax,
      string sort)
    {
      EnsureNotDisposed();
      var conditions = QueryConditions(fromTime, toTime, depId, retId, distMin, distMax, durMin, durMax);
      var q = @"
SELECT Id, DepTime, RetTime, DepStation AS DepStationId, RetStation AS RetStationId, Distance, Duration
FROM [dbo].[Rides]
";
      if(conditions.Count > 0)
      {
        q = q + "WHERE " + String.Join(@"
  AND ", conditions) + @"
";
      }
      switch(sort)
        // Explicitly handle each valid option.
        // Otherwise you may be creating an SQL Injection vulnerability!
      {
        case null:
        case "":
        case "default":
          // REQUIRED TO MAKE OFFSET / FETCH WORK!
          q += @"ORDER BY DepTime, RetTime, DepStation, RetStation, Distance, Duration
";
          break;
        default:
          // Here be dragons...
          throw new NotImplementedException(
            $"Sort orders other than default are NotYetImplemented");
      }
      q += @"
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY";
      var rides = Connection.Query<Ride>(q, new { Offset = pageOffset, PageSize = pageSize });
      return rides.ToList();
    }

    int ICitybikeQueries.GetRidesCount2(
      DateTime? fromTime,
      DateTime? toTime,
      int depId,
      int retId,
      int distMin,
      int distMax,
      int durMin,
      int durMax)
    {
      EnsureNotDisposed();
      var conditions = QueryConditions(fromTime, toTime, depId, retId, distMin, distMax, durMin, durMax);
      var q = @"
SELECT COUNT(*)
FROM [dbo].[Rides]
";
      if(conditions.Count > 0)
      {
        q = q + "WHERE " + String.Join(" AND ", conditions);
      }
      return Connection.QuerySingle<int>(q);
    }

    StationDateCount[] ICitybikeQueries.GetDepartureStats()
    {
      EnsureNotDisposed();
      var query = @"
SELECT DepStation AS StationId, CONVERT(DATE, DepTime) AS [Day], COUNT(*) AS [Count]
FROM [Rides]
GROUP BY DepStation, CONVERT(DATE, DepTime)";
      var results = Connection.Query<StationDateCount>(query);
      return results.ToArray();
    }

    StationDateCount[] ICitybikeQueries.GetReturnStats()
    {
      EnsureNotDisposed();
      var query = @"
SELECT RetStation AS StationId, CONVERT(DATE, RetTime) AS [Day], COUNT(*) AS [Count]
FROM [Rides]
GROUP BY RetStation, CONVERT(DATE, RetTime)";
      var results = Connection.Query<StationDateCount>(query);
      return results.ToArray();
    }

    StationPairStats[] ICitybikeQueries.GetStationPairStats(
      DateTime? fromTime, DateTime? toTime)
    {
      EnsureNotDisposed();
      var tFrom = fromTime ?? __minDate;
      var tTo = toTime ?? __maxDate;
      var conditions = new List<string>();
      if(fromTime.HasValue)
      {
        conditions.Add("DepTime >= @TFrom");
      }
      if(toTime.HasValue)
      {
        conditions.Add("RetTime <= @TTo");
      }
      var query = @"
SELECT  DepStation AS DepId, RetStation AS RetId, COUNT(*) AS [count],
        SUM(Distance) AS DistSum, SUM(Duration) AS DurSum
FROM Rides";
      if(conditions.Count > 0)
      {
        query += @"
WHERE " + String.Join(" AND ", conditions);
      }
      query += @"
GROUP BY DepStation, RetStation";
      var results = Connection.Query<StationPairStats>(query, new {TFrom = tFrom, TTo = tTo });
      return results.ToArray();
    }

    private List<string> QueryConditions(
      DateTime? fromTime,
      DateTime? toTime,
      int depId,
      int retId,
      int distMin,
      int distMax,
      int durMin,
      int durMax)
    {
      var l = new List<string>();
      var tFrom = fromTime ?? __minDate;
      var tTo = toTime ?? __maxDate;
      if(tFrom > __minDate)
      {
        l.Add($"DepTime >= '{fromTime:s}'");
      }
      if(tTo < __maxDate)
      {
        l.Add($"DepTime <= '{toTime:s}'");
      }
      if(depId > 0)
      {
        l.Add($"DepStation = {depId}");
      }
      if(retId > 0)
      {
        l.Add($"RetStation = {retId}");
      }
      if(distMin > 0)
      {
        l.Add($"Distance >= {distMin}");
      }
      if(distMax > 0 && distMax < 100000)
      {
        l.Add($"Distance <= {distMax}");
      }
      if(durMin > 0)
      {
        l.Add($"Duration >= {durMin}");
      }
      if(durMax > 0 && durMax < 3600*24*31)
      {
        l.Add($"Duration <= {durMax}");
      }
      return l;
    }

    private void EnsureNotDisposed()
    {
      if(Disposed)
      {
        throw new ObjectDisposedException(
          "Database Accessor");
      }
    }

    private void CreateCitiesTable()
    {
      EnsureNotDisposed();
      var sql = @"
CREATE TABLE [dbo].[Cities]
(
  [Id]     INT          NOT NULL PRIMARY KEY, 
  [CityFi] NVARCHAR(20) NOT NULL, 
  [CitySe] NVARCHAR(20) NOT NULL
)";
      Connection.Execute(sql);
    }

    private void FillCitiesTable()
    {
      EnsureNotDisposed();
      Connection.Execute(@"
INSERT INTO Cities (Id, CityFi, CitySe) VALUES (@Id, @CityFi, @CitySe)",
        AllCities.Default.All);
    }

    private void CreateStationsTable()
    {
      EnsureNotDisposed();
      var sql = @"
CREATE TABLE [dbo].[Stations]
(
    [Id]        INT          NOT NULL PRIMARY KEY, 
    [NameFi]    NVARCHAR(48) NOT NULL, 
    [NameSe]    NVARCHAR(48) NOT NULL, 
    [NameEn]    NVARCHAR(48) NOT NULL, 
    [AddrFi]    NVARCHAR(48) NOT NULL, 
    [AddrSe]    NVARCHAR(48) NOT NULL, 
    [City]      INT          NOT NULL FOREIGN KEY REFERENCES Cities(Id), 
    [Capacity]  INT          NOT NULL, 
    [Latitude]  FLOAT        NOT NULL, 
    [Longitude] FLOAT        NOT NULL
)";
      Connection.Execute(sql);
    }

    private int CreateRidesTable()
    {
      EnsureNotDisposed();
      var count = 0;
      var sql = @"
CREATE TABLE [dbo].[Rides]
(
    [Id]         uniqueidentifier DEFAULT NEWSEQUENTIALID() PRIMARY KEY, 
    [DepTime]    DATETIME  NOT NULL, 
    [RetTime]    DATETIME  NOT NULL, 
    [DepStation] INT       NOT NULL FOREIGN KEY REFERENCES Stations(Id),
    [RetStation] INT       NOT NULL FOREIGN KEY REFERENCES Stations(Id), 
    [Distance]   INT       NOT NULL, 
    [Duration]   INT       NOT NULL,

    CONSTRAINT UC_All UNIQUE (DepTime, RetTime, DepStation, RetStation, Distance, Duration)
        WITH (IGNORE_DUP_KEY = ON)
)";
      Connection.Execute(sql);
      count++;

      sql = @"
CREATE INDEX ByDepRetStation
ON [dbo].[Rides] (DepStation, RetStation, DepTime)";
      Connection.Execute(sql);
      count++;

      sql = @"
CREATE INDEX ByRetDepStation
ON [dbo].[Rides] (RetStation, DepStation, RetTime)";
      Connection.Execute(sql);
      count++;

      sql = @"
CREATE INDEX ByDepStationTime
ON [dbo].[Rides] (DepStation, DepTime)";
      Connection.Execute(sql);
      count++;

      sql = @"
CREATE INDEX ByRetStationTime
ON [dbo].[Rides] (RetStation, RetTime)";
      Connection.Execute(sql);
      count++;

      return count;
    }
  }

}
