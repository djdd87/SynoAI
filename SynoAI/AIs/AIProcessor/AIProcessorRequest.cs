using Newtonsoft.Json;

namespace SynoAI.AIs.AIProcessor
{
    /// <summary>
    /// Represents a request to DeepStackAI.
    /// </summary>
    public class AIProcessorRequest
    {
        /// <summary>
        /// Gets or sets the minimum confidence for the request.
        /// </summary>
        [JsonProperty("min_confidence")]
        public decimal MinConfidence { get; set; }
    }
}
