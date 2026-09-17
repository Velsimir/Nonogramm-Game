using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using G.Core.Common;

namespace G.Core.Boot
{
    public class LoadingErrorHandler : ILoadingErrorHandler
    {
        private readonly LoadingScreenView _view;

        public LoadingErrorHandler(LoadingScreenView view)
        {
            _view = view;
        }

        public async UniTask<bool> AskRetryAsync(string stepDescription, Exception exception,
            CancellationToken cancellationToken)
        {
            if (_view == null)
            {
                GameDebug.LogError($"[Loading] Нет экрана загрузки, чтобы показать ошибку шага «{stepDescription}».");
                return false;
            }

            string message = $"{stepDescription}\n{exception.Message}";
            bool retry = await _view.ShowErrorAsync(message, cancellationToken);

            if (retry == false)
                Application.Quit();

            return retry;
        }
    }
}
