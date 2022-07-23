# City bike application

> :warning: This is a work in progress. _As long as this banner is present, 
the assignment has not been submitted for evaluation yet!_

This repository will receive my submission to the City Bike pre-assignment
(deadline 2022-08-14). 
<!-- (see https://github.com/solita/dev-academy-2022-fall-exercise ) -->

## Coding technologies

* The main development environment used is Visual Studio 2022
(to open everything, open the the solution `lcl-bike-app.sln`).
The following "workloads" and components of VS 2022 are used:
    * The standard C# support
    * "ASP.NET and web development"
    * F#
    * "Data storage and processing"
* Some parts of the project are not exposed in that solution, but
were developed using VS Code as editor. For instance: this very
README.md file, as well as the other markdown files in the
`design-and-implementation-notes` folder.
* Backend libraries are implemented in C#
* The backend and data loading utility is implemented in F# (using the 
above-mentioned C# libraries)
* The database implementation currently assumes SQL Server and was
tested against a LocalDb version and a serverless Azure SQL version.
* Web API and server side UI are implemented in C# using ASP.NET Core 6.
Currently both of those are implemented in the same app. The server side
UI uses Razor pages.
* The front-end ... is not yet implemented; my intention is to
use Vue.JS

## Development process

This section provides some information about my development process (including
links to more detailed information)

* Phase 1: [Reviewing the data files](design-and-implementation-notes/DataReview.md)
* Phase 2: [Database modeling](design-and-implementation-notes/DataModel.md)
* Phase 3: Database and database access layer library implementation
    * See the library code in folder `LclBikeApp.Database` and Unit Tests in `UnitTests.DataWrangling`.
    * Also see the library code in folder `LclBikeApp.DataWrangling` and Unit Tests in
    `UnitTests.DataWrangling`.
* Phase 4: Database loading. See [Database Loading](design-and-implementation-notes/DatabaseLoading.md)
    * The data loading command line app is in the folder `CitybikeUtility`. Note that
    to compile it your Visual Studio 2022 needs to have the F# support components installed
* Phase 5: [Web API design](design-and-implementation-notes/WebApiDesign.md) and implementation
 \[WIP\]
* Phase 6: Server side UI \[NYI\]
* Phase 7: Client side UI \[NYI\]
* ...
