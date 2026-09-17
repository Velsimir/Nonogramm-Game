using System.Threading;
using Cysharp.Threading.Tasks;
using G.Platform;

namespace G.Core.Save
{
    public class CloudSaveStorage : ISaveStorage
    {
        private readonly ICloudSaveSdkService _cloudSave;

        public CloudSaveStorage(IPlatformSdk platformSdk)
        {
            _cloudSave = platformSdk.CloudSave;
        }

        public string Id => "cloud";

        public bool IsAvailable => _cloudSave != null && _cloudSave.IsAvailable;

        public UniTask<string> ReadAsync(CancellationToken cancellationToken)
        {
            return _cloudSave.ReadAsync(cancellationToken);
        }

        public UniTask WriteAsync(string json, CancellationToken cancellationToken)
        {
            return _cloudSave.WriteAsync(json, cancellationToken);
        }
    }
}
