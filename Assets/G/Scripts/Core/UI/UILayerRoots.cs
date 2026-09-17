using System;
using System.Collections.Generic;
using UnityEngine;
using G.Core.Common;

namespace G.Core.UI
{
    public class UILayerRoots : MonoBehaviour
    {
        [SerializeField] private List<LayerRoot> _roots = new();
        [SerializeField] private InputBlocker _inputBlocker;

        public InputBlocker InputBlocker => _inputBlocker;

        public Transform GetRoot(UILayer layer)
        {
            foreach (LayerRoot root in _roots)
            {
                if (root.Layer == layer)
                    return root.Root;
            }

            GameDebug.LogConfigurationError($"[UI] Для слоя {layer} не задан корень в UILayerRoots.");
            return transform;
        }

        public IReadOnlyList<string> Validate()
        {
            List<string> problems = new();

            if (_inputBlocker == null)
                problems.Add("Не назначен InputBlocker.");

            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                bool found = false;

                foreach (LayerRoot root in _roots)
                {
                    if (root.Layer != layer)
                        continue;

                    found = true;

                    if (root.Root == null)
                        problems.Add($"У слоя {layer} пустой корень.");

                    break;
                }

                if (found == false)
                    problems.Add($"Слой {layer} отсутствует в списке корней.");
            }

            return problems;
        }

        [Serializable]
        private class LayerRoot
        {
            [SerializeField] private UILayer _layer;
            [SerializeField] private Transform _root;

            public UILayer Layer => _layer;
            public Transform Root => _root;
        }
    }
}
