using System.Threading;
using Cysharp.Threading.Tasks;

namespace G.Core.UI
{
    public abstract class Page : PageBase, IPage
    {
        public async UniTask ShowAsync(CancellationToken cancellationToken = default)
        {
            if (State.CurrentValue != PageState.Hidden)
                return;

            OnShow();

            await RunShowAsync(cancellationToken);
        }

        protected virtual void OnShow() { }
    }
}
