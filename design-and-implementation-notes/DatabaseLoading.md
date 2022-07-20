# Database Loading

## The CitybikeUtility command line tool

### Introduction

To load data into a database (SQL Server assumed), I created a command line
tool "CitybikeUtility" in F#. It wraps the C# libraries that I created earlier
(LclBikeApp.DataWrangling and LclBikeApp.Database) that implement the
data validation and database access logic.

To get usage instructions, run the tool without arguments. Or read the
description below.

### Configuring the database

To use, you first need to create an SQL Server database.
I tested it with the following variants:

* SQL LocalDb (as installed in the VS 2022 installation by the "Data storage
and processing" workload) running locally
* SQL Azure (serverless). I created a DB in my azure account with fairly minimal
settings (Gen5, 1 vCore, 12 GB storage)

Once you have a DB running you need to tell CitybikeUtility about it, by 
adding the database's Connection String to either the "configuration.json"
file in the CitybikeUtility binary folder or in the "user secrets" file for
the CitybikeUtility project (see https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets )

The configuration.json or secrets.json files should have a section named
"ConnectionStrings" that looks a bit like (with parts removed):

```json
{
  ........
  "ConnectionStrings": {
    "default": "Server=(LocalDB)\\MSSQLLocalDB;........",
    "azure": "Server=tcp:SERVER_NAME_REMOVED.database.windows.net,1433;........"
  }
  ........
}
```

Note that this sets up two distinct databases; CitybikeUtility's "-db" option allows
choosing which one you want to operate on.

### Configuring the data folder

Data files will be loaded from a data folder that is searched amongst a series of
candidates relative to the current directory from where you run CitybikeUtility.exe:
The first of these folder that actually exists is used: 
"_data", "../_data", "../../_data", etc. If you want to use a different name than
"_data" as search key, you can change that with the "-df" option.

Create a folder like that (in a place that doesn't run the risk of accidentally
being added to GIT), and store the downloaded data files there. The data file
locations can be found in the assignment.

### Loading station data

The first part to load into the database is the list of citybike station details.
This will also create the database tables and indexes if not already done so.

Use a command like:
```
CitybikeUtility init-stations [-db <dbtag>] [-df <datafolder>] [-s <stations.csv>]
```
where:
* `<dbtag>` is the tag of your database connection string in your configuration 
(by default that is _"default"_)
* `<datafolder>` is an optional override for the data folder search tag (by
default that is "_data")
* `<stations.csv>` is the name of the file in your data folder where you downloaded
the station data. If omitted, the app tries _"stations.csv"_ and
_"Helsingin_ja_Espoon_kaupunkipyöräasemat_avoin.csv"_

Note that this command only uploads new stations, it does not update changes.

### Validating and loading ride data

The "init-rides" command can be used in two ways: to just validate ride data files,
or to validate and upload those data files. You can process multiple data files at
once, or conversely, select a date range within a data file to process.

The command selects rides according to the configured data validation rules and
further processes those in batches spanning one day at a time.

The default data validation rules are as were derived in the data review phase,
see [DataReview.md](DataReview.md). An additional trick is used to skip the
duplicate data rows (as observed during the data review,
the second half of the data files is an exact copy of the
first half): by default, only data rows with a departure time equal or earlier
than all previous data rows are considered.

The data validation rules are also specified explicitly in the configuration.json
file, enabling you to override them. Overriding them is not reccommended and is 
not tested (and will lead to ride data matching different validation in the 
database)

```json
{
    "ValidationParameters": {
         "MinDistance": 400,
         "MaxDistance": 8000,
         "MinDuration": 120,
         "MaxDuration": 14400,
         "TimeTolerance": 20,
         "RequireNonAscendingDepartures": true
    }
}
```

In addition to these configurable rules there are a few hard rules:
* Only rides that refer to departure and return stations that are already
in the database are accepted. Therefore it is important to load the
station data first!
* Rides with a blank distance column are rejected.

#### Example invocations for validation only:

```
CitybikeUtility init-rides -i 2021-07.csv -from 2021-07-24 -to 2021-07-31
CitybikeUtility init-rides -i 2021-07.csv
CitybikeUtility init-rides -S -i 2021-07.csv
```

Running these will run the validation rules on the data and print 
statistics on the outcome.

* The first invocation uses a local station data file and then validates rides
from July 24 to July 31 (printing info as it goes).
* The second processes the entire file (day by day)
* The third uses the stations loaded in the default database instead of a stations
file to find valid station IDs

_Reminder: call CitybikeUtility.exe without arguments for full command line option
details_

#### Example invocations for actual database insertion:

```
CitybikeUtility init-rides -insert -db azure -i 2021-07.csv -from 2021-07-24 -to 2021-07-31
CitybikeUtility init-rides -insert -db default -i 2021-07.csv
CitybikeUtility init-rides -insert -i 2021-07.csv
```

* The first does the same validation of the data, and uploads accepted rides to the
database that was configured as "azure". Note that in this case the valid station IDs
are always taken from that DB ("-insert" implies "-S")
* The second uploads the entire July data file to the "default" database.
* The third is equivalent to the second.

**IMPORTANT**
* Rides that already exist are uploaded, but ignored by the database. So the number
of inserted rides may be lower than the number of accepted rides
* Data row upload currently could use some redesign. Especially on networked database
servers it is SLOW. I recommend selecting short time spans using "-from" and "-to"
(one day at a time). In my test with Azure SQL, uploading a day's worth of rides
(about 15000) took 7-8 minutes. Don't panic if the app seems to hang, just be patient.

