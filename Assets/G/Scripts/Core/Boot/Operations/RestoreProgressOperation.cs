using System.Threading;
using Cysharp.Threading.Tasks;
using G.Core.Save;

namespace G.Core.Boot.Operations
{
    public class RestoreProgressOperation : ILoadingOperation
    {
        private readonly ISaveService _saveService;

        public RestoreProgressOperation(ISaveService saveService)
        {
            _saveService = saveService;
        }

        public string Description => "Восстановление прогресса";
        public float Weight => 1f;

        public UniTask ExecuteAsync(CancellationToken cancellationToken)
        {
            _saveService.RestoreAll();
            return UniTask.CompletedTask;
        }
    }
}
