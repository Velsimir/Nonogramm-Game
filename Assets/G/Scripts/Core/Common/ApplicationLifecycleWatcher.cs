using R3;
using UnityEngine;

namespace G.Core.Common
{
    public class ApplicationLifecycleWatcher : MonoBehaviour, IApplicationLifecycle
    {
        private readonly Subject<bool> _paused = new();
        private readonly Subject<bool> _focusChanged = new();
        private readonly Subject<Unit> _quitting = new();

        public Observable<bool> Paused => _paused;
        public Observable<bool> FocusChanged => _focusChanged;
        public Observable<Unit> Quitting => _quitting;

        private void OnApplicationPause(bool isPaused)
        {
            _paused.OnNext(isPaused);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            _focusChanged.OnNext(hasFocus);
        }

        private void OnApplicationQuit()
        {
            _quitting.OnNext(Unit.Default);
        }

        private void OnDestroy()
        {
            _paused.Dispose();
            _focusChanged.Dispose();
            _quitting.Dispose();
        }
    }
}
