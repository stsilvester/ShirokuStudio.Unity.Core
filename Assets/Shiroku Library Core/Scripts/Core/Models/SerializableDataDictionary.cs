using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShirokuStudio.Core.Models
{
    [Serializable]
    public class SerializableDataDictionary<TData> : IDictionary<string, TData>, ISerializationCallbackReceiver
        where TData : IDictionaryEntry
    {
        [SerializeReference]
        private List<TData> datas = new();

        private Dictionary<string, TData> dictionary;

        public void Add(string key, TData value)
        {
            ((IDictionary<string, TData>)dictionary).Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, TData>)dictionary).ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, TData>)dictionary).Remove(key);
        }

        public bool TryGetValue(string key, out TData value)
        {
            return ((IDictionary<string, TData>)dictionary).TryGetValue(key, out value);
        }

        public TData this[string key] { get => ((IDictionary<string, TData>)dictionary)[key]; set => ((IDictionary<string, TData>)dictionary)[key] = value; }

        public ICollection<string> Keys => ((IDictionary<string, TData>)dictionary).Keys;

        public ICollection<TData> Values => ((IDictionary<string, TData>)dictionary).Values;

        public void Add(KeyValuePair<string, TData> item)
        {
            ((ICollection<KeyValuePair<string, TData>>)dictionary).Add(item);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<string, TData>>)dictionary).Clear();
        }

        public bool Contains(KeyValuePair<string, TData> item)
        {
            return ((ICollection<KeyValuePair<string, TData>>)dictionary).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, TData>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, TData>>)dictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, TData> item)
        {
            return ((ICollection<KeyValuePair<string, TData>>)dictionary).Remove(item);
        }

        public int Count => ((ICollection<KeyValuePair<string, TData>>)dictionary).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<string, TData>>)dictionary).IsReadOnly;

        public IEnumerator<KeyValuePair<string, TData>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, TData>>)dictionary).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)dictionary).GetEnumerator();
        }

        #region implements ISerializationCallbackReceiver

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {

        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
        }

        #endregion
    }

    public interface IDictionaryEntry
    {
        public string Key { get; set; }
    }
}