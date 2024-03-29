﻿/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LclBikeApp.DataWrangling.Utilities
{
  /// <summary>
  /// Static methods for splitting a sequence in batches
  /// </summary>
  public static class SequenceBatching
  {

    /// <summary>
    /// Split a sequence of objects into a series of batches.
    /// Items are added to a batch as long as the given key extraction function
    /// returns the same value for the items. As soon as an item is
    /// encountered with a new key value, a new batch is started
    /// </summary>
    /// <typeparam name="T">
    /// The item type
    /// </typeparam>
    /// <typeparam name="K">
    /// The type of the extracted key value
    /// </typeparam>
    /// <param name="sequence">
    /// The input sequence (extension argument)
    /// </param>
    /// <param name="extractKey">
    /// The function to extract the key from the item
    /// </param>
    /// <returns>
    /// A sequence of Lists ("batches") of items with the same key.
    /// </returns>
    public static IEnumerable<List<T>> BatchByKeyFunc<T,K>(
      this IEnumerable<T> sequence, Func<T, K> extractKey)
      where K: IEquatable<K>
    {
      var batcher = new SequenceBatcher<T, K>(extractKey);
      return batcher.BatchAll(sequence);
    }

  }
}
