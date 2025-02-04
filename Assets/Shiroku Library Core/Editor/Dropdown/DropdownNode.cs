using ShirokuStudio.Core;
using ShirokuStudio.Core.Models;
using System.Linq;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    public class DropdownNode<T> : DataNode<T>, ISelectable
    {
        public DropdownMenu<T> Menu { get; }

        public bool IsExpanded { get; set; }
        public Rect Rect { get; private set; }

        public BoolReactiveProperty IsSelected { get; } = new();
        public BoolReactiveProperty IsEnabled { get; } = new();
        protected bool IsHovered => Rect.Contains(Event.current.mousePosition);

        public DropdownNode(DropdownMenu<T> menu, DropdownNode<T> parent, T value,
            string name, string searchName, Texture icon)
            : base(name, searchName, parent, value, icon)
        {
            Menu = menu;
            menu.SelectedNode
                .Select(n => n == this)
                .DistinctUntilChanged()
                .Subscribe(v => IsSelected.Value = v)
                .AddTo(this);
        }

        #region GUI

        public virtual void Draw(int indentLevel, Rect visibleRect)
        {
            //draw selg
            draw(indentLevel, visibleRect);

            //draw children
            if (false == IsExpanded)
                return;

            foreach (var child in Children.OfType<DropdownNode<T>>())
                child.Draw(indentLevel + 1, visibleRect);
        }

        private void draw(int indentLevel, Rect visibleRect)
        {
            if (IsReservesSpace())
                return;

            if (Rect.y > 1000f && IsOutOfVisibleRect(visibleRect))
                return;

            if (Event.current.type == EventType.Repaint)
                DrawNodeContnet(indentLevel);

            HandleMouseEvent();
        }

        /// <summary>
        /// 是否保留空間且不必繪製UI
        /// </summary>
        protected bool IsReservesSpace()
        {
            var rect = GUILayoutUtility.GetRect(0f, DropdownStyle.NodeHeight);

            if (Event.current.type == EventType.Layout)
                return true;

            if (Event.current.type == EventType.Repaint || Rect.width == 0f)
                Rect = rect;

            return false;
        }

        protected bool IsOutOfVisibleRect(Rect visibleRect)
            => Rect.yMax < visibleRect.y || Rect.y > visibleRect.yMax;

        protected virtual void DrawNodeContnet(int indentLevel, int raiseText = 0)
        {
            //draw background
            if (IsSelected.Value)
                EditorGUI.DrawRect(Rect, DropdownStyle.SelectedColor);
            else if (IsHovered)
                EditorGUI.DrawRect(Rect, DropdownStyle.HighlightedColor);

            //calc indent
            var offset = DropdownStyle.GlobalOffset + indentLevel * DropdownStyle.IndentWidth;
            var indentedNodeRect = Rect;
            indentedNodeRect.xMin += offset;
            indentedNodeRect.y -= raiseText;

            DrawItem(indentedNodeRect);

            DraeSeparator();
        }

        protected virtual void DrawItem(Rect indentedNodeRect)
        {
            var labelRect = indentedNodeRect.AlignVertically(VerticalAlignmentType.Middle, DropdownStyle.LabelHeight);
            var text = Menu.IsInSearchMode ? SearchName : Name;
            if (HasChild)
            {
                labelRect.xMin -= DropdownStyle.GlobalOffset;
                EditorGUI.Foldout(labelRect, IsExpanded, text);
            }
            else
            {
                var style = IsSelected.Value ? DropdownStyle.SelectedLabelStyle : DropdownStyle.DefaultLabelStyle;
                GUI.Label(labelRect, text, style);
            }
        }

        protected virtual void DraeSeparator()
        {
            var lineRect = new Rect(Rect.x, Rect.y - 1f, Rect.width, 1f);
            EditorGUI.DrawRect(lineRect, DropdownStyle.DarkSeparatorLine);
            lineRect.y += 1;
            EditorGUI.DrawRect(lineRect, DropdownStyle.LightSeparatorLine);
        }

        protected virtual void HandleMouseEvent()
        {
            var isMouseLeftDown = Event.current.type == EventType.MouseDown
                && IsHovered
                && Event.current.button == 0;

            if (false == isMouseLeftDown)
                return;

            if (HasChild)
            {
                IsExpanded = !IsExpanded;
            }
            else
            {
                Menu.SelectedNode.Value = this;
                Menu.FinalizeSelection();
            }

            Event.current.Use();
        }

        #endregion

        #region Manipulate

        public DropdownNode<T> AddChild(string name, DropdownItem<T> item)
        {
            var node = new DropdownNode<T>(Menu, this, item.Value, name, item.FullName, item.Icon);
            if (item.IsSelected)
                Menu.SelectedNode.Value = node;

            base.AddChild(node);
            return node;
        }

        public DropdownNode<T> AddChildFolder(string name)
        {
            var node = new DropdownNode<T>(Menu, this, default, name, null, null);
            base.AddChild(node);
            return node;
        }

        #endregion
    }
}