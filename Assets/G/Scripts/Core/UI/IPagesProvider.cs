using System;

namespace G.Core.UI
{
    public interface IPagesProvider
    {
        bool TryGet(Type pageType, out IPageBase page);

        bool TryGet<TPage>(out TPage page) where TPage : class, IPageBase;
    }
}
