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

type private InitRideOptions = {
  Inputs: string list
  DataFolderKey: string
  StationsData: string
  StationsFromDb: bool
  StartTime: DateTime  // note: this is a Time, not just a date
  EndTime: DateTime  // note: this is a Time, not just a date
  DbTag: string
  DoInsert: bool
}

type private InitStationOptions = {
  DataFolderKey: string
  StationsData: string
  DbTag: string
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

let runInitRides args =
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
    | "-db" :: dbtag :: rest ->
      rest |> parsemore {o with DbTag = dbtag}
    | "-s" :: stationfile :: rest ->
      if o.StationsFromDb then
        failwith "-s and -S are mutually exclusive"
      rest |> parsemore {o with StationsData = stationfile}
    | "-S" :: rest ->
      if o.StationsData |> String.IsNullOrEmpty |> not then
        failwith "-S and -s are mutually exclusive"
      rest |> parsemore {o with StationsFromDb = true}
    | "-insert" :: rest ->
      if o.StationsData |> String.IsNullOrEmpty |> not then
        failwith "-insert and -s are mutually exclusive (-insert implies -S)"
      rest |> parsemore {o with StationsFromDb = true; DoInsert = true}
    | x :: _ ->
      failwithf $"Unrecognized argument {x}"
  let o = args |> parsemore {
    Inputs = []
    DataFolderKey = "_data"
    StationsData = null
    StationsFromDb = false
    StartTime = DateTime.MinValue
    EndTime = DateTime.MaxValue
    DbTag = "default"
    DoInsert = false
  }
  let df = DataFolder.LocateAsAncestorSibling(o.DataFolderKey)
  cp $"Data folder is \fg{df.Root}"
  
  let lazyDbString = lazy (
      Config.getConnectionString(o.DbTag)
    )

  /// Connect to the database. Make sure to "use" (not "let") the return value
  let openDb () =
    let connstring = lazyDbString.Force()
    new CitybikeDbSqlServer(connstring)

  let stationIds =
    if o.StationsFromDb then
      cp $"Loading known station IDs from database '\fo{o.DbTag}\f0'"
      use db = openDb()
      let ncreate = db.InitDb()
      if ncreate > 0 then
        cp $"\frInitialized or updated database!\f0 \fb{ncreate}\f0 DB objects created"
      else
        cp $"Existing database contacted succesfully"
      db.GetStationIds()
      |> Seq.toList
    else
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
      cp $"Reading known station IDs from \fg{stationsFileName}"
      let stationAdapter = new StationCursor()
      let stations =
        Xsv.ReadXsvCursor(stationsFileName, stationAdapter)
        |> Seq.map RawStation.FromCursor
        |> Seq.toList
      let stationIds = stations |> List.map (fun s -> s.Id)
      stationIds
  
  if stationIds |> List.isEmpty then
    cp $"\frNo Citybike Station information found! \foAborting!"
    cp $"(Please initialize the station data first with the 'init-stations' command)"
    failwith "No station data found"
  let stationcount = stationIds |> List.length
  if stationcount < 400 then
    cp $"Found \fr{stationcount}\f0 station IDs. \frThat looks suspiciously low."
  else
    cp $"Found \fb{stationcount}\f0 station IDs"

  let rules = Config.getValidationParameters()
  cp $"Validation rules in effect: \fy{rules.ToJson()}"

  let validator = new RideValidator(rules, stationIds)

  if o.DoInsert then
    cp "Running in \foinsertion\f0 mode, validating and inserting rides."
  else
    cp "Running in \fbdry-run\f0 mode, validating but not inserting rides (pass '-insert' to change)"
  
  let mutable totalInserted = 0

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

      // Turn the cursors (views on the CSV data) into actual in-memory records
      let ridesSequence =
        validatedCursors
        |> Seq.map (fun c -> RideBase.FromCursor(c))

      let batcher = new SequenceBatcher<RideBase, DateTime>(fun rb -> rb.DepTime.Date)

      let rideBatchesPerDay =
        ridesSequence
        |> batcher.BatchAll

      for dayRides in rideBatchesPerDay do
        let day = dayRides[0].DepTime.Date
        cpx $"  Found \fb%6d{dayRides.Count}\f0 valid rides on \fc{day:``yyyy-MM-dd``}\f0. "
        if o.DoInsert then
          let count =
            use db = openDb()
            db.AddBaseRides(dayRides)
          let colortag =
            if count = dayRides.Count then
              "\fg"
            elif count = 0 then
              "\fr"
            else
              "\fy"
          cp $" Actually inserted: {colortag}{count}"
          totalInserted <- totalInserted + count
        else
          cp ""
      
      cp ""
      cp $"\fcRejection and acceptance statistics\f0:"
      for kvp in validator.Statistics do
        if kvp.Key = "ACCEPTED" then
          cp $"\fG%6d{kvp.Value}\f0 : \fg{kvp.Key}"
        else
          cp $"\fo%6d{kvp.Value}\f0 : \fy{kvp.Key}"
      ()
  
  cp ""
  if o.DoInsert then
    cp $"\foInsertion\f0 mode: inserted \fg{totalInserted}\f0 ride records"
  else
    cp "\fbDry-run\f0 mode: no data was inserted in the DB (pass '-insert' to change)"
  

  0


let runInitStations args =
  let rec parsemore (o: InitStationOptions) args =
    match args with
    | "-v" :: rest ->
      verbose <- true
      rest |> parsemore o
    | [] ->
      o
    | "-df" :: dfkey :: rest ->
      rest |> parsemore {o with DataFolderKey = dfkey}
    | "-db" :: dbtag :: rest ->
      rest |> parsemore {o with DbTag = dbtag}
    | "-s" :: stationfile :: rest ->
      rest |> parsemore {o with StationsData = stationfile}
    | x :: _ ->
      failwithf $"Unrecognized argument {x}"
  let o = args |> parsemore {
    DataFolderKey = "_data"
    StationsData = null
    DbTag = "default"
  }
  let df = DataFolder.LocateAsAncestorSibling(o.DataFolderKey)
  cp $"Data folder is \fg{df.Root}"
  
  let lazyDbString = lazy (
      Config.getConnectionString(o.DbTag)
    )

  /// Connect to the database. Make sure to "use" (not "let") the return value
  let openDb () =
    let connstring = lazyDbString.Force()
    new CitybikeDbSqlServer(connstring)

  let stations =
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
    cp $"Reading known station info from \fg{stationsFileName}"
    Xsv.ReadXsvCursor(stationsFileName, new StationCursor())
    |> Seq.map Station.TryFromCursor
    |> Seq.where (fun s -> s <> null)
    |> Seq.toList

  if stations |> List.isEmpty then
    failwith "No station data found in data file!"

  let count =
    use db = openDb()
    let ncreate = db.InitDb()
    if ncreate > 0 then
      cp $"\frInitialized or updated database!\f0 \fb{ncreate}\f0 DB objects created"
    else
      cp $"Existing database contacted succesfully"
    let knownIds = db.GetStationIds() |> Set.ofSeq
    cp $"\fgBefore insertion\f0: there are \fb{knownIds.Count}\f0 stations in the DB"
    let newstations =
      stations
      |> Seq.where (fun s -> knownIds |> Set.contains s.Id |> not)
      |> Seq.toList
    if newstations |> List.isEmpty then
      cp $"\foAll loaded stations are already present in the DB\fy Not uploading anything!"
      0
    else
      cp $"Inserting \fg{newstations |> List.length}\f0 new stations"
      let n = db.AddStations(newstations)
      cp $"Successfully inserted \fb{n}\f0 stations"
      n
  
  0
