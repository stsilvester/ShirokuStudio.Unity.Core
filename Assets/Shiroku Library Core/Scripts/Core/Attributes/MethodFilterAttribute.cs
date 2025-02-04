using System;
using UnityEngine;

namespace ShirokuStudio.Core
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class MethodFilterAttribute : PropertyAttribute
    {
        public int MaxParameterLength;
        public string Regex;
        public Type ReturnType;

        public MethodFilterAttribute(
            int parLen = 0,
            Type reType = null,
            string regex = null)
        {
            MaxParameterLength = parLen;
            ReturnType = reType;
            Regex = regex;
        }
    }
}