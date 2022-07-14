/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LclBikeApp.DataWrangling.RawModel
{
  /// <summary>
  /// This class models the raw bike station data, as
  /// read from the CSV file. Note that a few fields are NOT
  /// represented here, since I ignore them
  /// </summary>
  public class RawStation
  {
    /// <summary>
    /// Create a new RawStation
    /// </summary>
    public RawStation(
      int id,
      string nameFi,
      string nameSe,
      string nameEn,
      string addressFi,
      string addressSe,
      CityCode city,
      int? capacity,
      double? latitude,
      double? longitude)
    {
      Id = id;
      NameFi = nameFi;
      NameSe = nameSe;
      NameEn = nameEn;
      AddressFi = addressFi;
      AddressSe = addressSe;
      City = city;
      Capacity = capacity;
      Latitude = latitude;
      Longitude = longitude;
    }

    /// <summary>
    /// The station identifier ("ID")
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// The main (Finnish) name of the station. ("Nimi")
    /// </summary>
    public string NameFi { get; set; }

    /// <summary>
    /// The Swedish name of the station. ("Namn")
    /// </summary>
    public string NameSe { get; set; }

    /// <summary>
    /// The English name of the station. ("Name")
    /// </summary>
    public string NameEn { get; set; }

    /// <summary>
    /// The address in Finnish (if known)
    /// </summary>
    public string AddressFi { get; set; }

    /// <summary>
    /// The address in Swedish (if known)
    /// </summary>
    public string AddressSe { get; set; }

    /// <summary>
    /// The city code (0 for Helsinki or unknown, 1 for Espoo.)
    /// </summary>
    public CityCode City { get; set; }

    /// <summary>
    /// The station capacity (null if unknown)
    /// </summary>
    public int? Capacity { get; set; } 

    /// <summary>
    /// The latitude of the station if known, null otherwise ("y" in the data file)
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is typed as a nullable double rather than using NaN or 0.0 if unknown
    /// to avoid improper use.
    /// </para>
    /// </remarks>
    public double? Latitude { get; set; }

    /// <summary>
    /// The longitude of the station if known, null otherwise ("x" in the data file)
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is typed as a nullable double rather than using NaN or 0.0 if unknown
    /// to avoid improper use.
    /// </para>
    /// </remarks>
    public double? Longitude { get; set; }

  }
}
