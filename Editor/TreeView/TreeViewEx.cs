using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    public abstract partial class TreeViewEx<TNode> : TreeView
        where TNode : TreeNodeEx
    {
        public bool HasChanged => stateEx.isChanged;
        public Action<TNode> OnDoubleClicked;
        public Action<TNode> OnSingleClicked;

        public event Action<string> OnSearchTextChanged;

        protected bool isDirty { get; private set; } = true;
        public virtual SearchBase Search { get; protected set; }

        protected readonly TreeStateEx stateEx;
        private readonly string saveKey;
        private bool hasSetup = false;
        protected bool initFitSize = true;
        public bool DisableFoldout { get; set; }

        public TreeViewEx(string saveKey, bool init = true) : base(retrieveStateSave(saveKey))
        {
            this.saveKey = saveKey;
            stateEx = state as TreeStateEx;
            useScrollView = true;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            if (init)
            {
                setup();
                Reload();
            }
        }

        public TreeViewEx(bool init = true) : this("", init)
        {
        }

        private static TreeViewState retrieveStateSave(string saveKey)
        {
            if (string.IsNullOrWhiteSpace(saveKey))
            {
                return new TreeStateEx();
            }
            var save = EditorPrefs.GetString(saveKey, "");
            if (string.IsNullOrWhiteSpace(save) == false)
            {
                Debug.Log($"retrieve treeview state save [{saveKey}]");
            }
            return JsonUtility.FromJson<TreeStateEx>(save) ?? new TreeStateEx();
        }

        protected virtual void setup()
        {
            if (hasSetup)
            {
                return;
            }
            hasSetup = true;
            var cols = setupColumns().ToArray();
            for (var i = 0; i < cols.Length; i++)
            {
                cols[i].ColumnIndex = i;
            }
            var state = initializeMultiColumnHeaderState(cols);
            multiColumnHeader = initializeMultiColumnHeader(state);
            columnIndexForTreeFoldouts = Mathf.Max(cols.IndexOf(c => c.IsHierarchy), 0);
            multiColumnHeader.sortingChanged += (h) => handleColumnSorted(h.sortedColumnIndex, multiColumnHeader.GetColumn(h.sortedColumnIndex) as Column, multiColumnHeader.IsSortedAscending(h.sortedColumnIndex));
        }

        protected virtual MultiColumnHeaderState initializeMultiColumnHeaderState(Column[] cols)
        {
            return new MultiColumnHeaderState(cols);
        }

        protected virtual MultiColumnHeader initializeMultiColumnHeader(MultiColumnHeaderState state)
        {
            return new MultiColumnHeader(state);
        }

        protected virtual void handleColumnSorted(int colIndex, Column column, bool isAscending)
        {
        }

        public new void Reload()
        {
            base.Reload();
            isDirty = false;
            afterReload();
        }

        protected virtual void afterReload()
        {
        }

        protected abstract IEnumerable<Column> setupColumns();

        protected override TreeViewItem BuildRoot()
        {
            var nodes = buildRoot();
            stateEx.isChanged = false;
            return nodes;
        }

        protected abstract TreeViewItem buildRoot();

        protected static TreeViewItem createRootNode()
        {
            return new TreeViewItem(-1, -1, "Root");
        }

        protected static TreeViewItem createEmptyNode(int id = 1)
        {
            return new TreeViewItem(id, -1, "Empty");
        }

        protected TreeViewItem simpleBuildRoot(IEnumerable<TNode> children)
        {
            var root = createRootNode();
            if (children.IsNullOrEmpty())
                root.AddChild(createEmptyNode());
            else if (DisableFoldout == false
                && typeof(INestableTreeNode).IsAssignableFrom(typeof(TNode)))
                RearrangeNestedNodes(children).Foreach(root.AddChild);
            else
                children.Foreach(root.AddChild);

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        protected class DataNode
        {
            public string FullPath;
            public TNode Node;
            public DataNode Parent;

            public DataNode(string fullPath, TNode node = null)
            {
                FullPath = fullPath;
                Node = node;
            }
        }

        protected virtual IEnumerable<TNode> RearrangeNestedNodes(IEnumerable<TNode> children)
        {
            if (typeof(INestableTreeNode).IsAssignableFrom(typeof(TNode)) == false)
                return children;

            var roots = new List<TNode>();
            var id = children.Select(d => d.id).Max();
            var directories = new Dictionary<string, TNode>();
            children.OfType<INestableTreeNode>()
               .GroupBy(d => d.Path)
               .OrderBy(d => d.Key)
               .Foreach(g =>
               {
                   if (g.Count() == 1)
                       arrangeNode(g.Key, g.First() as TNode);
                   else
                       g.Foreach(node => arrangeNode(g.Key, node as TNode, true));
               });

            void arrangeNode(string path, TNode node, bool isMultiple = false)
            {
                //root item
                if (path.Contains("/") == false)
                {
                    if (isMultiple == false)
                        directories.Add(path, node);

                    roots.Add(node);
                    return;
                }
                else
                {
                    var dir = path.Substring(0, path.LastIndexOf("/"));
                    var parent = getOrCreateDirectory(dir);
                    node.displayName = path.Split("/").Last();
                    parent.AddChild(node);
                }
            }

            TNode getOrCreateDirectory(string directory)
            {
                var parts = directory.Split("/");
                TNode result = null;
                for (int i = 0; i < parts.Length; i++)
                {
                    var path = parts.Take(i + 1).Join("/");
                    if (directories.TryGetValue(path, out var parent) == false)
                    {
                        parent = Activator.CreateInstance<TNode>();
                        parent.id = ++id;
                        parent.displayName = parts[i];
                        (parent as INestableTreeNode).IsFoldout = true;
                        directories.Add(path, parent);
                        result?.AddChild(parent);
                        if (i == 0)
                            roots.Add(parent);
                    }

                    result = parent;
                }

                return result;
            }

            return roots;
        }

        protected void defaultRowGUI(Rect cellRect, ref RowGUIArgs args)
        {
            args.rowRect = cellRect;
            base.RowGUI(args);
        }

        private static GUILayoutOption[] defaultOptions = new[] { GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinWidth(200), GUILayout.MinHeight(300) };

        public void OnGUILayout(bool inheritOpts = true, params GUILayoutOption[] options)
        {
            if (initFitSize)
            {
                multiColumnHeader.ResizeToFit();
                initFitSize = false;
            }
            options = inheritOpts
                ? options.Any() == true
                    ? defaultOptions.Concat(options).ToArray()
                    : defaultOptions
                : options;
            onGUILayout(options);
        }

        public void OnGUILayoutWithSearchBar(params GUILayoutOption[] options)
        {
            if (Search != null)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    Search.DrawSearchBar();
                }
            }
            OnGUILayout(options: options);
        }

        public void SetDirty()
        {
            isDirty = true;
        }

        protected void onGUILayout(params GUILayoutOption[] layoutOptions)
        {
            if (isDirty)
            {
                Reload();
            }
            OnGUI(EditorGUILayout.GetControlRect(layoutOptions));
        }

        public virtual void FullReload()
        {
            Reload();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            for (var i = 0; i < args.GetNumVisibleColumns(); i++)
            {
                var cellRect = args.GetCellRect(i);
                var col = multiColumnHeader.GetColumn(args.GetColumn(i)) as Column;
                col?.OnCellGUI(cellRect, i, ref args);
            }
        }

        protected override void ContextClicked()
        {
            base.ContextClicked();
            var menu = new GenericMenu();
            appendContextMenu(menu);
            menu.ShowAsContext();
        }

        protected virtual void appendContextMenu(GenericMenu menu)
        {
            menu.AddFuncItem("重新整理", () => FullReload());
            menu.AddFuncItem("全部收合", () => CollapseAll());
            menu.AddFuncItem("全部展開", () => ExpandAll());
            menu.AddFuncItem("取消編輯", () => CancelAllEdit(), enabled: enumerateAllNodes().Any(node => node.IsEditing));
        }

        private IEnumerable<TNode> enumerateAllNodes()
        {
            foreach (var child in enumerate(rootItem))
            {
                yield return child;
            }

            IEnumerable<TNode> enumerate(TreeViewItem item)
            {
                if (item == null)
                {
                    yield break;
                }
                if (item is TNode node)
                {
                    yield return node;
                }
                if (item.children.IsNullOrEmpty())
                {
                    yield break;
                }

                foreach (var child in item.children)
                {
                    foreach (var c in enumerate(child))
                    {
                        yield return c;
                    }
                }
            }
        }

        public void CancelAllEdit()
        {
            foreach (var item in enumerateAllNodes().Where(n => n.IsEditing))
            {
                item.IsEditing = false;
            }
        }

        protected override void ContextClickedItem(int id)
        {
            base.ContextClickedItem(id);
            var node = getNodeByID(id);
            var menu = new GenericMenu();
            appendItemContextMenu(menu, node);
            menu.ShowAsContext();
            Event.current.Use();
        }

        protected virtual void appendItemContextMenu(GenericMenu menu, TNode node)
        {
            appendContextMenu(menu);
        }

        protected override void SingleClickedItem(int id)
        {
            var node = getNodeByID(id);
            handleSingleClickedItem(node);
            if (node != null)
                OnSingleClicked?.Invoke(node);
        }

        protected virtual void handleSingleClickedItem(TNode node)
        {
            if (node == null)
            {
                SetSelection(new int[0]);
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            var node = getNodeByID(id);
            handleDoubleClickItem(node);
            if (node != null)
                OnDoubleClicked?.Invoke(node);
        }

        protected virtual void handleDoubleClickItem(TNode node)
        {
        }

        protected TNode getNodeByID(int id)
        {
            return GetRows().FirstOrDefault(d => d.id == id) as TNode;
        }

        public IEnumerable<TNode> GetSelecedNodes()
        {
            return base.FindRows(GetSelection()).OfType<TNode>();
        }

        protected override void SearchChanged(string newSearch)
        {
            Search?.HandleSearchChanged(newSearch);
            OnSearchTextChanged?.Invoke(newSearch);
            base.SearchChanged(newSearch);
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            if (Search != null && item is TNode n)
            {
                return Search.IsMatch(n);
            }
            return base.DoesItemMatchSearch(item, search);
        }

        protected override void ExpandedStateChanged()
        {
            base.ExpandedStateChanged();
            if (string.IsNullOrWhiteSpace(saveKey) == false)
            {
                EditorPrefs.SetString(saveKey, JsonUtility.ToJson(state));
            }
        }

        #region Cacher

        protected class NodeCacher
        {
            private readonly Dictionary<int, TNode> caches = new Dictionary<int, TNode>();
            private readonly Func<int, TNode> nodeConstructor;

            public NodeCacher(Func<int, TNode> constructor)
            {
                nodeConstructor = constructor;
            }

            public void Clear()
            {
                caches.Clear();
            }

            public TNode Get(int id)
            {
                if (caches.TryGetValue(id, out var node) == false)
                {
                    caches[id] = node = nodeConstructor(id);
                }
                return node;
            }
        }

        #endregion Cacher
    }

    public enum EditType
    {
        None = 0,
        InLine,
        EditEvent,
    }

    public abstract class TreeViewEx<TDataNode, TData> : TreeViewEx<TDataNode>
        where TDataNode : TreeNodeEx<TData>, new()
    {
        public EditType CanEdit { get; set; } = EditType.None;

        public List<TData> Datas
        {
            get => datas;
            set
            {
                if (datas != value)
                {
                    datas = value;
                    SetDirty();
                    onListDataChanged();
                }
            }
        }

        private List<TData> datas;
        private ObservableCollection<TData> dataSource;

        public Action OnAdd;
        public Action<TData> OnEdit;
        public Action<TData> OnDelete;
        public Action<TData> OnMoveUp;
        public Action<TData> OnMoveDown;

        public Action<TDataNode> OnNodeEdit;
        public Action<TDataNode> OnNodeDelete;
        public Action<TDataNode> OnNodeMoveUp;
        public Action<TDataNode> OnNodeMoveDown;

        public TreeViewEx()
        {
        }

        public TreeViewEx(ObservableCollection<TData> source)
        {
            dataSource = source;
            dataSource.CollectionChanged += (_, e) => Datas = e.NewItems.Cast<TData>().ToList();
            Datas = source.ToList();
        }

        public void DrawSearchBar(params GUILayoutOption[] options)
        {
            Search?.DrawSearchBar(options);
        }

        protected virtual void onListDataChanged()
        { }

        protected override void handleDoubleClickItem(TDataNode node)
        {
            var revertable = node as IRevertable;
            var nestable = node as INestableTreeNode;
            if (node != null
                && CanEdit != EditType.None
                && node.CanEdit
                && nestable?.IsFoldout != true)
            {
                switch (CanEdit)
                {
                    case EditType.InLine:
                        if (node.IsEditing == false && revertable != null)
                            revertable.Cache();
                        node.IsEditing = !node.IsEditing;
                        break;

                    case EditType.EditEvent:
                        revertable?.Cache();
                        OnEdit?.Invoke(node.Data);
                        OnNodeEdit?.Invoke(node);
                        break;
                }
            }
            base.handleDoubleClickItem(node);
        }

        protected override TreeViewItem buildRoot()
        {
            var id = 1;
            var children = Datas?.Select(d => new TDataNode().Setup(id++, d)).Cast<TDataNode>().ToArray();
            return simpleBuildRoot(children);
        }
    }

    public abstract class TreeNodeEx : TreeViewItem
    {
        public virtual bool Changed { get; set; }
        public virtual bool IsEditing { get; set; }
        public virtual bool CanEdit { get; protected set; } = true;

        public TreeNodeEx()
        {
        }

        public TreeNodeEx(int id)
        {
            this.id = id;
        }
    }

    public abstract class TreeNodeEx<TData> : TreeNodeEx
    {
        public TData Data { get; private set; }

        public TreeNodeEx()
        {
        }

        public TreeNodeEx(int id, TData data) : base(id)
        {
            Data = data;
        }

        public virtual TreeNodeEx Setup(int id, TData data)
        {
            this.id = id;
            Data = data;
            return this;
        }
    }

    public interface INestableTreeNode
    {
        public string Path { get; }
        public bool IsFoldout { get; set; }
    }

    public interface IRevertable
    {
        void Cache();

        void Revert();

        void Apply();
    }

    public class TreeStateEx : TreeViewState
    {
        public bool isChanged { get; set; }
    }
}