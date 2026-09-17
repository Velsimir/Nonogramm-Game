using System.Threading;
using Cysharp.Threading.Tasks;
using G.Gameplay.Nonogram.Level;

namespace G.Meta.LevelFlow
{
    public interface ILevelFlowService
    {
        NonogramLevelAsset PendingLevel { get; }

        bool IsTransitioning { get; }

        UniTask StartLevelAsync(NonogramLevelAsset level, CancellationToken cancellationToken = default);

        UniTask ReturnToMenuAsync(CancellationToken cancellationToken = default);

        UniTask CompleteTransitionAsync(CancellationToken cancellationToken = default);
    }
}
