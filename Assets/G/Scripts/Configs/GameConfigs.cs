using UnityEngine;
using G.Core.UI;
using G.Gameplay.Nonogram.Config;
using G.Gameplay.Nonogram.Level;
using G.Platform;

namespace G.Configs
{
    [CreateAssetMenu(fileName = "GameConfigs", menuName = "Configs/Game Configs")]
    public class GameConfigs : ScriptableObject
    {
        [SerializeField] private PagesConfig _pages;
        [SerializeField] private PlatformSdkConfig _platformSdk;
        [SerializeField] private NonogramRuleConfig _nonogramRule;
        [SerializeField] private NonogramLevelsConfig _nonogramLevels;

        public PagesConfig Pages => _pages;
        public PlatformSdkConfig PlatformSdk => _platformSdk;
        public NonogramRuleConfig NonogramRule => _nonogramRule;
        public NonogramLevelsConfig NonogramLevels => _nonogramLevels;

        private void OnValidate()
        {
            WarnIfMissing(_pages, nameof(_pages));
            WarnIfMissing(_platformSdk, nameof(_platformSdk));
            WarnIfMissing(_nonogramRule, nameof(_nonogramRule));
            WarnIfMissing(_nonogramLevels, nameof(_nonogramLevels));
        }

        private void WarnIfMissing(Object reference, string fieldName)
        {
            if (reference == null)
                Debug.LogError($"[GameConfigs] Не назначен конфиг «{fieldName}».", this);
        }
    }
}
