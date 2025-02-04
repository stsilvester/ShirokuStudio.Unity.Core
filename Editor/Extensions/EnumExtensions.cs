using System;
using UnityEditor;

namespace ShirokuStudio.Editor
{
    public static class EnumExtensions
    {
        public static TEnum GetEnumValue<TEnum>(this SerializedProperty property)
            where TEnum : Enum
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), property.intValue);
        }

        public static void SetEnumValue<TEnum>(this SerializedProperty property, TEnum value)
            where TEnum : Enum
        {
            property.intValue = Convert.ToInt32(value);
        }
    }
}