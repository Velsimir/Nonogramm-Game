using System.Threading;
using Cysharp.Threading.Tasks;

namespace G.Platform
{
    public readonly struct PurchaseResult
    {
        public bool IsSuccess { get; }
        public string ProductId { get; }
        public string Error { get; }

        public PurchaseResult(bool isSuccess, string productId, string error = null)
        {
            IsSuccess = isSuccess;
            ProductId = productId;
            Error = error;
        }
    }

    public interface IPurchaseSdkService
    {
        UniTask<PurchaseResult> PurchaseAsync(string productId, CancellationToken cancellationToken = default);
        UniTask RestoreAsync(CancellationToken cancellationToken = default);
        string GetLocalizedPrice(string productId);
    }
}
