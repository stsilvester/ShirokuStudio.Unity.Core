using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace ShirokuStudio.Editor
{
    [DefaultExecutionOrder(1)]
    public class StaticEditorContext
    {
        private static readonly Lazy<DiContainer> lazyContainer = new Lazy<DiContainer>(() => StaticContext.Container.CreateSubContainer());
        public static DiContainer Container => lazyContainer.Value;

        [DefaultExecutionOrder(1)]
        public class Installer
        {
            [InjectLocal]
            private readonly IEnumerable<IInitializable> initializables;

            [InitializeOnLoadMethod]
            private static async void initialize()
            {
                await Task.Yield();
                if (Application.isPlaying)
                {
                    UnityEngine.Debug.LogWarning("StaticEditorContext should not be initialized in play mode.");
                    return;
                }
                var instance = new Installer();
                Container.Inject(instance);

                instance.initializables.ToList().ForEach(i => i.Initialize());
            }
        }
    }
}