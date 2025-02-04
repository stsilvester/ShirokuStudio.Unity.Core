using ShirokuStudio.Core.Reflection;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace ShirokuStudio.Editor
{
    public static class CustomEditorUtility
    {
        public static Vector2 GetCurrentMousePosition()
        {
            return FastCacher<string, Func<Vector2>>.Get(nameof(GetCurrentMousePosition),
                (key) =>
                {
                    var m = typeof(UnityEditor.Editor).GetMethod("GetCurrentMousePosition", BindingFlags.Static | BindingFlags.NonPublic);
                    Assert.IsNotNull(m);
                    return m.CreateDelegate(typeof(Func<Vector2>)) as Func<Vector2>;
                }).Invoke();
        }

        //unity editor icon references: https://github.com/halak/unity-editor-icons

        public static GUIContent GetGUIContent(string text = null, Texture icon = null, string tooltip = null)
        {
            return FastCacher<(string text, string tooltip, Texture icon), GUIContent>
                .Get((text, tooltip, icon), (key) => new GUIContent(key.text, key.icon, key.tooltip));
        }

        public static Texture GetEditorIcon(string name)
        {
            return FastCacher<string, Texture>.Get(name, (key) => EditorGUIUtility.IconContent(key).image);
        }

        public static GUIContent GetIconContent(string name)
        {
            var icon = GetEditorIcon(name);
            return GetGUIContent(icon: icon);
        }

        public static void PingObject(UnityEngine.Object target)
        {
            if (target is GameObject go)
            {
                Selection.activeGameObject = go;
                EditorGUIUtility.PingObject(go);
            }
            else
            {
                Selection.activeObject = target;
                EditorGUIUtility.PingObject(target);
            }
        }
    }
}