using Newtonsoft.Json;

namespace SynoAI.Notifiers.Pushbullet
{
    /// <summary>
    /// Class for uploading the response to PushBUllet
    /// </summary>
    public class PushbulletUploadRequestResponse
    {
        /// <summary>
        /// Gets or sets the FileName.
        /// </summary>
        [JsonProperty("file_name")]
        public string FileName { get; set; }
        /// <summary>
        /// Gets or sets the FileType.
        /// </summary>
        [JsonProperty("file_type")]
        public string FileType { get; set; }
        /// <summary>
        /// Gets or sets the FileUrl.
        /// </summary>
        [JsonProperty("file_url")]
        public string FileUrl { get; set; }
        /// <summary>
        /// Gets or sets the UploadUrl.
        /// </summary>
        [JsonProperty("upload_url")]
        public string UploadUrl { get; set; }
    }
}
