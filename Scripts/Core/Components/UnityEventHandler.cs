using NaughtyAttributes;
using UltEvents;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Zenject;

namespace ShirokuStudio.Core
{
    public class UnityEventHandler : MonoBehaviour
    {
        [SerializeField]
        private UltEvent onEnabled;

        [SerializeField]
        private UltEvent onAwake;

        [SerializeField]
        private UltEvent onStart;

        [SerializeField]
        private UltEvent onDisabled;

        [SerializeField]
        private UltEvent onDestroy;

        [SerializeField]
        [Foldout("Updates")]
        [InfoBox("可能會降低效能，請謹慎使用", EInfoBoxType.Warning)]
        private UltEvent onUpdate;

        [SerializeField]
        [Foldout("Updates")]
        private UltEvent onFixedUpdate;

        [SerializeField]
        [Foldout("Updates")]
        private UltEvent onLateUpdate;

        [SerializeField]
        [Foldout("Injection")]
        private UltEvent onInitialized;

        [Inject]
        private void initialize()
        {
            onInitialized.Invoke();
        }

        private void Awake()
        {
            onAwake?.Invoke();

            if (onUpdate.HasCalls)
            {
                this.UpdateAsObservable()
                    .Subscribe(_ => onUpdate.Invoke());
            }

            if (onFixedUpdate.HasCalls)
            {
                this.FixedUpdateAsObservable()
                    .Subscribe(_ => onFixedUpdate.Invoke());
            }

            if (onLateUpdate.HasCalls)
            {
                this.LateUpdateAsObservable()
                    .Subscribe(_ => onLateUpdate.Invoke());
            }
        }

        private void Start()
        {
            onStart?.Invoke();
        }

        private void OnEnable()
        {
            onEnabled?.Invoke();
        }

        private void OnDisable()
        {
            onDisabled?.Invoke();
        }

        private void OnDestroy()
        {
            onDestroy?.Invoke();
        }
    }
}