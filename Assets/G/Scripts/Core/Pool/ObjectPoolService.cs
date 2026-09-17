using System;
using System.Collections.Generic;
using UnityEngine;
using G.Core.Common;
using G.Core.Factory;
using Object = UnityEngine.Object;

namespace G.Core.Pool
{
    public class ObjectPoolService : IDisposable
    {
        private readonly Dictionary<Object, Pool> _pools = new();

        private readonly Dictionary<IPoolable, Pool> _owners = new();

        private readonly GameFactory _factory;
        private readonly Transform _root;

        public ObjectPoolService(GameFactory factory)
        {
            _factory = factory;

            GameObject rootObject = new("[Pools]");
            Object.DontDestroyOnLoad(rootObject);
            _root = rootObject.transform;
        }

        public TPoolable Spawn<TPoolable>(TPoolable prefab, Vector3 position, Quaternion rotation,
            Transform parent = null)
            where TPoolable : MonoBehaviour, IPoolable
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            Pool pool = GetOrCreatePool(prefab);
            IPoolable instance = pool.Get(parent ?? _root);
            _owners[instance] = pool;

            instance.Transform.SetPositionAndRotation(position, rotation);
            instance.GameObject.SetActive(true);
            instance.OnSpawned();

            return (TPoolable)instance;
        }

        public void Despawn(IPoolable instance)
        {
            if (instance == null)
                return;

            if (_owners.TryGetValue(instance, out Pool pool) == false)
            {
                GameDebug.LogWarning($"[Pool] {instance.GameObject.name} не принадлежит пулу, объект уничтожен.");
                Object.Destroy(instance.GameObject);
                return;
            }

            instance.OnDespawned();
            instance.GameObject.SetActive(false);
            instance.Transform.SetParent(_root, worldPositionStays: false);
            pool.Return(instance);
        }

        public void Prewarm<TPoolable>(TPoolable prefab, int count)
            where TPoolable : MonoBehaviour, IPoolable
        {
            Pool pool = GetOrCreatePool(prefab);
            pool.Prewarm(count, _root);
        }

        public void Dispose()
        {
            _pools.Clear();
            _owners.Clear();

            if (_root != null)
                Object.Destroy(_root.gameObject);
        }

        private Pool GetOrCreatePool<TPoolable>(TPoolable prefab)
            where TPoolable : MonoBehaviour, IPoolable
        {
            if (_pools.TryGetValue(prefab, out Pool pool))
                return pool;

            pool = new Pool(prefab.gameObject, _factory);
            _pools.Add(prefab, pool);

            return pool;
        }

        private class Pool
        {
            private readonly Stack<IPoolable> _inactive = new();
            private readonly GameObject _prefab;
            private readonly GameFactory _factory;

            public Pool(GameObject prefab, GameFactory factory)
            {
                _prefab = prefab;
                _factory = factory;
            }

            public IPoolable Get(Transform parent)
            {
                while (_inactive.Count > 0)
                {
                    IPoolable pooled = _inactive.Pop();

                    if (pooled == null || pooled.GameObject == null)
                        continue;

                    pooled.Transform.SetParent(parent, worldPositionStays: false);
                    return pooled;
                }

                IPoolable created = _factory.Instantiate<IPoolable>(_prefab, parent);
                return created;
            }

            public void Return(IPoolable instance)
            {
                _inactive.Push(instance);
            }

            public void Prewarm(int count, Transform root)
            {
                for (int i = 0; i < count; i++)
                {
                    IPoolable instance = _factory.Instantiate<IPoolable>(_prefab, root);
                    instance.GameObject.SetActive(false);
                    _inactive.Push(instance);
                }
            }
        }
    }
}
