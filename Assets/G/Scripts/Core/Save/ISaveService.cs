using System.Threading;
using Cysharp.Threading.Tasks;

namespace G.Core.Save
{
    public interface ISaveService
    {
        UniTask LoadAsync(CancellationToken cancellationToken = default);

        void RestoreAll();

        void MarkDirty(SaveReason reason);

        UniTask SaveNowAsync(bool includeCloud, CancellationToken cancellationToken = default);
    }
}
