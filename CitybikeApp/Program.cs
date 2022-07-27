using System.IO;
using System.Reflection;
using System;

using CitybikeApp.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#pragma warning disable CS1591

namespace CitybikeApp
{

  /*
   * Some of the modifications I added to the template were inspired by these
   * two articles about integrating an SPA in an ASP.NET Core app:
   * - https://abhinandanaryal.info.np/article/setting-up-quasar-app-with-asp-net-core
   * - https://mike.koder.fi/en/2021/03/hosting-spa-aspnetcore
   * Neither tells the whole story, but I picked and combined info from both.
   */

  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      // Add services to the container.
      builder.Services.AddRazorPages();

      builder.Services.AddControllers();

      // ref https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle
      builder.Services.AddSwaggerGen(options => {
        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "LclBikeApp.Database.xml"));
      });

      builder.Services.AddSpaStaticFiles(configuration =>
      {
        configuration.RootPath = "citybike-quasar/dist/spa";
      });

      // Add our own services
      builder.Services.AddSqlserverCitybikeDatabase("default");
      builder.Services.AddSingleton<StationCacheService>();
      builder.Services.AddScoped<StationListService>();

      // Tweaks. Note: there may be a security concern here ...
      builder.Services.AddResponseCompression(options => { 
        options.EnableForHttps = true;
      });

      var app = builder.Build();

      // Configure the HTTP request pipeline.
      if(!app.Environment.IsDevelopment())
      {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production
        // scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      // ref https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle
      if(app.Environment.IsDevelopment() || true)
        // Due to the nature of this project, enable swagger even on non-dev builds
      {
        app.UseSwagger();
        app.UseSwaggerUI();
      }

      app.UseHttpsRedirection();
      app.UseStaticFiles();

      app.UseSpaStaticFiles(); // added for SPA support

      app.UseRouting();

      app.UseAuthorization();

      app.UseResponseCompression();

      // Boilerplate code modified to allow for hooking in the SPA
      //app.MapRazorPages();
      //app.MapControllers();

      app.UseEndpoints(endpoints => {
        endpoints.MapRazorPages();
        endpoints.MapControllers();
        // Hook up our SPA entrypoint, as registered above with AddSpaStaticFiles()
        // and UseSpaStaticFiles()
        endpoints.MapFallbackToFile("/index.html");
      });

      app.UseSpa(spa => {
        spa.Options.SourcePath = "citybike-quasar";
        if(app.Environment.IsDevelopment())
        {
          spa.UseProxyToSpaDevelopmentServer("http://localhost:9000");
        }
      });

      app.Run();
    }

  }
}
