using NaughtyAttributes;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Zenject;

namespace ShirokuStudio.Core
{
    public class GameObjectTracker : MonoBehaviour
    {
#if UNITY_EDITOR

        [SerializeField]
#endif
        [ReadOnly]
        private bool isTracked = false;

        [Inject(Optional = true)]
        private readonly GameObjectManager manager;

        [Inject]
        protected void onInjected()
        {
            if (manager != null)
            {
                isTracked = true;
                manager.Register(gameObject);
                gameObject.OnDestroyAsObservable()
                    .Subscribe(_ =>
                    {
                        isTracked = false;
                        manager?.Unregister(gameObject);
                    })
                    .AddTo(this);
            }
        }
    }
}