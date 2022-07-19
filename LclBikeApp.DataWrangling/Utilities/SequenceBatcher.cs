/*
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
  /// Implements a state machine for turning a large sequence of items into
  /// a sequence of smaller batches (List of those items), based on those
  /// items have a common key
  /// </summary>
  public class SequenceBatcher<TItem, TKey>
    where TKey : IEquatable<TKey>
  {
    private readonly Func<TItem, TKey> _extractKey;
    private List<TItem>? _items;
    private TKey? _currentKey;

    /// <summary>
    /// Create a new SequenceBatcher
    /// </summary>
    public SequenceBatcher(
      Func<TItem,TKey> extractKey)
    {
      _extractKey = extractKey;
      _items = null;
    }

    /// <summary>
    /// Turn the input sequence into a sequence of smaller batches of
    /// items (that all evaluate to the same key).
    /// </summary>
    /// <param name="items">
    /// The input sequence
    /// </param>
    /// <returns>
    /// The sequence of batches
    /// </returns>
    public IEnumerable<List<TItem>> BatchAll(IEnumerable<TItem> items)
    {
      Flush(); // make sure our state starts neutral!
      List<TItem>? list;
      foreach(var item in items)
      {
        list = PushItem(item);
        if(list != null)
        {
          yield return list;
        }
      }
      list = Flush();
      if(list != null)
      {
        yield return list;
      }
    }

    /// <summary>
    /// Push the next item into this batcher. If it has a different key
    /// than the previous item, the list containing the previous batch
    /// is returned.
    /// </summary>
    /// <param name="item">
    /// The new item to add to the batch (or add as first item of a
    /// new batch)
    /// </param>
    /// <returns>
    /// Either null, or the previous batch of items that all have the
    /// same key, distinct from the newly added item's key.
    /// </returns>
    public List<TItem>? PushItem(TItem item)
    {
      if(_items == null)
      {
        _items = new List<TItem>();
        _items.Add(item);
        _currentKey = _extractKey(item);
        return null;
      }
      else
      {
        TKey key = _extractKey(item);
        TKey ckey = _currentKey!;
        if(!key.Equals(ckey))
        {
          var list = _items;
          _items = new List<TItem>();
          _currentKey = key;
          _items.Add(item);
          return list;
        }
        else
        {
          _items.Add(item);
          return null;
        }
      }
    }

    /// <summary>
    /// Extract the data that is left in this batcher, if there is any
    /// </summary>
    /// <returns>
    /// A list of the items that were still in this batcher, or null if
    /// there were none.
    /// </returns>
    public List<TItem>? Flush()
    {
      if(HasData)
      {
        var list = _items;
        _items = null;
        return list;
      }
      else
      {
        _items = null;
        return null;
      }
    }

    /// <summary>
    /// True if there are items being held in this batcher
    /// </summary>
    public bool HasData => _items != null && _items.Count > 0;

  }
}
