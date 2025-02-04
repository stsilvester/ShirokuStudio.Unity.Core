using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    public static class LinqExtensions
    {
        public static IEnumerable<TOutput> SelectWhere<TElement, TOutput>(this IEnumerable<TElement> list, Func<TElement, (bool, TOutput)> selector)
        {
            foreach (var item in list)
            {
                var (isMatch, output) = selector(item);
                if (isMatch)
                    yield return output;
            }
        }

        public static IOrderedEnumerable<TElement> OrderBy<TElement>(this IEnumerable<TElement> list, Func<TElement, object> getter, bool isAsc)
        {
            if (isAsc)
                return list.OrderBy(getter);
            else
                return list.OrderByDescending(getter);
        }

        public static IOrderedEnumerable<TElement> OrderBy<TElement, TKey>(this IEnumerable<TElement> list, Func<TElement, TKey> getter, IComparer<TKey> comparer, bool isAsc)
        {
            if (isAsc)
                return list.OrderBy(getter, comparer);
            else
                return list.OrderByDescending(getter, comparer);
        }

        public static IOrderedEnumerable<TElement> ThenBy<TElement, TKey>(this IOrderedEnumerable<TElement> list, Func<TElement, TKey> getter, bool isAsc)
        {
            if (isAsc)
                return list.ThenBy(getter);
            else
                return list.ThenByDescending(getter);
        }

        public static IOrderedEnumerable<TElement> ThenBy<TElement, TKey>(this IOrderedEnumerable<TElement> list, Func<TElement, TKey> getter, IComparer<TKey> comparer, bool isAsc)
        {
            if (isAsc)
                return list.ThenBy(getter, comparer);
            else
                return list.ThenByDescending(getter, comparer);
        }

        public static bool IsNullOrEmpty<TElement>(this IEnumerable<TElement> list)
        {
            return list == null || list.Any() == false;
        }

        /// <summary>輸出以字串串接元素的字串</summary>
        /// <typeparam name="TElement">元素型別</typeparam>
        /// <param name="list">目標集合物件</param>
        /// <param name="seperator">串接字串</param>
        public static string Join<TElement>(this IEnumerable<TElement> list, string seperator = ", ")
        {
            return string.Join(seperator, list.OfType<object>());
        }

        public static string Join<TElement>(this IEnumerable<TElement> list, string seperator, Func<TElement, object> selector)
        {
            return string.Join(seperator, list.Select(selector).OfType<object>());
        }

        /// <summary>For迴圈Linq版</summary>
        /// <typeparam name="TElement">集合元素</typeparam>
        /// <param name="src">集合來源</param>
        /// <param name="action">回圈內對每個元素的行為</param>
        public static void Foreach<TElement>(this IEnumerable<TElement> src, Action<TElement> action)
        {
            if (src == null
                || src.Any() == false)
            {
                return;
            }

            foreach (var item in src)
            {
                action(item);
            }
        }

        /// <summary>For迴圈Linq版</summary>
        /// <typeparam name="TElement">集合元素</typeparam>
        /// <param name="src">集合來源</param>
        /// <param name="action">回圈內對每個元素的行為(可接受有回傳值的委派方法)</param>
        public static void Foreach<TElement, TAny>(this IEnumerable<TElement> src, Func<TElement, TAny> action)
        {
            if (src == null
                || src.Any() == false)
            {
                return;
            }

            foreach (var item in src)
            {
                action(item);
            }
        }

        /// <summary>For迴圈Linq版</summary>
        /// <typeparam name="TElement">集合元素</typeparam>
        /// <param name="src">集合來源</param>
        /// <param name="action">回圈內對每個元素的行為</param>
        public static void Foreach<TElement>(this TElement[] src, Action<TElement, int> action)
        {
            if (src == null
                || src.Any() == false)
            {
                return;
            }

            for (var i = 0; i < src.Length; i++)
            {
                action(src[i], i);
            }
        }

        /// <summary>For迴圈Linq版</summary>
        /// <typeparam name="TElement">集合元素</typeparam>
        /// <param name="src">集合來源</param>
        /// <param name="action">回圈內對每個元素的行為</param>
        public static void Foreach<TElement>(this IEnumerable<TElement> src, Action<TElement, int> action)
        {
            if (src == null
                || src.Any() == false)
            {
                return;
            }

            var index = 0;
            foreach (var item in src)
            {
                action(item, index++);
            }
        }

        public static IEnumerable<TElement> Except<TElement>(this IEnumerable<TElement> src, params TElement[] exclude)
        {
            return Linq.Enumerable.Except(src, second: exclude);
        }

        public static TElement Random<TElement>(this IEnumerable<TElement> src)
        {
            return src.ElementAt(UnityEngine.Random.Range(0, src.Count()));
        }

        public static int IndexOf<TElement>(this IEnumerable<TElement> src, TElement value)
        {
            if (src is IList list)
                return list.IndexOf(value);

            var index = 0;
            foreach (var item in src)
            {
                if (item.Equals(value))
                    return index;

                index++;
            }

            return -1;
        }

        public static int IndexOf<TElement>(this IEnumerable<TElement> src, Func<TElement, bool> predicate)
        {
            var index = 0;
            foreach (var item in src)
            {
                if (predicate(item))
                    return index;

                index++;
            }

            return -1;
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> src)
            => src.ToDictionary(pair => pair.Key, pair => pair.Value);

        public static IEnumerable<TElement> DistinctBy<TElement, TKey>(this IEnumerable<TElement> src, Func<TElement, TKey> selector)
        {
            var keys = new HashSet<TKey>();
            foreach (var item in src)
            {
                if (keys.Add(selector(item)))
                    yield return item;
            }
        }
    }
}

namespace System.Linq
{
    using UnityEngine;

    public static class LinqExtensionForUnity
    {
        public static Vector4 Sum(this IEnumerable<Vector4> src)
        {
            var result = Vector4.zero;
            src.Foreach(item => result += item);
            return result;
        }

        public static Vector4 Sum<T>(this IEnumerable<T> src, Func<T, Vector4> selector)
        {
            var result = Vector4.zero;
            src.Foreach(item => result += selector(item));
            return result;
        }

        public static Vector3 Sum(this IEnumerable<Vector3> src)
        {
            var result = Vector3.zero;
            src.Foreach(item => result += item);
            return result;
        }

        public static Vector3 Sum<T>(this IEnumerable<T> src, Func<T, Vector3> selector)
        {
            var result = Vector3.zero;
            src.Foreach(item => result += selector(item));
            return result;
        }

        public static Vector2 Sum(this IEnumerable<Vector2> src)
        {
            var result = Vector2.zero;
            src.Foreach(item => result += item);
            return result;
        }

        public static Vector2 Sum<T>(this IEnumerable<T> src, Func<T, Vector2> selector)
        {
            var result = Vector2.zero;
            src.Foreach(item => result += selector(item));
            return result;
        }

        public static bool TryFind<T>(this IEnumerable<T> src, Func<T, bool> predicate, out T target)
        {
            target = src.FirstOrDefault(predicate);
            return Equals(target, default);
        }
    }
}