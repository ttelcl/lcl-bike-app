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
      /*
       * To make this test succeed:
       * - Make sure you have set up the "TestDb:ConnectionString" with the connection string
       *   to the SQL Server database you intend to use.
       * - Assuming you want to use the development server, make sure it is installed (by
       *   including the "Data storage and processing" workload in your VS2022 setup.) In that
       *   case your DB connection string may look like:
       *   "Server=(LocalDB)\MSSQLLocalDB;Integrated Security=true;AttachDbFileName=C:\_database\biketests.mdf"
       *     - Remember doubling the backslashes in JSON!!
       * - If you get an error like the following:
       *     System.Data.SqlClient.SqlException : An attempt to attach an auto-named database
       *     for file <<<YOUR-DB-FILE.mdf>>> failed. A database with the same name exists, or specified file
       *     cannot be opened, or it is located on UNC share.
       *   I managed to fix it to actually create an empty database at the given location first
       *   using SMSS (download from microsoft at https://aka.ms/ssmsfullsetup). Connect to 
       *   localdb using SMSS using server name "(LocalDB)\MSSQLLocalDB", then create your
       *   empty database ("biketests"). Note that the folder where these are created can be changed
       *   by editing the paths for the two database files if you are not happy wityh the default
       *   folder.
       */
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
