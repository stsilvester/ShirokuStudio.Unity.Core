using ShirokuStudio.Core.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ShirokuStudio.Core
{
    public static class EnumUtility
    {
        public static T[] GetFlags<T>() where T : Enum
        {
            return FastCacher<Type, Enum[]>
                .Get(typeof(T), getFlags)
                .Cast<T>()
                .ToArray();
        }

        public static T[] GetFlags<T>(this T value) where T : Enum
        {
            var flags = GetFlags<T>();
            return flags.Where(f => value.HasFlag(f)).ToArray();
        }

        public static Enum[] GetFlags(Type type)
        {
            return FastCacher<Type, Enum[]>
                .Get(type, getFlags);
        }

        private static Enum[] getFlags(Type type)
        {
            return Enum.GetValues(type).Cast<Enum>().ToArray();
        }

        public static TEnum ParseEnumFlag<TEnum>(string input, string seperator = ",")
            where TEnum : Enum
        {
            var parts = input.Split(seperator);
            var enumMap = GetDisplayNameMap<TEnum>();

            return parts.Aggregate(default(TEnum), (v, str) =>
            {
                var kv = enumMap.FirstOrDefault(kv => kv.Value.Equals(str, StringComparison.OrdinalIgnoreCase));
                return string.IsNullOrWhiteSpace(kv.Value) ? v : v.Toggle(kv.Key, true);
            });
        }

        public static Dictionary<Enum, string> GetDisplayNameMap(Type enumType)
        {
            return FastCacher<Type, Dictionary<Enum, string>>.Get(enumType,
                _ => Enum.GetValues(enumType)
                    .Cast<Enum>()
                    .Select(flag => (flag, field: enumType.GetField(flag.ToString())))
                    .Where(kv => kv.field.IsDefined(typeof(InspectorNameAttribute), true))
                    .ToDictionary(kv => kv.flag,
                        kv => kv.field.GetCustomAttribute<InspectorNameAttribute>().displayName));
        }

        /// <summary>
        /// 取得<see cref="InspectorNameAttribute"/>的顯示名稱映射
        /// </summary>
        /// <typeparam name="TEnum">列舉類型</typeparam>
        public static Dictionary<TEnum, string> GetDisplayNameMap<TEnum>()
            where TEnum : Enum
            => FastCacher<Type, Dictionary<TEnum, string>>.Get(typeof(TEnum),
                enumType => GetDisplayNameMap(enumType)
                    .ToDictionary(kv => (TEnum)kv.Key, kv => kv.Value));

        /// <summary>
        /// 取得<see cref="InspectorNameAttribute"/>的顯示名稱陣列
        /// </summary>
        /// <typeparam name="TEnum">列舉類型</typeparam>
        /// <returns></returns>
        public static string[] GetDisplayNames<TEnum>() where TEnum : Enum
            => FastCacher<Type, string[]>.Get(typeof(TEnum),
                _ => GetDisplayNameMap(typeof(TEnum)).Values.ToArray());
    }
}