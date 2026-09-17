using System.Threading;
using Cysharp.Threading.Tasks;

namespace G.Core.Save
{
    public interface ISaveStorage
    {
        string Id { get; }

        bool IsAvailable { get; }

        UniTask<string> ReadAsync(CancellationToken cancellationToken);

        UniTask WriteAsync(string json, CancellationToken cancellationToken);
    }
}
