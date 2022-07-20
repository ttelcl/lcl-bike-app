// (c) 2022  ttelcl / ttelcl
module Usage

open CommonTools
open ColorPrint

// Reminder: the escapes like "\fo" and the like in the strings below
// are turned into colored console output by ColorPrint.cp

let usage detailed =
  cp "\foCitybikeUtility\f0 - Utility for handling Citybike data files and database."
  cp ""
  cp "\foCitybikeUtility \fyconfig"
  cp "  Print out the configuration values as found in configuration.json "
  cp "  and the user secrets configuration."
  cp ""
  cpx "\foCitybikeUtility \fyinit-stations\f0 [\fg-df\f0 <\fcdata-folder\f0>] "
  cp "[\fg-s\f0 <\fcstations.csv\f0>] [\fg-db\f0 <\fcdb-tag\f0>]"
  cp "  Upload missing station data to the database"
  cp ""
  cp "\foCitybikeUtility \fyinit-rides\f0 <\fgARGUMENTS\f0>\f0"
  cp "  Validate rows in a ride data file and optionally insert it day-by-day into a DB."
  cp "  \fg-df\f0 <\fcdata-folder\f0>      Sets the data-folder. It is located by searching"     
  cp "  \fx\fx\fx\fx                       this name relative to the current directory and"
  cp "  \fx\fx\fx\fx                       each ancestor. Default: '\fc_data\f0'"
  cp "  \fg-db\f0 <\fcdb-tag\f0>           Use the specified DB connection instead of '\fydefault\f0'"
  cp "  \fg-insert\f0 \fx\fx               Insert ride data into the DB instead of just checking. Implies '-S'"
  cp "  \fg-S\f0 \fx\fx                    Retrieve valid station ids from the DB instead of a file"
  cp "  \fg-s\f0 <\fcstations.csv\f0>      Input stations data file (for valid station IDs)."
  cp "     If not specified, the following defaults are tried: \fg-s \fystations.csv"
  cp "     and \fg-s \fyHelsingin_ja_Espoon_kaupunkipyöräasemat_avoin.csv"
  cp "  \fg-i\f0 <\fcdatafile.csv\f0>      Input rides data file to process. Repeatable."
  cp "  \fg-from\f0 <\fcyyyy-MM-dd\f0>     First day (skip earlier data)"
  cp "  \fg-to\f0 <\fcyyyy-MM-dd\f0>       Last day (skip later data)"
  cp ""
  cp "Common options:"
  cp "\fg-v               \f0Verbose mode"



