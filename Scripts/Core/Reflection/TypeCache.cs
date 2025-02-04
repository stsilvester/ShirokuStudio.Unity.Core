using ShirokuStudio.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ShirokuStudio.Core.Reflection
{
    public static class TypeCache
    {
        private struct Key
        {
            public Type Type;
            public ITypeFilter Filter;
        }

        private static Dictionary<string, Type> typeCache = new Dictionary<string, Type>();
        private static Dictionary<Type, string> typeNameCache = new Dictionary<Type, string>();
        private static Dictionary<Key, Type[]> assignables = new();
        private static Dictionary<Key, Type[]> ancestoers = new();

        public static Type GetType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return default;

            if (typeCache.TryGetValue(typeName, out var type) == false
                || type == null)
            {
                type = Type.GetType(typeName);
                typeCache[typeName] = type;
            }
            return type;
        }

        public static string GetTypeName(Type type)
        {
            if (!typeNameCache.TryGetValue(type, out var typeName))
            {
                typeName = type.AssemblyQualifiedName;
                typeNameCache[type] = typeName;
            }
            return typeName;
        }

        #region Event Cache

        private class EventCache
        {
            private Type type;

            private Dictionary<string, EventInfo> events = new();

            public EventCache(Type type)
            {
                this.type = type;
            }

            public EventInfo GetEvent(string name)
            {
                if (events.TryGetValue(name, out var eventInfo) == false)
                {
                    eventInfo = type.GetEvent(name);
                    events[name] = eventInfo;
                }

                return eventInfo;
            }
        }

        private static Dictionary<Type, EventCache> eventCached = new();

        public static EventInfo GetEvent(string typeName, string name)
        {
            var type = GetType(typeName);
            return GetEvent(type, name);
        }

        public static EventInfo GetEvent(Type type, string name)
        {
            if (type == null)
                return default;

            if (!eventCached.TryGetValue(type, out var cache))
            {
                cache = new EventCache(type);
                eventCached[type] = cache;
            }

            return cache.GetEvent(name);
        }

        #endregion

        #region Method Cache

        private class MethodCache
        {
            private Type type;

            private Dictionary<string, Dictionary<int, MethodInfo>> methods = new();

            public MethodCache(Type type)
            {
                this.type = type;
            }

            public MethodInfo GetMethod(string name, int hash)
            {
                if (methods.TryGetValue(name, out var hashMap) == false)
                {
                    hashMap = type.GetMethods(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance)
                         .Where(m => m.Name == name)
                         .Where(m => m.IsSpecialName == false)
                         .Where(m => m.IsGenericMethod == false && m.ContainsGenericParameters == false)
                         .ToDictionary(m => GetHash(m), m => m);
                    methods[name] = hashMap;
                }

                if (hashMap.TryGetValue(hash, out var method))
                {
                    return method;
                }

                return default;
            }

            public int GetHash(MemberInfo info)
            {
                var hash = 0;
                if (info is MethodInfo method)
                {
                    hash = method.Name.GetHashCode();
                    foreach (var parameter in method.GetParameters())
                    {
                        hash ^= parameter.ParameterType.GetHashCode();
                    }
                }
                return hash;
            }
        }

        private static Dictionary<Type, MethodCache> methodCached = new();

        public static MethodInfo GetMethod(string typeName, string name, int hash)
        {
            var type = GetType(typeName);
            return GetMethod(type, name, hash);
        }

        public static MethodInfo GetMethod(Type type, string name, int hash)
        {
            if (type == null)
                return default;

            if (!methodCached.TryGetValue(type, out var cache))
            {
                cache = new MethodCache(type);
                methodCached[type] = cache;
            }

            return cache.GetMethod(name, hash);
        }

        public static int GetMethodHash(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                return default;

            var type = methodInfo.DeclaringType;
            if (!methodCached.TryGetValue(type, out var cache))
            {
                cache = new MethodCache(type);
                methodCached[type] = cache;
            }

            return cache.GetHash(methodInfo);
        }

        public static string GetFriendlyName(this MethodInfo methodInfo)
        {
            if (methodInfo is null)
                return string.Empty;

            var returnType = methodInfo.ReturnType.GetFriendlyName();
            var parameters = methodInfo.GetParameters().Select(p => $"{p.ParameterType.GetFriendlyName()} {p.Name}");
            var methodName = methodInfo.Name;
            return $"{returnType} {methodName}({string.Join(", ", parameters)})";
        }

        #endregion

        public static IEnumerable<Type> GetAssignablesFrom(Type type, ITypeFilter filter = null)
        {
            var key = new Key { Type = type, Filter = filter };
            if (assignables.TryGetValue(key, out var types) == false)
            {
                var collection = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => type.IsAssignableFrom(t));

                if (filter != null)
                    collection = collection.Where(filter.IsMatch);

                assignables[key] = types = collection.ToArray();
            }

            return types;
        }

        public static IEnumerable<Type> GetAncestors(Type type, ITypeFilter filter = null)
        {
            var key = new Key { Type = type, Filter = filter };
            if (ancestoers.TryGetValue(key, out var types) == false)
            {
                var collection = getAncestors(type);

                if (filter != null)
                    collection = collection.Where(filter.IsMatch);

                ancestoers[key] = types = collection.ToArray();
            }

            return types;
        }

        private static IEnumerable<Type> getAncestors(Type type)
        {
            if (type == null)
                yield break;

            foreach (var i in type.GetInterfaces())
                yield return i;

            var cur = type.BaseType;
            while (cur != null)
            {
                yield return cur;
                cur = cur.BaseType;
            }
        }
    }
}