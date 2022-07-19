module Config

open System
open System.IO
open System.Text.RegularExpressions

open Microsoft.Extensions.Configuration

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

// Why a type instead of just exposing the content?
// Because we need *some* public type in this assembly to
// make AddUserSecrets work, and we have no others!
type DbConfig = {
  Root: IConfigurationRoot 
}

let private lazyConfig =
  lazy (
    let root =
      (new ConfigurationBuilder())
        .AddJsonFile("configuration.json")
        .AddUserSecrets<DbConfig>()
        .Build()
    {
      Root = root
    }
  )

let getConfig () = lazyConfig.Force()

/// Get a connection string from the public or secret config
/// Pass "default" as argument for the default connection
let getConnectionString dbKey =
  if Regex.IsMatch(dbKey, @"^[a-zA-Z][a-zA-Z0-9]*$") |> not then
    failwithf $"Expecting a identifier-like name but got '{dbKey}'"
  let cfg = getConfig().Root
  let cs = cfg.GetConnectionString(dbKey)
  if cs |> String.IsNullOrEmpty then
    cp "\frConnection string '\fo{dbKey}\f0' not found in the configuration"
    failwith "Configuration error"
  cs
  
/// Create a new ValidationConfiguration and override it based on config values
let getValidationParameters () =
  let vp = new ValidationConfiguration()
  let vpsection = getConfig().Root.GetSection("ValidationParameters")
  // Note that 'vpsection' is never null. If it is missing it is just empty.

  // There must be a more elegant way to unload these values, but I don't
  // have time to figure that out now:
  vp.MinDistance <- vpsection.GetValue<int>("MinDistance", 400)
  vp.MaxDistance <- vpsection.GetValue<int>("MaxDistance", 8000)
  vp.MinDuration <- vpsection.GetValue<int>("MinDuration", 120)
  vp.MaxDuration <- vpsection.GetValue<int>("MaxDuration", 14400)
  vp.TimeTolerance <- vpsection.GetValue<int>("TimeTolerance", 20)
  vp.RequireNonAscendingDepartures <- vpsection.GetValue<bool>("RequireNonAscendingDepartures", true)
  vp



