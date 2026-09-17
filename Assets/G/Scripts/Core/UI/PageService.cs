using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using G.Core.Common;

namespace G.Core.UI
{
    public class PageService : Service, IPageService
    {
        private readonly List<(Type Type, IPageBase Page)> _stack = new();
        private readonly ReactiveProperty<Type> _topPage = new();

        private readonly IPagesProvider _provider;
        private readonly PagesConfig _config;
        private readonly UILayerRoots _layerRoots;

        public PageService(IPagesProvider provider, PagesConfig config, UILayerRoots layerRoots)
        {
            _provider = provider;
            _config = config;
            _layerRoots = layerRoots;
        }

        public ReadOnlyReactiveProperty<Type> TopPage => _topPage;

        protected override void OnInitialize()
        {
            _topPage.AddTo(Disposables);

            ShowStartPageAsync().Forget();
        }

        public async UniTask ShowAsync<TPage>(CancellationToken cancellationToken = default)
            where TPage : class, IPage
        {
            if (_provider.TryGet(out TPage page) == false)
                return;

            using (BlockInput())
            {
                Push(typeof(TPage), page);
                await page.ShowAsync(cancellationToken);
            }
        }

        public async UniTask ShowAsync<TPage, TPayload>(TPayload payload,
            CancellationToken cancellationToken = default)
            where TPage : class, IPayloadPage<TPayload>
        {
            if (_provider.TryGet(out TPage page) == false)
                return;

            using (BlockInput())
            {
                Push(typeof(TPage), page);
                await page.ShowAsync(payload, cancellationToken);
            }
        }

        public async UniTask<TResult> ShowForResultAsync<TPage, TResult>(
            CancellationToken cancellationToken = default)
            where TPage : class, IPage, IResultPage<TResult>
        {
            if (_provider.TryGet(out TPage page) == false)
                return default;

            using (BlockInput())
            {
                Push(typeof(TPage), page);
                await page.ShowAsync(cancellationToken);
            }

            try
            {
                TResult result = await page.ResultAsync(cancellationToken);
                await HideAsync<TPage>(cancellationToken);
                return result;
            }
            catch (OperationCanceledException)
            {
                return default;
            }
        }

        public async UniTask HideAsync<TPage>(CancellationToken cancellationToken = default)
            where TPage : class, IPageBase
        {
            int index = IndexOf(typeof(TPage));

            if (index < 0)
                return;

            await HideAtAsync(index, cancellationToken);
        }

        public async UniTask HideTopAsync(CancellationToken cancellationToken = default)
        {
            if (_stack.Count == 0)
                return;

            await HideAtAsync(_stack.Count - 1, cancellationToken);
        }

        public bool IsOpen<TPage>() where TPage : class, IPageBase
        {
            int index = IndexOf(typeof(TPage));

            if (index < 0)
                return false;

            return _stack[index].Page.State.CurrentValue == PageState.Shown;
        }

        public TPage Get<TPage>() where TPage : class, IPageBase
        {
            return _provider.TryGet(out TPage page) ? page : null;
        }

        private async UniTaskVoid ShowStartPageAsync()
        {
            Type startType = _config.StartPageType;

            if (startType == null)
            {
                GameDebug.LogConfigurationError("[UI] В PagesConfig не назначена стартовая страница.");
                return;
            }

            if (_provider.TryGet(startType, out IPageBase startPage) == false)
                return;

            if (startPage is IPage page == false)
            {
                GameDebug.LogConfigurationError(
                    $"[UI] Стартовая страница {startType.Name} должна реализовывать IPage.");
                return;
            }

            using (BlockInput())
            {
                Push(startType, page);
                await page.ShowAsync(DisposeToken);
            }
        }

        private async UniTask HideAtAsync(int index, CancellationToken cancellationToken)
        {
            using (BlockInput())
            {
                for (int i = _stack.Count - 1; i >= index; i--)
                {
                    IPageBase page = _stack[i].Page;
                    _stack.RemoveAt(i);

                    await page.HideAsync(cancellationToken);
                }

                UpdateTopPage();
            }
        }

        private void Push(Type type, IPageBase page)
        {
            int existing = IndexOf(type);

            if (existing >= 0)
                _stack.RemoveAt(existing);

            _stack.Add((type, page));
            UpdateTopPage();
        }

        private int IndexOf(Type type)
        {
            for (int i = 0; i < _stack.Count; i++)
            {
                if (_stack[i].Type == type)
                    return i;
            }

            return -1;
        }

        private void UpdateTopPage()
        {
            _topPage.Value = _stack.Count > 0 ? _stack[^1].Type : null;
        }

        private InputLock BlockInput()
        {
            return new InputLock(_layerRoots.InputBlocker);
        }

        private readonly struct InputLock : IDisposable
        {
            private readonly InputBlocker _blocker;

            public InputLock(InputBlocker blocker)
            {
                _blocker = blocker;
                _blocker?.Lock();
            }

            public void Dispose()
            {
                _blocker?.Unlock();
            }
        }
    }
}
