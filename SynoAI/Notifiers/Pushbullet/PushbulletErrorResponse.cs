using Newtonsoft.Json;

namespace SynoAI.Notifiers.Pushbullet
{
    internal class PushbulletErrorResponse
    {
        public PushbulletError Error { get; set; }
        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }
    }
}
