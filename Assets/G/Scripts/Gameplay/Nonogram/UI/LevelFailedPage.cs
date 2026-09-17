using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using G.Core.Common;
using G.Core.UI;

namespace G.Gameplay.Nonogram.UI
{
    public class LevelFailedPage : Page, IResultPage<LevelFailedResult>
    {
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _menuButton;

        private UniTaskCompletionSource<LevelFailedResult> _completion;

        public UniTask<LevelFailedResult> ResultAsync(CancellationToken cancellationToken = default)
        {
            if (_completion == null)
            {
                GameDebug.LogConfigurationError("[Nonogram] Результат LevelFailedPage запрошен до показа страницы.");
                return UniTask.FromResult(LevelFailedResult.Menu);
            }

            return _completion.Task.AttachExternalCancellation(cancellationToken);
        }

        protected override void OnShow()
        {
            if (_continueButton == null || _menuButton == null)
            {
                GameDebug.LogConfigurationError("[Nonogram] На LevelFailedPage не назначены кнопки.");
                return;
            }

            _completion = new UniTaskCompletionSource<LevelFailedResult>();

            _continueButton.onClick.AddListener(OnContinueClicked);
            _menuButton.onClick.AddListener(OnMenuClicked);
        }

        protected override void OnHidden()
        {
            if (_continueButton != null)
                _continueButton.onClick.RemoveListener(OnContinueClicked);

            if (_menuButton != null)
                _menuButton.onClick.RemoveListener(OnMenuClicked);

            _completion?.TrySetCanceled();
            _completion = null;
        }

        private void OnContinueClicked()
        {
            _completion?.TrySetResult(LevelFailedResult.Continue);
        }

        private void OnMenuClicked()
        {
            _completion?.TrySetResult(LevelFailedResult.Menu);
        }
    }
}
