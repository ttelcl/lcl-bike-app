/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LclBikeApp.Database.Models
{
  /// <summary>
  /// A subset of Station information, containing just the ID and a label
  /// </summary>
  public class StationBasics
  {
    /// <summary>
    /// Create a new StationBasics
    /// </summary>
    public StationBasics(
      int id,
      string label)
    {
      Id = id;
      Label = label;
    }

    /// <summary>
    /// The station ID
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// The default name for the station
    /// </summary>
    public string Label { get; }

  }
}
