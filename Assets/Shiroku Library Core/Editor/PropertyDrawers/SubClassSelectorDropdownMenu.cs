using ShirokuStudio.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    public class SubClassSelectorDropdownMenu
    {
        private class Cache : DataCacher<Cache, Type, SubClassSelectorDropdownMenu>
        {
            protected override SubClassSelectorDropdownMenu CreateCache(Type baseType)
            {
                return new SubClassSelectorDropdownMenu(baseType);
            }
        }

        public const string DisplayName_NULL = "<null>";

        public Action<Type> OnSelected { get; set; }
        public SerializedProperty TargetProperty { get; set; }

        private readonly DropdownItem<Type>[] options;
        private readonly DropdownMenu<Type> menu;
        private readonly Type baseType;

        public SubClassSelectorDropdownMenu(string managedTypeName)
            : this(EditorReflectionUtility.GetType(managedTypeName)) { }

        public SubClassSelectorDropdownMenu(Type baseType)
        {
            this.baseType = baseType;
            options = GetOptions(baseType)
                .GroupBy(type => type.FullName.Substring(0, type.FullName.LastIndexOf('.')))
                .SelectMany(g
                    => g.Select(type => new DropdownItem<Type>(g.Key + "/" + EditorReflectionUtility.GetTypeName(type), type
                        , fullName: type.FullName + EditorReflectionUtility.GetTypeName(type))))
                .ToArray();
            menu = new(options, type => OnSelected?.Invoke(type));

            OnSelected = delegateOnComplete;
        }

        public void Show()
        {
            menu.ShowAsContext(windowHeight: 300);
            menu.ScrollTo(baseType);
        }

        public void Show(Rect position)
        {
            menu.ShowAsDropdown(position, windowHeight: 300);
            menu.ScrollTo(baseType);
        }

        public static void DefaultPropertyCallback(SerializedProperty property, Type type)
        {
            if (property == null)
                return;

            foreach (var targetObject in property.serializedObject.targetObjects)
            {
                var so = new SerializedObject(targetObject);
                var se = so.FindProperty(property.propertyPath);
                var obj = type is null
                    ? null
                    : (se.managedReferenceValue = Activator.CreateInstance(type));
                se.isExpanded = obj != null;

                so.ApplyModifiedProperties();
                so.Update();
            }
        }

        public static Type[] GetOptions(Type baseType)
        {
          return EditorReflectionUtility.GetDerivedTypes(baseType)
                .Prepend(baseType)
                .Where(type => (type.IsPublic || type.IsNestedPublic)
                    && false == type.IsAbstract
                    && false == type.IsGenericType
                    && false == EditorReflectionUtility.UnityObjectType.IsAssignableFrom(type)
                    && type.IsDefined<SerializableAttribute>(false))
                .Distinct()
                .ToArray();
        }

        private void delegateOnComplete(Type type)
        {
            DefaultPropertyCallback(TargetProperty, type);
        }

        public SubClassSelectorDropdownMenu SetOnSelected(Action<Type> action)
        {
            OnSelected = action;
            return this;
        }

        public static void Show<T>(Action<Type> callback) => Show(typeof(T), callback);

        public static void Show(Type baseType, Action<Type> callback) => Cache.Get(baseType).SetOnSelected(callback).Show();

        public static void Show(Rect position, Type baseType, Action<Type> callback, Type current = null)
        {
            var menu = Cache.Get(baseType).SetOnSelected(callback);
            menu.Show(position);
            menu.menu.ScrollTo(current ?? baseType);
        }
    }
}