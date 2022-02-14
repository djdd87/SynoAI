using Newtonsoft.Json;

namespace SynoAI.Notifiers.SynologyChat
{
    public class SynologyChatErrorReasonResponse
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("reason")]
        public string Reason { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Reason})";
        }
    }
}
