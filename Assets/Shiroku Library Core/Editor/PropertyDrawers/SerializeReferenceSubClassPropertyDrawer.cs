using ShirokuStudio.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    [CustomPropertyDrawer(typeof(SubClassSelectorAttribute), true)]
    public class SerializeReferenceSubClassPropertyDrawer : PropertyDrawer
    {
        private static readonly GUIContent DisplayName_NULL = new(SubClassSelectorDropdownMenu.DisplayName_NULL);

        private readonly Dictionary<string, SubClassSelectorDropdownMenu> selectors = new();
        private readonly Dictionary<string, GUIContent> classes = new();
        private readonly Dictionary<Type, Type> baseTypes = new();

        private Type baseType => getBaseType();
        private Type currentType;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using var scope = new EditorGUI.PropertyScope(position, label, property);

            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.LabelField(position, label.text, "Non-ManagedReference type is not supported.");
                return;
            }

            var rect_foldout = new Rect(position) { height = EditorGUIUtility.singleLineHeight };
            rect_foldout = EditorGUI.IndentedRect(rect_foldout);
            var rect_dropdown = EditorGUI.PrefixLabel(rect_foldout, label);

            //selector
            if (EditorGUI.DropdownButton(rect_dropdown, getTypeName(property), FocusType.Keyboard))
            {
                SubClassSelectorDropdownMenu.Show(rect_dropdown,
                    baseType,
                    type => SubClassSelectorDropdownMenu.DefaultPropertyCallback(property, type),
                    currentType);
            }

            //foldout
            if (string.IsNullOrWhiteSpace(property.managedReferenceFullTypename) == false)
            {
                property.isExpanded = EditorGUI.Foldout(rect_foldout, property.isExpanded, GUIContent.none, true);
            }

            //inner properties
            if (property.isExpanded)
            {
                using var indentScope = new EditorGUI.IndentLevelScope();
                var type = EditorReflectionUtility.GetType(property.managedReferenceFullTypename);
                var customDrawer = EditorReflectionUtility.GetCustomPropertyDrawer(type);
                var rect_property = new Rect(position);
                rect_property.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                if (customDrawer != null)
                {
                    rect_property.height = customDrawer.GetPropertyHeight(property, label);
                    customDrawer.OnGUI(rect_property, property, label);
                }
                else
                {
                    foreach (var child in property.GetChildProperties())
                    {
                        rect_property.height
                            = Mathf.Max(EditorGUIUtility.singleLineHeight, EditorGUI.GetPropertyHeight(child, new GUIContent(child.displayName, child.tooltip)));
                        EditorGUI.PropertyField(rect_property, child, true);
                        rect_property.OffsetSelf(0, rect_property.height + EditorGUIUtility.standardVerticalSpacing);
                    }
                }
            }
        }

        private Type getBaseType()
        {
            var fieldType = fieldInfo.FieldType;
            if (baseTypes.TryGetValue(fieldType, out var type) == false)
            {
                if (typeof(IList).IsAssignableFrom(fieldType))
                {
                    type = fieldType.GetAncestors()
                        .Where(t => t.IsGenericType)
                        .FirstOrDefault(t => typeof(IList<>) == t.GetGenericTypeDefinition())
                        .GetGenericArguments()[0];
                }
                else
                {
                    type = fieldType;
                }

                baseTypes[fieldType] = type;
            }

            return type;
        }

        private GUIContent getTypeName(SerializedProperty property)
        {
            string fullTypeName = property.managedReferenceFullTypename;

            if (string.IsNullOrEmpty(fullTypeName))
                return DisplayName_NULL;

            if (classes.TryGetValue(fullTypeName, out GUIContent cachedTypeName))
                return cachedTypeName;

            currentType = EditorReflectionUtility.GetType(fullTypeName);
            var typeName = EditorReflectionUtility.GetTypeName(currentType);
            var result = new GUIContent(typeName);
            classes[fullTypeName] = result;
            return result;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded == false)
                return EditorGUIUtility.singleLineHeight;

            var customDrawer = EditorReflectionUtility.GetCustomPropertyDrawer(property);
            return customDrawer != null
                ? customDrawer.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing
                : EditorGUI.GetPropertyHeight(property, true);
        }
    }
}