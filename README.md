# City bike application

> :warning: This is a work in progress. _As long as this banner is present, 
  the assignment has not been submitted for evaluation yet!_
>
> Status 2021-08-05: still progressing ... Apologies for it taking so long.

This repository will receive my submission to the City Bike pre-assignment
(deadline 2022-08-14). 
<!-- (see https://github.com/solita/dev-academy-2022-fall-exercise ) -->

In this page:
* [Instructions](#instructions)
* [Coding Technologies](#coding-technologies)
* [Development process](#development-process)

## Instructions

((Instructions for building, setting up and loading the DB, running in
dev mode, deploying, etc. will be added here later.))

## Coding technologies

This section describes the various technologies used.

* The main development environment used is Visual Studio 2022.
  To open (almost) everything, open the the solution `lcl-bike-app.sln`.
  The following "workloads" and components of VS 2022 are used:
    * The standard C# support
    * "ASP.NET and web development"
    * F#
        * You may need to dig through some options or the "individual 
          components" section of the VS2022 installer if you don't
          already have it.
    * "Data storage and processing"
        * This includes SQL Server LocalDb.
* Some parts of the project are not exposed in that solution, but
  were developed using VS Code as editor:
    * Documentation: this very README.md file, as well as the other
      markdown files in the `design-and-implementation-notes` folder.
    * The frontend SPA. Apologies for hiding it a bit. It lives in
      the folder `CitybikeApp/citybike-quasar/`. It therefore actually
      is exposed in Visual Studio 2022 as part of the `CitybikeApp`
      project (the backend), but development of the SPA is done in 
      Visual Studio Code.
* Backend libraries are implemented in C# (`LclBikeApp.Database`,
  `LclBikeApp.DataWrangling`)
  * These in turn use another project of mine. For ease
    of use I included it here as the project `XsvLib`, but you can
    find the original at https://github.com/ttelcl/LclXsv .
* The backend and data loading utility is implemented in F# (using the 
  above-mentioned C# libraries): `CitybikeUtility`.
* The database implementation currently assumes SQL Server and was
  tested against a LocalDb version and a serverless Azure SQL version.
* Web API and server side UI are implemented in C# using ASP.NET Core 6,
  see `CitybikeApp`. Currently both of those are implemented in the same
  app. The server side UI uses Razor pages.
  However, the `CitybikeApp/citybike-quasar/` subfolder is not part of the
  backend but actually is the frontend...
    * Update: In practice it is mostly a Web API server, with minimal or
      no server side UI. Reason: actually the client app seems to work
      well enough, so no need for much server side UI.
      The backend server does provide a Swagger interface for documenting
      the Web API that it implements though.
* The front-end code went through a few initial attempts and finally
  ended up with the version you can find at `CitybikeApp/citybike-quasar/`.
  Technologies used:
    * Quasar (https://quasar.dev/).
    * ... which in turn builds on top of Vue 3 (https://vuejs.org/)
    * Pinia (https://pinia.vuejs.org/) for state storage sharing
    * I use the recommended approach of Quasar CLI with VITE (as 
      implication: so I am _not_ using WebPack for building).
    * The code in it is JavaScript, not TypeScript. Since I had already
      quite a few technologies to re-learn in their modern day version,
      I didn't want to further complicate it by adding _"learn TypeScript"_
      to the to-do list for this project :)
* To aid with database development (including the creation of an initial empty
  database) I used SMSS (SQL Server Management Studio), see
  https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms
* I also created an SQL Server database in Azure, but it seems I won't have
  time to put that to proper use before the project deadline.

## Development process

This section provides some information about my development process (including
links to more detailed information)

* Phase 1: [Reviewing the data files](design-and-implementation-notes/DataReview.md)
* Phase 2: [Database modeling](design-and-implementation-notes/DataModel.md)
* Phase 3: Database and database access layer library implementation
    * See the library code in folder `LclBikeApp.Database` and Unit Tests in `UnitTests.Database`.
    * Also see the library code in folder `LclBikeApp.DataWrangling` and Unit Tests in
      `UnitTests.DataWrangling`.
* Phase 4: [Database Loading](design-and-implementation-notes/DatabaseLoading.md)
    * The data loading command line app is in the folder `CitybikeUtility`. Note that
      to compile it your Visual Studio 2022 needs to have the F# support components installed
* Phase 5: [Web API design](design-and-implementation-notes/WebApiDesign.md) and implementation.
    * Of course this is an ongoing design cycle together with Phase 7.
* Phase 6: Server side UI _\[MOSTLY SKIPPED\]_
    * It seems that there isn't much need for a separate server side UI. I won't have
      time to implement a server-based data loading solution; for now the CLI developed
      in phase 4 will have to do.
    * It does contain a Swagger based Web API documentation page at {_server_}/Swagger/index.html
      (it turned out that was amazingly trivial to add ...)
* Phase 7: Client side UI. Implemented in Quasar, as mentioned above.
    * Code is in `CitybikeApp/citybike-quasar/`. Start a VS Code instance on that folder
      to edit / develop.
    * During development I use a two-server setup. The main accessor is the Quasar 
      development server running at htt<span>p:/</span>/localhost:9000/ , which is set up
      to proxy certain URLs to the backend server which runs at htt<span>ps:/</span>/localhost:7185/ .
* Phase 8: make sure documentation is OK
* Phase 9: Server deployment _\[It seems I am not going to have enough time for that\]_
