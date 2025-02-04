using System.Collections.Generic;

namespace ShirokuStudio.Core
{
    public abstract class DataCacher<TCacher, TKey, TValue>
        where TCacher : DataCacher<TCacher, TKey, TValue>, new()
    {
        private static TCacher instance { get; } = new();

        private static Dictionary<TKey, TValue> _dictionary = new();

        protected abstract TValue CreateCache(TKey key);

        public static void Clear()
        {
            _dictionary.Clear();
        }

        public static TValue Get(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var value))
                return value;

            value = instance.CreateCache(key);
            _dictionary.Add(key, value);
            return value;
        }
    }
}