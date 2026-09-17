using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace G.Core.Common
{
    public abstract class View : MonoBehaviour
    {
        protected readonly CompositeDisposable Disposables = new();

        protected CancellationToken DestroyToken => this.GetCancellationTokenOnDestroy();

        private void OnDestroy()
        {
            Disposables.Dispose();
            OnDestroyed();
        }

        protected virtual void OnDestroyed() { }
    }
}
