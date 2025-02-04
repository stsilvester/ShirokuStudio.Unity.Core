using ShirokuStudio.Core.Models;
using System;
using UnityEditor;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    public static class EditorExtensions
    {
        public static void AddSeparator(this GenericMenu menu)
        {
            menu.AddSeparator("");
        }

        public static void AddLabel(this GenericMenu menu, string label, bool isOn = false)
        {
            menu.AddDisabledItem(new GUIContent(label), isOn);
        }

        public static void AddFuncItem(this GenericMenu menu, string name, GenericMenu.MenuFunction callback, bool enabled = true, bool isOn = false)
        {
            if (enabled)
            {
                menu.AddItem(new GUIContent(name), isOn, callback);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(name), isOn);
            }
        }

        public static void AddFuncItem(this GenericMenu menu, string name, GenericMenu.MenuFunction2 callback, object data, bool enabled = true, bool isOn = false)
        {
            if (enabled)
            {
                menu.AddItem(new GUIContent(name), isOn, callback, data);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(name), isOn);
            }
        }

        public static void AddFuncItem<T>(this GenericMenu menu, string name, T data, Action<T> callback, bool enabled = true, bool isOn = false)
        {
            if (enabled)
            {
                menu.AddItem(new GUIContent(name), isOn, obj => callback((T)obj), data);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(name), isOn);
            }
        }

        /// <summary>
        /// 將目前的 Rect 向下移動指定行數
        /// </summary>
        /// <param name="rect">目前的 Rect</param>
        /// <param name="lines">行數</param>
        public static void MoveNextLine(this ref Rect rect, int lines = 1)
        {
            var offset = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * lines;
            rect.y += offset;
        }

        public static Rect Expand(this ref Rect rect, RectOffset offset)
        {
            rect = offset.Add(rect);
            return rect;
        }

        public static Rect Expand(this ref Rect rect, float left = 0, float top = 0, float right = 0, float bottom = 0)
        {
            rect = new Rect(rect.x - left, rect.y - top, rect.width + left + right, rect.height + top + bottom);
            return rect;
        }
    }
}