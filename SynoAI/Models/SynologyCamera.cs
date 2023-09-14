using Newtonsoft.Json;

namespace SynoAI.Models
{
    /// <summary>
    /// Class to get the Synology Camera
    /// </summary>
    public class SynologyCamera
    {
        /// <summary>
        /// Gets or sets camera ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        [JsonProperty("Name")]
        public string NameOld { get; set; }
        /// <summary>
        /// Gets or sets the NameOld.
        /// </summary>
        [JsonProperty("newName")]
        public string NameNew { get; set; }
        /// <summary>
        /// Gets or sets the NameNew.
        /// </summary>
        public string GetName()
        {
            return string.IsNullOrWhiteSpace(NameNew) ? NameOld : NameNew;    
        }
    }
}
