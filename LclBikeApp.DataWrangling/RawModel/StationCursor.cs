/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using XsvLib;
using XsvLib.Tables.Cursor;

namespace LclBikeApp.DataWrangling.RawModel
{
  /// <summary>
  /// An XsvCursor subclass for reading the Citybike Station list
  /// </summary>
  public class StationCursor: XsvCursor
  {
    private readonly MappedColumn _colId;
    private readonly MappedColumn _colNameFi;
    private readonly MappedColumn _colNameSe;
    private readonly MappedColumn _colNameEn;
    private readonly MappedColumn _colAddressFi;
    private readonly MappedColumn _colAddressSe;
    private readonly MappedColumn _colCityNameFi;
    private readonly MappedColumn _colCapacity;
    private readonly MappedColumn _colLatitude;
    private readonly MappedColumn _colLongitude;


    /// <summary>
    /// Create a new StationCursor
    /// </summary>
    public StationCursor()
      : base(null)
    {
      var map = ColumnMapping;
      // map.Declare("FID");
      _colId = map.Declare("ID");
      _colNameFi = map.Declare("Nimi");
      _colNameSe = map.Declare("Namn");
      _colNameEn = map.Declare("Name");
      _colAddressFi = map.Declare("Osoite");
      _colAddressSe = map.Declare("Adress");
      _colCityNameFi = map.Declare("Kaupunki");
      // map.Declare("Stad");
      // map.Declare("Operaattor");
      _colCapacity = map.Declare("Kapasiteet");
      _colLatitude = map.Declare("y");
      _colLongitude = map.Declare("x");
    }

    /// <summary>
    /// True if the format of the current record seems valid.
    /// If this is false accessing other properties may throw an exception
    /// </summary>
    public bool FormatValid {
      get {
        return HasData;
      }
    }

    /// <summary>
    /// The station identifier in the current record
    /// </summary>
    public int Id => GetInt32(_colId);

    /// <summary>
    /// The name in Finnish for the station in the current record
    /// </summary>
    public string NameFi => GetString(_colNameFi);

    /// <summary>
    /// The name in Swedish for the station in the current record
    /// </summary>
    public string NameSe => GetString(_colNameSe);

    /// <summary>
    /// The name in English for the station in the current record
    /// </summary>
    public string NameEn => GetString(_colNameEn);

    /// <summary>
    /// The name in Finnish for the adress of the station in the current record
    /// </summary>
    public string AddrFi => GetString(_colAddressFi);

    /// <summary>
    /// The name in Swedish for the adress of the station in the current record
    /// </summary>
    public string AddrSe => GetString(_colAddressSe);

    /// <summary>
    /// The Finnish name for the city the current station is in.
    /// When blank, this should be interpreted as "Helsinki"
    /// </summary>
    public string CityNameFi => GetString(_colCityNameFi);

    /// <summary>
    /// The capacity of the citybike station
    /// </summary>
    public int Capacity => GetInt32(_colCapacity);

    /// <summary>
    /// The latitude part of the station's location
    /// </summary>
    public double Latitude => Double.Parse(GetString(_colLatitude));

    /// <summary>
    /// The longitude part of the station's location
    /// </summary>
    public double Longitude => Double.Parse(GetString(_colLongitude));

    /// <summary>
    /// The city identifier, as determined from CityNameFi
    /// </summary>
    public int City {
      get {
        var cityCode = CityName.ParseCity(CityNameFi);
        return (int)cityCode;
      }
    }

  }
}
