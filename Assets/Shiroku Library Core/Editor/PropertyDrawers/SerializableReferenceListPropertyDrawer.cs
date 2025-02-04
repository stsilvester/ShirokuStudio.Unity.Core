using ShirokuStudio.Core;
using ShirokuStudio.Core.Models;
using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    [CustomPropertyDrawer(typeof(SerializableReferenceListBase), true)]
    public class SerializableReferenceListPropertyDrawer : PropertyDrawer
    {
        private bool initialized = false;
        private ReorderableList reorderableList;
        private Type baseType;
        private SerializedProperty currentProperty;
        private SerializedProperty property_list;
        private static Lazy<GUIStyle> style_footer = new(() => new GUIStyle("RL Footer"));
        private static Lazy<GUIStyle> style_btn_add = new(() => new GUIStyle("ToolbarCreateAddNewDropDown"));
        private static Lazy<GUIStyle> style_btn_remove = new(() => new GUIStyle("RL FooterButton"));

        private void initialize(SerializedProperty property)
        {
            if (initialized)
                return;

            initialized = true;
            property_list = property.FindPropertyRelative("_list");
            var listType = fieldInfo.FieldType;
            baseType = listType.GetAncestors()
                .Where(t => t.IsGenericType)
                .FirstOrDefault(t => typeof(SerializableReferenceList<>) == t.GetGenericTypeDefinition())
                .GetGenericArguments()[0];

            var list = new ReorderableList(property.serializedObject, property_list, true, false, true, true);
            list.drawFooterCallback = drawFooter;
            list.drawElementCallback = drawElement;
            list.elementHeightCallback = getElementHeight;

            reorderableList = list;

            #region local functions

            void drawFooter(Rect rect)
            {
                var rect_remove = new Rect(rect);
                rect_remove.xMax -= 6f;
                rect_remove.xMin = rect_remove.xMax - 24f;

                var rect_add = new Rect(rect_remove).Offset(-40, 0);
                rect_add.width = 32f;

                var rect_box = new Rect(rect_add).Offset(-4, 0);
                rect_box.xMax = rect_remove.xMax;

                GUI.Box(rect_box, GUIContent.none, style_footer.Value);
                if (GUI.Button(rect_add, "＋", style_btn_add.Value))
                {
                    SubClassSelectorDropdownMenu.Show(rect_add, baseType, t =>
                    {
                        if (t == null && baseType.IsAssignableFrom(t) == false)
                            return;

                        property_list.arraySize++;
                        var element = property_list.GetArrayElementAtIndex(property_list.arraySize - 1);
                        element.managedReferenceValue = Activator.CreateInstance(t);
                        currentProperty.serializedObject.ApplyModifiedProperties();
                    });
                }
                if (GUI.Button(rect_remove, "－", style_btn_remove.Value))
                {
                    var selected = list.index;
                    if (selected >= 0 && property_list.arraySize > selected)
                    {
                        property_list.DeleteArrayElementAtIndex(selected);
                        list.index = Mathf.Clamp(selected - 1, 0, property_list.arraySize - 1);
                        currentProperty.serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            float getElementHeight(int index)
            {
                var element = property_list.GetArrayElementAtIndex(index);
                var type = EditorReflectionUtility.GetType(element.managedReferenceFullTypename);
                var customDrawer = EditorReflectionUtility.GetCustomPropertyDrawer(type);
                if (customDrawer != null)
                {
                    return customDrawer.GetPropertyHeight(element, new GUIContent(element.displayName, element.tooltip));
                }
                else
                {
                    var height = EditorGUI.GetPropertyHeight(element, new GUIContent(element.displayName, element.tooltip));
                    return height;
                }
            }

            void drawElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                rect.xMin += 10;
                var element = property_list.GetArrayElementAtIndex(index);
                var type = EditorReflectionUtility.GetType(element.managedReferenceFullTypename);
                var customDrawer = EditorReflectionUtility.GetCustomPropertyDrawer(type);
                if (customDrawer != null)
                {
                    customDrawer.OnGUI(rect, element, new GUIContent(element.displayName, element.tooltip));
                }
                else
                {
                    EditorGUI.PropertyField(rect, element, new GUIContent(element.displayName, element.tooltip), true);
                }
            }

            #endregion
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            currentProperty = property;
            using var p = new EditorGUI.PropertyScope(position, label, property);
            initialize(property);

            var height = reorderableList.GetHeight();
            var rect_foldout = EditorGUI.IndentedRect(new Rect(position.x, position.y, position.width - 50, EditorGUIUtility.singleLineHeight));
            property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(rect_foldout, property.isExpanded, label);
            var rect_arrayLSize = new Rect(rect_foldout.xMax, rect_foldout.y, 50, rect_foldout.height);
            EditorGUI.BeginChangeCheck();
            var size = EditorGUI.DelayedIntField(rect_arrayLSize, property_list.arraySize);
            if (EditorGUI.EndChangeCheck())
            {
                property_list.arraySize = size;
                property.serializedObject.ApplyModifiedProperties();
            }

            if (property.isExpanded)
            {
                var rect_list = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, height);
                reorderableList.DoList(rect_list);
                property.serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.EndFoldoutHeaderGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            initialize(property);
            if (property.isExpanded == false)
                return EditorGUIUtility.singleLineHeight;

            return reorderableList.GetHeight() + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}