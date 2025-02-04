using ShirokuStudio.Core;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    public class DropdownItem<T> : IGUIEntry
    {
        public string Name { get; }
        public string FullName { get; }
        public Texture Icon { get; }
        public T Value { get; }
        public bool IsSelected { get; set; }

        public DropdownItem(string name,
            T value,
            Texture icon = null,
            string fullName = null,
            bool isSelected = false)
        {
            Name = name;
            Value = value;
            FullName = fullName ?? name;
            Icon = icon;
            IsSelected = isSelected;
        }

        public DropdownItem(KeyValuePair<string, T> pair,
            Texture icon = null,
            bool isSelected = false)
            : this(pair.Key, pair.Value, icon, null, isSelected)
        {
        }

        public static IEnumerable<DropdownItem<T>> Parse(IEnumerable<T> source,
            Expression<Func<T, string>> name,
            T selectedValue = default)
        {
            var func = name.Compile();
            foreach (var item in source)
            {
                var _name = func(item);
                yield return new DropdownItem<T>(_name, item, null, _name, item.Equals(selectedValue));
            }
        }
    }
}