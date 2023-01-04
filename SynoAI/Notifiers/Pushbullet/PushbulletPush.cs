using Newtonsoft.Json;

namespace SynoAI.Notifiers.Pushbullet
{
    internal class PushbulletPush
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("body")]
        public string Body { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("file_name")]
        public string FileName { get; set; }
        [JsonProperty("file_type")]
        public string FileType { get; set; }
        [JsonProperty("file_url")]
        public string FileUrl { get; set; }
        [JsonProperty("source_device_iden")]
        public string SourceDeviceIdentifier { get; set; }
        [JsonProperty("device_iden")]
        public string DeviceIdentifier { get; set; }
        [JsonProperty("client_iden")]
        public string ClientIdentifier { get; set; }
        [JsonProperty("channel_tag")]
        public string ChannelTag { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("guid")]
        public string Guid { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }
    }
}
