using Zenject;
using G.Meta.LevelFlow;
using G.Meta.LevelProgress;
using G.Meta.LevelSelect;

namespace G.Core.DI
{
    public class MetaInstaller : Installer<MetaInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindService<LevelFlowService>();
            Container.BindService<LevelProgressService>();
            Container.BindService<LevelPreviewFactory>();
        }
    }
}
