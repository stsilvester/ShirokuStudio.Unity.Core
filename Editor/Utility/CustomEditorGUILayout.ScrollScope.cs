using ShirokuStudio.Core.Reflection;
using UnityEditor;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    public static partial class CustomEditorGUILayout
    {
        public class ScrollScope : EditorGUILayout.ScrollViewScope
        {
            public readonly string Key;

            public ScrollScope(string key, params GUILayoutOption[] options)
                : base(getCached(key), options)
            {
                Key = key;
            }

            public ScrollScope(string key,
                bool alwaysShowHorizontal,
                bool alwaysShowVertical,
                GUIStyle horizontalScrollbar,
                GUIStyle verticalScrollbar,
                GUIStyle background,
                params GUILayoutOption[] options)
                : base(getCached(key), alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbar, verticalScrollbar, background, options)
            {
                Key = key;
            }

            protected override void CloseScope()
            {
                base.CloseScope();
                setCached(Key, scrollPosition);
            }

            private const string prefix = "scroll-scope-";

            private void setCached(string key, Vector2 scrollPosition)
            {
                FastCacher<string, Vector2>.Set(prefix + key, scrollPosition);
            }

            private static Vector2 getCached(string key)
            {
                return FastCacher<string, Vector2>.Get(prefix + key, _ => new Vector2());
            }
        }
    }
}