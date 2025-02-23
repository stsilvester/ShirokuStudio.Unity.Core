using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    public abstract partial class TreeViewEx<TNode> where TNode : TreeNodeEx
    {
        protected partial class Column : MultiColumnHeaderState.Column
        {
            public TreeViewEx<TNode> Tree { get; }

            /// <summary>是否使用預設的顯示(純文字+摺疊)</summary>
            public virtual bool UseDefault
            {
                get => useDefault ?? displayNameGetter == null;
                set => useDefault = value;
            }

            /// <summary>標記為樹狀結構的主要欄位</summary>
            public bool IsHierarchy;

            /// <summary>顯示名稱委派</summary>
            public Func<TNode, string> displayNameGetter;

            public Func<TNode, string> tooltipGetter;

            /// <summary>判斷是否跳過顯示的委派</summary>
            public Func<TNode, bool> Ignore;

            /// <summary>文字對齊方式</summary>
            public TextAnchor textAnchor = TextAnchor.UpperLeft;

            internal int ColumnIndex;

            private Lazy<GUIStyle> label;
            private bool? useDefault;

            protected Func<Column, GUIStyle> GetLabelStyle = (Column col) => new GUIStyle(EditorStyles.label)
            {
                alignment = col.textAnchor,
                wordWrap = true,
                richText = true
            };

            public Column(TreeViewEx<TNode> tree, string header) : this(tree)
            {
                if (string.IsNullOrWhiteSpace(header) == false)
                {
                    headerContent = new GUIContent(header);
                }
            }

            public Column(TreeViewEx<TNode> tree)
            {
                Tree = tree;
                width = 80;
                canSort = false;
                minWidth = 40;
                autoResize = true;
                label = new Lazy<GUIStyle>(() => GetLabelStyle(this));
            }

            public virtual void OnCellGUI(Rect cellRect, int visibleColIndex, ref RowGUIArgs args)
            {
                var node = args.item as TNode;
                if (node == null)
                {
                    if (IsHierarchy)
                    {
                        defaultGUI(cellRect, ref args);
                    }
                    return;
                }

                if (node is INestableTreeNode nest
                    && nest.IsFoldout
                    && Tree.columnIndexForTreeFoldouts == visibleColIndex)
                {
                    defaultGUI(cellRect, ref args);
                    return;
                }

                if (Ignore?.Invoke(node) == true)
                {
                    return;
                }
                if (UseDefault)
                {
                    defaultGUI(cellRect, ref args);
                }
                else
                {
                    onGUI(cellRect, ref args, node, displayNameGetter != null ? displayNameGetter.Invoke(node) : args.item.displayName);
                }
            }

            protected virtual void onGUI(Rect rect, ref RowGUIArgs args, TNode item, string displayName)
            {
                var ct = new GUIContent(displayName, tooltipGetter?.Invoke(item));
                GUI.Label(rect, ct, label.Value);
            }

            protected void defaultGUI(Rect rect, ref RowGUIArgs args)
            {
                Tree.defaultRowGUI(rect, ref args);
            }
        }
    }
}