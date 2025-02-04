using System;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    public static partial class CustomGUI
    {
        public static bool Button(Rect rect, string text, Action onClick, GUIStyle style = null)
            => Button(rect, new GUIContent(text), onClick, style);

        public static bool Button(Rect rect, GUIContent content, Action onClick, GUIStyle style = null)
        {
            if (GUI.Button(rect, content, style ?? GUI.skin.button))
            {
                onClick?.Invoke();
                return true;
            }
            return false;
        }
    }
}