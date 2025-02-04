using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace ShirokuStudio.Core.Models
{
    public class DataNode<TValue> : DisposableCollection, IGUIEntry
    {
        /// <summary>
        /// text for display
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// text for fuzzy search
        /// </summary>
        public string SearchName { get; }

        public TValue Value { get; }

        public Texture Icon { get; }

        public bool IsRoot => Parent is null;
        public bool HasChild => children.Any();
        public DataNode<TValue> Parent { get; private set; }
        public IReadOnlyCollection<DataNode<TValue>> Children => children;
        private readonly List<DataNode<TValue>> children = new();

        public DataNode(string name, string search, DataNode<TValue> parent, TValue value, Texture icon = null)
        {
            Name = name;
            SearchName = search;
            Parent = parent;
            Value = value;
            Icon = icon;
        }

        public virtual DataNode<TValue> AddChild(DataNode<TValue> child)
        {
            children.Add(child);
            child.Parent = this;
            child.AddTo(this);
            return child;
        }

        public IEnumerable<DataNode<TValue>> GetAncestors(bool includeSelf = false)
        {
            if (includeSelf)
                yield return this;

            if (IsRoot)
                yield break;

            foreach (var ancestor in Parent.GetAncestors(true))
                yield return ancestor;
        }

        public IEnumerable<DataNode<TValue>> GetAllChildren()
        {
            foreach (var child in Children)
            {
                yield return child;

                foreach (var grandChild in child.GetAllChildren())
                    yield return grandChild;
            }
        }

        public DataNode<TValue> FindChild(string name)
            => FindChild(name.AsSpan());

        public DataNode<TValue> FindChild(ReadOnlySpan<char> name)
        {
            for (int i = children.Count - 1; i >= 0; i--)
            {
                var child = children[i];
                if (name.Equals(child.Name.AsSpan(), StringComparison.Ordinal))
                {
                    return child;
                }
            }

            return null;
        }

        public DataNode<TValue> GetNextChild(DataNode<TValue> currentChild)
        {
            var index = children.IndexOf(currentChild);
            if (index == -1)
                return null;

            if (index == children.Count - 1)
                return null;

            return children[index + 1];
        }

        public DataNode<TValue> GetPreviousChild(DataNode<TValue> currentChild)
        {
            var index = children.IndexOf(currentChild);
            if (index == -1)
                return null;

            if (index == 0)
                return null;

            return children[index - 1];
        }
    }
}