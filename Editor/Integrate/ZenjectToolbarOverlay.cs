using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Zenject;

namespace ShirokuStudio.Editor
{
    public abstract class ZenjectToolbarOverlay : UnityEditor.Overlays.ToolbarOverlay
    {
        [Inject]
        [NonSerialized]
        private Kernel _kernel;

        [NonSerialized]
        private DiContainer _container;

        protected VisualElement Root { get; private set; }

        protected DiContainer Container => _container;

        public ZenjectToolbarOverlay(params string[] toolbarElementsIDs) : base(toolbarElementsIDs)
        {
            Assert.IsNull(_container);

            var parents = new[] { StaticEditorContext.Container } as IEnumerable<DiContainer>;
            _container = new DiContainer(parents);

            // Make sure we don't create any game objects since editor windows don't have a scene
            _container.AssertOnNewGameObjects = true;

            ZenjectManagersInstaller.Install(_container);
            _container.Bind<Kernel>().AsSingle();
            _container.BindInstance(this);

            InstallBindings();

            _container.QueueForInject(this);
            _container.ResolveRoots();

            _kernel.Initialize();

            Initialize();
        }

        protected abstract void InstallBindings();

        public override void OnWillBeDestroyed()
        {
            base.OnWillBeDestroyed();
            _kernel.Dispose();
        }

        public virtual void Initialize()
        {
        }
    }
}