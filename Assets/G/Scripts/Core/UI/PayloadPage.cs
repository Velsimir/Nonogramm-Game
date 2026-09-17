using System.Threading;
using Cysharp.Threading.Tasks;

namespace G.Core.UI
{
    public abstract class PayloadPage<TPayload> : PageBase, IPayloadPage<TPayload>
    {
        public async UniTask ShowAsync(TPayload payload, CancellationToken cancellationToken = default)
        {
            if (State.CurrentValue != PageState.Hidden)
                return;

            OnShow(payload);

            await RunShowAsync(cancellationToken);
        }

        protected virtual void OnShow(TPayload payload) { }
    }
}
