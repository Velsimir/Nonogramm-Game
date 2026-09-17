using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using G.Core.Common;

namespace G.Core.Boot
{
    public class LoadingService : ILoadingService
    {
        private const float STEP_TIMEOUT_SECONDS = 30f;

        private readonly ILoadingErrorHandler _errorHandler;
        private readonly ReactiveProperty<float> _progress = new(0f);
        private readonly ReactiveProperty<string> _currentStep = new(string.Empty);

        public LoadingService(ILoadingErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }

        public ReadOnlyReactiveProperty<float> Progress => _progress;
        public ReadOnlyReactiveProperty<string> CurrentStep => _currentStep;

        public async UniTask<bool> BeginLoadingAsync(ILoadingOperation[] operations,
            CancellationToken cancellationToken = default)
        {
            if (operations == null || operations.Length == 0)
            {
                GameDebug.LogConfigurationError("[Loading] Пустой список операций загрузки.");
                return false;
            }

            float totalWeight = 0f;

            foreach (ILoadingOperation operation in operations)
                totalWeight += Mathf.Max(operation.Weight, 0f);

            if (totalWeight <= 0f)
                totalWeight = operations.Length;

            float completedWeight = 0f;
            _progress.Value = 0f;

            foreach (ILoadingOperation operation in operations)
            {
                _currentStep.Value = operation.Description;
                GameDebug.Log($"[Loading] {operation.Description}");

                bool succeeded = await ExecuteWithRetryAsync(operation, cancellationToken);

                if (succeeded == false)
                {
                    GameDebug.LogError($"[Loading] Загрузка прервана на шаге «{operation.Description}».");
                    return false;
                }

                completedWeight += Mathf.Max(operation.Weight, 0f);
                _progress.Value = Mathf.Clamp01(completedWeight / totalWeight);
            }

            _progress.Value = 1f;
            _currentStep.Value = string.Empty;
            return true;
        }

        private async UniTask<bool> ExecuteWithRetryAsync(ILoadingOperation operation,
            CancellationToken cancellationToken)
        {
            while (true)
            {
                Exception failure = null;

                try
                {
                    bool timedOut = await operation.ExecuteAsync(cancellationToken)
                        .TimeoutWithoutException(TimeSpan.FromSeconds(STEP_TIMEOUT_SECONDS));

                    if (timedOut == false)
                        return true;

                    failure = new TimeoutException(
                        $"Шаг «{operation.Description}» не завершился за {STEP_TIMEOUT_SECONDS} с.");
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    failure = exception;
                }

                GameDebug.LogError($"[Loading] {operation.Description}: {failure.Message}");

                bool retry = await _errorHandler.AskRetryAsync(operation.Description, failure, cancellationToken);

                if (retry == false)
                    return false;
            }
        }
    }
}
