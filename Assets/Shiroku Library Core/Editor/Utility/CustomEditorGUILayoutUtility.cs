using ShirokuStudio.Core;
using ShirokuStudio.Core.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    public static partial class CustomEditorGUILayoutUtility
    {
        public static Rect GetLastRect()
        {
            var _rect = GUILayoutUtility.GetLastRect();
            var prevID = GUIUtility.GetControlID(FocusType.Passive);
            var rect = FastCacher<int, Rect>.Get(prevID, id => new Rect(0, 0, 1, 1));
            if (Event.current.type == EventType.Repaint)
            {
                rect = _rect;
                FastCacher<int, Rect>.Set(prevID, rect);
            }

            return rect;
        }

        /// <summary>
        /// get rect for this layout component
        /// </summary>
        public static Rect GetRect(params GUILayoutOption[] options)
        {
            return GetRect("", null, options);
        }

        /// <summary>
        /// get rect for this layout component
        /// </summary>
        public static Rect GetRect(string label, GUIStyle style, params GUILayoutOption[] options)
        {
            if (string.IsNullOrWhiteSpace(label))
                label = "";

            style ??= GUI.skin.box;

            return GetRect(new GUIContent(label), style, options);
        }

        /// <summary>
        /// get rect for this layout component
        /// </summary>
        public static Rect GetRect(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            var _rect = GUILayoutUtility.GetRect(content, style, options);
            var prevID = GUIUtility.GetControlID(FocusType.Passive);
            var rect = FastCacher<int, Rect>.Get(prevID, id => new Rect(0, 0, 1, 1));
            if (Event.current.type == EventType.Repaint)
            {
                rect = _rect;
                FastCacher<int, Rect>.Set(prevID, rect);
            }

            return rect;
        }

        public static float CalculateProperWidth(
            IEnumerable<IGUIEntry> entries,
            int indentOffset,
            int padding = 0,
            GUIStyle containerStyle = null)
        {
            containerStyle ??= EditorStyles.helpBox;
            var width = containerStyle.CalcSize(CustomEditorUtility.GetGUIContent()).x + padding;
            if (entries?.Any() != true)
                return width;

            var style = EditorStyles.label;
            return width
                + entries.Max(e => style.CalcSize(CustomEditorUtility.GetGUIContent(e.Name, e.Icon)).x
                + (e.Name.Split('/').Length - 1) * indentOffset);
        }

        public static List<Rect> GetFlowLayoutedRects(Rect rect, GUIStyle style, float horizontalSpacing, float verticalSpacing, List<string> items)
        {
            if (Event.current.type == EventType.Layout)
                return new();

            var rects = EditorGUIUtility.GetFlowLayoutedRects(rect, style, horizontalSpacing, verticalSpacing, items);
            if (Event.current.type != EventType.Repaint)
                return rects;

            var width = Mathf.Max(rects.Max(r => r.xMax), rect.xMax) - rect.xMin;
            var yMin = rects.Min(r => r.yMin);
            var yMax = rects.Max(r => r.yMax);
            var height = yMax > 0 ? yMax - rect.yMin - rect.height : 0;
            GetRect(GUILayout.Width(width), GUILayout.Height(yMax - yMin));
            Debug.Log($"flow rect: {new { rect, width, height, yMin, yMax }}");
            return rects;
        }
    }
}