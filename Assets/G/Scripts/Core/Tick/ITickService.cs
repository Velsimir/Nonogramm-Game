using R3;

namespace G.Core.Tick
{
    public interface ITickService
    {
        Observable<float> Update { get; }
        Observable<float> FixedUpdate { get; }
        Observable<float> LateUpdate { get; }

        Observable<Unit> SecondTick { get; }
    }
}
