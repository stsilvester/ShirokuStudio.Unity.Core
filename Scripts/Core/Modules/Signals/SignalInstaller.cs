using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShirokuStudio.Siganls
{
    public class SignalInstaller : Zenject.Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<SignalCenter>()
                .FromInstance(SignalCenter.Instance)
                .AsSingle()
                .IfNotBound();

            var debug = new List<string>();
            var type_builder = typeof(ISignalBuilder);
            var type_generic_builder = typeof(ISignalBuilder<>);
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => asm.FullName.Contains("Assembly-CSharp"))
                .SelectMany(asm => asm.GetTypes())
                .Where(type => type_builder.IsAssignableFrom(type) && type.IsAbstract == false && type.IsInterface == false)
                .Foreach(type =>
                {
                    var genericType = type.GetInterfaces()
                        .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == type_generic_builder);

                    if (genericType != null)
                    {
                        var signalType = genericType.GetGenericArguments().First();
                        Container.Bind(type, genericType).To(type).AsTransient();
                        debug.Add(type.FullName);
                    }
                });
            UnityEngine.Debug.Log($"註冊SignalBuilder:[{debug.Join(", \n")}]");
        }
    }
}