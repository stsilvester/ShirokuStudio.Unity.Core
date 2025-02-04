using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

namespace ShirokuStudio.Core
{
    public abstract class SerializableDictionaryBase
    {
        protected ISerializable _object;
    }

    [Serializable]
    public class SerializableDictionary<TKey, TValue>
        : SerializableDictionaryBase, IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        protected TKey[] m_keys;

        [SerializeField]
        protected TValue[] m_values;

        public ICollection<TKey> Keys => _dictionary.Keys;

        public ICollection<TValue> Values => _dictionary.Values;

        public int Count => _dictionary.Count;

        public bool IsReadOnly => false;

        private Dictionary<TKey, TValue> _dictionary => _object as Dictionary<TKey, TValue>;

        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        public SerializableDictionary()
        {
            _object = new Dictionary<TKey, TValue>();
        }

        public void AddRange(IDictionary<TKey, TValue> dict)
        {
            if (dict == null)
            {
                return;
            }
            foreach (KeyValuePair<TKey, TValue> kvp in dict)
            {
                _dictionary[kvp.Key] = kvp.Value;
            }
        }

        public void CopyFrom(IDictionary<TKey, TValue> dict)
        {
            _dictionary.Clear();
            foreach (KeyValuePair<TKey, TValue> kvp in dict)
            {
                _dictionary[kvp.Key] = kvp.Value;
            }
        }

        public void OnAfterDeserialize()
        {
            if (m_keys != null && m_values != null && m_keys.Length == m_values.Length)
            {
                _dictionary.Clear();
                int n = m_keys.Length;
                for (int i = 0; i < n; ++i)
                {
                    _dictionary[m_keys[i]] = m_values[i];
                }

                m_keys = null;
                m_values = null;
            }
        }

        public virtual void OnBeforeSerialize()
        {
            int n = _dictionary.Count;
            m_keys = new TKey[n];
            m_values = new TValue[n];

            int i = 0;
            foreach (KeyValuePair<TKey, TValue> kvp in _dictionary)
            {
                m_keys[i] = kvp.Key;
                m_values[i] = kvp.Value;
                ++i;
            }
        }

        public virtual void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool ContainsValue(TValue val)
        {
            return _dictionary.ContainsValue(val);
        }

        public bool Remove(TKey key)
        {
            return _dictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key is null)
            {
                value = default;
                return false;
            }
            return _dictionary.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _dictionary.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            foreach (KeyValuePair<TKey, TValue> item in this)
            {
                array[arrayIndex++] = item;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            _dictionary.GetObjectData(info, context);
        }

        public void OnDeserialization(object sender)
        {
            _dictionary.OnDeserialization(sender);
        }

        public T Clone<T>() where T : SerializableDictionary<TKey, TValue>
        {
            return new SerializableDictionary<TKey, TValue>()
            {
                m_keys = m_keys?.ToArray(),
                m_values = m_values?.ToArray()
            } as T;
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            Dictionary<TKey, TValue> dic = new Dictionary<TKey, TValue>(Count);
            foreach (KeyValuePair<TKey, TValue> item in this)
            {
                dic[item.Key] = item.Value;
            }
            return dic;
        }
    }

    [Serializable]
    public class GameObjectMap : SerializableDictionary<string, GameObject>
    { }
}