using Newtonsoft.Json;

namespace TwitchTokenPoc
{
    public class TwitchUser
    {
        public string Id { get; set; }
        public string Login { get; set; }
        
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("view_count")]
        public int ViewCount { get; set; }
    }
}