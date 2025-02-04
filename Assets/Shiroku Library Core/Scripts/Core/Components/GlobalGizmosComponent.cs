using System;
using UnityEngine;

namespace ShirokuStudio.Core
{
    public class GlobalGizmosComponent : MonoBehaviour
    {
        public event Action OnDrawGizmosEvent;

        public event Action OnDrawGizmosSelectedEvent;

        private static GlobalGizmosComponent instance;

        public static GlobalGizmosComponent Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject("GlobalGizmosComponent").AddComponent<GlobalGizmosComponent>();
                    DontDestroyOnLoad(instance.gameObject);
                }

                return instance;
            }
        }

        public void OnDrawGizmos()
        {
            OnDrawGizmosEvent?.Invoke();
        }

        public void OnDrawGizmosSelected()
        {
            OnDrawGizmosSelectedEvent?.Invoke();
        }
    }
}