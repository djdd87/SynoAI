namespace SynoAI.Models
{
    /// <summary>
    /// Represents the API Info from Synology
    /// </summary>
    public class SynologyApiInfo
    {
        /// <summary>
        /// Gets or sets the maximum version of the API
        /// </summary>
        public int MaxVersion { get; set; }
        /// <summary>
        /// Gets or sets the minimum version of the API
        /// </summary>
        public int MinVersion { get; set; }
        /// <summary>
        /// Gets or sets the path of the API
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Gets or sets the requested format of the api
        /// </summary>
        public string RequestFormat { get; set; }
    }
}