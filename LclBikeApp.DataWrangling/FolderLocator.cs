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

namespace LclBikeApp.DataWrangling
{
  /// <summary>
  /// Utility class to generate a sequence of folder names, based
  /// on an anchor folder and its ancestors.
  /// </summary>
  public static class FolderLocator
  {
    /// <summary>
    /// Return the full paths of the specified folder and its ancestors.
    /// </summary>
    /// <param name="anchorFolder">
    /// The folder to start walking up the directory tree from.
    /// As a special case, if this is null or empty, Environment.CurrentDirectory
    /// is substituted.
    /// </param>
    /// <returns>
    /// A sequence of full folder names starting at the given folder and
    /// continuing with its parent, its parent's parent, etc.
    /// </returns>
    public static IEnumerable<string> SelfAndAncestors(string anchorFolder)
    {
      if(String.IsNullOrEmpty(anchorFolder))
      {
        anchorFolder = Environment.CurrentDirectory;
      }
      var anchor = Path.GetFullPath(anchorFolder);
      var di = new DirectoryInfo(anchor);
      while(di != null)
      {
        yield return di.FullName;
        di = di.Parent;
      }
    }

    /// <summary>
    /// Return the full paths of the current working directory and its ancestors.
    /// </summary>
    /// <returns>
    /// A sequence of full folder names starting at the given folder and
    /// continuing with its parent, its parent's parent, etc.
    /// </returns>
    public static IEnumerable<string> SelfAndAncestors()
    {
      return SelfAndAncestors(Environment.CurrentDirectory);
    }

    /// <summary>
    /// Return a sequence of directory names constructed from the
    /// anchor folder and its ancestors with the given relative sibling
    /// path appended. By default only actually existing folders
    /// are returned.
    /// </summary>
    /// <param name="siblingPath">
    /// The relative path to append to amchor and each of its ancestors
    /// </param>
    /// <param name="anchorPath">
    /// The anchor folder where to start walking up the directory tree.
    /// As a special case, if this is null or empty, Environment.CurrentDirectory
    /// is substituted.
    /// </param>
    /// <param name="existingOnly">
    /// When true (default): only folders that actually exist are returned.
    /// When false: all the generated folder names are returned.
    /// </param>
    /// <returns>
    /// A sequence of zero or more directory names
    /// </returns>
    public static IEnumerable<string> SelfAndAncestorSiblings(
      string siblingPath,
      string anchorPath,
      bool existingOnly = true)
    {
      if(String.IsNullOrEmpty(anchorPath))
      { 
        anchorPath = Environment.CurrentDirectory;
      }
      foreach(var ancestor in SelfAndAncestors(anchorPath))
      {
        
        var sibling = Path.Combine(ancestor, siblingPath);
        if(!existingOnly || Directory.Exists(sibling))
        {
          yield return sibling;
        }
      }
    }

    /// <summary>
    /// Return a sequence of directory names constructed from the
    /// current directory and its ancestors with the given relative sibling
    /// path appended. By default only actually existing folders
    /// are returned.
    /// </summary>
    /// <param name="siblingPath">
    /// The relative path to append to amchor and each of its ancestors
    /// </param>
    /// <param name="existingOnly">
    /// When true (default): only folders that actually exist are returned.
    /// When false: all the generated folder names are returned.
    /// </param>
    /// <returns>
    /// A sequence of zero or more directory names
    /// </returns>
    public static IEnumerable<string> SelfAndAncestorSiblings(
      string siblingPath,
      bool existingOnly = true)
    {
      return SelfAndAncestorSiblings(siblingPath, Environment.CurrentDirectory, existingOnly);
    }

  }
}
