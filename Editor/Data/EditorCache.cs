using System;
using System.Collections.Generic;
using System.Dynamic;
using UnityEditor;

namespace ShirokuStudio.Editor
{
    public class EditorCache : DynamicObject
    {
        private readonly string id;
        private readonly string propertyPath;
        private readonly int objectID;
        private Dictionary<string, object> dynamicData;

        public EditorCache(SerializedProperty property)
        {
            propertyPath = property.propertyPath;
            objectID = property.serializedObject.targetObject.GetInstanceID();
            id = $"{objectID}.{propertyPath}";
        }

        private static string getID(SerializedProperty property)
        {
            var propertyPath = property.propertyPath;
            var objectID = property.serializedObject.targetObject.GetInstanceID();
            return $"{objectID}.{propertyPath}";
        }

        private static Dictionary<string, EditorCache> cachedData = new();

        public static T Get<T>(SerializedProperty property) where T : EditorCache
        {
            if (cachedData.TryGetValue(getID(property), out var data))
                return data as T;

            var result = Activator.CreateInstance(typeof(T), property) as T;
            Set(result);
            return result;
        }

        public static void Set(EditorCache data)
        {
            cachedData[data.id] = data;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var key = binder.Name.ToLower();
            return dynamicData.TryGetValue(key, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var key = binder.Name.ToLower();
            dynamicData[key] = value;
            return true;
        }
    }
}
