using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace G.Core.UI
{
    public interface IPageService
    {
        ReadOnlyReactiveProperty<Type> TopPage { get; }

        UniTask ShowAsync<TPage>(CancellationToken cancellationToken = default)
            where TPage : class, IPage;

        UniTask ShowAsync<TPage, TPayload>(TPayload payload, CancellationToken cancellationToken = default)
            where TPage : class, IPayloadPage<TPayload>;

        UniTask<TResult> ShowForResultAsync<TPage, TResult>(CancellationToken cancellationToken = default)
            where TPage : class, IPage, IResultPage<TResult>;

        UniTask HideAsync<TPage>(CancellationToken cancellationToken = default)
            where TPage : class, IPageBase;

        UniTask HideTopAsync(CancellationToken cancellationToken = default);

        bool IsOpen<TPage>() where TPage : class, IPageBase;

        TPage Get<TPage>() where TPage : class, IPageBase;
    }
}
