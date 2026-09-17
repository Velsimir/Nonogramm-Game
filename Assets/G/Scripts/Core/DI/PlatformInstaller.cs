using Zenject;
using G.Core.Common;
using G.Platform;
using G.Platform.Stub;

namespace G.Core.DI
{
    public class PlatformInstaller : Installer<PlatformSdkConfig, PlatformInstaller>
    {
        private readonly PlatformSdkConfig _config;

        public PlatformInstaller(PlatformSdkConfig config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
#if UNITY_EDITOR
            BindStub();
#else
            switch (_config != null ? _config.Kind : PlatformSdkKind.EditorStub)
            {
                default:
                    GameDebug.LogWarning("[Platform] Адаптер площадки не подключён, используется заглушка.");
                    BindStub();
                    break;
            }
#endif
        }

        private void BindStub()
        {
            Container.Bind<IPlatformSdk>().To<StubPlatformSdk>().AsSingle();
        }
    }
}
