using System.IO;
using System.Reflection;
using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using CitybikeApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

#pragma warning disable CS1591

namespace CitybikeApp
{
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

      builder.Services.AddSpaStaticFiles(cfg => {
        cfg.RootPath = "wwwroot/spa";
      });

      // Add our own services
      builder.Services.AddSqlserverCitybikeDatabase("default");
      builder.Services.AddSingleton<StationCacheService>();
      builder.Services.AddSingleton<RideStatsCacheService>();
      builder.Services.AddScoped<StationListService>();
      builder.Services.AddScoped<RideStatsService>();

      // Tweaks. Note: there may be a security concern here ...
      builder.Services.AddResponseCompression(options => { 
        options.EnableForHttps = true;
      });

      var app = builder.Build();

      ILogger logger = app.Services.GetService<ILogger<Program>>()!;
      logger.LogInformation($"ContentRootPath is {app.Environment.ContentRootPath}");
      logger.LogInformation($"WebRootPath is {app.Environment.WebRootPath}");

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

      // app.UseHttpsRedirection();
      app.UseDefaultFiles(new DefaultFilesOptions {
        RequestPath = new PathString(""),
      });

      app.UseStaticFiles();
      app.UseSpaStaticFiles();

      app.UseRouting();

      app.UseAuthorization();

      app.UseResponseCompression();

      app.MapRazorPages();
      app.MapControllers();

      var isDeployed = Directory.Exists(Path.Combine(app.Environment.WebRootPath, "spa"));
      if (isDeployed)
      {
        logger.LogInformation("Detected that this is a deployed instance, enabling SPA hook");
        app.UseSpa(cfg => {
          cfg.Options.DefaultPage = "/index.html";
        });
      }
      else
      {
        logger.LogInformation("Detected that this is a not-deployed instance, disabling SPA hook");
        logger.LogWarning("The root page will therefore cause a 404 instead of being redirected to SPA");
      }

      app.Run();
    }

  }
}
