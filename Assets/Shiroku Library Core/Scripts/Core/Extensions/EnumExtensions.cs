using ShirokuStudio.Core.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ShirokuStudio.Core
{
    public static class EnumExtensions
    {
        public static T Toggle<T>(this T currentValue, T flag)
            where T : Enum
        {
            var isOn = currentValue.HasFlag(flag);
            return currentValue.Toggle(flag, !isOn);
        }

        public static T Toggle<T>(this T currentValue, T flag, bool isOn)
            where T : Enum
        {
            return isOn
                ? (T)Enum.ToObject(typeof(T), Convert.ToInt32(currentValue) | Convert.ToInt32(flag))
                : (T)Enum.ToObject(typeof(T), Convert.ToInt32(currentValue) & ~Convert.ToInt32(flag));
        }
        /// <summary>
        /// 取得<see cref="InspectorNameAttribute"/>的完整顯示名稱字串
        /// </summary>
        /// <typeparam name="TEnum">列舉類型</typeparam>
        /// <param name="enumValue">列舉值</param>
        /// <param name="seperator">分割字串</param>
        public static string GetDisplayName<TEnum>(this TEnum enumValue, string seperator = ",")
            where TEnum : Enum
        {
            var map = EnumUtility.GetDisplayNameMap<TEnum>();
            var isFlag = enumValue.GetType().IsDefined(typeof(FlagsAttribute), true);
            if (isFlag)
            {
                var result = new List<string>();
                var remains = map.OrderByDescending(kv => kv.Key)
                    .Aggregate(enumValue, (v, kv) =>
                    {
                        if (v.HasFlag(kv.Key))
                        {
                            v = v.Toggle(kv.Key, false);
                            result.Add(kv.Value);
                        }
                        return v;
                    });
                result.Reverse();
                return result.Join(seperator);
            }
            else
            {
                return map.TryGetValue(enumValue, out var name) ? name : enumValue.ToString();
            }
        }
    }
}