using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace G.Core.Boot
{
    public interface ILoadingService
    {
        ReadOnlyReactiveProperty<float> Progress { get; }
        ReadOnlyReactiveProperty<string> CurrentStep { get; }

        UniTask<bool> BeginLoadingAsync(ILoadingOperation[] operations, CancellationToken cancellationToken = default);
    }
}
