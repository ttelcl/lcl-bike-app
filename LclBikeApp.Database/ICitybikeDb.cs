/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LclBikeApp.Database.Models;

namespace LclBikeApp.Database
{
  /// <summary>
  /// Defines the API available for interacting with the Citybike DB
  /// in a database neutral way.
  /// </summary>
  public interface ICitybikeDb: IDisposable
  {
    /// <summary>
    /// True after the instance has been disposed
    /// </summary>
    bool Disposed { get; }

    /// <summary>
    /// Initialize the database, creating missing tables if necessary
    /// and initializing the Cities table to default content
    /// </summary>
    /// <param name="erase">
    /// When true ALL DATABASE CONTENT IS REMOVED first
    /// (the "factory reset" option).
    /// </param>
    /// <returns>
    /// The number of DB objects created.
    /// </returns>
    int InitDb(bool erase = false);

    /// <summary>
    /// Load the full cities table from the database
    /// </summary>
    AllCities LoadCities();

    /// <summary>
    /// Enumerate all station records in the DB.
    /// See also GetStationIds() and GetStationBasics()
    /// </summary>
    IReadOnlyList<Station> GetStations();

    /// <summary>
    /// Enumerate a brief summary for each station in the DB
    /// </summary>
    IReadOnlyList<StationBasics> GetStationBasics();

    /// <summary>
    /// Enumerate all known station IDs. To load the full
    /// station data use GetStations() instead.
    /// </summary>
    IReadOnlyList<int> GetStationIds();

    /// <summary>
    /// Insert the given stations into the DB, unless they already
    /// are present. This method does not update existing stations.
    /// </summary>
    /// <returns>
    /// The number of stations inserted
    /// </returns>
    int AddStations(IEnumerable<Station> stations);

    /// <summary>
    /// Insert the provided batch of RideBase instances.
    /// No validation is done - that is supposed to have happened
    /// already. The rides are inserted as new instances, with 
    /// the DB generating new IDs for them
    /// </summary>
    /// <param name="rides">
    /// The rides to insert
    /// </param>
    /// <returns>
    /// The number of rides inserted, which may be less than
    /// the number presented rides when duplicates are rejected.
    /// </returns>
    int AddBaseRides(IEnumerable<RideBase> rides);
  }
}


