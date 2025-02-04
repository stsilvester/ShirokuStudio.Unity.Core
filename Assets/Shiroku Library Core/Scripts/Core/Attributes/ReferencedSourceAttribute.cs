using System;
using UnityEngine;

namespace ShirokuStudio.Core
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ReferencedSourceAttribute : PropertyAttribute
    {
        public string Name { get; set; }

        public ReferencedSourceAttribute(string name)
        {
            Name = name;
        }
    }
}