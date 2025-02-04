using ShirokuStudio.Core.Models;
using ShirokuStudio.Core.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShirokuStudio.Editor
{
    [CustomPropertyDrawer(typeof(SerializableType), true)]
    public class SerializableTypePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var isInArray = property.propertyPath.Contains(".Array.data[");
            label = isInArray ? GUIContent.none : label;
            var rect_field = EditorGUI.PrefixLabel(position, label);
            var p = property.FindPropertyRelative("typeName");
            var typeName = p.stringValue;
            var displayName = Core.Reflection.TypeCache.GetType(typeName)?.GetFriendlyName() ?? "None";
            if (EditorGUI.DropdownButton(rect_field, new GUIContent(displayName), FocusType.Passive))
            {
                var attr = fieldInfo.GetCustomAttribute<SupportTypeAttribute>(true);
                new DropdownMenu<Type>(createOptions(attr?.Type, attr), type =>
                {
                    p.stringValue = type.AssemblyQualifiedName;
                    p.serializedObject.ApplyModifiedProperties();
                }).ShowAsDropdown(rect_field, 300);
            }

            EditorGUI.EndProperty();
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Row;

            var label = property.GetLabel();
            var dropdown = new DropdownMenuField<Type>(label);
            dropdown.RegisterValueChangedCallback(e =>
            {
                var p = property.FindPropertyRelative("typeName");
                p.stringValue = e.newValue.AssemblyQualifiedName;
                p.serializedObject.ApplyModifiedProperties();
            });

            var inst = property.GetValue<SerializableType>();
            var attr = fieldInfo.GetCustomAttribute<SupportTypeAttribute>(true);
            if (attr != null)
                dropdown.Choices = createOptions(attr.Type, attr);
            dropdown.value = inst.Type;

            root.Add(dropdown);

            return root;
        }

        private List<DropdownItem<Type>> createOptions(Type type, ITypeFilter filter)
        {
            var types = Core.Reflection.TypeCache.GetAssignablesFrom(type, filter);
            return types.Select(t => new DropdownItem<Type>(t.GetFriendlyName(), t)).ToList();
        }
    }
}