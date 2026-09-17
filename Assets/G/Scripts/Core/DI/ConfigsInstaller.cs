using UnityEngine;
using Zenject;
using G.Configs;
using G.Core.Common;
using G.Core.UI;
using G.Platform;

namespace G.Core.DI
{
    [CreateAssetMenu(fileName = "ConfigsInstaller", menuName = "Installers/Configs Installer")]
    public class ConfigsInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private GameConfigs _gameConfigs;

        public override void InstallBindings()
        {
            if (_gameConfigs == null)
            {
                GameDebug.LogConfigurationError("[ConfigsInstaller] Не назначен GameConfigs.");
                return;
            }

            Container.Bind<GameConfigs>().FromInstance(_gameConfigs).AsSingle();

            BindChild(_gameConfigs.Pages, nameof(GameConfigs.Pages));
            BindChild(_gameConfigs.PlatformSdk, nameof(GameConfigs.PlatformSdk));
            BindChild(_gameConfigs.NonogramRule, nameof(GameConfigs.NonogramRule));
            BindChild(_gameConfigs.NonogramLevels, nameof(GameConfigs.NonogramLevels));
        }

        private void BindChild<TConfig>(TConfig config, string name) where TConfig : ScriptableObject
        {
            if (config == null)
            {
                GameDebug.LogConfigurationError($"[ConfigsInstaller] В GameConfigs не назначен «{name}».");
                return;
            }

            Container.Bind<TConfig>().FromInstance(config).AsSingle();
        }
    }
}
