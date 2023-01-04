﻿using Newtonsoft.Json;

namespace SynoAI.Notifiers.SynologyChat
{
    internal class SynologyChatResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("error")]
        public SynologyChatErrorResponse Error { get; set; }
    }
}
