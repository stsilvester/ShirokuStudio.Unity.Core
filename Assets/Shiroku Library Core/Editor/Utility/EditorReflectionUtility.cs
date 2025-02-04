using ShirokuStudio.Core;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace ShirokuStudio.Editor
{
    public static class EditorReflectionUtility
    {
        public static readonly Type UnityObjectType = typeof(UnityEngine.Object);

        private class TypeFilterCache : DataCacher<TypeFilterCache, Type, Type[]>
        {
            public static Lazy<Type[]> Types = new(() =>
            {
                var asms = AppDomain.CurrentDomain.GetAssemblies();
                return asms.SelectMany(asm => asm.GetTypes()).ToArray();
            });

            protected override Type[] CreateCache(Type type)
            {
                using var debug = new DebugScope();
                var result = Types.Value
                    .AsParallel()
                    .Where(t => type.IsAssignableFrom(t))
                    .ToArray();
                debug.Log($"Collect {result.Length} types derived from {type.Name}");
                return result;
            }
        }

        private class FieldCacher : DataCacher<FieldCacher, Type, MemberInfo[]>
        {
            protected override MemberInfo[] CreateCache(Type type)
            {
                var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                return props.Cast<MemberInfo>().Union(fields).ToArray();
            }
        }

        public static Type[] GetDerivedTypes(Type type)
        {
            return TypeFilterCache.Get(type);
        }

        public static MemberInfo[] GetPropertyAndFields(Type type)
        {
            return FieldCacher.Get(type);
        }

        public static MemberInfo[] GetPropertyAndFields<T>()
        {
            return FieldCacher.Get(typeof(T));
        }

        public static Type[] CollectTypesWithFieldOf<T>()
        {
            using var debug = new DebugScope();
            var allTypes = TypeFilterCache.Types.Value;
            var type = typeof(T);
            var result = allTypes
                .AsParallel()
                .Where(type => type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .AsParallel().Any(m => type.IsAssignableFrom(m.FieldType)))
                .ToArray();
            debug.Log($"Collect {result.Length} types contains {type.Name} field");
            return result;
        }

        private class ManagedTypeCache : DataCacher<ManagedTypeCache, string, Type>
        {
            protected override Type CreateCache(string typeName)
            {
                if (string.IsNullOrEmpty(typeName))
                    return null;

                int splitIndex = typeName.IndexOf(' ');
                var assembly = Assembly.Load(typeName.Substring(0, splitIndex));
                return assembly.GetType(typeName.Substring(splitIndex + 1));
            }
        }

        public static Type GetType(string typeName) => ManagedTypeCache.Get(typeName);

        public static string GetTypeName(Type type)
        {
            var name = type.Name;
            var attr = type.GetCustomAttribute<TypeMenuNameAttribute>();
            if (attr != null)
            {
                name = attr.Name;
            }

            return ObjectNames.NicifyVariableName(name);
        }

        #region PropertyDrawer

        private class CustomPropertyDrawerCache : DataCacher<CustomPropertyDrawerCache, Type, PropertyDrawer>
        {
            protected override PropertyDrawer CreateCache(Type type)
            {
                var drawerTypes = TypeCache.GetTypesWithAttribute<CustomPropertyDrawer>();
                var drawerType = drawerTypes
                    .FirstOrDefault(e =>
                    {
                        var attrs = e.GetCustomAttributes<CustomPropertyDrawer>(true);
                        return attrs.Any(attr => isMatch(attr, type, type.GetInterfaces()));
                    });

                if (drawerType != null)
                    return (PropertyDrawer)Activator.CreateInstance(drawerType);
                else
                    return null;

                bool isMatch(CustomPropertyDrawer attr, Type mainType, Type[] interfaces)
                {
                    var typeField = attr.GetType().GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (typeField == null)
                        return false;

                    var Type = typeField.GetValue(attr) as Type;
                    if (Type == mainType)
                        return true;

                    var useForChildrenField = attr.GetType().GetField("m_UseForChildren", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (useForChildrenField == null)
                        return false;

                    var UseForChildren = (bool)useForChildrenField.GetValue(attr);
                    if (UseForChildren == false)
                        return false;

                    if (Array.Exists(interfaces, t => t == Type))
                        return true;

                    if (Type.IsAssignableFrom(mainType))
                        return true;

                    return false;
                }
            }
        }

        public static PropertyDrawer GetCustomPropertyDrawer(SerializedProperty property)
            => GetCustomPropertyDrawer(GetType(property.managedReferenceFullTypename));

        public static PropertyDrawer GetCustomPropertyDrawer(Type type)
            => CustomPropertyDrawerCache.Get(type);

        #endregion
    }
}