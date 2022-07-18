module AppPrepare

open System
open System.IO

open XsvLib
open XsvLib.Tables.Cursor

open LclBikeApp.Database
open LclBikeApp.Database.ImplementationSqlServer
open LclBikeApp.Database.Models

open LclBikeApp.DataWrangling.DataLocation
open LclBikeApp.DataWrangling.RawModel
open LclBikeApp.DataWrangling.Utilities
open LclBikeApp.DataWrangling.Validation

open CommonTools
open ColorPrint

type private PrepOptions = {
  Inputs: string list
  DataFolderKey: string
  StationsData: string
}

let run args =
  let rec parsemore o args =
    match args with
    | "-v" :: rest ->
      verbose <- true
      rest |> parsemore o
    | [] ->
      if o.Inputs |> List.isEmpty then
        failwith "No input files specified"
      {o with Inputs = o.Inputs |> List.rev}
    | "-i" :: filename :: rest ->
      rest |> parsemore {o with Inputs = filename :: o.Inputs}
    | "-df" :: dfkey :: rest ->
      rest |> parsemore {o with DataFolderKey = dfkey}
    | x :: _ ->
      failwithf $"Unrecognized argument {x}"
  let o = args |> parsemore {
    Inputs = []
    DataFolderKey = "_data"
    StationsData = null
  }
  let df = DataFolder.LocateAsAncestorSibling(o.DataFolderKey)
  cp $"Data folder is \fg{df.Root}"
  let preparedFilesFolder = df.ResolveOrCreateDirectory("prepared")
  cp $"Outputs will be written to \fc{preparedFilesFolder}"
  
  let stationsFileName =
    if o.StationsData |> String.IsNullOrEmpty then
      cp "\fkNo stations data file given - trying to find one of the defaults"
      let f = df.ResolveFile("stations.csv")
      if File.Exists(f) then
        f
      else
        let f = df.ResolveFile("Helsingin_ja_Espoon_kaupunkipyöräasemat_avoin.csv")
        if File.Exists(f) then
          f
        else
          cp $"\frNo stations data file given, and none of the defaults found."
          failwith "Cannot find stations data"
    else
      o.StationsData
  cp $"Reading known station IDs from \fy{stationsFileName}"
  let stationAdapter = new StationCursor()
  let stations =
    Xsv.ReadXsvCursor(stationsFileName, stationAdapter)
    |> Seq.map RawStation.FromCursor
    |> Seq.toList
  let stationIds = stations |> List.map (fun s -> s.Id)

  let rules = new ValidationConfiguration()
  // TODO: allow customizing these rules
  cp $"Validation rules in effect: \fy{rules.ToJson()}"

  let validator = new RideValidator(rules, stationIds)

  cp $"\frWork In Progress! No output is written yet"
  for inputName in o.Inputs do
    let inputFile = df.ResolveFile(inputName)
    if inputFile |> File.Exists |> not then
      cp $"\frFile \fo{inputFile} \fr not found! \fySkipping!"
    else
      validator.Reset()
      let rideCursor = new RideCursor()
      use xr = Xsv.ReadXsv(inputFile).AsXsvReader()
      let validatedCursors = xr.ReadCursor(rideCursor) |> validator.Validate

      // TEMPORARY CODE - TO BE REPLACED
      let mutable currentDate = new DateTime(3000, 1, 1) // long in the future, as a marker
      let mutable entriesThisDay = 0
      for validCursor in validatedCursors do
        let date = validCursor.DepTime.Date
        if currentDate <> date then
          if entriesThisDay > 0 then // else: stay silent
            cp $"  \fg{currentDate:``yyyy-MM-dd``}\f0: \fb%6d{entriesThisDay}\f0 valid rides"
            entriesThisDay <- 0
          currentDate <- date
          // Todo: start a new output file
        entriesThisDay <- entriesThisDay + 1
        // Todo: copy current record to current file
        ()
      // Don't forget the final day of the file!
      if entriesThisDay > 0 then // else: stay silent
        cp $"  \fg{currentDate:``yyyy-MM-dd``}\f0: \fb%6d{entriesThisDay}\f0 valid rides"
        entriesThisDay <- 0
      
      cp $"\fcRejection and acceptance statistics\f0:"
      for kvp in validator.Statistics do
        if kvp.Key = "ACCEPTED" then
          cp $"\fG%6d{kvp.Value}\f0 : \fg{kvp.Key}"
        else
          cp $"\fo%6d{kvp.Value}\f0 : \fy{kvp.Key}"
      ()
  
  failwith "Not Yet Implemented"
  0
