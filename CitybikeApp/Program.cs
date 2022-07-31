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
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      // Add services to the container.
      builder.Services.AddRazorPages();

      builder.Services.AddControllers();

      // The reverse proxy to wrap the SPA dev server in dev scenarios
      builder.Services.AddSpaYarp();

      // ref https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle
      builder.Services.AddSwaggerGen(options => {
        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "LclBikeApp.Database.xml"));
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

      app.UseRouting();

      app.UseAuthorization();

      app.UseResponseCompression();

      // app.MapRazorPages();
      // app.MapControllers();

      app.UseEndpoints(endpoints => {
        endpoints.MapRazorPages();
        //endpoints.MapControllers();
        endpoints.MapControllerRoute(
          "default",
          pattern: "{controller}/{action=Index}/{id?}");
        endpoints.MapSpaYarp();
        // The fallback that hooks up the SPA in production
        endpoints.MapFallbackToFile("index.html");
      });

      app.Run();
    }

  }
}
