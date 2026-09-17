using Zenject;
using G.Gameplay.Nonogram.Board;
using G.Gameplay.Nonogram.Presentation;

namespace G.Core.DI
{
    public class GameplayInstaller : Installer<GameplayInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindService<NonogramBoardService>();
            Container.BindPresenter<NonogramPresenter>();
        }
    }
}
