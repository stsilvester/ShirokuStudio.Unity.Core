using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    public partial class DropdownWindow : EditorWindow
    {
        private DropdownMenu menu;
        private float contentHeight;
        private Vector2 size;

        public static DropdownWindow ShowAsContext(DropdownMenu menu, int windowHeight = 0)
        {
            var window = CreateInstance<DropdownWindow>();
            window.initialize(menu, windowHeight, CustomEditorUtility.GetCurrentMousePosition());
            window.ShowPopup();
            menu.FocusSearchBar = true;
            return window;
        }

        public static DropdownWindow ShowDropdown(Rect rect, DropdownMenu menu, int windowHeight = 0)
        {
            var window = CreateInstance<DropdownWindow>();
            window.initialize(menu, windowHeight, CustomEditorUtility.GetCurrentMousePosition());
            rect = GUIUtility.GUIToScreenRect(rect);
            window.ShowAsDropDown(rect, window.size);
            menu.FocusSearchBar = true;
            return window;
        }

        protected void initialize(DropdownMenu menu, int windowHeight, Vector2 mousePosition)
        {
            GUIUtility.hotControl = 0;
            GUIUtility.keyboardControl = 0;

            this.menu = menu;
            menu.OnComplete += Close;

            wantsMouseMove = true;
            var width = CustomEditorGUILayoutUtility.CalculateProperWidth(
                menu.Items, (int)DropdownStyle.IndentWidth, 40);
            width = Mathf.Max(width, 150);

            if (windowHeight == 0)
                windowHeight = Mathf.Min(Mathf.CeilToInt((menu.Items.Count() + 2) * DropdownStyle.NodeHeight) + 10, DropdownStyle.WindowMaxHeight);

            size = new Vector2(Mathf.Max(40, width), Mathf.Max(10f, windowHeight));
            position = new Rect(mousePosition, size);
        }

        private void Update()
        {
            if (Mathf.Abs(contentHeight - position.height) > 10f)
            {
                position = new Rect(position.x, position.y, position.width, contentHeight);
            }
        }

        private void OnGUI()
        {
            if (menu == null)
                Close();

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                Close();
                Event.current.Use();
            }

            menu.VisibleRect = new(Vector2.zero, position.size);
            var v = EditorGUILayout.BeginVertical(DropdownStyle.Box);
            menu.Draw();
            EditorGUILayout.EndVertical();

            if (Event.current.type == EventType.Repaint)
                contentHeight = v.height;

            if (Event.current.isMouse || Event.current.type == EventType.Used)
                Repaint();
        }

        public void OnLostFocus()
        {
            Close();
        }
    }
}