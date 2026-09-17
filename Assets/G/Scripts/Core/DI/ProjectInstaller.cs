using UnityEngine;
using Zenject;
using G.Core.Common;
using G.Core.Curtain;
using G.Core.Tick;
using G.Platform;

namespace G.Core.DI
{
    public class ProjectInstaller : MonoInstaller
    {
        private const int TARGET_FRAMERATE = 60;

        [SerializeField] private CurtainView _curtainPrefab;

        [SerializeField] private PlatformSdkConfig _platformSdkConfig;

        public override void InstallBindings()
        {
            ApplyApplicationSettings();

            GameObject servicesRoot = CreateServicesRoot();
            BindServicesRootComponents(servicesRoot);
            BindCurtain();

            PlatformInstaller.Install(Container, _platformSdkConfig);
            CoreInstaller.Install(Container);
            MetaInstaller.Install(Container);
            GameplayInstaller.Install(Container);
        }

        private void ApplyApplicationSettings()
        {
            Application.targetFrameRate = TARGET_FRAMERATE;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Cysharp.Threading.Tasks.UniTaskScheduler.UnobservedTaskException +=
                exception => GameDebug.LogError($"[UniTask] Необработанное исключение: {exception}");
        }

        private GameObject CreateServicesRoot()
        {
            GameObject servicesRoot = new("[ServicesRoot]");
            DontDestroyOnLoad(servicesRoot);

            return servicesRoot;
        }

        private void BindServicesRootComponents(GameObject servicesRoot)
        {
            TickService tickService = servicesRoot.AddComponent<TickService>();
            Container.Bind<ITickService>().FromInstance(tickService).AsSingle();

            ApplicationLifecycleWatcher lifecycle = servicesRoot.AddComponent<ApplicationLifecycleWatcher>();
            Container.Bind<IApplicationLifecycle>().FromInstance(lifecycle).AsSingle();
        }

        private void BindCurtain()
        {
            if (_curtainPrefab == null)
            {
                GameDebug.LogConfigurationError("[ProjectInstaller] Не назначен префаб шторки.");
                return;
            }

            CurtainView curtain = Instantiate(_curtainPrefab);
            DontDestroyOnLoad(curtain.gameObject);

            Container.Bind<ICurtain>().FromInstance(curtain).AsSingle();
        }
    }
}
