# Designing the data model

This document describes my design for the data model (looking at it
from the viewpoint of a database). The design is based on the 
results of the [Data Review](DataReview.md).

A few additional considerations:
* For now, I only support Citybike stations that appear in the list
of stations. Any rides that refer to stations that are not listed 
there are rejected by the validation logic. Without this limitation
I would either not be able to reference stations from the rides with
a foreign key, or would need to support partially known stations
(stations with just an ID and a name, but with other details null).
Keep in mind that such rides are very rare, so the impact is small.
* I ignore the names of the citybike stations as given in the
ride data, using the names from the station table instead. In most
cases the names in the ride data matches the Finnish name in the
station table, but there are a small number of exceptions.
* I use english-like names for the database fields. That means that
some of the data columns have a different name in the tables.
* I use a separate lookup table for the name of the city. In it
I assume that a missing name actually means "Helsinki"
* In the design I keep the intended database in mind: SQL Server,
since that would make it easier to get running in Azure, if I get
to that. In practice this is the reason for typing the "Rides"
table ID as "uniqueidentifier" (Guid)

## Table: Cities

This is a small lookup table for matching a city code to a city name
(thereby conserving space in the storage of city names in the station
table). There are only two entries: 0 = Helsinki and 1 = Espoo

| Name | Type | Nullable? | Notes |
| ---- | ---- | --------- | ----- |
| Id   | Integer | No | Primary key |
| CityFi | String | No | Name in Finnish (& English) |
| CitySe | String | No | Name in Swedish |

## Table: Stations

This table provides information about the citybike stations. Its main
source is
https://opendata.arcgis.com/datasets/726277c507ef4914b0aec3cbcfcbfafc_0.csv .

| Name | Type | Nullable? | Notes |
| ---- | ---- | --------- | ----- |
| Id   | Integer | No | Primary Key |
| NameFi | String | No | Name in Finnish ("Nimi" in source) |
| NameSe | String | No | Name in Swedish ("Namn" in source) |
| NameEn | String | No | Name in English ("Name" in source) |
| AddrFi | String | No | Address in Finnish ("Osoite" in source) |
| AddrSe | String | No | Address in Swedish ("Adress" in source) |
| City | Integer | No | Reference to Cities.Id |
| Capacity | Integer | No | Station capacity |
| Latitude | Double | No | Latitude ('y' in source) |
| Longitude | Double | No | Longitude ('x' in source) |

## Table: Rides

This table stores one row per recorded ride.

| Name | Type | Nullable | Notes |
| ---- | ---- | -------- | ----- |
| Id | GUID | No | Primary key, see comment below (*) |
| DepTime | DateTime | No | Departure time |
| RetTime| DateTime | No | Return time |
| DepStation | Integer | No | Departure station ID, Reference to Stations.Id |
| RetStation | Integer | No | Return station ID, Reference to Stations.Id |
| Distance | Integer (**) | No | Distance in meters |
| Duration | Integer | No | Duration in seconds (***) |

(*) Note that the type for this column is inspired by SQL Server
features, and will be server-generated. As explained in the data review,
there is no simple and natural combination of fields that could otherwise
act as primary key.

Should there be an opportunity to revisit what fields
are in the ride data, I would recommend checking if a unique ID of the
bike used for the ride could be included. Together with the departure
time that would make a fine unique ride ID.

(**) Almost all ride distances are already expressed in integer meters.
A very few cases express distance as fractional meters (always
multiples of 1/3rd of a meter), but these are so rare that extending the
type to a floating point number doesn't seem worth the bother.

(***) Logically the duration would not be needed, since you could calculate
it from Departure and Return times. But as mentioned in the analysis: 
the duration does not exactly match those!

## Indexes

The problem description suggests that the following types of queries will
be common:

* Searching Stations by station name
* Finding rides departing from a specific station
* Finding rides ending at a specific station
* Searching within a specific time range (per month)
* Sorting rides by any column

The table of stations is actually not that large, and at least initially,
adding extra indexes to speed up name search doesn't seem much use.

Quickly accessing rides starting or ending at a specific station would benefit
greatly from adding additional indexes: when, for example, answering the question
_"what rides started at station 123"_ you do not want the server to have to look
at _all_ rides to return the answer.


