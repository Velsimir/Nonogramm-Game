using System;
using UnityEngine;
using Zenject;

namespace G.Core.Factory
{
    public class GameFactory
    {
        private readonly DiContainer _container;

        public GameFactory(DiContainer container)
        {
            _container = container;
        }

        public TComponent Instantiate<TComponent>(GameObject prefab, Transform parent,
            Action<DiContainer> extraBindings = null)
        {
            DiContainer subContainer = _container.CreateSubContainer();
            extraBindings?.Invoke(subContainer);

            return subContainer.InstantiatePrefabForComponent<TComponent>(prefab, parent);
        }

        public TComponent Instantiate<TComponent>(TComponent prefab, Transform parent,
            Action<DiContainer> extraBindings = null)
            where TComponent : MonoBehaviour
        {
            return Instantiate<TComponent>(prefab.gameObject, parent, extraBindings);
        }

        public TInstance Create<TInstance>(Action<DiContainer> extraBindings = null)
        {
            DiContainer subContainer = _container.CreateSubContainer();
            extraBindings?.Invoke(subContainer);

            return subContainer.Instantiate<TInstance>();
        }

        public void Inject(GameObject target, Action<DiContainer> extraBindings = null)
        {
            DiContainer subContainer = _container.CreateSubContainer();
            extraBindings?.Invoke(subContainer);

            subContainer.InjectGameObject(target);
        }
    }
}
