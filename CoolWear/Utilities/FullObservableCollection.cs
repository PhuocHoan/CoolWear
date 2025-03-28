using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace CoolWear.Utilities;

public class FullObservableCollection<T> : ObservableCollection<T> where T : class, INotifyPropertyChanged
{
    public FullObservableCollection() : base() { }

    public FullObservableCollection(IEnumerable<T> collection) : base(collection)
    {
        foreach (var item in this)
        {
            item.PropertyChanged += Item_PropertyChanged!;
        }
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (T item in e.NewItems)
            {
                item.PropertyChanged += Item_PropertyChanged!;
            }
        }

        if (e.OldItems != null)
        {
            foreach (T item in e.OldItems)
            {
                item.PropertyChanged -= Item_PropertyChanged!;
            }
        }

        base.OnCollectionChanged(e);
    }

    private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        NotifyCollectionChangedEventArgs args = new(
            NotifyCollectionChangedAction.Replace,
            sender,
            sender,
            IndexOf((T)sender)
        );
        OnCollectionChanged(args);
    }
}