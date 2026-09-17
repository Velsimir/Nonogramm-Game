using R3;
using UnityEngine;

namespace G.Core.Tick
{
    public class TickService : MonoBehaviour, ITickService
    {
        private const float SECOND = 1f;

        private readonly Subject<float> _update = new();
        private readonly Subject<float> _fixedUpdate = new();
        private readonly Subject<float> _lateUpdate = new();
        private readonly Subject<Unit> _secondTick = new();

        private float _secondAccumulator;

        Observable<float> ITickService.Update => _update;
        Observable<float> ITickService.FixedUpdate => _fixedUpdate;
        Observable<float> ITickService.LateUpdate => _lateUpdate;
        Observable<Unit> ITickService.SecondTick => _secondTick;

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            _update.OnNext(deltaTime);

            _secondAccumulator += deltaTime;

            while (_secondAccumulator >= SECOND)
            {
                _secondAccumulator -= SECOND;
                _secondTick.OnNext(Unit.Default);
            }
        }

        private void FixedUpdate()
        {
            _fixedUpdate.OnNext(Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            _lateUpdate.OnNext(Time.deltaTime);
        }

        private void OnDestroy()
        {
            _update.Dispose();
            _fixedUpdate.Dispose();
            _lateUpdate.Dispose();
            _secondTick.Dispose();
        }
    }
}
