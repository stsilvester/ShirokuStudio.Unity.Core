using System;
using UnityEditor;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    public static partial class CustomEditorGUI
    {
        public static object ValueField(Rect rect, Type type, object value)
        {
            try
            {
                using var check = new EditorGUI.ChangeCheckScope();
                object result = value;

                if (type.IsEnum)
                    result = EditorGUI.EnumPopup(rect, (Enum)value);
                else if (type == typeof(bool))
                    result = EditorGUI.Toggle(rect, ((bool?)value) ?? false);
                else if (type == typeof(int))
                    result = EditorGUI.IntField(rect, ((int?)value) ?? 0);
                else if (type == typeof(float))
                    result = EditorGUI.FloatField(rect, (float?)value?? 0);
                else if (type == typeof(string))
                    result = EditorGUI.TextField(rect, (string)value ?? "");
                
                return check.changed ? result : value;
            }
            catch (Exception ex)
            {
                throw new Exception("GUI FAIELD");
            }
        }
    }
}