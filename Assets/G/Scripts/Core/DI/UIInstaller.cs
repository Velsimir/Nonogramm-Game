using UnityEngine;
using Zenject;
using G.Core.Common;
using G.Core.UI;

namespace G.Core.DI
{
    public class UIInstaller : MonoInstaller
    {
        [SerializeField] private UILayerRoots _layerRoots;
        [SerializeField] private PagesConfig _pages;

        public override void InstallBindings()
        {
            if (_pages == null)
            {
                GameDebug.LogConfigurationError("[UIInstaller] Не назначен PagesConfig сцены.");
                return;
            }

            Container.Bind<UILayerRoots>().FromInstance(_layerRoots).AsSingle();
            Container.Bind<PagesConfig>().FromInstance(_pages).AsSingle();

            Container.BindServiceInterfacesOnly<PagesProvider>();
            Container.BindService<PageService>();
        }
    }
}
