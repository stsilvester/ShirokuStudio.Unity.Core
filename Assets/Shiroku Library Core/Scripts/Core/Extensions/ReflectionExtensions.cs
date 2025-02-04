using ShirokuStudio.Core;
using ShirokuStudio.Core.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System
{
    public static class ReflectionExtensions
    {
        public static Type[] GetAncestors(this Type type) => TypeCache.GetAncestors(type).ToArray();

        public static string GetBackingFieldName(this Type type, string propertyName)
        {
            var field = type.GetField($"<{propertyName}>k__BackingField");
            return field?.Name;
        }

        public static bool IsDefined<TAttribute>(this Type type, bool inherit)
            => type.IsDefined(typeof(TAttribute), inherit);

        public static string GetFriendlyName(this Type type)
        {
            if (type is null)
                return "<null>";

            if (type.IsGenericType)
                return $"{type.Name.Split('`')[0]}<{type.GetGenericArguments().Select(t => t.GetFriendlyName()).Join(", ")}>";

            if (ReflectionUtility.FriendlyTypeNames.TryGetValue(type, out var name))
                return name;

            return type.Name;
        }

        public static bool IsDefined<TAttr>(this MemberInfo m, bool inherit)
            where TAttr : Attribute
        {
            return m.IsDefined(typeof(TAttr), inherit);
        }

        public static IEnumerable<Assembly> GetDependents(this Assembly assembly)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var a in assemblies)
            {
                if (a.GetReferencedAssemblies().Any(r => r.FullName == assembly.FullName))
                    yield return a;
            }
        }

        public static bool IsAssignableFromGenericInterface(this Type type, Type genericType)
        {
            if (!genericType.IsInterface || !genericType.IsGenericType)
                return false;

            if (type.IsInterface)
            {
                return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericType);
            }
            else
            {
                return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericType)
                    || type.BaseType?.IsAssignableFromGenericInterface(genericType) == true;
            }
        }

        public static bool TryGetGenericInterface(this Type type, Type genericType, out Type result)
        {
            if (!genericType.IsInterface || !genericType.IsGenericType)
            {
                result = null;
                return false;
            }

            result = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericType);
            return result != null;
        }
    }
}