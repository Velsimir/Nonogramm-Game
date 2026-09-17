using System;
using System.Collections.Generic;
using UnityEngine;

namespace G.Core.UI
{
    [CreateAssetMenu(fileName = "PagesConfig", menuName = "Configs/Pages Config")]
    public class PagesConfig : ScriptableObject
    {
        [SerializeField] private PageBase _startPage;
        [SerializeField] private List<PageBase> _pages = new();

        public PageBase StartPage => _startPage;
        public IReadOnlyList<PageBase> Pages => _pages;

        public Type StartPageType => _startPage != null ? _startPage.GetType() : null;

#if UNITY_EDITOR
        [ContextMenu("Собрать все префабы страниц")]
        private void CollectAllPagePrefabs()
        {
            _pages.Clear();

            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab");

            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                    continue;

                PageBase page = prefab.GetComponent<PageBase>();

                if (page != null)
                    _pages.Add(page);
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
