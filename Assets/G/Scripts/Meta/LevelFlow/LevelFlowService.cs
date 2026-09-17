using System.Threading;
using Cysharp.Threading.Tasks;
using G.Core.Common;
using G.Core.Curtain;
using G.Core.Scenes;
using G.Gameplay.Nonogram.Level;

namespace G.Meta.LevelFlow
{
    public class LevelFlowService : Service, ILevelFlowService
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly ICurtain _curtain;

        private NonogramLevelAsset _pendingLevel;
        private bool _isTransitioning;

        public LevelFlowService(ISceneLoader sceneLoader, ICurtain curtain)
        {
            _sceneLoader = sceneLoader;
            _curtain = curtain;
        }

        public NonogramLevelAsset PendingLevel => _pendingLevel;
        public bool IsTransitioning => _isTransitioning;

        public async UniTask StartLevelAsync(NonogramLevelAsset level,
            CancellationToken cancellationToken = default)
        {
            if (level == null)
            {
                GameDebug.LogConfigurationError("[LevelFlow] Запуск уровня без ассета уровня.");
                return;
            }

            if (_isTransitioning)
                return;

            _isTransitioning = true;
            _pendingLevel = level;

            await SwapScenesAsync(ScenesName.Game, ScenesName.Menu, cancellationToken);
        }

        public async UniTask ReturnToMenuAsync(CancellationToken cancellationToken = default)
        {
            if (_isTransitioning)
                return;

            _isTransitioning = true;
            _pendingLevel = null;

            await SwapScenesAsync(ScenesName.Menu, ScenesName.Game, cancellationToken);
        }

        public async UniTask CompleteTransitionAsync(CancellationToken cancellationToken = default)
        {
            if (_isTransitioning == false)
                return;

            _isTransitioning = false;

            if (_curtain.IsShown)
                await _curtain.HideAsync(cancellationToken);
        }

        private async UniTask SwapScenesAsync(ScenesName target, ScenesName previous,
            CancellationToken cancellationToken)
        {
            await _curtain.ShowAsync(cancellationToken);

            await _sceneLoader.LoadAdditiveAsync(target, cancellationToken: cancellationToken);
            _sceneLoader.SetActive(target);

            if (_sceneLoader.IsLoaded(previous))
                await _sceneLoader.UnloadAsync(previous, cancellationToken);
        }
    }
}
