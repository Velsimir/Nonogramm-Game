using Zenject;
using G.Core.DI;

namespace G.Gameplay.DI
{
    public class GameSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindServiceInterfacesOnly<GameSceneEntry>();
        }
    }
}
