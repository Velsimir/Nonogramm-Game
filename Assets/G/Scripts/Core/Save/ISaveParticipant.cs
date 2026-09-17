using Newtonsoft.Json.Linq;

namespace G.Core.Save
{
    public interface ISaveParticipant
    {
        string Key { get; }

        int Order { get; }

        object Capture();

        void Restore(JToken section);
    }
}
