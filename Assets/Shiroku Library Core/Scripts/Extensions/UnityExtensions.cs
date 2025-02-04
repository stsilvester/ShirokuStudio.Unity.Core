using System.Collections.Generic;

namespace UnityEngine
{
    public static class UnityGameObjectExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            var cmp = go.GetComponent<T>();
            if (!cmp)
            {
#if UNITY_EDITOR
                if (Application.isEditor && Application.isPlaying == false)
                    UnityEditor.Undo.AddComponent<T>(go);
                else
#endif
                    cmp = go.AddComponent<T>();
            }
            return cmp;
        }
    }

    public static class UnityComponentExtensions
    {
        public static T GetOrAddComponent<T>(this Component cmp) where T : Component
        {
            return cmp.gameObject.GetOrAddComponent<T>();
        }
    }

    public static class UnityTransformExtensions
    {
        public static IEnumerable<Transform> GetAncestors(this Transform transform, bool includeSelf = false)
        {
            if (includeSelf)
            {
                yield return transform;
            }
            var parent = transform.parent;
            while (parent)
            {
                yield return parent;
                parent = parent.parent;
            }
        }
    }
}