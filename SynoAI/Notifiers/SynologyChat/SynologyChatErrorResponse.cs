using Newtonsoft.Json;
using System.Collections.Generic;

namespace SynoAI.Notifiers.SynologyChat
{
    public class SynologyChatErrorResponse
    {
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("errors")]
        public SynologyChatErrorReasonResponse Errors { get; set; }
    }
}
