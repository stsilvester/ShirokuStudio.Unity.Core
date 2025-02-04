using ShirokuStudio.Core;
using ShirokuStudio.Core.Models;
using ShirokuStudio.Core.Reflection;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShirokuStudio.Editor
{
    [CustomPropertyDrawer(typeof(SerializableMethod))]
    public class SerializableMethodPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var attr = fieldInfo.GetCustomAttribute<ReferencedSourceAttribute>();
            if (attr == null)
                return new Label("ReferencedSourceAttribute is required");

            var filter = fieldInfo.GetCustomAttribute<MethodFilterAttribute>() ?? new MethodFilterAttribute();

            Type cachedType = null;
            DropdownMenu<MethodInfo> menu = null;

            var sourceFieldName = attr.Name;

            var rect = new VisualElement();
            rect.style.flexDirection = FlexDirection.Row;
            rect.style.flexGrow = 1f;
            rect.style.paddingLeft = 1.5f;
            rect.style.unityTextAlign = TextAnchor.MiddleLeft;

            var label = new Label(property.GetLabel());
            rect.Add(label);

            var button = new Button();
            rect.Add(button);

            button.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft);
            button.clicked += showDropdownMenu;

            updateText();

            return rect;

            void updateText()
            {
                button.text = property.GetValue<SerializableMethod>().MethodInfo?.GetFriendlyName() ?? "(none)";
            }

            void showDropdownMenu()
            {
                //get source value from object
                var target = property.GetTargetObjcet();
                var sourceType = FastCacher.Get(fieldInfo.DeclaringType, target, sourceFieldName) as Type;

                var refreshRequired = cachedType != sourceType || menu == null;
                if (refreshRequired)
                    buildMenu(sourceType);

                menu.ShowAsContext(300);
            }

            void buildMenu(Type type)
            {
                cachedType = type;

                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance)
                    .Where(m => m.IsSpecialName == false)
                    .Where(m => m.IsGenericMethod == false && m.ContainsGenericParameters == false);

                if (filter.ReturnType != null)
                    methods = methods.Where(m => m.ReturnType == filter.ReturnType);

                if (filter.MaxParameterLength > 0)
                    methods = methods.Where(m => m.GetParameters().Length <= filter.MaxParameterLength);

                if (string.IsNullOrWhiteSpace(filter.Regex) == false)
                {
                    var regex = new System.Text.RegularExpressions.Regex(filter.Regex);
                    methods = methods.Where(m => regex.IsMatch(m.Name));
                }

                var options = methods
                    .GroupBy(m => m.DeclaringType).OrderBy(g => g.Key.Name)
                    .SelectMany(g => g.OrderBy(m => m.GetParameters().Length).Select(m =>
                    {
                        var name = m.GetFriendlyName();
                        var path = g.Key.GetFriendlyName() + "/" + name;
                        return new DropdownItem<MethodInfo>(name, m, fullName: path);
                    }))
                    .ToList();

                menu = new DropdownMenu<MethodInfo>(options, onMethodSelected);
            }

            void onMethodSelected(MethodInfo method)
            {
                property.SetValue(new SerializableMethod(method));
                updateText();

                UnityEngine.Debug.Log($"Set value {method.GetFriendlyName()}");
                property.serializedObject.Update();
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}