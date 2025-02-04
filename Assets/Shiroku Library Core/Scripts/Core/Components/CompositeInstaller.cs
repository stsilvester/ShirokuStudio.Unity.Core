using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace ShirokuStudio.Core
{
    public class CompositeInstaller : MonoInstaller
    {
        [SerializeField]
        public List<MonoInstaller> Installers = new();

        public override void InstallBindings()
        {
            foreach (var installer in Installers)
            {
                Container.Inject(installer);
                installer.InstallBindings();
            }
        }
    }
}