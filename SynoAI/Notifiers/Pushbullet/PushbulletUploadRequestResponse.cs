using Newtonsoft.Json;

namespace SynoAI.Notifiers.Pushbullet
{
    public class PushbulletUploadRequestResponse
    {
        [JsonProperty("file_name")]
        public string FileName { get; set; }
        [JsonProperty("file_type")]
        public string FileType { get; set; }
        [JsonProperty("file_url")]
        public string FileUrl { get; set; }
        [JsonProperty("upload_url")]
        public string UploadUrl { get; set; }
    }
}
