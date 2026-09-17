using System.Threading;
using Cysharp.Threading.Tasks;

namespace G.Core.UI
{
    public interface IPage : IPageBase
    {
        UniTask ShowAsync(CancellationToken cancellationToken = default);
    }
}
