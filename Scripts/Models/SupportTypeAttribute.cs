using System;
using UnityEngine;

namespace ShirokuStudio.Core.Models
{
    [System.AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class SupportTypeAttribute : PropertyAttribute, ITypeFilter
    {
        public Type Type { get; }

        private TriState[] flags = new TriState[4];

        public TriState IsAbstract { get => flags[0]; set => flags[0] = value; }
        public TriState IsInterface { get => flags[1]; set => flags[1] = value; }
        public TriState IsStruct { get => flags[2]; set => flags[2] = value; }
        public TriState IsGeneric { get => flags[3]; set => flags[3] = value; }

        public SupportTypeAttribute(Type type)
        {
            Type = type;
        }
    }
}