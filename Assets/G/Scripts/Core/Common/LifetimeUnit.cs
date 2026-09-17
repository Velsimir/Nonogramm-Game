using System;
using System.Threading;
using R3;
using Zenject;

namespace G.Core.Common
{
    public abstract class LifetimeUnit : IInitializable, IDisposable
    {
        protected readonly CompositeDisposable Disposables = new();

        private readonly CancellationTokenSource _disposeTokenSource = new();

        protected CancellationToken DisposeToken => _disposeTokenSource.Token;

        void IInitializable.Initialize()
        {
            OnInitialize();
        }

        void IDisposable.Dispose()
        {
            OnDispose();
            Disposables.Dispose();
            _disposeTokenSource.Cancel();
            _disposeTokenSource.Dispose();
        }

        protected virtual void OnInitialize() { }

        protected virtual void OnDispose() { }
    }
}
