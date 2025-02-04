using ShirokuStudio.Core.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace System
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string str)
            => string.IsNullOrWhiteSpace(str);

        public static readonly Regex Pattern_NativeFormatter = new Regex(@"\{(?<name>[0-9]+)(?<fmt>:[^\}]+)?\}", RegexOptions.Compiled);
        public static readonly Regex Pattern_MapFormatter = new Regex(@"\{(?<name>[^\}\:]+)(?<fmt>:[^\}]+)?\}", RegexOptions.Compiled);
        public const string Pattern_Replacer = @"\$(?<name>[^\$:]+)(:(?<pat>[^\$]+))?\$";

        /// <summary>
        /// 檢測格式字串是否為原生格式字串(String.Format)
        /// </summary>
        /// <returns><list type="bullet">
        /// <item>true: 原生格式字串</item>
        /// <item>false: 映射格式字串</item>
        /// <item>null: 非格式字串</item>
        /// </list></returns>
        public static bool? IsNativeFormatString(this string format)
            => Pattern_MapFormatter.IsMatch(format) ? false
                : Pattern_NativeFormatter.IsMatch(format) ? true
                : null;

        public static string Format(this string format, params object[] args)
            => string.Format(format, args);

        public static string MapFormat<TMap>(this string format, TMap data, string replacerPattern = Pattern_Replacer)
        {
            if (string.IsNullOrWhiteSpace(format))
                return null;

            var result = format;
            var regex = new Regex(replacerPattern);
            string[] names;
            Func<string, object> getter;
            strategy(data);

            var matches = regex.Matches(format);
            foreach (var item in matches.Cast<Match>())
            {
                var name = item.Groups["name"].Value;
                if (string.IsNullOrWhiteSpace(name) || !names.Contains(name))
                    continue;

                var val = getter(name);
                var pattern = item.Groups["pat"].Value;
                if (!string.IsNullOrWhiteSpace(pattern))
                    val = $"{{0:{pattern}}}".Format(val);

                result = result.Replace(item.Value, val.ToString());
            }

            return result;

            void strategy(TMap data)
            {
                if (data is IDictionary<string, object> dict)
                {
                    names = dict.Keys.ToArray();
                    getter = key => dict[key];
                }
                else
                {
                    names = FastCacher.GetMemberNames(data.GetType()).ToArray();
                    getter = key => FastCacher.Get(data, key);
                }
            }
        }
    }
}