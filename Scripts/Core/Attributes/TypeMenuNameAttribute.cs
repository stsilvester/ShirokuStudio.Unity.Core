using System;

namespace ShirokuStudio.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TypeMenuNameAttribute : Attribute
    {
        public string Name { get; set; }

        public TypeMenuNameAttribute(string name)
        {
            Name = name;
        }
    }
}