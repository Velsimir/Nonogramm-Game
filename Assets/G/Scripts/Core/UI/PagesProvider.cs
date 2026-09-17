using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Zenject;
using G.Core.Common;
using G.Core.Factory;

namespace G.Core.UI
{
    public class PagesProvider : IPagesProvider, IInitializable
    {
        private readonly Dictionary<Type, IPageBase> _cache = new();
        private readonly Dictionary<Type, PageBase> _prefabs = new();

        private readonly PagesConfig _config;
        private readonly UILayerRoots _layerRoots;
        private readonly GameFactory _factory;

        public PagesProvider(PagesConfig config, UILayerRoots layerRoots, GameFactory factory)
        {
            _config = config;
            _layerRoots = layerRoots;
            _factory = factory;
        }

        public void Initialize()
        {
            ValidateConfiguration();
        }

        public bool TryGet<TPage>(out TPage page) where TPage : class, IPageBase
        {
            bool found = TryGet(typeof(TPage), out IPageBase instance);
            page = instance as TPage;
            return found && page != null;
        }

        public bool TryGet(Type pageType, out IPageBase page)
        {
            if (_cache.TryGetValue(pageType, out page))
                return true;

            if (_prefabs.TryGetValue(pageType, out PageBase prefab) == false)
            {
                GameDebug.LogConfigurationError($"[UI] Страница {pageType.Name} не найдена в PagesConfig.");
                page = null;
                return false;
            }

            Transform root = _layerRoots.GetRoot(prefab.Layer);

            page = _factory.Instantiate<PageBase>(
                prefab.gameObject,
                root,
                container => container.BindInstance(new PageId(pageType)));

            _cache.Add(pageType, page);
            return true;
        }

        private bool ValidateConfiguration()
        {
            List<string> problems = new();

            if (_config == null)
            {
                GameDebug.LogConfigurationError("[UI] PagesConfig не назначен.");
                return false;
            }

            foreach (string problem in _layerRoots.Validate())
                problems.Add(problem);

            for (int i = 0; i < _config.Pages.Count; i++)
            {
                PageBase prefab = _config.Pages[i];

                if (prefab == null)
                {
                    problems.Add($"Пустая ссылка на префаб под индексом {i}.");
                    continue;
                }

                Type type = prefab.GetType();

                if (_prefabs.ContainsKey(type))
                {
                    problems.Add($"Дубликат страницы {type.Name}: один тип встречается дважды.");
                    continue;
                }

                _prefabs.Add(type, prefab);
            }

            if (problems.Count == 0)
                return true;

            StringBuilder builder = new();
            builder.AppendLine("[UI] Конфигурация страниц содержит ошибки:");

            foreach (string problem in problems)
                builder.AppendLine("  - " + problem);

            GameDebug.LogConfigurationError(builder.ToString());
            return false;
        }
    }
}
