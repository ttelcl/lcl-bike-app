# Designing the data model

This document describes my design for the data model (looking at it
from the viewpoint of a database).

## Table: Stations

This table provides information about the citybike stations. Its main
source is
https://opendata.arcgis.com/datasets/726277c507ef4914b0aec3cbcfcbfafc_0.csv .

| Name | Type | Nullable | Notes |
| ---- | ---- | -------- | ----- |
| ID   | Integer | No | Primary Key |
| Nimi | String | No | (default name, Finnish) |
| Namn | String | ? | (name in Swedish) |
| Name | String | ? | (name in English) |
| Address | String | ? | Address (Finnish) |
| AddrSe | String | ? | Address in Swedish |
| City | ? | ? | Reference to city (Helsinki or Espoo) (**) |
| Capacity | Integer | ? | Station capacity |
| Latitude | Number (*) | ? | Latitude ('y' in source data) |
| Longitude | Number (*) | ? | Longitude ('x' in source table) |

(*) How to encode the geocoordinates in the database? One could use a high
precision floating point number. Or use a pseudo-fixed point solution,
storing an integer that is the value scaled by 1000000 (using 1000000 since
the source data has them in 6 digit precision).

(**) Should the city be encoded as a code value or as full name? If
a code: should that be a number (indexing another table), or a brief
abbreviation ('HEL', 'ESP')?

## Table: Rides

This table stores one row per recorded ride.

| Name | Type | Nullable | Notes |
| ---- | ---- | -------- | ----- |
| ID | ? | No | Primary key, but type still is undecided |
| Departure | DateTime? | No | (*) |
| Return | DateTime? | No | (*) |
| DepStation | Integer | No | Departure station ID, Reference to Stations.ID |
| RetStation | Integer | No | Return station ID, Reference to Stations.ID |
| Duration | Integer | No | Duration in seconds (**) |

(*) How to best represent Departure and return time stamps? A DateTime
may be natural in databases. The timezone does not need to be stored
explicitly: in this case we know it is Helsinki time.
An alternative would be a UNIX style numerical time, but then you need
to be explicit about the time zone (since those usually imply UTC)

(**) Logically the duration would not be needed, since you could calculate
it from Departure and Return times. But as mentioned in the analysis: 
the duration does not exactly match those!

## Indices

The problem description suggests that the following types of queries will
be common:

* Searching Stations by station name
* Finding rides departing from a specific station
* Finding rides ending at a specific station
* Searching within a specific time range (per month)
* Sorting rides by any column




