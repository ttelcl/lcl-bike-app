/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LclBikeApp.DataWrangling.DataLocation
{
  /// <summary>
  /// Logically represents the storage medium for data files.
  /// This can be implemented as a normal file system folder or
  /// some more exotic storage medium, e.g. in a cloud blob
  /// container.
  /// </summary>
  public abstract class DataContainer
  {
    /// <summary>
    /// Create a new DataContainer
    /// </summary>
    protected DataContainer(
      bool canWrite)
    {
      CanWrite = canWrite;
    }

    /// <summary>
    /// True for data containers that support writing
    /// </summary>
    public bool CanWrite { get; }

    /// <summary>
    /// Returns true if the indicated file exists
    /// </summary>
    /// <param name="relativeName">
    /// The path to the file relative to the container root
    /// </param>
    /// <returns>
    /// True if the file is available, false if it isn't
    /// </returns>
    public abstract bool HasFile(string relativeName);

    /// <summary>
    /// Open an existing file for reading as UTF8 text
    /// </summary>
    /// <param name="relativeName">
    /// The path to the file relative to the container root
    /// </param>
    /// <returns>
    /// The opened file
    /// </returns>
    public abstract TextReader OpenReadText(string relativeName);

    /// <summary>
    /// Create a new file (or overwrite an existing file) for writing as UTF8 text
    /// </summary>
    /// <param name="relativeName">
    /// The path to the file relative to the container root
    /// </param>
    /// <returns>
    /// The opened file
    /// </returns>
    public abstract TextWriter CreateWriteText(string relativeName);

    /// <summary>
    /// Do the standard "backup shuffle" of a file: delete an existing backup
    /// (if it exists), move the existing target file to the backup, and move
    /// the temporary file to the target file.
    /// </summary>
    /// <param name="relativeName">
    /// The final name of the target file, relative to the container root
    /// </param>
    /// <param name="tmpName">
    /// The name of the temporary file containing the new content of the target file,
    /// or null to use the target name + ".tmp"
    /// </param>
    /// <param name="bakName">
    /// The name of the backup file to hold the previous content of the target file,
    /// or null to use the target name + ".bak"
    /// </param>
    public abstract void BackupShuffle(string relativeName, string? tmpName = null, string? bakName = null);

  }
}
