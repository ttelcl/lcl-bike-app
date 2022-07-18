/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LclBikeApp.DataWrangling.RawModel;

namespace LclBikeApp.Database.Models
{
  /// <summary>
  /// Describes a citybike station in detail (content of table Stations)
  /// </summary>
  public class Station
  {
    /// <summary>
    /// Create a new Station
    /// </summary>
    public Station(int id, string nameFi, string nameSe, string nameEn,
      string addrFi, string addrSe, int cityId, int capacity, 
      double latitude, double longitude)
    {
      Id=id;
      NameFi=nameFi;
      NameSe=nameSe;
      NameEn=nameEn;
      AddrFi=addrFi;
      AddrSe=addrSe;
      CityId=cityId;
      Capacity=capacity;
      Latitude=latitude;
      Longitude=longitude;
    }

    /// <summary>
    /// Return a new Station instance initialized from the given XSV cursor.
    /// Returns null if no data is loaded in the cursor.
    /// </summary>
    public static Station? TryFromCursor(StationCursor cursor)
    {
      if(cursor.HasData && cursor.FormatValid)
      {
        return new Station(
          cursor.Id,
          cursor.NameFi,
          cursor.NameSe,
          cursor.NameEn,
          cursor.AddrFi,
          cursor.AddrSe,
          cursor.City,
          cursor.Capacity,
          cursor.Latitude,
          cursor.Longitude);
      }
      else
      {
        return null;
      }
    }

    /// <summary>
    /// The station ID
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// The name in Finnish (also used as default name)
    /// </summary>
    public string NameFi { get; }

    /// <summary>
    /// The name in Swedish
    /// </summary>
    public string NameSe { get; }

    /// <summary>
    /// The name in English
    /// </summary>
    public string NameEn { get; }

    /// <summary>
    /// The address in Finnish
    /// </summary>
    public string AddrFi { get; }

    /// <summary>
    /// The address in Swedish
    /// </summary>
    public string AddrSe { get; }

    /// <summary>
    /// The id code for the city (foreign key to a City instance)
    /// </summary>
    public int CityId { get; }

    /// <summary>
    /// Capacity
    /// </summary>
    public int Capacity { get; }

    /// <summary>
    /// Latitude (degrees north)
    /// </summary>
    public double Latitude { get; }

    /// <summary>
    /// Longitude (degrees east)
    /// </summary>
    public double Longitude { get; }

  }
}

