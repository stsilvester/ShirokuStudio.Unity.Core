using System;
using UnityEngine;

namespace ShirokuStudio.Core
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class SubClassSelectorAttribute : PropertyAttribute
    {
    }
}