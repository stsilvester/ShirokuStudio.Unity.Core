using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace ShirokuStudio.Core
{
    public class ObjectProvider : MonoInstaller, IObjectProvider
    {
        [Serializable]
        public class ObjectEntry
        {
            [SerializeField, HideInInspector]
            public string ID;

            [SerializeField]
            public UnityEngine.Object Object;

            public override string ToString()
            {
                return $"{Object?.name ?? "null"}({Object?.GetType()?.GetFriendlyName()})";
            }
        }

        [SerializeField]
        public List<ObjectEntry> List = new();

        private void OnValidate()
        {
            List = List.Where(e => e.Object == true).ToList();
        }

        public override void InstallBindings()
        {
            foreach (var entry in List)
            {
                if (entry.Object is null)
                    continue;

                Container.Bind<UnityEngine.Object>()
                    .WithId(entry.ID)
                    .FromInstance(entry.Object);
            }

            UnityEngine.Debug.Log($"ObjectProvider.InstallBindings: {List.Count} objects bound. [{List.Join(",\n\t")}]");
        }

        public void OnDestroy()
        {
            if (Container == null)
            {
                if (List.Any())
                {
                    UnityEngine.Debug.LogWarning("ObjectProvider.OnDestroy: Container is null. z");
                }
                return;
            }
            foreach (var entry in List)
            {
                Container.UnbindId<UnityEngine.Object>(entry.ID);
            }

            UnityEngine.Debug.Log($"ObjectProvider.OnDestroy: {List.Count} objects unbound. [{List.Join(", ")}]");

            List.Clear();
        }

        public T GetObject<T>(string id) where T : UnityEngine.Object
            => List.FirstOrDefault(e => e.ID == id)?.Object as T;

        public string SetObject<T>(T obj) where T : UnityEngine.Object
        {
            var entry = List.FirstOrDefault(e => e.Object == obj);

            if (entry is null)
            {
                entry = new ObjectEntry { ID = Guid.NewGuid().ToString() };
                List.Add(entry);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }

            entry.Object = obj;
            return entry.ID;
        }
    }
}