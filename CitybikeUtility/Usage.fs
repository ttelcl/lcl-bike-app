// (c) 2022  ttelcl / ttelcl
module Usage

open CommonTools
open ColorPrint

// Reminder: the escapes like "\fo" and the like in the strings below
// are turned into colored console output by ColorPrint.cp

let usage detailed =
  cp "\foCitybikeUtility\f0 - Utility for handling Citybike data files and database."
  cp ""
  cp "\foCitybikeUtility \fyprepare <\fgARGUMENTS\f0>\f0"
  cp "  Validate rows in a ride data file and split it in one-day segments."
  cp "  \fg-df\f0 <\fcdata-folder\f0>      Sets the data-folder. It is located by searching"     
  cp "  \fx\fx\fx\fx                       this name relative to the current directory and"
  cp "  \fx\fx\fx\fx                       each ancestor. Default: '\fc_data\f0'"
  cp "  \fg-s\f0 <\fcstations.csv\f0>      Input stations data file (for valid station IDs)."
  cp "     If not specified, the following defaults are tried: \fg-s \fystations.csv"
  cp "     and \fg-s \fyHelsingin_ja_Espoon_kaupunkipyöräasemat_avoin.csv"
  cp "  \fg-i\f0 <\fcdatafile.csv\f0>      Input rides data file to process. Repeatable."
  cp ""
  cp "Common options:"
  cp "\fg-v               \f0Verbose mode"



