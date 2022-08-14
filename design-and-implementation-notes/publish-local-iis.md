# Publishing to an IIS web server (local)

Publishing the app to a local IIS web server and using a local
database connected to it works, but requires quite a bit of
installation and configuration. This page contains some notes
on what I had to do to get it working on my two test computers
(desktop and laptop). "This works for everyone" instructions
are probably impossible to give, so consider these hints, not
definite instructions.

- Use _SQL Server Express_ as database (not _SQL Server LocalDb_).
  It can be downloaded from
  [https://www.microsoft.com/en-us/download/details.aspx?id=101064](https://www.microsoft.com/en-us/download/details.aspx?id=101064).
- Use SMSS again to connect to it and create a new empty database
  in it. I named mine "bicycle02", so the Connection String
  will now look something like
  ```
  Server=.\SQLEXPRESS;Database=bicycle02;Trusted_Connection=True
  ```
- Fill the database using CitybikeUtility, just like before. You
  will need to tell CitybikeUtility about the mew database's 
  connection string first of course. If you use any other name 
  than "default", don't forget to pass the "-db &lt;name&gt;"
  option to pick the right database.
- Set up IIS Express to use as web server. Download it from
  [https://www.microsoft.com/en-us/download/details.aspx?id=48264](https://www.microsoft.com/en-us/download/details.aspx?id=48264)
- If you never ran any ASP.NET Core 6 app in IIS before, make sure
  to install the
  [.NET Core Hosting Bundle](https://dotnet.microsoft.com/permalink/dotnetcore-current-windows-runtime-bundle-installer).
  (I took that link from
  [https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/))
- In Visual Studio 2022, right-click the project node for
  `CitybikeApp` and select "Publish". Publish the app to a folder
  on your computer. I used "C:\inetpub\bikeapp". The folder must
  be readable by IIS.
  - I set up the project such that "publishing" also builds the
    SPA and includes it in the publish result.
  - We'll fix the DB connection string later. Currently it is not
    emitted correctly "out of the box"
- In the IIS manager there are some decisions to be made and
  configurations set according to those.
  - You can run the app in an existing "Application Pool" or create a
    new one.
    - .NET Core does not rely on the old .NET Framework, so the
        application pool does not need to support any version of .NET.
        A bit confusing, but I noticed it is even more confusing if you
        choose an app pool that does support .NET, because the IIS
        manager then shows a lot of site options that actually don't
        affect the .NET Core 6 app at all ...
  - For demo purposes, assume I named my application pool "Bike Pool".
  - Take note of the name of the application pool, you will need it
    in a moment when enabling IIS to access the database.
- In the IIS manager right-click the "Sites" node and select
  "Add Website".
  - Give the site a sensible name ("bikeapp"), choose the application
    pool that you picked or created above.
  - For "Physical path" pick the folder where you just "published"
    the app.
  - Pick a free port number for your app. I picked 8088. We will
    initially serve the app using HTTP (and not yet HTTPS).
  - If you leave "Host name" blank, the app will be served on all
    valid host names for your computer (127.0.0.1, localhost, etc.)
- You could test if you can see the site now on
  htt<span>p:/</span>/localhost:8088/ .
  - It should show the SPA front page correctly, but you'll get errors
    when you do anything that makes the server try to use the database.
- There are actually two issues for the web server - database
  connection.
  - First, the app doesn't have the right connection string yet.
  - Second: the web server doesn't have the access rights to contact
    the database
- To get the connection string into the app, there are actually 
  a handful of methods. I'll name two:
  - Edit the file named `appsettings.json` in the deployment folder.
    its initial content will be the same as the file with the same
    name in the CitybikeApp source folder. Note that the original
    adds a connection string named "default", but gives it an empty
    string as value. Simply fill in the correct connection string
    there, and restart the web site.
  - Another way is to set an environment variable with the name
    `ConnectionStrings:default` (and the connection string as value).
    How to do that is a bit hidden in IIS.
    - In IIS Manager select the node for your bikeapp site.
    - Then double-click the Icon labeled "Configuration Editor".
    - In the "Section" box at the top of the editor select
      `system.webServer/aspNetCore`.
    - Select the row `environmentVariables` in the property table.
    - Click the "..." at the end of that row to get the "collection
      editor" dialog box.
    - Add the environment variable with the name and value mentioned
      above.
    - Close the dialog box.
    - Important! Now first click "Apply" in the "actions" column
      (the right column of IIS manager)
    - Restart the site.
- To make the web server - database connection work, we need to add
  a "user" for the application pool you used in SQL server.
  - In SMSS, open the node named "Security" that is at the same level
    as "Databases" (so: not the "Security" node inside your database).
  - From there right-click the "Logins" node and select "New Login".
  - Name the new user `IIS APPPOOL\Bike Pool` (if your application
    pool was named "Bike Pool" - otherwise replace with the name you
    choose).
  - While still in the "Login - New" dialog, select the "User Mapping"
    page, and add a mapping for the database ("bicycle02") for the
    new user, and fill "dbo" for the "Default Schema" column.
    - At the bottom of that page, check the following
      "role memberships": `db_datareader`, `db_datawriter`,
      `db_ddladmin` and `public`.
    - Click OK to create the user.
- Now the site, including the database-accessing parts, should be
  working.











