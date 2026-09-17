using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace G.Core.Save
{
    public class SaveEnvelope
    {
        public const int CURRENT_VERSION = 1;

        [JsonProperty("version")]
        public int Version { get; set; } = CURRENT_VERSION;

        [JsonProperty("savedAtUtc")]
        public DateTime SavedAtUtc { get; set; }

        [JsonProperty("sections")]
        public Dictionary<string, JToken> Sections { get; set; } = new();

        public static SaveEnvelope CreateDefault()
        {
            return new SaveEnvelope
            {
                Version = CURRENT_VERSION,
                SavedAtUtc = DateTime.UtcNow,
                Sections = new Dictionary<string, JToken>(),
            };
        }

        public JToken GetSection(string key)
        {
            return Sections.TryGetValue(key, out JToken section) ? section : null;
        }
    }
}
