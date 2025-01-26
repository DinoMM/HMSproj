using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents
{
    public class OwnObservedCollection<T>
    {
        public List<T> Items { get; private set; } = new();

        public EventCallback? OnCollectionChanged { get; set; } = EventCallback.Empty;

        public OwnObservedCollection(List<T> list)
        {
            Items = list;
        }
        public OwnObservedCollection()
        {
        }

        public void Add(T item)
        {
            Items.Add(item);
            OnCollectionChanged?.InvokeAsync();
        }

        public void Remove(T item)
        {
            if (Items.Remove(item))
            {
                OnCollectionChanged?.InvokeAsync();
            }
        }

        public void Clear()
        {
            bool res = Items.Count > 0;
            Items.Clear();
            if (res)
            {
                OnCollectionChanged?.InvokeAsync();
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            Items.AddRange(items);
            if (items.Any())
            {
                OnCollectionChanged?.InvokeAsync();
            }
        }

        public int RemoveAll(Predicate<T> match)
        {
            var res = Items.RemoveAll(match);
            if (res > 0)
            {
                OnCollectionChanged?.InvokeAsync();
            }
            return res;
        }

        public void SetItems(List<T> items)
        {
            var res = Items != items;
            Items = items;
            if (res)
            {
                OnCollectionChanged?.InvokeAsync();
            }
        }
    }
}
