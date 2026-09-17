using System.Threading;
using Cysharp.Threading.Tasks;
using G.Core.Save;

namespace G.Core.Boot.Operations
{
    public class LoadSaveOperation : ILoadingOperation
    {
        private readonly ISaveService _saveService;

        public LoadSaveOperation(ISaveService saveService)
        {
            _saveService = saveService;
        }

        public string Description => "Загрузка сохранения";
        public float Weight => 2f;

        public UniTask ExecuteAsync(CancellationToken cancellationToken)
        {
            return _saveService.LoadAsync(cancellationToken);
        }
    }
}
