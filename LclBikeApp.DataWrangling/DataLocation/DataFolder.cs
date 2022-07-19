/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LclBikeApp.DataWrangling.DataLocation
{
  /// <summary>
  /// Implements DataContainer for the normal filesystem, using a single
  /// folder as container root
  /// </summary>
  public class DataFolder: DataContainer
  {
    /// <summary>
    /// Create a new DataFolder
    /// </summary>
    /// <param name="root">
    /// The path to the data folder
    /// </param>
    /// <param name="createIfMissing">
    /// When true, and the folder is missing: create it.
    /// When false, and the folder is missing: throw an exception.
    /// If the folder exists the value doesn't matter
    /// </param>
    public DataFolder(
      string root,
      bool createIfMissing = false) :
      base(true)
    {
      Root = Path.GetFullPath(root);
      if(!Directory.Exists(Root))
      {
        if(createIfMissing)
        {
          Directory.CreateDirectory(Root);
        }
        else
        {
          throw new DirectoryNotFoundException(
            $"Data folder does not exist: {Root}");
        }
      }
    }

    /// <summary>
    /// Search for a data folder. For the current directory and each of its
    /// ancestors a candidate folder name is constructed by composing these
    /// folder names with the given "shortname" argument. The first to exist
    /// is returned.
    /// </summary>
    /// <param name="shortname">
    /// The name relative to the current directory and each of its parent folders
    /// to check for existence.
    /// </param>
    /// <returns>
    /// The existing folder that was found, wrapped as a DataFolder
    /// </returns>
    /// <exception cref="DirectoryNotFoundException">
    /// Thrown when none of the candidate folders exists.
    /// </exception>
    public static DataFolder LocateAsAncestorSibling(string shortname)
    {
      var folder =
        FolderLocator.SelfAndAncestorSiblings(shortname, Environment.CurrentDirectory, true)
        .FirstOrDefault();
      if(folder != null)
      {
        return new DataFolder(folder);
      }
      else
      {
        var failures =
          FolderLocator.SelfAndAncestorSiblings(shortname, Environment.CurrentDirectory, false);
        throw new DirectoryNotFoundException(
          $"Cannot locate data folder. None of these exist: {string.Join(", ", failures)}");
      }
    }

    /// <summary>
    /// The full path to the root data folder
    /// </summary>
    public string Root { get; }

    /// <summary>
    /// Resolve a file name relative to the data folder to a full path.
    /// Optionally check if the file exists.
    /// </summary>
    /// <param name="relativeName">
    /// The file name, relative to the data folder
    /// </param>
    /// <param name="mustExist">
    /// When true, a FileNotFoundException is thrown if the result
    /// is not the name of an existing file.
    /// </param>
    /// <returns>
    /// The full path of the file
    /// </returns>
    /// <exception cref="FileNotFoundException">
    /// Thrown if the file does not exist and "mustExist" was true
    /// </exception>
    public string ResolveFile(string relativeName, bool mustExist=false)
    {
      var fnm = Path.Combine(Root, relativeName);
      if(mustExist && !File.Exists(fnm))
      {
        throw new FileNotFoundException(
          $"File not found {fnm}");
      }
      return fnm;
    }

    /// <summary>
    /// Resolve a directory name relative to the data folder to a full path.
    /// Optionally check if the directory exists.
    /// </summary>
    /// <param name="relativeName">
    /// The directory name, relative to the data folder
    /// </param>
    /// <param name="mustExist">
    /// When true, a DirectoryNotFoundException is thrown if the result
    /// is not the name of an existing directory.
    /// </param>
    /// <returns>
    /// The full path of the directory
    /// </returns>
    /// <exception cref="DirectoryNotFoundException">
    /// Thrown if the directory does not exist and "mustExist" was true
    /// </exception>
    public string ResolveDirectory(string relativeName, bool mustExist=false)
    {
      var dnm = Path.Combine(Root, relativeName);
      if(mustExist && !Directory.Exists(dnm))
      {
        throw new DirectoryNotFoundException(
          $"Directory not found {dnm}");
      }
      return dnm;
    }

    /// <summary>
    /// Resolve a directory name relative to the data folder to a full path.
    /// Create it if it does not yet exist.
    /// </summary>
    /// <param name="relativeName">
    /// The directory name, relative to the data folder
    /// </param>
    /// <returns>
    /// The full path of the directory
    /// </returns>
    public string ResolveOrCreateDirectory(string relativeName)
    {
      var dnm = Path.Combine(Root, relativeName);
      if(!Directory.Exists(dnm))
      {
        Directory.CreateDirectory(dnm);
      }
      return dnm;
    }

    /// <summary>
    /// Returns true if the indicated file exists
    /// </summary>
    /// <param name="relativeName">
    /// The path to the file relative to the container root
    /// </param>
    /// <returns>
    /// True if the file is available, false if it isn't
    /// </returns>
    public override bool HasFile(string relativeName)
    {
      return File.Exists(Path.Combine(Root, relativeName));
    }

    /// <summary>
    /// Open an existing file for reading as UTF8 text
    /// </summary>
    /// <param name="relativeName">
    /// The path to the file relative to the container root
    /// </param>
    /// <returns>
    /// The opened file
    /// </returns>
    public override TextReader OpenReadText(string relativeName)
    {
      var fnm = Path.Combine(Root, relativeName);
      return File.OpenText(fnm);
    }

    /// <summary>
    /// Fully read the content of an UTF8 encoded text file
    /// </summary>
    public override string ReadAllText(string relativeName)
    {
      var fnm = Path.Combine(Root, relativeName);
      return File.ReadAllText(fnm);
    }

    /// <summary>
    /// Create a new file (or overwrite an existing file) for writing as UTF8 text
    /// </summary>
    /// <param name="relativeName">
    /// The path to the file relative to the container root
    /// </param>
    /// <returns>
    /// The opened file
    /// </returns>
    public override TextWriter CreateWriteText(string relativeName)
    {
      var fnm = Path.Combine(Root, relativeName);
      return File.CreateText(fnm);
    }

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
    public override void BackupShuffle(string relativeName, string? tmpName = null, string? bakName = null)
    {
      tmpName ??= relativeName + ".tmp";
      bakName ??= relativeName + ".bak";
      var fullName = Path.Combine(Root, relativeName);
      var tmpFullName = Path.Combine(Root, tmpName);
      var bakFullName = Path.Combine(Root, bakName);
      if(File.Exists(bakFullName) && File.Exists(fullName))
      {
        File.Delete(bakFullName);
      }
      if(File.Exists(tmpFullName))
      {
        // The normal case.
        if(File.Exists(fullName))
        {
          // Remember this system API! (it is a single transaction on supported filesystems)
          File.Replace(tmpFullName, fullName, bakFullName);
        }
        else
        {
          File.Move(tmpFullName, fullName);
        }
      }
      else
      {
        // Support this unusual case by considering the non-existence of the target
        // as intended result. Only overwrite the backup if the target exists.
        if(File.Exists(fullName))
        {
          File.Move(fullName, bakFullName);
        }
        // else: it is a no-op!
      }
    }

  }
}
