// (c) 2022  ttelcl / ttelcl

open System

open CommonTools
open ExceptionTool
open ColorPrint
open Usage

let rec run arglist =
  // For subcommand based apps, split based on subcommand here
  match arglist with
  | "-v" :: rest ->
    verbose <- true
    rest |> run
  | "--help" :: _
  | "-h" :: _
  | [] ->
    usage verbose
    0  // program return status code to the operating system; 0 == "OK"
  | "init-rides" :: rest ->
    rest |> AppDbLoad.runInitRides
  | "init-stations" :: rest ->
    rest |> AppDbLoad.runInitStations
  | "config" :: rest ->
    rest |> AppConfig.run
  | x :: _ ->
    cp $"\foUnrecognized command: \fr{x}"
    1

[<EntryPoint>]
let main args =
  try
    args |> Array.toList |> run
  with
  | ex ->
    ex |> fancyExceptionPrint verbose
    resetColor ()
    1



