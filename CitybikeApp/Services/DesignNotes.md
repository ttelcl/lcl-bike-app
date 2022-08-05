# Service object design notes

This folder contains several service objects.
Services that act as cache are typically implemented as two layers:

* The actual cache service, typically named _\<Something\>CacheService_
  (e.g. `RideStatsCacheService`, `StationCacheService`).
  These are installed as _singleton_ services in `Program.cs:Main()`. 
  In ASP.NET Core parlance that means that these services are long-lived
  and hang around in memory as long as the server is running.
  Client code doesn't use these services directly, but instead uses
  the accessor services.
* The cache accessor service (e.g. `RideStatsService`,
  `StationListService`) These are installed as _scoped_ services.
  In ASP.NET parlance that means they are created (as needed) once per
  request, so are relatively short-lived.

The singleton cache services require an explicit Load() call to load
data before accessing their data. For example:
`StationCacheService.loadCache()`. This load call requires access to
the database api object. The actual data access happens through
_methods_ that fail when data isn't loaded yet.
See for example `StationCacheService.GetCachedStations()`.

The scoped services trigger a cache load in their underlying cache
singleton when they are created by ASP.NET's Dependency Injection
system (in their constructor). Therefore they can assume that data
is present in the cache and can present the cached data as
_properties_ that don't require the database api object as argument.
See for example `StationListService.Stations`.

