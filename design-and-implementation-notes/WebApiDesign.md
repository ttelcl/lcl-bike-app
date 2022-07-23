# Web API design

During design of the Web API I made some assumptions on how it was 
going to be used:

* The web API is REST based
* I assume the client will load (or even hard-code) the entire list of
cities at startup, so the station records can just refer to a city ID
rather than a a city name. With only 2 cities (Helsinki & Espoo) that
assumption is not too far-fetched
* I assume the client will load and cache the entire list of citybike
stations at startup. At the current size of that list (457 stations)
that still seems reasonable. As alternative one may consider splitting
the list either as separate row groups (e.g. accessing the stations
for each city independently, or in a "paged" way), or as separate
column sets (e.g. separating the uncommonly used details such as
GeoCoordinates and capacity into a second endpoint)
* The ride data *must* be accessed in pages - the API will suggest that
by always including page size and offset parameters to all web API
endpoint that returns rides. The alternative would be a query that
runs the risk of returning _all_ rides - all 1.5 million of them...
* The Web API is, for now, _read-only_. There are only GET methods,
no POST, PUT, or DELETE methods. All parameters are therefore passed 
in the URL.

While making an initial implementation of the Web API I discovered
that my chosen backend technology (ASP.NET Core 6) has support for
Swagger, and with little additional effort can generate documention
"automatically". Upon building the backend that documentation can be
found (when enabled) at /swagger/index.html (in the default
development site that would be https://localhost:7185/swagger/index.html )

The rest of this document lists a subset of the Web API entries.
Full documentation that provides information beyond what Swagger
already shows is for now beyond scope (it would be double work...)

## GET /api/citybike/cities

Returns the list of cities:

```json
[
  { "id": 0, "cityFi": "Helsinki", "citySe": "Helsingfors" },
  { "id": 1, "cityFi": "Espoo", "citySe": "Esbo" }
]
```

## GET /api/citybike/stations

Returns the full list of all known stations:

```json
[
  {
    "id": 1,
    "nameFi": "Kaivopuisto",
    "nameSe": "Brunnsparken",
    "nameEn": "Kaivopuisto",
    "addrFi": "Meritori 1",
    "addrSe": "Havstorget 1",
    "cityId": 0,
    "capacity": 30,
    "latitude": 60.155369615074,
    "longitude": 24.9502114714031
  },
  ...
]
```

## GET /api/citybike/ridescount

```
GET /api/citybike/ridescount?t0=2021-05-01&t1=2021-05-02
```

Returns the number of rides in the given time interval as a single integer
(the parameters are optional). This can be used by the client to calculate
the number of pages.

## GET /api/citybike/ridespage

```
GET /api/citybike/ridespage?offset=0&pagesize=50&t0=2021-05-01&t1=2021-05-02
```

Returns one page of (up to) "pagesize" ride records from the given departure
time range.

```json
[
  {
    "depTime": "2021-05-01T00:00:11",
    "retTime": "2021-05-01T00:04:34",
    "depStationId": 138,
    "retStationId": 138,
    "distance": 1057,
    "duration": 259
  },
  ...
]
```

## _TBD_

_(for the rest, see the swagger UI)_
