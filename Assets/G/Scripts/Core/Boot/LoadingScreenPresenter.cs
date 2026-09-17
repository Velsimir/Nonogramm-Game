using R3;
using G.Core.Common;

namespace G.Core.Boot
{
    public class LoadingScreenPresenter : Presenter
    {
        private readonly ILoadingService _loadingService;
        private readonly LoadingScreenView _view;

        public LoadingScreenPresenter(ILoadingService loadingService, LoadingScreenView view)
        {
            _loadingService = loadingService;
            _view = view;
        }

        protected override void OnInitialize()
        {
            _loadingService.Progress
                .Subscribe(_view.SetProgress)
                .AddTo(Disposables);

            _loadingService.CurrentStep
                .Subscribe(_view.SetStep)
                .AddTo(Disposables);
        }
    }
}
