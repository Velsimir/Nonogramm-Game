using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using G.Core.Common;

namespace G.Core.Boot
{
    public class LoadingScreenView : View
    {
        [Header("Прогресс")]
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TMP_Text _stepLabel;

        [Header("Ошибка")]
        [SerializeField] private GameObject _errorPanel;
        [SerializeField] private TMP_Text _errorLabel;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _quitButton;

        private void Awake()
        {
            SetProgress(0f);
            HideError();
        }

        public void SetProgress(float normalized)
        {
            if (_progressBar != null)
                _progressBar.value = Mathf.Clamp01(normalized);
        }

        public void SetStep(string description)
        {
            if (_stepLabel != null)
                _stepLabel.text = description;
        }

        public async UniTask<bool> ShowErrorAsync(string message, CancellationToken cancellationToken)
        {
            _errorPanel.SetActive(true);

            if (_errorLabel != null)
                _errorLabel.text = message;

            UniTaskCompletionSource<bool> choice = new();

            void OnRetry() => choice.TrySetResult(true);
            void OnQuit() => choice.TrySetResult(false);

            _retryButton.onClick.AddListener(OnRetry);
            _quitButton.onClick.AddListener(OnQuit);

            try
            {
                return await choice.Task.AttachExternalCancellation(cancellationToken);
            }
            finally
            {
                _retryButton.onClick.RemoveListener(OnRetry);
                _quitButton.onClick.RemoveListener(OnQuit);
                HideError();
            }
        }

        public void HideError()
        {
            if (_errorPanel != null)
                _errorPanel.SetActive(false);
        }
    }
}
