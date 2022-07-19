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
open System.Globalization

type private PrepOptions = {
  Inputs: string list
  DataFolderKey: string
  StationsData: string
  StartTime: DateTime  // note: this is a Time, not just a date
  EndTime: DateTime  // note: this is a Time, not just a date
}

let private parseTime isend (txt: string) =
  let culture = CultureInfo.InvariantCulture
  let ok, t = DateTime.TryParseExact(
    txt,
    "yyyy-MM-dd",
    culture,
    DateTimeStyles.None)
  if ok then
    if isend then
      t.AddDays(1.0).AddSeconds(-1.0)
    else
      t
  else
    let ok, t = DateTime.TryParseExact(
      txt,
      [|
        "yyyy-MM-dd HH:mm:ss"
        "yyyy-MM-dd'T'HH:mm:ss"
        |],
      culture,
      DateTimeStyles.None)
    if ok then
      t
    else
      cp $"\frUnsupported or unrecognized time format in \fo{txt}"
      failwith "Unrecognized time format"

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
    | "-from" :: date :: time :: rest when not(time.StartsWith("-")) ->
      let t = $"{date} {time}" |> parseTime false
      rest |> parsemore {o with StartTime = t}
    | "-from" :: datetime :: rest ->
      let t = datetime |> parseTime false
      rest |> parsemore {o with StartTime = t}
    | "-to" :: date :: time :: rest when not(time.StartsWith("-")) ->
      let t = $"{date} {time}" |> parseTime true
      rest |> parsemore {o with EndTime = t}
    | "-to" :: datetime :: rest ->
      let t = datetime |> parseTime true
      rest |> parsemore {o with EndTime = t}
    | x :: _ ->
      failwithf $"Unrecognized argument {x}"
  let o = args |> parsemore {
    Inputs = []
    DataFolderKey = "_data"
    StationsData = null
    StartTime = DateTime.MinValue
    EndTime = DateTime.MaxValue
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

  let rules = Config.getValidationParameters()
  cp $"Validation rules in effect: \fy{rules.ToJson()}"

  let validator = new RideValidator(rules, stationIds)

  for inputName in o.Inputs do
    let inputFile = df.ResolveFile(inputName)
    if inputFile |> File.Exists |> not then
      cp $"\frFile \fo{inputFile} \fr not found! \fySkipping!"
    else
      cp $"Processing \fg{inputFile}\f0 from \fy{o.StartTime:s}\f0 to \fy{o.EndTime:s}"
      validator.Reset()
      let rideCursor = new RideCursor()
      use xr = Xsv.ReadXsv(inputFile).AsXsvReader()
      let validatedCursors = 
        xr.ReadCursor(rideCursor)
        |> Seq.where (fun c -> c.DepTime >= o.StartTime && c.DepTime <= o.EndTime)
        |> validator.Validate

      let ridesSequence =
        validatedCursors
        |> Seq.map (fun c -> RideBase.FromCursor(c))

      let batcher = new SequenceBatcher<RideBase, DateTime>(fun rb -> rb.DepTime.Date)

      let rideBatchesPerDay =
        ridesSequence
        |> batcher.BatchAll

      for dayRides in rideBatchesPerDay do
        let day = dayRides[0].DepTime.Date
        cp $"  Found \fb%6d{dayRides.Count}\f0 valid rides on \fg{day:``yyyy-MM-dd``}\f0"
        // TODO: actually do something with those rides ...
      
      cp $"\fcRejection and acceptance statistics\f0:"
      for kvp in validator.Statistics do
        if kvp.Key = "ACCEPTED" then
          cp $"\fG%6d{kvp.Value}\f0 : \fg{kvp.Key}"
        else
          cp $"\fo%6d{kvp.Value}\f0 : \fy{kvp.Key}"
      ()
  
  cp $"\frWork In Progress! No output is written yet"
  failwith "Not Yet Implemented"
  0
