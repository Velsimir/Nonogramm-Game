using MessagePipe;
using Zenject;
using G.Core.Factory;
using G.Core.Pool;
using G.Core.Save;
using G.Core.Scenes;

namespace G.Core.DI
{
    public class CoreInstaller : Installer<CoreInstaller>
    {
        public override void InstallBindings()
        {
            InstallMessagePipe();
            InstallFactory();
            InstallScenes();
            InstallSave();
        }

        private void InstallMessagePipe()
        {
            MessagePipeOptions options = Container.BindMessagePipe();
            Container.InstallAllMessageBrokers(options);
        }

        private void InstallFactory()
        {
            Container.Bind<GameFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<ObjectPoolService>().AsSingle();
        }

        private void InstallScenes()
        {
            Container.BindServiceInterfacesOnly<SceneLoaderService>();
        }

        private void InstallSave()
        {
            Container.Bind<ISaveStorage>()
                .WithId(SaveStorageId.LOCAL)
                .To<LocalFileSaveStorage>()
                .AsCached();

            Container.Bind<ISaveStorage>()
                .WithId(SaveStorageId.CLOUD)
                .To<CloudSaveStorage>()
                .AsCached();

            Container.BindService<SaveService>();
        }
    }
}
