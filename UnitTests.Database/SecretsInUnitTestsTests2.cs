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

using Xunit;
using Xunit.Abstractions;

namespace UnitTests.Database
{

  /*
    
  These tests use the .net Core "user secrets" system to access the database connection string
  as applies to your computer. You can find a description of this secrets handling system
  at https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets , but be aware that
  that is written from the viewpoint of an asp.net core *application*, not a unit test.
  
  See https://www.foobarton.com/posts/testing-with-user-secrets/ for hints on how to use
  it in a unit test project like this here.

  The whole point of this exercise is to have a way to get config info that is not safe
  to store in GIT, so you will actually have to create that secret configuration file
  ("secrets.json") to make these tests pass. To do so in Visual Studio 2022, right-click
  the project node for this test project in Solution Explorer ("UnitTests.Database")
  and select "Manage User Secrets". That will open your secrets.json file.

  Alternatively find the location of that file as described in the microsoft article above.
  
  For purpose of this test give the secrets.json file a content like the following
  (with "C:\\<<YOUR-DATABASE-DIRECTORY>>" replaced with the folder where ypu want the
  Sql Server database file (*.mdf) on your computer)

    {
      "TestDb:ConnectionString":
          "Server=(LocalDB)\\MSSQLLocalDB;Integrated Security=true;AttachDbFileName=C:\\<<YOUR-DATABASE-DIRECTORY>>\\biketests.mdf"
    }

  */

  public class SecretsInUnitTestsTests2
  {
    private readonly ITestOutputHelper _output;

    /// <summary>
    /// Create a new SecretsInUnitTestsTests
    /// </summary>
    public SecretsInUnitTestsTests2(ITestOutputHelper output)
    {
      _output=output;
    }

    [Fact]
    public void CanAccessSecrets2()
    {
      // See comment above for the correct setup to make this test pass!

      // The bit that took me longest to figure out is what
      // type parameter to pass in .AddUserSecrets<>. It turns out
      // it is just used to find the assembly, so any type in this
      // assembly would work ...
      var configuration = new ConfigurationBuilder()
        .AddUserSecrets<SecretsInUnitTestsTests2>()
        .Build();

      // Some diagnostics:
      var keys = configuration.AsEnumerable().Select(x => x.Key).ToList();

      Assert.True(keys.Count > 0,
        "No secrets found. Did you remember to configure the secrets? See comments at the top of this file.");

      _output.WriteLine($"The following {keys.Count} config keys are present:");
      foreach(var kvp in configuration.AsEnumerable())
      {
        var isEmpty = string.IsNullOrEmpty(kvp.Value);
        var valueText = isEmpty ? "Empty" : $"{kvp.Value.Length} characters";
        _output.WriteLine($"  '{kvp.Key}' ({valueText})");
      }

      // Normal use case:
      var connectionString = configuration["TestDb:ConnectionString"];
      Assert.NotNull(connectionString);
      Assert.NotEqual("", connectionString);

      var doesNotExist = configuration["Does:Not:Exist"];
      Assert.Null(doesNotExist);
    }

  }
}
