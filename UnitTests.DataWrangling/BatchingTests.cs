/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LclBikeApp.DataWrangling.Utilities;

using Xunit;
using Xunit.Abstractions;

namespace UnitTests.DataWrangling
{
  public class BatchingTests
  {
    private readonly ITestOutputHelper _output;

    public BatchingTests(ITestOutputHelper output)
    {
      _output=output;
    }

    [Fact]
    public void CanBatchSequence()
    {
      var samples = new List<DateTime> {
        new DateTime(2021, 5, 1, 0, 0, 0),
        new DateTime(2021, 5, 1, 0, 1, 0),
        new DateTime(2021, 5, 1, 0, 2, 0),
        new DateTime(2021, 5, 1, 0, 3, 0),

        new DateTime(2021, 5, 2, 0, 0, 0),
        new DateTime(2021, 5, 2, 0, 1, 0),
        new DateTime(2021, 5, 2, 0, 2, 0),
        new DateTime(2021, 5, 2, 0, 3, 0),

        new DateTime(2021, 5, 1, 1, 0, 0),
        new DateTime(2021, 5, 1, 1, 1, 0),
        new DateTime(2021, 5, 1, 1, 2, 0),
        new DateTime(2021, 5, 1, 1, 3, 0),
      };
      Func<DateTime, DateTime> keyfunc = dt => dt.Date;

      var batches = SequenceBatching.BatchByKeyFunc<DateTime, DateTime>(
        samples, dt => dt.Date)
        .ToList();

      Assert.Equal(3, batches.Count);
      Assert.Equal(4, batches[0].Count);
      Assert.Equal(4, batches[1].Count);
      Assert.Equal(4, batches[2].Count);
      Assert.Equal(new DateTime(2021, 5, 1), batches[0][0].Date);
      Assert.Equal(new DateTime(2021, 5, 2), batches[1][0].Date);
      Assert.Equal(new DateTime(2021, 5, 1), batches[2][0].Date);
    }

  }
}
