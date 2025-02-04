using ShirokuStudio.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    public abstract class DropdownMenu
    {
        public Rect VisibleRect { get; set; }

        public bool FocusSearchBar { get; set; } = true;

        public abstract void Draw();

        public Action OnComplete { get; set; }
        public IEnumerable<IGUIEntry> Items { get; set; }

        public virtual void ShowAsContext(int windowHeight = 0)
            => DropdownWindow.ShowAsContext(this, windowHeight);

        public virtual void ShowAsDropdown(Rect rect, int windowHeight = 0)
            => DropdownWindow.ShowDropdown(rect, this, windowHeight);
    }
}

namespace ShirokuStudio.Editor
{
    public class DropdownMenu<T> : DropdownMenu
    {
        private readonly string ctrlName_searchField = GUID.Generate().ToString();
        private readonly bool drawSearchbar;

        public ReactiveProperty<DropdownNode<T>> SelectedNode { get; } = new();
        public bool IsInSearchMode { get; private set; }

        public event Action<T> OnValueSelected;

        protected DropdownNode<T> NotFoundNode { get; }

        public readonly DropdownNode<T> Root;
        protected IEnumerable<DropdownNode<T>> Nodes => Root.Children.OfType<DropdownNode<T>>();

        private readonly List<DropdownNode<T>> searchResult = new();
        protected IReadOnlyCollection<DropdownNode<T>> SearchResult => searchResult;

        protected StringReactiveProperty SearchString = new();
        private DropdownNode<T> scrollToNode;

        private Vector2 scroll;

        public DropdownMenu(
            IEnumerable<DropdownItem<T>> items,
            Action<T> onSelected,
            int searchbarMinItemCount = 0)
        {
            Root = new DropdownNode<T>(this, null, default, string.Empty, string.Empty, null);
            fillNodes(items);

            Items = items;
            drawSearchbar = items?.Count() > searchbarMinItemCount;

            NotFoundNode = new DropdownNode<T>(this, null, default, "(None)", string.Empty, null);

            SearchString
                .DistinctUntilChanged()
                .Subscribe(updateSearchMode);

            OnValueSelected = onSelected;
        }

        private void fillNodes(IEnumerable<DropdownItem<T>> items)
        {
            if (items?.Any() != true)
                return;

            //only support single selection
            items.Where(item => item.IsSelected)
                .Aggregate(false, (foundSelected, item) =>
                {
                    if (foundSelected)
                        item.IsSelected = false;
                    return true;
                });

            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item?.FullName))
                    continue;

                var paths = item.FullName.Split('/');
                var parent = paths.Length == 1 ? Root : prepareFolderNodes(paths.Take(paths.Length - 1));
                var node = parent.AddChild(paths.Last(), item);

                if (item.IsSelected)
                    SelectedNode.Value = node;
            }

            DropdownNode<T> prepareFolderNodes(IEnumerable<string> paths)
            {
                var parentNode = Root;
                foreach (var folder in paths)
                {
                    parentNode = parentNode.FindChild(folder) as DropdownNode<T>
                        ?? parentNode.AddChildFolder(folder);
                }

                return parentNode;
            }
        }

        public void SetItems(IEnumerable<DropdownItem<T>> items)
        {
            Root.Clear();
            fillNodes(items);
        }

        private void updateSearchMode(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (!IsInSearchMode)
                    scroll = Vector2.zero;

                IsInSearchMode = true;
                updateSearchResult();
            }
            else
            {
                GUI.changed = true;
                IsInSearchMode = false;
                if (SelectedNode.Value is not null)
                    ScrollToNode(SelectedNode.Value);
            }
        }

        public void ScrollTo(T data)
        {
            Nodes.TryFind(node => node?.Value?.Equals(data) == true, out var node);
            if (node is not null)
                ScrollToNode(node);
        }

        public void ScrollToNode(DropdownNode<T> node)
        {
            scrollToNode = node;
            var ancestors =
            node.GetAncestors()
                .Cast<DropdownNode<T>>().ToList();

            ancestors.Foreach(p => p.IsExpanded = true);
        }

        public virtual void FinalizeSelection()
        {
            OnComplete?.Invoke();
            if (SelectedNode != null)
                OnValueSelected?.Invoke(SelectedNode.Value.Value);
        }

        public override void Draw()
        {
            if (false == Nodes.Any())
            {
                EditorGUILayout.HelpBox("No items to show", MessageType.Info);
                return;
            }

            if (drawSearchbar)
                draw_Searchbar();

            if (scrollToNode is not null && scrollToNode.Rect.height != 0)
            {
                scroll = scrollToNode.Rect.position;
                scrollToNode = null;
            }

            using var scrollScope = new EditorGUILayout.ScrollViewScope(scroll);
            VisibleRect = new(scroll, VisibleRect.size);
            draw_Treeview();
            scroll = scrollScope.scrollPosition;
            scrollScope.Dispose();
        }

        private void draw_Searchbar()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.SetNextControlName(ctrlName_searchField);
                SearchString.Value = EditorGUILayout.TextField(SearchString.Value, GUI.skin.FindStyle("ToolbarSeachTextField"));
                if (FocusSearchBar)
                {
                    GUI.FocusControl(ctrlName_searchField);
                    FocusSearchBar = false;
                }

                if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
                {
                    SearchString.Value = string.Empty;
                    GUI.FocusControl(null);
                }
            }
        }

        private void draw_Treeview()
        {
            if (false == IsInSearchMode)
                NotFoundNode?.Draw(0, default);

            var nodes = IsInSearchMode ? SearchResult : Nodes;

            foreach (var item in nodes)
                item.Draw(0, VisibleRect);

            GUILayout.FlexibleSpace();
        }

        public void Button(string text, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(text, EditorStyles.toolbarDropDown, options))
            {
                var rect = GUILayoutUtility.GetLastRect();
                ShowAsDropdown(rect);
            }
        }

        protected void updateSearchResult()
        {
            searchResult.Clear();
            var searchParts = SearchString.Value?.Split(' ') ?? Array.Empty<string>();
            long score = 0;
            searchResult.AddRange(Root
                .GetAllChildren()
                .Where(node => node.Value != null)
                .Select(node =>
                {
                    var isIncluded = searchParts
                        .All(keyword => FuzzySearch.FuzzyMatch(keyword, node.SearchName, ref score));
                    //var isIncluded = FuzzySearch.FuzzyMatch(SearchString.Value, node.SearchName, ref score);
                    return (score, node: node as DropdownNode<T>, isIncluded);
                })
                .Where(d => d.isIncluded)
                .OrderByDescending(d => d.score)
                .Select(x => x.node));
        }

        public static void ShowDropdown(Rect rect,
            IEnumerable<DropdownItem<T>> items,
            Action<T> onSelected,
            int searchbarMinItemCount = 0,
            int windowHeight = 0)
            => new DropdownMenu<T>(items.ToList(), onSelected, searchbarMinItemCount)
                .ShowAsDropdown(rect, windowHeight);

        public static void ShowAsContext(
            IEnumerable<DropdownItem<T>> items,
            Action<T> onSelected,
            int searchbarMinItemCount = 0,
            int windowHeight = 0)
            => new DropdownMenu<T>(items.ToList(), onSelected, searchbarMinItemCount)
                .ShowAsContext(windowHeight);
    }
}