using System.Linq;

using LclBikeApp.DataWrangling.DataLocation;

using Xunit;
using Xunit.Abstractions;

namespace UnitTests.DataWrangling
{
  public class DataLocatingTests
  {
    private readonly ITestOutputHelper _output;

    public DataLocatingTests(ITestOutputHelper output)
    {
      _output = output;
    }

    [Fact]
    public void FolderLocatorTests()
    {
      _output.WriteLine($"CWD is {Environment.CurrentDirectory}");

      var flst1 = FolderLocator.SelfAndAncestors().ToList();
      Assert.NotEmpty(flst1);
      // The Replace() is to support both windows and linux.  
      Assert.Contains("/bin/", flst1[0].Replace('\\', '/'));

      var flst2 = FolderLocator.SelfAndAncestorSiblings("datafolder0", false).ToList();
      Assert.NotEmpty(flst2);
      foreach(var folder in flst2)
      {
        _output.WriteLine($"Exists={Directory.Exists(folder)}:  {folder}");
      }
      Assert.True(Directory.Exists(flst2[0]),
        "The data sample folder was not correctly copied by the post-build step");
      // In the above case: fail fast and do not allow the rest of this test to run!

      var flst3 = FolderLocator.SelfAndAncestorSiblings("datafolder0").ToList();
      // Expect to find the data folder copy in the build output folder and the
      // original in the source.
      Assert.Equal(2, flst3.Count);

      // Throws an exception if not found
      var df = DataFolder.LocateAsAncestorSibling("datafolder0");
      Assert.NotNull(df);
      Assert.Contains("/bin/", df.Root.Replace('\\', '/'));

      Assert.True(df.HasFile("readme.txt"));
      Assert.True(df.HasFile("rides-subset.csv"));
      Assert.True(df.HasFile("stations-subset.csv"));
    }

    [Fact]
    public void ThrowForMissingDataFolder()
    {
      Assert.Throws<DirectoryNotFoundException>(
        () => {
          var unused = DataFolder.LocateAsAncestorSibling("no-such-folder");
        });
    }

    [Fact]
    public void CanFindRealDataFolder()
    {
      /*
       * Alert!
       * This test has an external dependency: a folder named "_data"
       * must exist as child of any of the ancestor folders, containing
       * the main data files.
       */
      // There is no Assert here; if this fails an exception is thrown
      var datafolder = DataFolder.LocateAsAncestorSibling("_data");
      Assert.True(datafolder.HasFile("2021-05.csv"));
      Assert.True(datafolder.HasFile("2021-06.csv"));
      Assert.True(datafolder.HasFile("2021-07.csv"));

    }


  }
}
