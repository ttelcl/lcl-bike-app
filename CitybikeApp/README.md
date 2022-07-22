# CitybikeApp

This project implements the web server for the citybike app, which
provides:

* A server based user interface
* A REST Web API for use by client based UIs
* [TBD] acts as host for a client-side user interface

It is implemented in C# using ASP.NET Core 6 and uses the library
LclBikeApp.Database that abstracts the database interface. It is
configured to use the SQL Server based implementation, and to run
you will need to provide the connection string to your database
in the configuration file (as value for "default" in the
"ConnectionStrings" section in appsettings.json)
For development, consider using Visual Studio's "user secrets"
feature instead, so your connection string doesn't accidentally
end up in GIT (values in the user secrets file override values
in appsettings.json)

As of writing this README file, this web app treats the database
mostly as read-only. In other words: it assumes you already filled
it with data, for instance using the "CitybikeUtility" command
line app.
