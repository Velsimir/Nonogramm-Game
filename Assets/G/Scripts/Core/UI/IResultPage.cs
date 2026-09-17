using System.Threading;
using Cysharp.Threading.Tasks;

namespace G.Core.UI
{
    public interface IResultPage<TResult> : IPageBase
    {
        UniTask<TResult> ResultAsync(CancellationToken cancellationToken = default);
    }
}
