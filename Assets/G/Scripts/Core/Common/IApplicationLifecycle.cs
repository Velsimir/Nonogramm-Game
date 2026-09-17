using R3;

namespace G.Core.Common
{
    public interface IApplicationLifecycle
    {
        Observable<bool> Paused { get; }

        Observable<bool> FocusChanged { get; }

        Observable<Unit> Quitting { get; }
    }
}
