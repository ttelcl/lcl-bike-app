/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LclBikeApp.DataWrangling.DataLocation;
using LclBikeApp.DataWrangling.RawModel;

using XsvLib;

using Xunit;
using Xunit.Abstractions;

namespace UnitTests.DataWrangling
{
  /// <summary>
  /// Description of DataLoadingTests
  /// </summary>
  public class DataLoadingTests
  {
    private readonly ITestOutputHelper _output;

    public DataLoadingTests(ITestOutputHelper output)
    {
      _output = output;
    }

    [Fact]
    public void CanLoadStations()
    {
      var df = DataFolder.LocateAsAncestorSibling("datafolder0");
      Assert.NotNull(df);
      Assert.Contains("/bin/", df.Root.Replace('\\', '/'));

      var adapter = new StationCursor();
      var stations = new List<RawStation>();
      using(var xsv = Xsv.ReadXsv(df.OpenReadText("stations-subset.csv"), ".csv").AsXsvReader())
      {
        foreach(var cursor in xsv.ReadCursor(adapter))
        {
          Assert.Same(adapter, cursor);
          Assert.True(cursor.HasData);
          var station = RawStation.FromCursor(cursor);
          stations.Add(station);
        }
      }
      Assert.NotEmpty(stations);
      _output.WriteLine($"Read {stations.Count} stations");

      var onm = "stations.json";
      using(var ow = df.CreateWriteTextTmp(onm))
      {
        var ser = new JsonSerializer();
        using(var jw = new JsonTextWriter(ow))
        {
          jw.Formatting = Formatting.Indented;
          ser.Serialize(jw, stations);
        }
      }
      df.BackupShuffle(onm);
      Assert.True(df.HasFile(onm));
    }


  }
}
