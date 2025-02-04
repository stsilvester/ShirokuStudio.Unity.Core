using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    public static partial class CustomEditorGUILayout
    {
        public static bool Toggle(string label,
            ReactiveProperty<bool> property,
            GUIStyle style = null,
            params GUILayoutOption[] options)
            => Toggle(label, property.Value, v => property.Value = v, style, options);

        public static void DropdownWindow<T>(string label,
            ReactiveProperty<T> property,
            IEnumerable<T> source,
            Expression<Func<T, string>> name,
            GUIStyle style = null,
            params GUILayoutOption[] options)
        {
            using var h = new GUILayout.HorizontalScope();
            EditorGUILayout.PrefixLabel(label);
            label = Equals(property.Value, null) ? "" : name.Compile().Invoke(property.Value);
            DropdownWindow(label, source, name, property.Value, v => property.Value = v, style, options);
        }
    }
}