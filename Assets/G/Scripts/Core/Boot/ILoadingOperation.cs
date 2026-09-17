using System.Threading;
using Cysharp.Threading.Tasks;

namespace G.Core.Boot
{
    public interface ILoadingOperation
    {
        string Description { get; }

        float Weight { get; }

        UniTask ExecuteAsync(CancellationToken cancellationToken);
    }
}
