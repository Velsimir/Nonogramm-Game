using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using G.Core.Scenes;

namespace G.Core.Boot.Operations
{
    public class LoadSceneOperation : ILoadingOperation
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly ScenesName _scene;
        private readonly IProgress<float> _progress;

        public LoadSceneOperation(ISceneLoader sceneLoader, ScenesName scene, IProgress<float> progress = null)
        {
            _sceneLoader = sceneLoader;
            _scene = scene;
            _progress = progress;
        }

        public string Description => $"Загрузка сцены {_scene}";
        public float Weight => 4f;

        public UniTask ExecuteAsync(CancellationToken cancellationToken)
        {
            return _sceneLoader.LoadAdditiveAsync(_scene, progress: _progress, cancellationToken: cancellationToken);
        }
    }
}
