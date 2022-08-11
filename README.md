# City bike application

This repository contains my submission to
[Solita's](https://www.solita.fi/en/)
_Fall 2022 Dev Academy pre-assignment_. You can find the problem statement
and links to the data on the
[assignment's own GitHub page](https://github.com/solita/dev-academy-2022-fall-exercise).

<!-- (see https://github.com/solita/dev-academy-2022-fall-exercise ) -->

In this page:

- [Features and Overview](#features-and-overview)
- [Instructions](#instructions)
- [Known Issues](#known-issues)
- [Coding Technologies](#coding-technologies)
- [Development process](#development-process)

## Features and Overview

- Front-end user interface:
  - [Citybike stations list](design-and-implementation-notes/screenshots.md#stations-list):
    Browse, Filter by name, Sort by any column, 
    Incoming and outgoing ride counts, Ranking by ride count, Links to rides
    browser, Links to Google maps, Average incoming and outgoing ride
    distance, Average incoming and outgoing ride duration, Name and address
    display language selection. Client side pagination and sorting.
  - [Rides browser](design-and-implementation-notes/screenshots.md#rides-browser):
    Display rides, Start station, End station, Day, Start
    and end times, Duration, Distance. Links to the station detail pages. 
    Filter by start station, end station, date range, distance range, 
    duration range. Server side pagination. Support for URL query
    parameters.
  - [Station detail page](design-and-implementation-notes/screenshots.md#single-station-view):
    List of other stations having rides to or from
    the focused station. Popularity of origin stations, Popularity of
    destination stations, Links to rides browser, Average incoming /
    outgoing ride distance and duration. Client side pagination and sorting.
  - Built with [Quasar](https://quasar.dev/) (on top of
    [Vue 3](https://vuejs.org/))
- Backend server:
  - Serving the data as REST API.
  - Swagger documentation for that REST API
  - Built using C# in
  [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core)
  - Server side UI, based on
  [Razor Pages](https://docs.microsoft.com/en-us/aspnet/core/razor-pages/).
  (Not really used).
- Data handling:
  - [Analysis](design-and-implementation-notes/DataReview.md) of the data
  files to derive reasonable data validation limits.
  - Database design, targeting SQL Server.
  - Database adaptation layer implemented using
  [Dapper](https://github.com/DapperLib/Dapper).
  - Data validation and database loading via a commandline app
  implemented in F# (and using the same C# libraries as used in the
  backend)

## Instructions

Instructions for building, setting up and loading the DB, running in
dev mode, deploying, etc. have been moved to their own
[instructions document](instructions.md).

## Known Issues

- Currently the app requires a "tandem" setup to run, using a separate
frontend server process and backend server process.
  - Initial steps have been taken to test deployment to IIS and while
  the basics work, there are still issues to resolve.
- The data loading app has a performance issue with bulk data upload.
It works well enough for use with SQL Server _LocalDb_, but is too slow
for non-local SQL Server instances (I tested this using Azure SQL).
- The rides browser does not support sorting. (The sorting in the
station list and the station detail page however works fine, for
all columns)
- Make sure to follow the steps in the [instructions](#instructions).
After cloning the repo there are some initial steps to be taken
_before_ compiling.
- Throughout the code and UI there are still some development phase
artefacts.
- Looking at the frontend and backend code you probably realize
that my "native" skillset is C# coding, and my JavaScript skills are 
not yet at the same level.

## Coding technologies

This section describes the various technologies used.

- The main development environment used is Visual Studio 2022.
  To open (almost) everything, open the the solution `lcl-bike-app.sln`.
  The following "workloads" and components of VS 2022 are used:
  - The standard C# support
  - "ASP.NET and web development"
  - F#
    - You may need to dig through some options or the "individual
      components" section of the VS2022 installer if you don't
      already have it.
  - "Data storage and processing"
    - This includes SQL Server LocalDb.
- Some parts of the project are not exposed in that solution, but
  were developed using VS Code as editor:
  - Documentation: this very README.md file, as well as the other
    markdown files in the `design-and-implementation-notes` folder.
  - The frontend SPA. Apologies for hiding it a bit. It lives in
    the folder `CitybikeApp/citybike-quasar/`. It therefore actually
    is exposed in Visual Studio 2022 as part of the `CitybikeApp`
    project (the backend), but development of the SPA is done in
    Visual Studio Code.
- Backend libraries are implemented in C# (`LclBikeApp.Database`,
  `LclBikeApp.DataWrangling`)
  - These in turn use another project of mine. For ease
    of use I included it here as the project `XsvLib`, but you can
    find the original at https://github.com/ttelcl/LclXsv .
- The backend and data loading utility is implemented in F# (using the
  above-mentioned C# libraries): `CitybikeUtility`.
- The database implementation currently assumes SQL Server and was
  tested against a LocalDb version and a serverless Azure SQL version.
  - Known Issue: performance of loading a remote database is not where it
    should be, but it seems I don't have enough time to fix that. The
    read performance appears to be just fine, as is any operation on a
    local DB; it is just the loading of a remote DB that has a problem.
- Web API and server side UI are implemented in C# using ASP.NET Core 6,
  see `CitybikeApp`. Currently both of those are implemented in the same
  app. The server side UI uses Razor pages.
  However, the `CitybikeApp/citybike-quasar/` subfolder is not part of the
  backend but actually is the frontend...
  - Update: In practice it is mostly a Web API server, with minimal or
    no server side UI. Reason: actually the client app seems to work
    well enough, so no need for much server side UI.
    The backend server does provide a Swagger interface for documenting
    the Web API that it implements though.
- Links about the technologies used in the backend:
  - [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core)
  - Dapper (the micro-ORM library I used): [https://github.com/DapperLib/Dapper](https://github.com/DapperLib/Dapper).
- The front-end code went through a few initial attempts and finally
  ended up with the version you can find at `CitybikeApp/citybike-quasar/`.
  Technologies used:
  - Quasar (https://quasar.dev/).
  - ... which in turn builds on top of Vue 3 (https://vuejs.org/)
  - Pinia (https://pinia.vuejs.org/) for state storage sharing
  - I use the recommended approach of Quasar CLI with VITE (as
    implication: so I am _not_ using WebPack for building).
  - The code in it is JavaScript, not TypeScript. Since I had already
    quite a few technologies to re-learn in their modern day version,
    I didn't want to further complicate it by adding _"learn TypeScript"_
    to the to-do list for this project :)
  - _Note_: When you look through the GIT timeline you can still see
    the dead ends; I didn't delete them, but left them "for educational
    purposes".
- To aid with database development (including the creation of an initial empty
  database) I used SMSS (SQL Server Management Studio), see
  https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms
- I also created an SQL Server database in Azure, but it seems I won't have
  time to put that to proper use before the project deadline.

## Development process

This section provides some information about my development process (including
links to more detailed information)

- Phase 1: [Reviewing the data files](design-and-implementation-notes/DataReview.md)
- Phase 2: [Database modeling](design-and-implementation-notes/DataModel.md)
- Phase 3: Database and database access layer library implementation
  - See the library code in folder `LclBikeApp.Database` and Unit Tests in `UnitTests.Database`.
  - Also see the library code in folder `LclBikeApp.DataWrangling` and Unit Tests in
    `UnitTests.DataWrangling`.
- Phase 4: [Database Loading](design-and-implementation-notes/DatabaseLoading.md)
  - The data loading command line app is in the folder `CitybikeUtility`. Note that
    to compile it your Visual Studio 2022 needs to have the F# support components installed
- Phase 5: [Web API design](design-and-implementation-notes/WebApiDesign.md) and implementation.
  - Of course this is an ongoing design cycle together with Phase 7.
- Phase 6: Server side UI _\[MOSTLY SKIPPED\]_
  - It seems that there isn't much need for a separate server side UI. I won't have
    time to implement a server-based data loading solution; for now the CLI developed
    in phase 4 will have to do.
  - It does contain a Swagger based Web API documentation page at {_server_}/Swagger/index.html
    (it turned out that was amazingly trivial to add ...)
- Phase 7: Client side UI. Implemented in Quasar, as mentioned above.
  - Code is in `CitybikeApp/citybike-quasar/`. Start a VS Code instance on that folder
    to edit / develop.
  - During development I use a two-server setup. The main accessor is the Quasar
    development server running at htt<span>p:/</span>/localhost:9000/ , which is set up
    to proxy certain URLs to the backend server which runs at htt<span>ps:/</span>/localhost:7185/ .
- Phase 8: make sure documentation is OK
- ~~Phase 9: Server deployment~~ _\[I am not going to have enough time for that\]_
