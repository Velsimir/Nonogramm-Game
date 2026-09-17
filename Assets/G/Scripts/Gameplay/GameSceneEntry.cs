using Cysharp.Threading.Tasks;
using MessagePipe;
using G.Core.Common;
using G.Core.UI;
using G.Gameplay.Nonogram.Board;
using G.Gameplay.Nonogram.Level;
using G.Gameplay.Nonogram.UI;
using G.Meta.LevelFlow;

namespace G.Gameplay
{
    public class GameSceneEntry : Service
    {
        private readonly IPageService _pageService;
        private readonly ILevelFlowService _flow;
        private readonly INonogramBoardService _board;
        private readonly NonogramLevelsConfig _levels;
        private readonly ISubscriber<NonogramMessages.BoardFailed> _boardFailed;

        public GameSceneEntry(
            IPageService pageService,
            ILevelFlowService flow,
            INonogramBoardService board,
            NonogramLevelsConfig levels,
            ISubscriber<NonogramMessages.BoardFailed> boardFailed)
        {
            _pageService = pageService;
            _flow = flow;
            _board = board;
            _levels = levels;
            _boardFailed = boardFailed;
        }

        protected override void OnInitialize()
        {
            Disposables.Add(_boardFailed.Subscribe(_ => ShowFailedAsync().Forget()));

            OpenLevelAsync().Forget();
        }

        private async UniTaskVoid OpenLevelAsync()
        {
            await UniTask.NextFrame(DisposeToken);

            if (TryResolveLevel(out NonogramLevelAsset level))
                await _pageService.ShowAsync<NonogramPage, NonogramLevelAsset>(level, DisposeToken);

            await _flow.CompleteTransitionAsync(DisposeToken);
        }

        private async UniTaskVoid ShowFailedAsync()
        {
            LevelFailedResult result =
                await _pageService.ShowForResultAsync<LevelFailedPage, LevelFailedResult>(DisposeToken);

            if (result == LevelFailedResult.Continue)
            {
                _board.Revive();
                return;
            }

            _flow.ReturnToMenuAsync().Forget();
        }

        private bool TryResolveLevel(out NonogramLevelAsset level)
        {
            level = _flow.PendingLevel;

            if (level != null)
                return true;

            if (_levels.TryGetByIndex(0, out level))
            {
                GameDebug.LogWarning(
                    "[LevelFlow] Сцена Game открыта без выбранного уровня, взят первый уровень из конфига.");
                return true;
            }

            GameDebug.LogConfigurationError(
                "[LevelFlow] Сцена Game открыта без выбранного уровня, а NonogramLevelsConfig пуст.");
            return false;
        }
    }
}
