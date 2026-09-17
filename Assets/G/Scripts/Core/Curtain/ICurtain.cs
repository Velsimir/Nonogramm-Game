using System.Threading;
using Cysharp.Threading.Tasks;

namespace G.Core.Curtain
{
    public interface ICurtain
    {
        bool IsShown { get; }

        UniTask ShowAsync(CancellationToken cancellationToken = default);
        UniTask HideAsync(CancellationToken cancellationToken = default);
    }
}
