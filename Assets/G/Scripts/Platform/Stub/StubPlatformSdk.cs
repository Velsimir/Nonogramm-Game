using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using G.Core.Common;

namespace G.Platform.Stub
{
    public class StubPlatformSdk : IPlatformSdk
    {
        public StubPlatformSdk()
        {
            Ads = new StubAdsSdkService();
            Purchases = new StubPurchaseSdkService();
            Analytics = new StubAnalyticsSdkService();
            Auth = new StubAuthSdkService();
            CloudSave = new StubCloudSaveSdkService();
            Leaderboards = new StubLeaderboardSdkService();
        }

        public IAdsSdkService Ads { get; }
        public IPurchaseSdkService Purchases { get; }
        public IAnalyticsSdkService Analytics { get; }
        public IAuthSdkService Auth { get; }
        public ICloudSaveSdkService CloudSave { get; }
        public ILeaderboardSdkService Leaderboards { get; }

        public bool IsInitialized { get; private set; }

        public bool Supports(PlatformCapability capability) => true;

        public UniTask InitializeAsync(CancellationToken cancellationToken = default)
        {
            IsInitialized = true;
            GameDebug.Log("[Platform] Активна заглушка SDK: реклама и покупки имитируются.");

            return UniTask.CompletedTask;
        }
    }

    public class StubAdsSdkService : IAdsSdkService
    {
        private const float FAKE_AD_SECONDS = 1f;

        private bool _isDisabled;

        public bool IsRewardedReady => _isDisabled == false;

        public async UniTask<bool> ShowRewardedAsync(string placement, CancellationToken cancellationToken = default)
        {
            if (_isDisabled)
                return false;

            GameDebug.Log($"[Ads] Имитация rewarded «{placement}».");
            await UniTask.Delay(System.TimeSpan.FromSeconds(FAKE_AD_SECONDS),
                DelayType.UnscaledDeltaTime, cancellationToken: cancellationToken);

            return true;
        }

        public async UniTask ShowInterstitialAsync(string placement, CancellationToken cancellationToken = default)
        {
            if (_isDisabled)
                return;

            GameDebug.Log($"[Ads] Имитация interstitial «{placement}».");
            await UniTask.Delay(System.TimeSpan.FromSeconds(FAKE_AD_SECONDS),
                DelayType.UnscaledDeltaTime, cancellationToken: cancellationToken);
        }

        public void ShowBanner() => GameDebug.Log("[Ads] Имитация показа баннера.");

        public void HideBanner() => GameDebug.Log("[Ads] Имитация скрытия баннера.");

        public void DisableAds()
        {
            _isDisabled = true;
            GameDebug.Log("[Ads] Реклама отключена.");
        }
    }

    public class StubPurchaseSdkService : IPurchaseSdkService
    {
        public UniTask<PurchaseResult> PurchaseAsync(string productId, CancellationToken cancellationToken = default)
        {
            GameDebug.Log($"[IAP] Имитация покупки «{productId}»: успех.");
            return UniTask.FromResult(new PurchaseResult(true, productId));
        }

        public UniTask RestoreAsync(CancellationToken cancellationToken = default)
        {
            GameDebug.Log("[IAP] Имитация восстановления покупок.");
            return UniTask.CompletedTask;
        }

        public string GetLocalizedPrice(string productId) => "$0.99";
    }

    public class StubAnalyticsSdkService : IAnalyticsSdkService
    {
        public void TrackEvent(string eventName, IReadOnlyDictionary<string, object> parameters = null)
        {
            GameDebug.Log($"[Analytics] {eventName}");
        }

        public void TrackRevenue(string productId, double amount, string currency)
        {
            GameDebug.Log($"[Analytics] revenue {productId} {amount} {currency}");
        }
    }

    public class StubAuthSdkService : IAuthSdkService
    {
        public bool IsSignedIn { get; private set; }
        public string UserId { get; private set; } = "editor-user";

        public UniTask<bool> SignInAsync(CancellationToken cancellationToken = default)
        {
            IsSignedIn = true;
            return UniTask.FromResult(true);
        }
    }

    public class StubCloudSaveSdkService : ICloudSaveSdkService
    {
        public bool IsAvailable => false;

        public UniTask<string> ReadAsync(CancellationToken cancellationToken = default)
            => UniTask.FromResult<string>(null);

        public UniTask WriteAsync(string payload, CancellationToken cancellationToken = default)
            => UniTask.CompletedTask;
    }

    public class StubLeaderboardSdkService : ILeaderboardSdkService
    {
        public UniTask SubmitScoreAsync(string boardId, long score, CancellationToken cancellationToken = default)
        {
            GameDebug.Log($"[Leaderboard] {boardId}: {score}");
            return UniTask.CompletedTask;
        }

        public UniTask ShowAsync(string boardId, CancellationToken cancellationToken = default)
        {
            GameDebug.Log($"[Leaderboard] Открытие {boardId}.");
            return UniTask.CompletedTask;
        }
    }
}
