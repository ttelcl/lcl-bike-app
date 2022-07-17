-- This script deletes all tables and data (if they exist) and
-- then recreates the tables.
-- This requires SQL Server 2016 or newer

USE [bicycle01];
GO

DROP TABLE IF EXISTS [dbo].[Rides];
GO

DROP TABLE IF EXISTS [dbo].[Stations];
GO

DROP TABLE IF EXISTS [dbo].[Cities];
GO

-- A tiny lookup table containing the names of the cities involved
CREATE TABLE [dbo].[Cities]
(
    [Id]     INT          NOT NULL PRIMARY KEY, 
    [CityFi] NVARCHAR(20) NOT NULL, 
    [CitySe] NVARCHAR(20) NOT NULL
);
GO

INSERT INTO Cities (Id, CityFi, CitySe) VALUES (0, 'Helsinki', 'Helsingfors');
INSERT INTO Cities (Id, CityFi, CitySe) VALUES (1, 'Espoo', 'Esbo');
GO

-- Station information. Compared to the original CSV files:
--  - Operator is not stored
--  - The city names are replaced with a reference to the Cities table
--    - It is assumed that a blank city name (ID 0) is always Helsinki
--  - The multilingual field names have been renamed for consistency
--  - The geocoordinate columns are now named Latitude and Longitude
--    instead of "y" and "x". Also note their orded reversal.
CREATE TABLE [dbo].[Stations]
(
    [Id]        INT          NOT NULL PRIMARY KEY, 
    [NameFi]    NVARCHAR(48) NOT NULL, 
    [NameSe]    NVARCHAR(48) NOT NULL, 
    [NameEn]    NVARCHAR(48) NOT NULL, 
    [AddrFi]    NVARCHAR(48) NOT NULL, 
    [AddrSe]    NVARCHAR(48) NOT NULL, 
    [City]      INT          NOT NULL FOREIGN KEY REFERENCES Cities(Id), 
    [Capacity]  INT          NOT NULL, 
    [Latitude]  FLOAT        NOT NULL, 
    [Longitude] FLOAT        NOT NULL
);
GO

-- The actual ride data
--  - Station names are omitted. Get them via the referenced row in
--    the Stations table.
--  - Distance is typed as integer. In the ride data it very, very
--    rarely was a non-integer floating point value.
--  - The constraint prevents entering exact duplicates
--  - The IGNORE_DUP_KEY modifier allows attempting to insert a
--    duplicate without causing an error (it is just ignored)
--    Other databases often have an "INSERT OR IGNORE" command
--    to do so more elegantly, but not SQL Server.
--  - The Id is not truly used, but it may be a good idea to have
--    some primary key.
CREATE TABLE [dbo].[Rides]
(
    [Id]         uniqueidentifier DEFAULT NEWSEQUENTIALID() PRIMARY KEY, 
    [DepTime]    DATETIME NOT NULL, 
    [RetTime]    DATETIME NOT NULL, 
    [DepStation] INT      NOT NULL FOREIGN KEY REFERENCES Stations(Id),
    [RetStation] INT      NOT NULL FOREIGN KEY REFERENCES Stations(Id), 
    [Distance]   INT      NOT NULL, 
    [Duration]   INT      NOT NULL,
    CONSTRAINT UC_All UNIQUE (DepTime, RetTime, DepStation, RetStation, Distance, Duration)
        WITH (IGNORE_DUP_KEY = ON)
);
GO

-- Index for locating destination stations for a given departure station
CREATE INDEX ByDepRetStation
ON [dbo].[Rides] (DepStation, RetStation, DepTime);
GO

-- Index for locating source stations for a given destination station
CREATE INDEX ByRetDepStation
ON [dbo].[Rides] (RetStation, DepStation, RetTime);
GO

