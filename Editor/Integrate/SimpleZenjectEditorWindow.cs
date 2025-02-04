using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEditor;
using UnityEngine.Assertions;
using Zenject;

namespace ShirokuStudio.Editor
{
    public abstract class SimpleZenjectEditorWindow : EditorWindow
    {
        [Inject]
        [NonSerialized]
        private Kernel _kernel;

        [Inject]
        private DisposableManager disposableManager;

        public DiContainer Container { get; private set; }
        private IEnumerable<DiContainer> parentContainers;

        public readonly IntReactiveProperty TickFPS = new();
        private float tickInterval;

        private readonly Subject<Unit> onTick = new();

        private double lastTime;
        public float DeltaTime { get; private set; }

        public SimpleZenjectEditorWindow(params DiContainer[] parentContainers)
        {
            this.parentContainers = parentContainers;
        }

        public virtual void OnEnable()
            => Initialize();

        protected virtual void Initialize()
        {
            Assert.IsNull(Container);

            var parents = new[] { StaticEditorContext.Container } as IEnumerable<DiContainer>;
            if (parentContainers?.Any() == true)
                parents = parents.Concat(parentContainers);
            Container = new DiContainer(parents);

            // Make sure we don't create any game objects since editor windows don't have a scene
            Container.AssertOnNewGameObjects = true;

            ZenjectManagersInstaller.Install(Container);
            Container.Bind<Kernel>().AsSingle();
            Container.BindInstance(this);

            lastTime = EditorApplication.timeSinceStartup;

            InstallBindings();

            Container.QueueForInject(this);
            Container.ResolveRoots();

            _kernel.Initialize();

            initializeTick();
        }

        private void initializeTick()
        {
            disposableManager.Add(TickFPS
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromSeconds(0.4f))
                .Subscribe(fps =>
                {
                    if (fps > 0)
                    {
                        lastTime = EditorApplication.timeSinceStartup;
                        tickInterval = 1f / fps;
                    }
                    else
                    {
                        tickInterval = 0;
                    }
                }));

            disposableManager.Add(onTick
                .Where(_ => tickInterval > 0)
                .Select(_ => EditorApplication.timeSinceStartup)
                .Where(now => now - lastTime >= tickInterval)
                .Subscribe(now =>
                {
                    DeltaTime = (float)(now - lastTime);
                    _kernel.Tick();
                    lastTime = now;
                }));
        }

        public virtual void OnDisable()
            => _kernel.Dispose();

        public abstract void InstallBindings();

        public virtual void OnGUI()
        {
        }

        public virtual void Update()
        {
            if (TickFPS.Value > 0)
                onTick.OnNext(Unit.Default);
        }
    }
}