using System;

namespace UnityEngine
{
    public static class TransformExtensions
    {
        public static bool TryFind(this Transform trans, string name, out Transform target, bool recursively = true)
            => trans.name == name ? (bool)(target = trans)
                : (target = recursively ? FindRecursively(trans, name) : trans.Find(name));

        public static Transform FindRecursively(this Transform trans, string name)
        {
            if (trans.name == name)
                return trans;

            for (var i = 0; i < trans.childCount; i++)
            {
                var child = trans.GetChild(i);
                var result = child.FindRecursively(name);
                if (result)
                    return result;
            }

            return null;
        }

        public static Transform Find(this Transform transform,Func<Transform, bool> predicate)
        {
            if (predicate(transform))
                return transform;

            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                var result = child.Find(predicate);
                if (result)
                    return result;
            }

            return null;
        }

        public static Transform Reset(this Transform transform, bool isLocal = true)
        {
            if (isLocal)
            {
                transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                transform.localScale = Vector3.one;
            }
            else
            {
                transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                transform.localScale = Vector3.one;
            }
            return transform;
        }
    }
}