using System.Threading;
using Cysharp.Threading.Tasks;

namespace G.Platform
{
    public interface IAuthSdkService
    {
        bool IsSignedIn { get; }
        string UserId { get; }

        UniTask<bool> SignInAsync(CancellationToken cancellationToken = default);
    }
}
