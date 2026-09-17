using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

namespace G.Core.Scenes
{
    public interface ISceneLoader
    {
        UniTask LoadSingleAsync(ScenesName scene, Action<DiContainer> bindings = null,
            IProgress<float> progress = null, CancellationToken cancellationToken = default);

        UniTask LoadAdditiveAsync(ScenesName scene, Action<DiContainer> bindings = null,
            IProgress<float> progress = null, CancellationToken cancellationToken = default);

        UniTask UnloadAsync(ScenesName scene, CancellationToken cancellationToken = default);

        bool IsLoaded(ScenesName scene);

        void SetActive(ScenesName scene);
    }
}
