namespace System.Collections.Generic
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
            where TValue : new() => dict.GetOrAdd(key, () => new TValue());

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> valueFactory)
        {
            if (dict.TryGetValue(key, out var value))
                return value;

            value = valueFactory();
            dict.Add(key, value);
            return value;
        }

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> valueFactory)
        {
            if (dict.TryGetValue(key, out var value))
                return value;

            value = valueFactory(key);
            dict[key] = value;
            return value;
        }
    }
}