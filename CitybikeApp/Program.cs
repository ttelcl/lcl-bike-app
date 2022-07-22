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

      // ref https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle
      builder.Services.AddSwaggerGen(options => {
        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "LclBikeApp.Database.xml"));
      });

      // Add our own service
      builder.Services.AddSqlserverCitybikeDatabase("default");

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
      if(app.Environment.IsDevelopment())
      {
        app.UseSwagger();
        app.UseSwaggerUI();
      }

      app.UseHttpsRedirection();
      app.UseStaticFiles();

      app.UseRouting();

      app.UseAuthorization();

      app.MapRazorPages();
      app.MapControllers();

      app.Run();
    }

  }
}
