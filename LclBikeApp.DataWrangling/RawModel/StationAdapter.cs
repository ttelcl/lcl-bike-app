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
  /// Adapter for reading RawStation objects from CSV using my
  /// XsvLib library
  /// </summary>
  public class StationAdapter
  {
    /// <summary>
    /// Create a new StationReadAdapter
    /// </summary>
    public StationAdapter()
    {
      ColumnMapping = new ColumnMap(false);
      //FidColumn = ColumnMapping.Declare("FID");
      //CitySeColumn = ColumnMapping.Declare("Stad");
      //OperatorColumn = ColumnMapping.Declare("Operaattor");
      IdColumn = ColumnMapping.Declare("ID");
      NameFiColumn = ColumnMapping.Declare("Nimi");
      NameSeColumn = ColumnMapping.Declare("Namn");
      NameEnColumn = ColumnMapping.Declare("Name");
      AddressFiColumn = ColumnMapping.Declare("Osoite");
      AddressSeColumn = ColumnMapping.Declare("Adress");
      CityColumn = ColumnMapping.Declare("Kaupunki");
      CapacityColumn = ColumnMapping.Declare("Kapasiteet");
      LongitudeColumn = ColumnMapping.Declare("x");
      LatitudeColumn = ColumnMapping.Declare("y");
    }

    /// <summary>
    /// The ColumnMap owning the MappedColumns (and controlling
    /// the mapping of each logical column to an actual column index)
    /// </summary>
    public ColumnMap ColumnMapping { get; }

    ///// <summary>
    ///// Represents the "FID" column (unused)
    ///// </summary>
    //public MappedColumn FidColumn { get; }

    ///// <summary>
    ///// Represents the "Stad" column (unused)
    ///// </summary>
    //public MappedColumn CitySeColumn { get; }

    ///// <summary>
    ///// Represents the "Operaattor" column (unused)
    ///// </summary>
    //public MappedColumn OperatorColumn { get; }

    /// <summary>
    /// Represents the "ID" column
    /// </summary>
    public MappedColumn IdColumn { get; }

    /// <summary>
    /// Represents the "Nimi" column
    /// </summary>
    public MappedColumn NameFiColumn { get; }

    /// <summary>
    /// Represents the "Namn" column
    /// </summary>
    public MappedColumn NameSeColumn { get; }

    /// <summary>
    /// Represents the "Name" column
    /// </summary>
    public MappedColumn NameEnColumn { get; }

    /// <summary>
    /// Represents the "Osoite" column
    /// </summary>
    public MappedColumn AddressFiColumn { get; }

    /// <summary>
    /// Represents the "Adress" column
    /// </summary>
    public MappedColumn AddressSeColumn { get; }

    /// <summary>
    /// Represents the "Kaupunki" column
    /// </summary>
    public MappedColumn CityColumn { get; }

    /// <summary>
    /// Represents the "Kapasiteet" column
    /// </summary>
    public MappedColumn CapacityColumn { get; }

    /// <summary>
    /// Represents the "y" column
    /// </summary>
    public MappedColumn LatitudeColumn { get; }

    /// <summary>
    /// Represents the "x" column
    /// </summary>
    public MappedColumn LongitudeColumn { get; }

    /// <summary>
    /// Read the current record in the cursor into a new RawStation instance
    /// </summary>
    /// <param name="cursor">
    /// The cursor, created using this StationAdapter's ColumnMapper as column map.
    /// </param>
    /// <returns>
    /// The newly created station instance, or null if the cursor had no data or
    /// there was no content in the ID, x, y, or capacity columns.
    /// </returns>
    public RawStation? Read(XsvCursor cursor)
    {
      if(cursor.HasData)
      {
        if(
          Int32.TryParse(cursor[IdColumn], out var id)
          && Double.TryParse(cursor[LatitudeColumn], out var latitude)
          && Double.TryParse(cursor[LongitudeColumn], out var longitude)
          && Int32.TryParse(cursor[CapacityColumn], out var capacity)
          )
        {
          var cityText = cursor[CityColumn];
          var city = CityName.ParseCity(cityText);
          // To avoid mismatched names, make sure to translate any
          // non-breaking spaces ('\u00A0') to ordinary spaces.
          return new RawStation(
            id,
            cursor[NameFiColumn]!.Replace('\u00A0', ' '),
            cursor[NameSeColumn]!.Replace('\u00A0', ' '),
            cursor[NameEnColumn]!.Replace('\u00A0', ' '),
            cursor[AddressFiColumn]!.Replace('\u00A0', ' '),
            cursor[AddressSeColumn]!.Replace('\u00A0', ' '),
            city,
            capacity,
            latitude,
            longitude);
        }
        else
        {
          return null;
        }
      }
      else
      {
        return null;
      }
    }
  }
}
