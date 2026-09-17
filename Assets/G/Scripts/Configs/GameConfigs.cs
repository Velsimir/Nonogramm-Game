using UnityEngine;
using G.Core.UI;
using G.Platform;

namespace G.Configs
{
    [CreateAssetMenu(fileName = "GameConfigs", menuName = "Configs/Game Configs")]
    public class GameConfigs : ScriptableObject
    {
        [SerializeField] private PagesConfig _pages;
        [SerializeField] private PlatformSdkConfig _platformSdk;

        public PagesConfig Pages => _pages;
        public PlatformSdkConfig PlatformSdk => _platformSdk;

        private void OnValidate()
        {
            WarnIfMissing(_pages, nameof(_pages));
            WarnIfMissing(_platformSdk, nameof(_platformSdk));
        }

        private void WarnIfMissing(Object reference, string fieldName)
        {
            if (reference == null)
                Debug.LogError($"[GameConfigs] Не назначен конфиг «{fieldName}».", this);
        }
    }
}
