using Newtonsoft.Json;

namespace SynoAI.Notifiers.SynologyChat
{
    internal class SynologyChatErrorResponse
    {
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("errors")]
        public SynologyChatErrorReasonResponse Errors { get; set; }
    }
}
