using UnityEditor;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    public class CustomGUIOption
    {
        public float? LabelWidth;
        public GUILayoutOption[] LabelOptions;
        public GUILayoutOption[] FieldOptions;
        public GUILayoutOption[] ScopeOptions;
        public GUIStyle LabelStyle = EditorStyles.label;

        public static CustomGUIOption Default = new();
    }
}
