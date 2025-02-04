using ShirokuStudio.Core;
using ShirokuStudio.Core.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    using NAEditor = NaughtyAttributes.Editor;

    public static partial class CustomEditorGUILayout
    {
        public static void Label(string label, CustomGUIOption option = null)
        {
            option ??= CustomGUIOption.Default;
            var w = EditorGUIUtility.labelWidth;
            if (option.LabelWidth.HasValue)
                EditorGUIUtility.labelWidth = option.LabelWidth.Value;

            GUILayout.Label(label, option.LabelStyle, option.LabelOptions);
            EditorGUIUtility.labelWidth = w;
        }

        public static void PrefixLabel(string label, CustomGUIOption option = null)
        {
            if (string.IsNullOrWhiteSpace(label))
                return;

            option ??= CustomGUIOption.Default;
            var w = EditorGUIUtility.labelWidth;
            if (option.LabelWidth.HasValue)
                EditorGUIUtility.labelWidth = option.LabelWidth.Value;

            EditorGUILayout.PrefixLabel(label, option.LabelStyle);
            EditorGUIUtility.labelWidth = w;
        }

        public static TEnum EnumPopup<TEnum>(string label, TEnum value, CustomGUIOption option = null)
            where TEnum : Enum
        {
            option ??= CustomGUIOption.Default;
            using var h = new EditorGUILayout.HorizontalScope(option.ScopeOptions);
            PrefixLabel(label, option);
            return (TEnum)EditorGUILayout.EnumPopup(value, option.FieldOptions);
        }

        public static TEnum EnumFlagPopup<TEnum>(string label, TEnum value, CustomGUIOption option = null)
            where TEnum : Enum
        {
            option ??= CustomGUIOption.Default;
            using var h = new EditorGUILayout.HorizontalScope(option.ScopeOptions);
            PrefixLabel(label, option);
            return (TEnum)EditorGUILayout.EnumFlagsField(value, option.FieldOptions);
        }

        public static int IntPopup(int value, int[] optionValues, CustomGUIOption option = null)
        {
            option ??= CustomGUIOption.Default;
            var options = optionValues.Select(i => i.ToString()).ToArray();
            var selected = Math.Max(0, Array.IndexOf(optionValues, value));
            var result = EditorGUILayout.Popup(selected, options, option.FieldOptions);
            return result < 0 ? value : optionValues[result];
        }

        public static int IntPopup(string label, int value, int[] optionValues, CustomGUIOption option = null)
        {
            option ??= CustomGUIOption.Default;
            var options = optionValues.Select(i => i.ToString()).ToArray();
            var selected = Math.Max(0, Array.IndexOf(optionValues, value));
            var result = EditorGUILayout.Popup(label, selected, options, option.FieldOptions);
            return result < 0 ? value : optionValues[result];
        }

        public static void Tooltip(string text)
        {
            var propRect = GUILayoutUtility.GetLastRect();
            GUI.Label(propRect, new GUIContent("", text));
        }

        public static void VerticalSeparator()
        {
            VerticalSeparator(1, Color.black);
        }

        public static void VerticalSeparator(int width, Color color)
        {
            var rect = GUILayoutUtility.GetLastRect();
            EditorGUI.DrawRect(new Rect(rect.xMax, rect.y, width, rect.height), color);
        }

        public static bool PropertyField(SerializedProperty property, params GUILayoutOption[] optins)
        {
            return PropertyField(property, true, optins);
        }

        public static bool PropertyField(SerializedProperty property, bool includeChildren, params GUILayoutOption[] optins)
        {
            if (NAEditor.PropertyUtility.IsVisible(property) == false)
                return false;

            using (new EditorGUI.DisabledGroupScope(!NAEditor.PropertyUtility.IsEnabled(property)))
            {
                var label = NAEditor.PropertyUtility.GetLabel(property);
                return EditorGUILayout.PropertyField(property, label, includeChildren, optins);
            }
        }

        public static T ObjectField<T>(string label, T value, bool allowSceneObject = true)
            where T : UnityEngine.Object
        {
            return EditorGUILayout.ObjectField(label, value, typeof(T), allowSceneObject) as T;
        }

        #region Draggable Field

        private static int dragID;
        private static float dragSensitive = 0.1f;

        public static int IntField(string label, int value, CustomGUIOption option = null)
        {
            option ??= CustomGUIOption.Default;
            EditorGUILayout.BeginHorizontal(option.ScopeOptions);
            PrefixLabel(label, option);
            var evt = Event.current;

            Rect labelRect = GUILayoutUtility.GetLastRect();
            var id = GUIUtility.GetControlID(FocusType.Keyboard, labelRect);
            if (evt.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition))
            {
                dragID = id;
                dragSensitive = calculateIntDragSensitivity(value);
                GUI.FocusControl(null);
                EditorGUIUtility.editingTextField = false;
                evt.Use();
            }
            else if (id == dragID)
            {
                switch (evt.type)
                {
                    case EventType.MouseUp:
                        dragID = 0;
                        evt.Use();
                        break;

                    case EventType.MouseDrag:
                        value += (int)Math.Round(HandleUtility.niceMouseDelta * dragSensitive);
                        evt.Use();
                        break;
                }
            }
            EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.SlideArrow);
            value = EditorGUILayout.IntField(value, option.FieldOptions);
            EditorGUILayout.EndHorizontal();

            return value;
        }

        private static int calculateIntDragSensitivity(int value)
        {
            return (int)Math.Max(1, Math.Pow(Math.Abs(value), 0.5) * .03f);
        }

        public static float FloatField(string label, float value, CustomGUIOption option = null)
        {
            option ??= CustomGUIOption.Default;
            EditorGUILayout.BeginHorizontal(option.ScopeOptions);
            Label(label, option);
            var evt = Event.current;
            Rect labelRect = GUILayoutUtility.GetLastRect();
            var id = GUIUtility.GetControlID(FocusType.Keyboard, labelRect);
            if (evt.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition))
            {
                dragID = id;
                dragSensitive = calculateFloatDragSensitivity(value);
                GUI.FocusControl(null);
                EditorGUIUtility.editingTextField = false;
                evt.Use();
            }
            else if (id == dragID)
            {
                switch (evt.type)
                {
                    case EventType.MouseUp:
                        dragID = 0;
                        evt.Use();
                        break;

                    case EventType.MouseDrag:
                        value += HandleUtility.niceMouseDelta * dragSensitive;
                        evt.Use();
                        break;
                }
            }
            EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.SlideArrow);
            value = EditorGUILayout.FloatField(value, option.FieldOptions);
            EditorGUILayout.EndHorizontal();
            return value;
        }

        private static float calculateFloatDragSensitivity(float value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
                return 0.0f;

            return (float)Math.Max(1, Math.Pow(Math.Abs(value), 0.5)) * 0.03f;
        }

        #endregion

        #region TagCloud

        public static string FlowLayoutTags(string value, bool showAddBtn = false, Action onAdd = null)
        {
            const string text_btn_add = "+";
            const string text_btn_remove = "X";
            const float width_btn_add = 24f;
            var style_wrap = CustomGUIStyles.TagWrap;
            var style_lb = CustomGUIStyles.TagLabel;
            var style_btn = CustomGUIStyles.TagButton;
            var tagValue = value ?? "";
            var currentTags = tagValue.Split(",").Where(t => string.IsNullOrWhiteSpace(t) == false).ToList();
            var canAdd = showAddBtn;
            var cellContents = currentTags.ToList();
            if (canAdd)
                cellContents.Add(text_btn_add);

            //Calc Total Width of items
            const float horizontalSpace = 2f;
            const float verticalSpace = 2f;
            var totalWidth = 0f;
            style_btn.CalcMinMaxWidth(new GUIContent(text_btn_remove), out var btnMin, out var btnMax);
            foreach (var item in currentTags)
            {
                style_lb.CalcMinMaxWidth(new GUIContent(item), out var min, out var max);
                totalWidth += min + horizontalSpace + btnMin;
            }

            if (canAdd)
            {
                var style_add = EditorStyles.miniButton;
                style_add.CalcMinMaxWidth(new GUIContent(text_btn_add), out var min, out var max);
                totalWidth += min + horizontalSpace;
            }

            //Get Field Width
            EditorGUILayout.BeginVertical();
            var rect = CustomEditorGUILayoutUtility.GetRect(GUILayout.MaxWidth(1600), GUILayout.MinHeight(1), GUILayout.Height(1));
            var lines = Mathf.CeilToInt(totalWidth / (rect.width));
            var height = EditorGUIUtility.singleLineHeight * lines
                + verticalSpace * (lines - 1);

            var rect_field = new Rect(rect.x, rect.y, rect.width, height);
            var cells = EditorGUIUtility.GetFlowLayoutedRects(rect_field, style_wrap, horizontalSpace, verticalSpace, cellContents);

            var expandHeight = Mathf.Max(cells.Max(c => c.yMax) - cells.Min(c => c.yMin) - rect.height, 0);
            CustomEditorGUILayoutUtility.GetRect(GUILayout.Height(expandHeight));
            EditorGUILayout.EndVertical();

            if (Event.current.type == EventType.Layout)
                return tagValue;

            //Draw
            GUI.Box(rect.Expand(bottom: expandHeight), "", GUI.skin.box);
            int i = 0;
            for (int tagIndex = 0; tagIndex < currentTags.Count; tagIndex++)
            {
                string tag = currentTags[tagIndex];
                var rect_tag = cells[i++];
                rect_tag.width -= btnMin;
                var rect_btn = rect_tag.Offset(rect_tag.width, 0);
                rect_btn.width = btnMin;
                GUI.Label(rect_tag, tag, style_lb);
                if (GUI.Button(rect_btn, text_btn_remove, style_btn))
                    currentTags.Remove(tag);
            }

            if (canAdd)
            {
                var rect_add = cells[i++];
                rect_add.width = width_btn_add;
                if (GUI.Button(rect_add, text_btn_add, EditorStyles.miniButton))
                    onAdd?.Invoke();
            }

            var result = string.Join(",", currentTags);
            GUI.changed = result != value;
            return result;
        }

        public static string TagCloud(string value, List<string> tags, GUIStyle style = null)
        {
            style ??= CustomGUIStyles.CloudTag;
            var tagValue = value;
            var currentTags = tagValue.Split(",").ToList();

            //get rect
            var rect = CustomEditorGUILayoutUtility.GetRect();

            //calc tags
            const float horizontalSpace = 2f;
            const float verticalSpace = 2f;
            var totalWidth = 0f;
            foreach (var item in tags)
            {
                style.CalcMinMaxWidth(new GUIContent(item), out var min, out var max);
                totalWidth += min + horizontalSpace;
            }
            var lines = Mathf.CeilToInt(totalWidth / (rect.width));
            var height = EditorGUIUtility.singleLineHeight * lines
                + verticalSpace * (lines - 1);
            var rect_field = new Rect(rect.x, rect.y, rect.width, height);

            //get expand rect
            CustomEditorGUILayoutUtility.GetRect(GUILayout.Height(height - rect.height));

            if (Event.current.type == EventType.Layout)
                return tagValue;

            //Draw Tags
            var cells = EditorGUIUtility.GetFlowLayoutedRects(rect_field, style, horizontalSpace, verticalSpace, tags);
            int i = 0;
            foreach (var tag in tags)
            {
                var isOn = currentTags.Contains(tag);
                EditorGUI.BeginChangeCheck();
                isOn = GUI.Toggle(cells[i++], isOn, tag, style);
                if (EditorGUI.EndChangeCheck())
                {
                    if (isOn)
                        currentTags.Add(tag);
                    else
                        currentTags.Remove(tag);
                }
            }

            var result = string.Join(",", currentTags);
            GUI.changed = result != value;
            return result;
        }

        public static void TagCloud(SerializedProperty property, List<string> tags, GUIStyle style = null)
        {
            using var h = new EditorGUILayout.HorizontalScope(GUIStyle.none);
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUILayout.HelpBox(new RichText($"property is not type of String value", Color.red), MessageType.Error);
                return;
            }

            style ??= CustomGUIStyles.CloudTag;
            var label = NAEditor.PropertyUtility.GetLabel(property);
            var tagValue = property.stringValue;
            var currentTags = tagValue.Split(",").ToList();

            //get rect
            var rect = CustomEditorGUILayoutUtility.GetRect(
                GUILayout.ExpandWidth(true),
                GUILayout.MinWidth(10),
                GUILayout.MinHeight(EditorGUIUtility.singleLineHeight));

            //calc label
            var rect_lb = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, rect.height);

            //calc tags
            const float horizontalSpace = 2f;
            const float verticalSpace = 2f;
            var totalWidth = 0f;
            foreach (var item in tags)
            {
                style.CalcMinMaxWidth(new GUIContent(item), out var min, out var max);
                totalWidth += min + horizontalSpace;
            }
            var lines = Mathf.CeilToInt(totalWidth / (rect.width - rect_lb.width));
            var height = EditorGUIUtility.singleLineHeight * lines
                + verticalSpace * (lines - 1);
            var rect_field = new Rect(rect_lb.xMax, rect.y, rect.width - rect_lb.width, height);

            //get expand rect
            CustomEditorGUILayoutUtility.GetRect(GUILayout.Height(height - rect.height));

            if (Event.current.type == EventType.Layout)
                return;

            //Draw Label
            EditorGUI.LabelField(rect_lb, label);

            //Draw Tags
            var cells = EditorGUIUtility.GetFlowLayoutedRects(rect_field, style, horizontalSpace, verticalSpace, tags);
            int i = 0;
            foreach (var tag in tags)
            {
                var isOn = currentTags.Contains(tag);
                EditorGUI.BeginChangeCheck();
                isOn = GUI.Toggle(cells[i++], isOn, tag, style);
                if (EditorGUI.EndChangeCheck())
                {
                    if (isOn)
                        currentTags.Add(tag);
                    else
                        currentTags.Remove(tag);

                    property.stringValue = string.Join(",", currentTags);
                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        public static void EnumTagCloud(SerializedProperty property,
            GUIStyle style = null,
            Vector2? spacing = null)
        {
            if (property.propertyType != SerializedPropertyType.Enum)
            {
                EditorGUILayout.HelpBox(new RichText($"property is not type of Enum value", Color.red), MessageType.Error);
                return;
            }

            var type = FastCacher<(Type type, string path), Type>.Get(
                (property.serializedObject.targetObject.GetType(), property.propertyPath),
                getFieldType);

            if (type.IsEnum == false
                || type.GetCustomAttributes<FlagsAttribute>().Any() == false)
            {
                EditorGUILayout.HelpBox(new RichText($"property is not type of Enum value", Color.red), MessageType.Error);
                return;
            }

            style ??= CustomGUIStyles.CloudTag;
            var map = EnumUtility.GetDisplayNameMap(type);
            var label = NAEditor.PropertyUtility.GetLabel(property);

            var rect = CustomEditorGUILayoutUtility.GetRect(
                GUILayout.ExpandWidth(true),
                GUILayout.MinWidth(EditorGUIUtility.fieldWidth),
                GUILayout.MinHeight(EditorGUIUtility.singleLineHeight));

            //label
            var rect_lb = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, rect.height);

            //tags
            var horizontalSpace = spacing?.x ?? 2f;
            var verticalSpace = spacing?.y ?? 2f;
            var totalWidth = 0f;
            foreach (var item in map.Values)
            {
                style.CalcMinMaxWidth(new GUIContent(item), out var min, out var max);
                totalWidth += min + horizontalSpace;
            }
            var lines = Mathf.CeilToInt(totalWidth / (rect.width - rect_lb.width));
            var height = EditorGUIUtility.singleLineHeight * lines
                + verticalSpace * (lines - 1);

            var rect_field = new Rect(rect_lb.xMax, rect.y, rect.width - rect_lb.width, height);

            var cells = EditorGUIUtility.GetFlowLayoutedRects(rect_field, style,
                horizontalSpace, verticalSpace, map.Values.ToList());

            var cloudHeight = cells.Max(c => c.yMax) - cells.Min(c => c.y);
            CustomEditorGUILayoutUtility.GetRect(GUILayout.Height(cloudHeight - EditorGUIUtility.singleLineHeight));

            EditorGUI.LabelField(rect_lb, label);
            int i = 0;
            foreach (var item in map.Keys)
            {
                var isOn = (property.enumValueFlag & (int)(object)item) > 0;
                var text = map[item];
                EditorGUI.BeginChangeCheck();
                isOn = GUI.Toggle(cells[i++], isOn, text, style);
                if (EditorGUI.EndChangeCheck())
                {
                    if (isOn)
                        property.enumValueFlag |= (int)(object)item;
                    else
                        property.enumValueFlag &= ~(int)(object)item;

                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            #region inner functions

            static Type getFieldType((Type type, string path) e) => e.type.GetField(e.path).FieldType;

            #endregion
        }

        #endregion

        public static TEnum Tabs<TEnum>(TEnum current,
            GUIStyle tabStyle = null,
            GUIStyle tabGroupStyle = null,
            params GUILayoutOption[] options)
            where TEnum : Enum
        {
            tabStyle ??= CustomGUIStyles.Tab;
            tabGroupStyle ??= CustomGUIStyles.TabGroup;
            var tabs = EnumUtility.GetDisplayNameMap<TEnum>();

            EditorGUILayout.BeginHorizontal(tabGroupStyle);

            foreach (var tab in tabs)
                if (GUILayout.Toggle(tab.Key.Equals(current), tab.Value, tabStyle, options))
                    current = tab.Key;

            EditorGUILayout.EndHorizontal();

            return current;
        }

        public static T Popup<T>(T value, IEnumerable<KeyValuePair<string, T>> displayedOptions, params GUILayoutOption[] options)
        {
            var _displayedOptions = displayedOptions.Select(pair => pair.Key).ToArray();
            var index = displayedOptions.IndexOf(pair => Equals(pair.Value, value));
            var newIndex = EditorGUILayout.Popup(Mathf.Clamp(index, 0, _displayedOptions.Length), _displayedOptions, options);
            return displayedOptions.ElementAtOrDefault(newIndex).Value;
        }

        public static void Button(string text, Action onClick, params GUILayoutOption[] options)
            => Button(text, onClick, style: null, options: options);

        public static void Button(string text, Action onClick, GUIStyle style = null, params GUILayoutOption[] options)
        {
            style ??= GUI.skin.button;
            var content = new GUIContent(text);
            var rect = CustomEditorGUILayoutUtility.GetRect(content, style, options);
            rect = EditorGUI.IndentedRect(rect);
            if (GUI.Button(rect, content, style))
                onClick?.Invoke();
        }

        public static bool Toggle(string text, bool value, Action<bool> onChanged, GUIStyle style = null, params GUILayoutOption[] options)
        {
            style ??= GUI.skin.toggle;
            EditorGUI.BeginChangeCheck();
            var content = new GUIContent(text);
            var rect = CustomEditorGUILayoutUtility.GetRect(text, style, options);
            var result = GUI.Toggle(rect, value, content, style);
            if (EditorGUI.EndChangeCheck())
                onChanged.Invoke(result);
            return result;
        }

        public static void DropdownWindow<T>(GUIContent content,
            IEnumerable<DropdownItem<T>> items,
            Action<T> onSelected,
            GUIStyle style = null, params GUILayoutOption[] options)
        {
            style ??= EditorStyles.popup;
            var rect = CustomEditorGUILayoutUtility.GetRect(content, style, options);
            CustomGUI.Button(rect, content, () => DropdownMenu<T>.ShowDropdown(rect, items, onSelected));
        }

        public static void DropdownWindow<T>(string text,
            IEnumerable<DropdownItem<T>> items,
            Action<T> onSelected,
            GUIStyle style = null, params GUILayoutOption[] options)
            => DropdownWindow(new GUIContent(text), items, onSelected, style, options);

        public static void DropdownWindow<T>(string text,
            IEnumerable<T> source, Expression<Func<T, string>> name, T selectedValue,
            Action<T> onSelected,
            GUIStyle style = null, params GUILayoutOption[] options)
            => DropdownWindow(text, DropdownItem<T>.Parse(source, name, selectedValue),
                onSelected, style, options);

        public static void FoldoutScope(string label, Action onContentGUI = null, bool foldout = true, Action<Rect> menuAction = null, GUIStyle style = null)
        {
            var id = GUIUtility.GetControlID(FocusType.Passive);
            var key = $"CustomGUI.Foldout.{label}";
            foldout = EditorPrefs.GetBool(key, foldout);
            style ??= EditorStyles.foldoutHeader;

            var newFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, label, style, menuAction);
            if (newFoldout != foldout)
                EditorPrefs.SetBool(key, newFoldout);
            EditorGUILayout.EndFoldoutHeaderGroup();

            using (var indent = new EditorGUI.IndentLevelScope(1))
            {
                if (newFoldout)
                    onContentGUI?.Invoke();
            }
        }

        public static object DynamicObjectInspector(object val, string label = null)
        {
            var modelType = val.GetType();
            var members = FastCacher.GetMemberInfos(modelType)
                .Where(m => m is FieldInfo || m is PropertyInfo)
                .ToArray();

            if (modelType.IsPrimitive)
            {
                var lb = string.IsNullOrWhiteSpace(label) ? GUIContent.none : new GUIContent(label);
                switch (Type.GetTypeCode(modelType))
                {
                    case TypeCode.Boolean:
                        val = EditorGUILayout.Toggle(lb, (bool)val);
                        break;

                    case TypeCode.Decimal:
                        val = (decimal)EditorGUILayout.DoubleField(lb, (double)(decimal)val);
                        break;

                    case TypeCode.Double:
                        val = EditorGUILayout.DoubleField(lb, (double)val);
                        break;

                    case TypeCode.Int16:
                        val = EditorGUILayout.IntField(lb, (short)val);
                        break;

                    case TypeCode.Int32:
                        val = EditorGUILayout.IntField(lb, (int)val);
                        break;

                    case TypeCode.Int64:
                        val = EditorGUILayout.LongField(lb, (long)val);
                        break;

                    case TypeCode.Single:
                        val = EditorGUILayout.FloatField(lb, (float)val);
                        break;

                    case TypeCode.String:
                        val = EditorGUILayout.TextField(lb, (string)val);
                        break;
                }

                return val;
            }

            EditorGUILayout.LabelField(label ?? modelType.Name, EditorStyles.boldLabel);

            using var indent = new EditorGUI.IndentLevelScope();
            foreach (var m in members)
            {
                EditorGUI.BeginChangeCheck();
                var memberType = FastCacher.GetMemberType(modelType, m.Name);
                var typeCode = Type.GetTypeCode(memberType);
                var value = FastCacher.Get(modelType, val, m.Name);

                switch (typeCode)
                {
                    case TypeCode.Object:
                        if (typeof(object) == memberType)
                        {
                            continue;
                        }
                        if (typeof(UnityEngine.Object).IsAssignableFrom(memberType))
                        {
                            value = EditorGUILayout.ObjectField(m.Name, (UnityEngine.Object)value, memberType, allowSceneObjects: true);
                        }
                        else
                        {
                            value = DynamicObjectInspector(value, m.Name);
                        }
                        break;

                    case TypeCode.Boolean:
                        value = EditorGUILayout.Toggle(m.Name, (bool)value);
                        break;

                    case TypeCode.Decimal:
                        value = (decimal)EditorGUILayout.DoubleField(m.Name, (double)(decimal)value);
                        break;

                    case TypeCode.Double:
                        value = EditorGUILayout.DoubleField(m.Name, (double)value);
                        break;

                    case TypeCode.Int16:
                        value = EditorGUILayout.IntField(m.Name, (short)value);
                        break;

                    case TypeCode.Int32:
                        value = EditorGUILayout.IntField(m.Name, (int)value);
                        break;

                    case TypeCode.Int64:
                        value = EditorGUILayout.LongField(m.Name, (long)value);
                        break;

                    case TypeCode.Single:
                        value = EditorGUILayout.FloatField(m.Name, (float)value);
                        break;

                    case TypeCode.String:
                        value = EditorGUILayout.TextField(m.Name, (string)value);
                        break;
                }

                if (EditorGUI.EndChangeCheck())
                {
                    FastCacher.Set(modelType, val, m.Name, value);
                }
            }

            return val;
        }
    }
}