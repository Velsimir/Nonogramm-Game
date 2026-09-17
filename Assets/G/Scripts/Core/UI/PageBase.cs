using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using Zenject;

namespace G.Core.UI
{
    public abstract class PageBase : MonoBehaviour, IPageBase
    {
        [SerializeField] private UILayer _layer = UILayer.Window;

        private readonly ReactiveProperty<PageState> _state = new(PageState.Hidden);
        private readonly CompositeDisposable _hiddenDisposables = new();

        private CancellationTokenSource _hiddenTokenSource;
        private PageId _pageId;

        public PageId PageId => _pageId;
        public UILayer Layer => _layer;
        public ReadOnlyReactiveProperty<PageState> State => _state;

        protected CancellationToken HiddenToken => _hiddenTokenSource?.Token ?? CancellationToken.None;

        protected CompositeDisposable HiddenDisposables => _hiddenDisposables;

        [Inject]
        private void Construct(PageId pageId)
        {
            _pageId = pageId;
            ApplyHiddenState();
        }

        public async UniTask HideAsync(CancellationToken cancellationToken = default)
        {
            if (_state.Value != PageState.Shown)
                return;

            _state.Value = PageState.Hiding;

            await OnHideAnimationAsync(cancellationToken);

            ApplyHiddenState();
            OnHidden();
        }

        protected async UniTask RunShowAsync(CancellationToken cancellationToken)
        {
            _hiddenTokenSource = new CancellationTokenSource();
            _state.Value = PageState.Showing;
            gameObject.SetActive(true);

            await OnShowAnimationAsync(cancellationToken);

            _state.Value = PageState.Shown;
        }

        protected virtual UniTask OnShowAnimationAsync(CancellationToken cancellationToken) => UniTask.CompletedTask;

        protected virtual UniTask OnHideAnimationAsync(CancellationToken cancellationToken) => UniTask.CompletedTask;

        protected virtual void OnHidden() { }

        private void ApplyHiddenState()
        {
            gameObject.SetActive(false);
            _state.Value = PageState.Hidden;

            _hiddenTokenSource?.Cancel();
            _hiddenTokenSource?.Dispose();
            _hiddenTokenSource = null;

            _hiddenDisposables.Clear();
        }

        private void OnDestroy()
        {
            _hiddenDisposables.Dispose();
            _state.Dispose();
            _hiddenTokenSource?.Cancel();
            _hiddenTokenSource?.Dispose();
        }
    }
}
