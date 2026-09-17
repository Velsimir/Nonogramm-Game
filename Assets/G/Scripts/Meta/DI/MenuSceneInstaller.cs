using Zenject;
using G.Core.DI;
using G.Meta.LevelSelect;

namespace G.Meta.DI
{
    public class MenuSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindServiceInterfacesOnly<MenuSceneEntry>();
        }
    }
}
