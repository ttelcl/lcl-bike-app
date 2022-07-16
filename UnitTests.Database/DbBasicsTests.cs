/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Dapper;

using Xunit;
using Xunit.Abstractions;

namespace UnitTests.Database
{
  /// <summary>
  /// Basic Database functionality tests 
  /// </summary>
  public class DbBasicsTests
  {
    private readonly ITestOutputHelper _output;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Create a new DbBasicsTests
    /// </summary>
    public DbBasicsTests(ITestOutputHelper output)
    {
      _output=output;
      _configuration = new ConfigurationBuilder()
        .AddUserSecrets<SecretsInUnitTestsTests2>()
        .Build();
    }

    [Fact]
    public void CanAccessDatabase()
    {
      var connstring = _configuration["TestDb:ConnectionString"];
      Assert.NotNull(connstring);

      var tableNames = new List<string>();
      using(var conn = new SqlConnection(connstring))
      {
        conn.Open();

        // Do a simple query at low level (ADO.NET).
        using(var cmd = new SqlCommand(
          @"
SELECT TABLE_SCHEMA, TABLE_NAME
FROM [INFORMATION_SCHEMA].[TABLES]
WHERE TABLE_TYPE = 'BASE TABLE'",
          conn))
        {
          using(var reader = cmd.ExecuteReader())
          {
            while(reader.Read())
            {
              var schema = reader.GetString(0);
              var tbl = reader.GetString(1);
              tableNames.Add($"{schema}.{tbl}");
            }
          }
        }
      }
      _output.WriteLine($"Discovered {tableNames.Count} plain tables:");
      foreach(var tableName in tableNames)
      {
        _output.WriteLine($"- {tableName}");
      }

      // Note that we cannot assert anything in this test beyond running without
      // throwing an exception, since this test does not make any assumptions on
      // the content of the database. For instance, it may be empty.
    }

    [Fact]
    public void CanUseDapper()
    {
      // Example use of Dapper (micro ORM library)

      var connstring = _configuration["TestDb:ConnectionString"];
      Assert.NotNull(connstring);

      var results = new List<TableInfoPoco>();

      using(var conn = new SqlConnection(connstring))
      {
        conn.Open();
        // Call Dapper's "Query<T>" extension method.
        // In the query don't forget to escape the aliases (for example 'schema' is an
        // SQL keyword that would be illegal without the surrounding [])
        var queryResults = conn.Query<TableInfoPoco>(
          @"
SELECT TABLE_CATALOG AS [catalog], TABLE_SCHEMA AS [schema], TABLE_NAME as [name]
FROM [INFORMATION_SCHEMA].[TABLES]
WHERE TABLE_TYPE = 'BASE TABLE'"
          );
        results.AddRange(queryResults);
      }

      _output.WriteLine($"Discovered {results.Count} plain tables:");
      foreach(var result in results)
      {
        _output.WriteLine($"- {result.Catalog} : {result.Schema} : {result.Name}");
      }

      // Note that we cannot assert anything in this test beyond running without
      // throwing an exception, since this test does not make any assumptions on
      // the content of the database. For instance, it may be empty.

    }

  }
}
