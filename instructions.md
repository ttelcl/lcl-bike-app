# Build and Setup instructions

## Contents

- [Required and recommended tools](#required-and-recommended-tools)
- [Empty database creation](#empty-database-creation)
- [Before you build](#before-you-build)
- [Putting the data files in place](#putting-the-data-files-in-place)
- [Load data into the database (and validate data)](#load-data-into-the-database-and-validate-data)
- [Test run of the backend](#test-run-of-the-backend)
- [Intermezzo](#intermezzo)
- [Running the front-end server](#running-the-front-end-server)


## Required and recommended tools

To build this application you will need all of the following tools:

- `Visual Studio 2022`.
  - Required for the backend.
  - Make sure the following "workloads" are installed:
    - "ASP.NET and web development".
    - "Data storage and processing". Note that this includes
    `SQL Server LocalDb`.
  - Also make sure F# support is installed. You may need to browse
  the available workloads in the Visual Studio installer, or check the
  "individual components" section.
  - Note that this project targets .NET Core 6, so Visual Studio 2019
  is not enough.
- `Visual Studio Code`.
  - Technically speaking not "required", but highly recommended as 
  editor for the frontend development.
- An SQL Server database.
  - For demo purposes, `SQL Server LocalDb` that comes with VS 2022 (as mentioned above) works well enough.
- `SQL Server Management Studio` (SMSS), or any other tool for managing
SQL Server databases.
  - You can 
  [download it from microsoft](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms).
  - For the record, I used version "v18.12.1".
  - Note! Reports are that you can create the empty databases in VS2022
  as well as an alternative to using SMSS. I haven't used that
  functionality in VS2022 myself though.
- `Node.js` ([nodejs.org](https://nodejs.org/)). I used version "v16.15.1" (to check your
version, use "node --version")
- `Quasar CLI` ([quasar.dev](https.//quasar.dev/)). Once you have node.js
installed, you can install Quasar CLI (globally)
with "npm i -g @quasar/cli". After installation the "quasar" commandline
tool is available; you can use "quasar --version" to check its version.
For the record: I am using version 1.0.5.

## Empty database creation

The system assumes availability of **two** independent databases to the
development environment. One is only used for Unit Testing (and will
be small), the other is the "real" database (at least for as far as
frontend and backend development is concerned). Note that some of the
unit tests wipe the entire database, so you definitely want to keep 
them separate!

I assume you will use SQL Server LocalDb to host these databases on your
development machine.

Follow these steps to create them using SMSS (you can also create them
in VS2022, I'll leave that as an exercise for the reader ...):
- Start `SQL Server Management Studio`
- Connect to the server with the magical name `(LocalDB)\MSSQLLocalDB`.
- Create two databases:
  - Right click on the 'folder' named "Database" below the server node,
  and click "New Database..." in the menu).
  - Use the name "bicycle01" for the main database, and "biketests" for
  the unit test one. The names actually don't matter much - feel free to
  choose whatever name you like.
  - Before you hit "OK" in the "New Database" dialog, you may want to
  review the "Path" for the two database files. You can't change that
  later.
  - **Pay attention to the "Connection String" that is displayed, and
  copy it for later use**. Recovering it later is a bit clumsy...
    - If you need to recover the connection string later, don't be
    surprised if the result looks different than the original. 
- There is no need to create any tables yet; the loader app and unit
tests can do so later.


## Before you build

Ater you clone the GitHub repository, there are some settings to be
applied before you attempt to build the code.

- Backend library file installation:
  - Start Visual Studio 2022 and open the solution file 
  `lcl-bike-app.sln`.
  - In the Solution Explorer pane navigate to the `libman.json` file in
  the `CitybikeApp` project (under the `HttpApps` solution folder).
  - Right-click that file and click "Restore Client-Side Libraries" in the
  pop-up menu. This will download the (big) JavaScript and CSS library
  files that I omitted from the repository.
- Frontend library installation:
  - Start a command prompt in the folder `CitybikeApp\citybike-quasar\`.
  - Type "npm install". This will install the JavaScript libraries for
  the frontend.
- Configure database connection strings in the projects that use them.
There are 4 projects in the solution that need access to one of the
databases. The easiest way to configure their connection strings without
putting those connecion string in a location that may accidentally get
checked in into GIT is to use the
"[user secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets#secret-manager)"
system in .NET Core, which is also directly supported in the VS2022 UI.
  - The general process is to right-click on the project for which 
  secrets need to be configured and select "Manage User Secrets", which
  opens your user secrets json file for the project (initially just
  an empty object). Edit it and save it; the .NET Core settings pipeline
  will then inject those settings in the settings onject during
  application setup.
  - For the two Unit Test projects (`UnitTests.Database` and 
  `UnitTests.DataWrangling`), the content should be like below, with
  `<<<your-connection-string-for-biketests>>>` replaced by your
  biketests' connection string.
    - The "_comments" field isn't actually
      used, it is just for your convenience, to help keeping multiple
      `secrets.json` files apart (if JSON would have supported
      comments it would have been a comment instead).

    ```json
    {
      "_comments": [
        "User secrets for the Unit Test projects"
      ],
      "TestDb": {
        "ConnectionString": "<<<your-connection-string-for-biketests>>>"
      }
    }
    ```

  - For the loader and backend app projects (`CitybikeUtility` and 
  `CitybikeApp`), the content should be like this, with
  `<<<your-connection-string-for-bicycle01>>>` replaced by your
  bicycle01's connection string.

    ```json
    {
      "_comments": [
        "User secrets for the CitybikeUtility and CitybikeApp projects"
      ],
      "ConnectionStrings": {
        "default": "<<<your-connection-string-for-bicycle01>>>"
      }
    }
    ```
    - If you wish you can add additional connection strings to the
    `CitybikeUtility` configuration; you can select which db to use
    via a commandline parameter ("default" is the default if you
    pass none).

If all is well, you can now build the solution without errors, and
you can use the CitybikeUtility application to load data into your
main database (see below)

## Putting the data files in place

To make it easy for the database loading utility to find the data
files, put the data files in a folder that adheres to a special naming
convention.

- Create a folder named "_data" in the same folder where you cloned
the lcl-bike-app repository (so "_data" and "lcl-bike-app" are children
of the same parent folder, but "_data" is not _inside_ the repository).
Alternatively, create that "_data" folder in any of the parent folders
of that same folder.
  - "CitybikeUtility.exe" will look for a "_data" folder in any of
  the parent folders from the folder where you run it. 
- Download the ride data files and the stations data file into that
"_data" folder. The ride data files are expected to be named
"2021-05.csv", "2021-06.csv", and "2021-07.csv". The station data file
can either be named "stations.csv" or
"Helsingin_ja_Espoon_kaupunkipyöräasemat_avoin.csv".

## Load data into the database (and validate data)

To validate data and put accepted data into your main database use
the CitybikeUtility program.

- Start a command prompt in the folder where the CitybikeUtility was
built (`CitybikeUtility\bin\Debug\net6.0`).
- Type `citybikeutility` without arguments to get its usage message.
- You can take a look at the effective configuration by typing
  ```bash
  citybikeutility config
  ```
  The effective configuration is created by
  merging the "configuration.json" file with the user secrets you set
  before for the CitybikeUtility project.
- First install the stations list using the command
  ```bash
  citybikeutility init-stations
  ```
  - Stations must be uploaded first because rides involving unknown
  stations are rejected by ride validation.
- Upload rides data
  - Ride data upload includes validation against the parameters set
  in the configuration.
    - Statistics on validation are printed after the upload.
    - Unless the data files have changed since I downloaded them, the validation rule
    _"Departure timestamp is not non-descending (duplicate data
    assumed)"_ will trigger for 50% of the data. That is expected,
    since all data rows are duplicated in the data files ...
  - Uploads are batched on a day-by-day basis
  - If you want to upload all ride data for one month in one go you
  can use (using the May data as example here)
    ```bash
    citybikeutility init-rides -insert -i 2021-5.csv 
    ```
    - ALERT! There is a known issue when the database is not local:
      ride data is very slow in that case, and you probably want
      to use the `-from` and `-to` parameters to constrain data
      upload to a small part of the data file, for instance a single
      day.
    - If you feel for it you can also pass multiple `-i` options, 
    to upload multiple months' data.

## Test run of the backend

You can run the backend in the following ways:
- From VS2022:
  - Right-click the `CitybikeApp` and select "Set as Startup Project"
  (only needs to be done once). Then click the green arrow "Run" button
  in the toolbar to start the app running in the Kestrel server. You
  can configure additional run options from the drop-down sub-button on
  that button if you wish, such as selecting which browser to open,
  or not to open any browser at all.
  - Alternatively, just right-click the `CitybikeApp` and select
  _"Debug | Start New Instance"_ from the menu.
- From the command prompt:
  - Start a command prompt in the directory `CitybikeApp` (_not_
    in the build output directory!), and type the following command:
    ```bash
    dotnet run
    ```
    - Note: This may require additional .net core sdk tools to
    work...
<!-- Reminder: use the <span> trick below to prevent GitHub from
creating a clickable link from it automagically. -->
- If no browser automatically opened, open the link
_http<span>s:/</span>/localhost:7185/home/_ in a browser.
  - Upon first run you probably need to allow the self-signed
  SSL certificate
- If all is well that should show an introduction page about the
backend. 
- You can navigate to the Swagger page from here for the Web API
docs and test tool.
- Navigating to the (front-end) home page 
(_http<span>s:/</span>/localhost:7185/_) will greet you with a
"page not found" error, because there is no front-end embedded
in non-deployed mode.

## Intermezzo

That behaviour of the backend showing a "page not found" error is
by design. To run both frontend and backend in this development
setup, you also need to run the (separate) frontend server
(discussed below). Both servers run in _tandem_, with the
front-end server in the lead: the front-end will proxy calls
intended for the backend to the backend server, making the browser
connected to the front-end server think that both servers are one.

Note that this tandem setup is only for the development setup.
In a deployed scenario, a hook is activated in the backend that
serves the (passive!) front-end files at its root.
Or that is at least the theory; I got that deployed scenario
partially to work, but not yet completely.

## Running the front-end server

- First, make sure the backend server is still running.
- To run the front end (development) server, start another command
  prompt, this time in the directory `CitybikeApp\citybike-quasar` and
  type the following command
  ```bash
  quasar dev
  ```
- This will start the quasar development server, running at
_http<span>:/</span>/localhost:9000/_. Sorry, that is indeed "http",
not "https"; I didn't get the HTTPS support working for the quasar
server.
- Browsing to that URL shows the front-end home page. That home
page is deliberately is kept light-weight, and not depending on the
backend nor the database.
- From there you can navigate to the "Citybike Stations" page or the
"Rides Browser" page. Both of those happen to have links to various
instances of links to the "single station" page.
- On the left side bar of the main layout you can also find links
to the "City" page (a leftover from early design) and the
"Development Extras" page with some debugging tools and a link
to the backend home page and swagger page.
- If you want to play around with the front-end code, I recommend
starting VS Code in the frontend root folder
(`CitybikeApp\citybike-quasar`). The Quasar dev server picks up
changes automatically when you save them, making for a quick and
effective edit cycle.

