using Newtonsoft.Json;

namespace SynoAI.Notifiers.Pushbullet
{
    public class PushbulletUploadRequest
    {
        [JsonProperty("file_name")]
        public string FileName { get; set; }
        [JsonProperty("file_type")]
        public string FileType { get; set; }
    }
}
