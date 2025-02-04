using ShirokuStudio.Core.Components;
using UnityEditor;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    [CustomPropertyDrawer(typeof(SetParameter.AnimatorParameterValue))]
    public class AnimatorParameterValuePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Type"));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            var labelPosition = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            if (string.IsNullOrWhiteSpace(label.text))
            {
                labelPosition.width = 0;
            }
            else
            {
                EditorGUI.PropertyField(labelPosition, property, label, false);
            }

            var r_type = new Rect(labelPosition.xMax, labelPosition.y, (position.width - labelPosition.width) / 2, position.height);
            var r_value = new Rect(r_type.xMax, labelPosition.y, r_type.width, r_type.height);

            var p_type = property.FindPropertyRelative("Type");
            var type = p_type.GetEnumValue<AnimatorControllerParameterType>();

            SerializedProperty p_value = null;
            switch (type)
            {
                default:
                case AnimatorControllerParameterType.Float:
                    p_value = property.FindPropertyRelative("FloatValue");
                    p_type.SetEnumValue(EditorGUI.EnumPopup(r_type, p_type.GetEnumValue<AnimatorControllerParameterType>()));
                    p_value.floatValue = EditorGUI.FloatField(r_value, p_value.floatValue);
                    break;

                case AnimatorControllerParameterType.Int:
                    p_value = property.FindPropertyRelative("IntValue");
                    p_type.SetEnumValue(EditorGUI.EnumPopup(r_type, p_type.GetEnumValue<AnimatorControllerParameterType>()));
                    p_value.intValue = EditorGUI.IntField(r_value, p_value.intValue);
                    break;

                case AnimatorControllerParameterType.Bool:
                    p_value = property.FindPropertyRelative("BoolValue");
                    p_type.SetEnumValue(EditorGUI.EnumPopup(r_type, p_type.GetEnumValue<AnimatorControllerParameterType>()));
                    p_value.boolValue = EditorGUI.Toggle(r_value, p_value.boolValue);
                    break;

                case AnimatorControllerParameterType.Trigger:
                    r_type.width = position.width - labelPosition.width;
                    p_type.SetEnumValue(EditorGUI.EnumPopup(r_type, p_type.GetEnumValue<AnimatorControllerParameterType>()));
                    break;
            }

            EditorGUI.EndProperty();
        }
    }
}