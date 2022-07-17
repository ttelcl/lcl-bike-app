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
  public class CitybikeDbSqlServer: IDisposable, ICitybikeDb
  {
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
        throw new NotImplementedException(
          "'erase' functionality is NYI");
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
    public IEnumerable<Station> GetStations()
    {
      EnsureNotDisposed();
      throw new NotImplementedException();
    }

    /// <summary>
    /// Enumerate a brief summary for each station in the DB
    /// </summary>
    public IEnumerable<StationBasics> GetStationBasics()
    {
      EnsureNotDisposed();
      throw new NotImplementedException();
    }

    /// <summary>
    /// Enumerate all known station IDs. To load the full
    /// station data use GetStations() instead.
    /// </summary>
    public IEnumerable<int> GetStationIds()
    {
      EnsureNotDisposed();
      throw new NotImplementedException();
    }

    /// <summary>
    /// Insert the given stations into the DB, unless they already
    /// are present. This method does not update existing stations.
    /// </summary>
    public void AddStations(IEnumerable<Station> stations)
    {
      EnsureNotDisposed();
      throw new NotImplementedException();
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
      Connection.Execute(
        "INSERT INTO Cities (Id, CityFi, CitySe) VALUES (@Id, @CityFi, @CitySe)",
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
      return count;
    }

  }
}
