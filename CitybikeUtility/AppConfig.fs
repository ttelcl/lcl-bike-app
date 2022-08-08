module AppConfig

open System
open System.IO

open Microsoft.Extensions.Configuration

open CommonTools
open ColorPrint
open Config

let run args =
  // We ignore "args" here, no need to parse them
  cp "Loading and merging configuration files..."
  let cfg = getConfig().Root
  cp "Found the following configuration settings:"
  let entries = cfg.AsEnumerable() |> Seq.toList |> List.rev
  for kvp in entries do
    let key = kvp.Key.Replace(":","\fw:\fg")
    if kvp.Value <> null then
      cp $"\fG{key}\f0 = \f0{kvp.Value}\f0"
    else
      cp $"\fG{key}\f0 \fb[Section]\f0"
  
  //let vp = getValidationParameters()
  //cp $"Validation parameters: \fy{vp.ToJson()}"
  0

