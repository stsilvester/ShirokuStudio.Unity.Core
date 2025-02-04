using ShirokuStudio.Core.Reflection;
using System;
using UnityEngine;

namespace ShirokuStudio.Core.Models
{
    [Serializable]
    public class SerializableType
    {
        public Type Type { get => get(); set => set(value); }

        [SerializeField]
        private string typeName;

        public SerializableType()
        { }

        public SerializableType(Type t)
        {
            Type = t;
        }

        private Type _type;

        private Type get()
        {
            if (_type != null)
                return _type;

            _type = TypeCache.GetType(typeName);
            return _type;
        }

        private void set(Type value)
        {
            _type = value;
            typeName = _type?.AssemblyQualifiedName;
            UnityEngine.Debug.Log($"Set Type: {typeName}");
        }
    }
}