using System.Collections.Generic;

namespace G.Platform
{
    public interface IAnalyticsSdkService
    {
        void TrackEvent(string eventName, IReadOnlyDictionary<string, object> parameters = null);
        void TrackRevenue(string productId, double amount, string currency);
    }
}
