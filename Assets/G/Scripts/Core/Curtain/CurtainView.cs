using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using G.Core.Common;

namespace G.Core.Curtain
{
    public class CurtainView : MonoBehaviour, ICurtain
    {
        private const float INTRO_TIMEOUT_SECONDS = 5f;

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = 0.3f;
        [SerializeField] private Animator _animator;
        [SerializeField] private bool _hasIntroAnimation;

        private UniTaskCompletionSource _introFinished;

        public bool IsShown { get; private set; }

        private void Awake()
        {
            _introFinished = new UniTaskCompletionSource();

            if (_hasIntroAnimation == false)
                _introFinished.TrySetResult();
        }

        public async UniTask ShowAsync(CancellationToken cancellationToken = default)
        {
            gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            IsShown = true;

            if (_hasIntroAnimation == false)
                return;

            bool timedOut = await _introFinished.Task
                .TimeoutWithoutException(TimeSpan.FromSeconds(INTRO_TIMEOUT_SECONDS));

            if (timedOut)
            {
                GameDebug.LogError(
                    "[Curtain] Intro-анимация не сообщила о завершении за " + INTRO_TIMEOUT_SECONDS +
                    " с. Проверь, что Animation Event вызывает IntroFinished().");
                _introFinished.TrySetResult();
            }
        }

        public async UniTask HideAsync(CancellationToken cancellationToken = default)
        {
            float elapsed = 0f;
            float startAlpha = _canvasGroup.alpha;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / _fadeDuration);

                await UniTask.Yield(cancellationToken);
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            IsShown = false;
            gameObject.SetActive(false);
        }

        public void IntroFinished()
        {
            _introFinished.TrySetResult();
        }
    }
}
