using System.Collections.Generic;
using UnityEngine;

namespace G.Gameplay.Nonogram.Level
{
    [CreateAssetMenu(fileName = "NonogramLevelsConfig", menuName = "Configs/Nonogram/Levels Config")]
    public class NonogramLevelsConfig : ScriptableObject
    {
        [SerializeField] private List<NonogramLevelAsset> _levels = new();

        public IReadOnlyList<NonogramLevelAsset> Levels => _levels;

        public bool TryGetByIndex(int index, out NonogramLevelAsset level)
        {
            level = null;

            if (index < 0 || index >= _levels.Count)
                return false;

            level = _levels[index];
            return level != null;
        }

        public bool TryGetById(string id, out NonogramLevelAsset level)
        {
            level = null;

            foreach (NonogramLevelAsset candidate in _levels)
            {
                if (candidate == null || candidate.Id != id)
                    continue;

                level = candidate;
                return true;
            }

            return false;
        }

        private void OnValidate()
        {
            HashSet<string> ids = new();

            for (int i = 0; i < _levels.Count; i++)
            {
                NonogramLevelAsset level = _levels[i];

                if (level == null)
                {
                    Debug.LogError($"[NonogramLevelsConfig] Пустая ссылка на уровень под индексом {i}.", this);
                    continue;
                }

                if (ids.Add(level.Id) == false)
                    Debug.LogError($"[NonogramLevelsConfig] Уровень {level.Id} встречается дважды.", this);
            }
        }
    }
}
