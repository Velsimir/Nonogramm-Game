using System.Threading;
using Cysharp.Threading.Tasks;

namespace G.Platform
{
    public interface IAdsSdkService
    {
        bool IsRewardedReady { get; }

        UniTask<bool> ShowRewardedAsync(string placement, CancellationToken cancellationToken = default);

        UniTask ShowInterstitialAsync(string placement, CancellationToken cancellationToken = default);

        void ShowBanner();
        void HideBanner();

        void DisableAds();
    }
}
