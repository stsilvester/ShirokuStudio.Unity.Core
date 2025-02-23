using System;
using System.Text.RegularExpressions;

namespace ShirokuStudio.Editor
{
    public abstract partial class TreeViewEx<TNode> where TNode : TreeNodeEx
    {
        public abstract class SearchEntryBase
        {
            public abstract bool IsActived { get; }
            public string Prefix { get; private set; }
            public Regex Pattern { get; private set; }

            public abstract void UpdateSearchInput(string input);
            public abstract bool Match(TNode node);
            public abstract string SearchText { get; set; }
            public event Action OnUpdated;

            public SearchEntryBase(string prefix, string pattern)
            {
                Prefix = prefix;
                Pattern = new Regex($"{prefix}:{pattern}", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }

            public override string ToString()
            {
                return $"{Prefix}:{SearchText}";
            }

            protected void RaiseUpdated()
            {
                OnUpdated?.Invoke();
            }
        }
    }
}