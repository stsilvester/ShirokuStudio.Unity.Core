using Zenject;

namespace ShirokuStudio.Editor
{
    public class ZenjectEditor : UnityEditor.Editor
    {
        [InjectLocal]
        private Kernel _kernel;

        public DiContainer Container { get; private set; }

        private void OnEnable()
        {
            if (Container != null)
                return;

            Container = new DiContainer(StaticEditorContext.Container);

            Container.AssertOnNewGameObjects = true;

            ZenjectManagersInstaller.Install(Container);
            Container.Bind<Kernel>().AsSingle();
            Container.BindInstance(this);

            InstallBindings();

            Container.QueueForInject(this);
            Container.ResolveRoots();

            _kernel.Initialize();
        }

        public virtual void InstallBindings()
        { }
    }
}