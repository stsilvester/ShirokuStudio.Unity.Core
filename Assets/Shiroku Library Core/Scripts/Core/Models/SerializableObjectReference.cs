using System;
using UnityEngine;

namespace ShirokuStudio.Core.Models
{
    [Serializable]
    public struct SerializableObjectReference
    {
        public enum RootTypes
        {
            Prefab,
            Scene
        }

        [SerializeField]
        public string ID;

        [SerializeField]
        public string rootGuid;

        [SerializeField]
        public RootTypes rootType;

        [SerializeField]
        public string Name;

        public UnityEngine.Object Value;

        [SerializeField]
        private SerializableType type;

        public Type Type { get => type?.Type ?? typeof(UnityEngine.Object); set => type = new(value); }

        public bool IsValid => string.IsNullOrWhiteSpace(ID) == false && string.IsNullOrWhiteSpace(rootGuid) == false;

        public SerializableObjectReference(string id, string rootGuid, RootTypes rootType, UnityEngine.Object value)
        {
            ID = id;
            this.rootGuid = rootGuid;
            this.rootType = rootType;
            Value = value;
            Name = value.name;
            var _type = value.GetType();
            type = new SerializableType(_type);
            Name = $"{value.name}({_type.GetFriendlyName()})";
        }

        public static Func<UnityEngine.Object, SerializableObjectReference> CreateFromObjectDelegate;

        public static SerializableObjectReference CreateFromObject(UnityEngine.Object value)
        {
            return CreateFromObjectDelegate.Invoke(value);
        }
    }
}