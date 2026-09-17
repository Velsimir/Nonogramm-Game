using Newtonsoft.Json.Linq;
using G.Core.Common;

namespace G.Core.Save
{
    public static class SaveSection
    {
        public static TData Read<TData>(JToken section, string key) where TData : class, new()
        {
            if (section == null)
                return new TData();

            try
            {
                return section.ToObject<TData>() ?? new TData();
            }
            catch (System.Exception exception)
            {
                GameDebug.LogError($"[Save] Секция «{key}» повреждена, взяты значения по умолчанию: {exception.Message}");
                return new TData();
            }
        }
    }
}
