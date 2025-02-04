using System;
using UniRx;
using UnityEngine;

namespace ShirokuStudio.Core
{
    public static class DebugUtility
    {
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void DrawCube(Vector3 center, Vector3? size = null, float duration = 5f)
        {
            size ??= Vector3.one * 0.2f;

            var act = (Action)(() => Gizmos.DrawCube(center, size.Value));

            GlobalGizmosComponent.Instance.OnDrawGizmosEvent += act;

            Observable.Timer(TimeSpan.FromSeconds(duration))
                .Subscribe(_ => GlobalGizmosComponent.Instance.OnDrawGizmosEvent -= act)
                .AddTo(GlobalGizmosComponent.Instance);
        }
    }
}