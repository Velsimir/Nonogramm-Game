using UnityEngine;
using Zenject;
using G.Core.Boot;

namespace G.Core.DI
{
    public class BootSceneInstaller : MonoInstaller
    {
        [SerializeField] private LoadingScreenView _loadingScreen;

        public override void InstallBindings()
        {
            Container.Bind<LoadingScreenView>().FromInstance(_loadingScreen).AsSingle();
            Container.Bind<ILoadingErrorHandler>().To<LoadingErrorHandler>().AsSingle();

            Container.BindServiceInterfacesOnly<LoadingService>();

            Container.BindPresenter<LoadingScreenPresenter>();
            Container.BindInterfacesTo<GameBootstrapper>().AsSingle().NonLazy();
        }
    }
}
