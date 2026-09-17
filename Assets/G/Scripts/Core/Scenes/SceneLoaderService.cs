using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using G.Core.Common;

namespace G.Core.Scenes
{
    public class SceneLoaderService : ISceneLoader
    {
        private const float LOAD_PROGRESS_CAP = 0.9f;

        private const float MAX_REPORTED_BEFORE_DONE = 0.99f;

        private readonly ZenjectSceneLoader _sceneLoader;

        public SceneLoaderService(ZenjectSceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        public UniTask LoadSingleAsync(ScenesName scene, Action<DiContainer> bindings = null,
            IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            return LoadAsync(scene, LoadSceneMode.Single, bindings, progress, cancellationToken);
        }

        public UniTask LoadAdditiveAsync(ScenesName scene, Action<DiContainer> bindings = null,
            IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            return LoadAsync(scene, LoadSceneMode.Additive, bindings, progress, cancellationToken);
        }

        public async UniTask UnloadAsync(ScenesName scene, CancellationToken cancellationToken = default)
        {
            if (IsLoaded(scene) == false)
            {
                GameDebug.LogWarning($"[SceneLoader] Сцена {scene} не загружена, выгружать нечего.");
                return;
            }

            AsyncOperation operation = SceneManager.UnloadSceneAsync(SceneNameMap.Get(scene));

            if (operation == null)
                return;

            await operation.ToUniTask(cancellationToken: cancellationToken);
        }

        public bool IsLoaded(ScenesName scene)
        {
            Scene loaded = SceneManager.GetSceneByName(SceneNameMap.Get(scene));
            return loaded.IsValid() && loaded.isLoaded;
        }

        public void SetActive(ScenesName scene)
        {
            Scene loaded = SceneManager.GetSceneByName(SceneNameMap.Get(scene));

            if (loaded.IsValid() == false || loaded.isLoaded == false)
            {
                GameDebug.LogError($"[SceneLoader] Нельзя сделать активной незагруженную сцену {scene}.");
                return;
            }

            SceneManager.SetActiveScene(loaded);
        }

        private async UniTask LoadAsync(ScenesName scene, LoadSceneMode mode, Action<DiContainer> bindings,
            IProgress<float> progress, CancellationToken cancellationToken)
        {
            string sceneName = SceneNameMap.Get(scene);
            AsyncOperation operation = _sceneLoader.LoadSceneAsync(sceneName, mode, bindings);

            if (progress == null)
            {
                await operation.ToUniTask(cancellationToken: cancellationToken);
                return;
            }

            float lastReported = -1f;

            while (operation.isDone == false)
            {
                float normalized = Mathf.Min(
                    Mathf.Clamp01(operation.progress / LOAD_PROGRESS_CAP),
                    MAX_REPORTED_BEFORE_DONE);

                if (Mathf.Approximately(normalized, lastReported) == false)
                {
                    lastReported = normalized;
                    progress.Report(normalized);
                }

                await UniTask.Yield(cancellationToken);
            }

            progress.Report(1f);
        }
    }
}
