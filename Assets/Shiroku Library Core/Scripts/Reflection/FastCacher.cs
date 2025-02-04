using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ShirokuStudio.Core.Reflection
{
    public static class FastCacher
    {
        public delegate bool TryGetValueDelegate(object inst, string key, out object value);

        private class CachedData
        {
            #region statics

            private static Dictionary<Type, CachedData> cached = new();

            public static CachedData Get(Type type)
            {
                if (type is null)
                    return null;

                if (!cached.TryGetValue(type, out var cachedData))
                {
                    try
                    {
                        cachedData = new CachedData(type);
                        cached.Add(type, cachedData);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError(ex);
                    }
                }

                return cachedData;
            }

            public static void ClearCache()
            {
                cached.Clear();
            }

            #endregion

            public class MemberCache
            {
                public string Name;
                public Type DeclaringType;
                public Func<object, object> Getter => getter.Value;
                public Action<object, object> Setter => setter.Value;
                public MemberInfo MemberInfo;

                private Lazy<Func<object, object>> getter;
                private Lazy<Action<object, object>> setter;

                public MemberCache(Type declaringType, MemberInfo member)
                {
                    MemberInfo = member;
                    DeclaringType = declaringType;
                    Name = member.Name;
                    getter = new Lazy<Func<object, object>>(() => createGetter(declaringType, member));
                    setter = new Lazy<Action<object, object>>(() => createSetter(declaringType, member));
                }
            }

            public Type Type { get; }
            public readonly string[] Keys;
            public readonly Func<object, string, object> Indexer;
            public readonly TryGetValueDelegate TryGetValue;
            public readonly IReadOnlyDictionary<string, MemberCache> members;

            public CachedData(Type type)
            {
                Type = type;
                members = type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m is FieldInfo || m is PropertyInfo)
                    .Where(m => m.Name.StartsWith("<") == false)
                    .Select(m => new MemberCache(type, m))
                    .DistinctBy(m => m.Name)
                    .ToDictionary(m => m.Name);
                Keys = members.Keys.ToArray();

                var indexer = type.GetProperties()
                    .FirstOrDefault(p => {
                        var param = p.GetIndexParameters();
                        return param.Length == 1 && param[0].ParameterType == typeof(string);
                    });
                if (indexer != null)
                    Indexer = createIndexer(type, indexer);

                var tgv = type.GetMethod("TryGetValue");
                if (tgv != null)
                    TryGetValue = createTryGetValue(type, tgv);
            }

            public MemberCache GetMember(string name)
                => members.TryGetValue(name, out var member) ? member : null;

            #region static delegate creator

            private static TryGetValueDelegate createTryGetValue(Type Type, MethodInfo tgv)
            {
                //TODO: test if this is correct
                var type_key = Type.GetGenericArguments()[0];
                var type_value = Type.GetGenericArguments()[1];
                var par_input = Expression.Parameter(typeof(object), "input");
                var par_instance = Expression.Convert(par_input, Type);
                var par_key = Expression.Parameter(type_key, "key");
                var par_value = Expression.Parameter(type_value.MakeByRefType(), "value");

                var m = Expression.Call(par_instance, tgv, par_key, par_value);
                var lambda = Expression.Lambda<TryGetValueDelegate>(m, par_input, par_key, par_value);
                return lambda.Compile();
            }

            private static Func<object, string, object> createIndexer(Type Type, PropertyInfo indexer)
            {
                var p1 = Expression.Parameter(typeof(object), "obj");
                var t = Expression.Convert(p1, Type);
                var p2 = Expression.Parameter(typeof(string), "name");
                var m = Expression.MakeIndex(t, indexer, new[] { p2 });
                var c = Expression.Convert(m, typeof(object));
                var l = Expression.Lambda<Func<object, string, object>>(c, p1, p2);
                return l.Compile();
            }

            private static Func<object, object> createGetter(Type Type, MemberInfo member)
            {
                var isValid = member is FieldInfo || member is PropertyInfo prop && !prop.GetIndexParameters().Any();
                if (isValid == false)
                    return null;

                var p = Expression.Parameter(typeof(object), "obj");
                var t = Expression.Convert(p, Type);
                var m = Expression.MakeMemberAccess(t, member);
                var c = Expression.Convert(m, typeof(object));
                var l = Expression.Lambda<Func<object, object>>(c, p);
                return l.Compile();
            }

            private static Action<object, object> createSetter(Type Type, MemberInfo member)
            {
                var isValid = member is FieldInfo finfo || member is PropertyInfo pinfo && pinfo.CanWrite && !pinfo.GetIndexParameters().Any()
                    && member.Name.Contains("BackingField") == false;

                var p1 = Expression.Parameter(typeof(object), "obj");
                var t = Expression.Convert(p1, Type);
                var p2 = Expression.Parameter(typeof(object), "value");
                var c = Expression.Convert(p2, member is FieldInfo f ? f.FieldType : member is PropertyInfo p ? p.PropertyType : default);
                var m = Expression.Assign(Expression.MakeMemberAccess(t, member), c);
                var l = Expression.Lambda<Action<object, object>>(m, p1, p2);
                return l.Compile();
            }

            #endregion
        }

        static FastCacher()
        {
            CachedData.ClearCache();
        }

        public static bool HasMember(Type type, string name)
            => CachedData.Get(type)?.GetMember(name) != null;

        public static MemberInfo GetMemberInfo(Type type, string name)
            => CachedData.Get(type)?.GetMember(name)?.MemberInfo;

        public static IEnumerable<MemberInfo> GetMemberInfos(Type type)
            => CachedData.Get(type)?.members.Values.Select(m => m.MemberInfo);

        public static IEnumerable<FieldInfo> GetFieldInfos(Type type)
            => CachedData.Get(type)?.members.Values
                .Where(m => m.MemberInfo is FieldInfo)
                .Select(m => m.MemberInfo as FieldInfo);

        public static IEnumerable<PropertyInfo> GetPropertyInfos(Type type)
            => CachedData.Get(type)?.members.Values
                .Where(m => m.MemberInfo is PropertyInfo)
                .Select(m => m.MemberInfo as PropertyInfo);

        public static Type GetMemberType(Type type, string name)
        {
            var member = CachedData.Get(type)?.GetMember(name)?.MemberInfo;
            if (member is PropertyInfo prop)
                return prop.PropertyType;
            else if (member is FieldInfo field)
                return field.FieldType;

            return null;
        }

        public static string[] GetMemberNames(Type type)
            => CachedData.Get(type)?.Keys;

        /// <summary>
        /// 取得指定類型的所有成員名稱
        /// </summary>
        /// <param name="type">目標物件類型</param>
        /// <returns>成員名稱集合</returns>
        public static IEnumerable<string> GetMemberNames<T>()
            => GetMemberNames(typeof(T));

        /// <summary>
        /// 取得物件的屬性值Getter
        /// </summary>
        /// <param name="type">目標物件類型</param>
        /// <param name="name">欄位或屬性名稱</param>
        /// <returns>取得物件屬性值的委派</returns>
        public static Func<object, object> GetGetter(Type type, string name)
            => CachedData.Get(type)?.GetMember(name).Getter;

        /// <summary>
        /// 取得物件的屬性值Getter
        /// </summary>
        /// <typeparam name="T">目標物件類型</typeparam>
        /// <param name="name">欄位或屬性名稱</param>
        /// <returns>取得物件屬性值的委派</returns>
        public static Func<T, object> GetGetter<T>(string name)
        {
            var getter = GetGetter(typeof(T), name);
            return getter != null ? (inst) => (T)getter(inst) : null;
        }

        /// <summary>
        /// 取得物件的屬性值Setter
        /// </summary>
        /// <param name="type">目標物件類型</param>
        /// <param name="name">欄位或屬性名稱</param>
        /// <returns>取得物件屬性值的委派</returns>
        public static Action<object, object> GetSetter(Type type, string name)
            => CachedData.Get(type)?.GetMember(name)?.Setter;

        /// <summary>
        /// 取得物件的屬性值Setter
        /// </summary>
        /// <typeparam name="T">目標物件類型</typeparam>
        /// <param name="name">欄位或屬性名稱</param>
        /// <returns>取得物件屬性值的委派</returns>
        public static Action<T, object> GetSetter<T>(string name)
        {
            var setter = GetSetter(typeof(T), name);
            return setter != null ? (inst, value) => setter(inst, value) : null;
        }

        /// <summary>
        /// 取得物件的索引子Getter
        /// </summary>
        /// <param name="type">目標物件類型</param>
        /// <returns>取得物件索引子值的委派</returns>
        public static Func<object, string, object> GetIndexer(Type type)
            => CachedData.Get(type)?.Indexer;

        /// <summary>
        /// 取得物件的屬性Getter
        /// </summary>
        /// <param name="type">目標物件類型</param>
        /// <returns>取得物件屬性值的委派</returns>
        public static TryGetValueDelegate GetTryGetValueDelegate(Type type)
            => CachedData.Get(type)?.TryGetValue;

        /// <summary>
        /// 取得物件的屬性值
        /// </summary>
        /// <param name="type">目標物件類型</param>
        /// <param name="obj">目標物件實體</param>
        /// <param name="name">欄位或屬性名稱</param>
        /// <returns>欄位或屬性值</returns>
        public static object Get(Type type, object obj, string name)
           => GetGetter(type, name)?.Invoke(obj);

        /// <summary>
        /// 取得物件的屬性值
        /// </summary>
        /// <typeparam name="T">目標物件類型</typeparam>
        /// <param name="obj">目標物件實體</param>
        /// <param name="name">欄位或屬性名稱</param>
        /// <returns>欄位戶屬性值</returns>
        public static object Get<T>(T obj, string name)
            => Get(obj.GetType(), obj, name);

        /// <summary>
        /// 設定物件的屬性值
        /// </summary>
        /// <param name="type">目標物件類型</param>
        /// <param name="obj">目標物件實體</param>
        /// <param name="name">欄位或屬性名稱</param>
        /// <param name="value">要設定的值</param>
        public static void Set(Type type, object obj, string name, object value)
            => GetSetter(type, name)?.Invoke(obj, value);

        /// <summary>
        /// 設定物件的屬性值
        /// </summary>
        /// <typeparam name="T">目標物件類型</typeparam>
        /// <param name="obj">目標物件實體</param>
        /// <param name="name">欄位或屬性名稱</param>
        /// <param name="value">要設定的值</param>
        public static void Set<T>(T obj, string name, object value)
            => GetSetter<T>(name)?.Invoke(obj, value);

        /// <summary>
        /// 取得物件的索引子值
        /// </summary>
        /// <param name="type">目標物件類型</param>
        /// <param name="obj">目標物件實體</param>
        /// <param name="name">索引子名稱</param>
        /// <returns>索引子值</returns>
        public static object Indexer(Type type, object obj, string name)
            => GetIndexer(type)?.Invoke(obj, name);

        /// <summary>
        /// 取得物件的索引子值
        /// </summary>
        /// <typeparam name="T">目標物件類型</typeparam>
        /// <param name="obj">目標物件實體</param>
        /// <param name="name">索引子名稱</param>
        /// <returns>索引子值</returns>
        public static object Indexer<T>(T obj, string name)
            => GetIndexer(typeof(T))?.Invoke(obj, name);

        /// <summary>
        /// 嘗試從物件中取得指定名稱的值
        /// </summary>
        /// <param name="type">目標物件類型</param>
        /// <param name="obj">目標物件實體</param>
        /// <param name="name">欄位或屬性名稱</param>
        /// <param name="value">取得的值</param>
        /// <returns>是否成功取得值</returns>
        public static bool TryGetValue(Type type, object obj, string name, out object value)
        {
            value = null;
            return CachedData.Get(type)?.TryGetValue?.Invoke(obj, name, out value) == true;
        }

        /// <summary>
        /// 清除快取
        /// </summary>
        public static void ClearCache()
            => CachedData.ClearCache();
    }
}

namespace ShirokuStudio.Core.Reflection
{
    public class FastCacher<TCachedObject>
    {
        private static TCachedObject _value;

        public static TCachedObject Get(Func<TCachedObject> constructor)
        {
            _value ??= constructor();
            return _value;
        }

        public static void Set(TCachedObject value)
        {
            _value = value;
        }
    }
}

namespace ShirokuStudio.Core.Reflection
{
    public class FastCacher<TKey, TCacheObject>
    {
        private static Dictionary<TKey, TCacheObject> cache = new Dictionary<TKey, TCacheObject>();

        public static TCacheObject Get(TKey key, Func<TKey, TCacheObject> constructor)
        {
            if (!cache.TryGetValue(key, out var value))
            {
                value = constructor(key);
                cache.Add(key, value);
            }
            return value;
        }

        public static void Set(TKey key, TCacheObject value)
        {
            cache[key] = value;
        }

        public static void Clear()
        {
            cache.Clear();
        }
    }
}