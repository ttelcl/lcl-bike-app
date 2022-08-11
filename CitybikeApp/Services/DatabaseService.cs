using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LclBikeApp.Database;
using LclBikeApp.Database.ImplementationSqlServer;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CitybikeApp.Services
{
  /// <summary>
  /// Static configuration helper methods for accessing the Citybike DB
  /// </summary>
  public static class DatabaseService
  {
    /// <summary>
    /// Configuration helper that registers a factory method in the standard 
    /// dependency injection container to create an ICitybikDb instance
    /// implemented by CitybikeDbSqlServer (with lifetime "Scoped" == one per
    /// web request)
    /// </summary>
    /// <param name="services">
    /// The service collection to register the factory in
    /// </param>
    /// <param name="dbKey">
    /// The name of the connection string in the configuration, by default
    /// "default".
    /// </param>
    /// <returns></returns>
    public static IServiceCollection AddSqlserverCitybikeDatabase(
      this IServiceCollection services,
      string dbKey = "default")
    {
      services.AddScoped<ICitybikeDb, CitybikeDbSqlServer>(
        isp => CreateCitybikeDbService(isp, dbKey));
      return services;
    }

    private static CitybikeDbSqlServer CreateCitybikeDbService(
      IServiceProvider services, string dbKey)
    {
      var configuration = services.GetService<IConfiguration>();
      if(configuration == null)
      {
        throw new InvalidOperationException(
          $"Configuration error: cannot access config service");
      }
      // var cs = configuration.GetConnectionString(dbKey);
      var cs = configuration.GetSection("ConnectionStrings")[dbKey];
      if(String.IsNullOrEmpty(cs))
      {
        var section = configuration.GetSection("ConnectionStrings");
        var keys = section.AsEnumerable(true).Where(kvp => kvp.Value!=null).Select(kvp => kvp.Key).ToList();
        var keyList = "'" + String.Join("', '", keys) + "'";
        throw new InvalidOperationException(
          $"Configuration error: missing DB connection string with name '{dbKey}'. Known keys are {keyList}");
      }
      return new CitybikeDbSqlServer(cs);
    }
  }
}
