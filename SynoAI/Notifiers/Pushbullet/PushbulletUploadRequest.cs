using Newtonsoft.Json;

namespace SynoAI.Notifiers.Pushbullet
{
    /// <summary>
    /// Represents the PusbBullet Upload Request
    /// </summary>
    public class PushbulletUploadRequest
    {
        /// <summary>
        /// Gets or sets the FileName
        /// </summary>
        [JsonProperty("file_name")]
        public string FileName { get; set; }
        /// <summary>
        /// Gets or sets the filetype.
        /// </summary>
        [JsonProperty("file_type")]
        public string FileType { get; set; }
    }
}
