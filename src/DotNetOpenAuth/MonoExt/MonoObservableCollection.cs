using System;
using System.Windows;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace MonoExt.System.Collections.ObjectModel
{
  /// <summary>
  /// </summary>
  public enum NotifyCollectionChangedAction
  {
    /// <summary>
    /// </summary>
    Add,
    /// <summary>
    /// </summary>
    Remove,
    /// <summary>
    /// </summary>
    Replace,
    /// <summary>
    /// </summary>
    Reset = 4
  }

  /// <summary>
  /// </summary>
  public sealed class NotifyCollectionChangedEventArgs : EventArgs
  {
    List<object> new_items, old_items;

    /// <summary>
    /// </summary>
    public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action)
    {
      if (action != NotifyCollectionChangedAction.Reset)
        throw new NotSupportedException();

      Action = action;
      NewStartingIndex = -1;
      OldStartingIndex = -1;
    }

    /// <summary>
    /// </summary>
    public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, int index)
    {
      switch (action)
      {
        case NotifyCollectionChangedAction.Add:
          new_items = new List<object>();
          new_items.Add(changedItem);
          NewStartingIndex = index;
          OldStartingIndex = -1;
          break;
        case NotifyCollectionChangedAction.Remove:
          old_items = new List<object>();
          old_items.Add(changedItem);
          OldStartingIndex = index;
          NewStartingIndex = -1;
          break;
        default:
          throw new NotSupportedException();
      }

      Action = action;
    }

    /// <summary>
    /// </summary>
    public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem, int index)
    {
      if (action != NotifyCollectionChangedAction.Replace)
        throw new NotSupportedException();

      Action = action;

      new_items = new List<object>();
      new_items.Add(newItem);

      old_items = new List<object>();
      old_items.Add(oldItem);

      NewStartingIndex = index;
      OldStartingIndex = -1;
    }

    /// <summary>
    /// </summary>
    public NotifyCollectionChangedAction Action { get; private set; }

    /// <summary>
    /// </summary>
    public IList NewItems
    {
      get { return new_items; }
    }

    /// <summary>
    /// </summary>
    public IList OldItems
    {
      get { return old_items; }
    }

    /// <summary>
    /// </summary>
    public int NewStartingIndex { get; private set; }

    /// <summary>
    /// </summary>
    public int OldStartingIndex { get; private set; }
  }

  /// <summary>
  /// </summary>
  public interface INotifyCollectionChanged
  {
    /// <summary>
    /// </summary>
    event NotifyCollectionChangedEventHandler CollectionChanged;
  }

  /// <summary>
  /// </summary>
  public delegate void NotifyCollectionChangedEventHandler(object sender, NotifyCollectionChangedEventArgs e);

  /// <summary>
  /// </summary>
  public class ObservableCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
  {
    /// <summary>
    /// </summary>
    public ObservableCollection()
    {
    }

    /// <summary>
    /// </summary>
    public ObservableCollection(IEnumerable<T> collection)
    {
      Console.WriteLine("System.Collections.ObjectModel.ObservableCollection.ctor (IEnumerable<T>): NIEX");
      throw new NotImplementedException();
    }

    /// <summary>
    /// </summary>
    public ObservableCollection(List<T> list)
    {
      Console.WriteLine("System.Collections.ObjectModel.ObservableCollection.ctor (List<T>): NIEX");
      throw new NotImplementedException();
    }

    /// <summary>
    /// </summary>
    protected override void ClearItems()
    {
      base.ClearItems();
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// </summary>
    protected override void InsertItem(int index, T item)
    {
      base.InsertItem(index, item);
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                       item,
                       index));
    }

    /// <summary>
    /// </summary>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
      if (CollectionChanged != null)
        CollectionChanged(this, e);
    }

    /// <summary>
    /// </summary>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, e);
    }

    /// <summary>
    /// </summary>
    protected override void RemoveItem(int index)
    {
      T old_item = this[index];
      base.RemoveItem(index);
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                       old_item,
                       index));
    }

    /// <summary>
    /// </summary>
    protected override void SetItem(int index, T item)
    {
      T old_item = this[index];
      base.SetItem(index, item);

      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                       item,
                       old_item,
                       index));
    }

    /// <summary>
    /// </summary>
    public event NotifyCollectionChangedEventHandler CollectionChanged;
    /// <summary>
    /// </summary>
    protected event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// </summary>
    event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
    {
      add { PropertyChanged += value; }
      remove { PropertyChanged -= value; }
    }
  }
}