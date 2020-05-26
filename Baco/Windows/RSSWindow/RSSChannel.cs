using Newtonsoft.Json;
using System;

namespace Baco.Windows.RSSWindow
{
    [Serializable]
    public class RSSChannel
    {
        [JsonProperty("rss")]
        public string RSS { get; private set; }
        [JsonProperty("name")]
        public string Name { get; private set; }

        public RSSChannel()
        {
        }
    }
}
