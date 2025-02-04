using ShirokuStudio.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Zenject;

namespace ShirokuStudio.Core
{
    public class GameObjectManager : Installer
    {
        private readonly ConcurrentDictionary<int, WeakReference<GameObject>> _gameObjects = new();
        private DisposableCollection _disposables = new();

        public override void InstallBindings()
        {
            Container.BindInstance(this).AsSingle();
        }

        public void Register(Component component)
        {
            if (component == false)
                return;

            Register(component.gameObject);
        }

        public void Register(GameObject gameObject)
        {
            if (gameObject == false)
                return;

            var id = gameObject.GetInstanceID();
            if (false == _gameObjects.TryAdd(id, new(gameObject)))
                return;

            if (gameObject.GetComponent<GameObjectTracker>())
                return;

            IDisposable d = null;
            d = gameObject.OnDestroyAsObservable()
                .Subscribe(_ =>
                {
                    _disposables.Remove(d);
                    Unregister(gameObject);
                })
                .AddTo(_disposables);
        }

        public void Unregister(GameObject gameObject)
        {
            if (gameObject == false)
                return;

            _gameObjects.TryRemove(gameObject.GetInstanceID(), out _);
        }

        public IEnumerable<GameObject> GetGameObjects()
        {
            foreach (var kv in _gameObjects.ToArray())
            {
                if (kv.Value.TryGetTarget(out var go) != false)
                    yield return go;
                else
                    _gameObjects.TryRemove(kv.Key, out var w);
            }
        }

        public void Refresh()
            => _gameObjects.Keys.ToArray()
                .Where(key => _gameObjects.TryGetValue(key, out var v) == false || v.TryGetTarget(out _) == false)
                .Foreach(key => _gameObjects.TryRemove(key, out _));

        public void Clear()
        {
            _gameObjects.Clear();
            _disposables.Dispose();
        }
    }
}