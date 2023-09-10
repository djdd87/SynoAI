using Newtonsoft.Json;

namespace SynoAI.AIs.AIProcessor
{
    /// <summary>
    /// Represents a prediction made by AIProcessor AI.
    /// </summary>
    public class AIProcessorPrediction
    {
        /// <summary>
        /// Gets or sets the confidence level of the prediction.
        /// </summary>
        public decimal Confidence { get; set; }

        /// <summary>
        /// Gets or sets the label associated with the prediction.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the minimum X-coordinate of the bounding box for the prediction.
        /// </summary>
        [JsonProperty("x_min")]
        public int MinX { get; set; }

        /// <summary>
        /// Gets or sets the minimum Y-coordinate of the bounding box for the prediction.
        /// </summary>
        [JsonProperty("y_min")]
        public int MinY { get; set; }

        /// <summary>
        /// Gets or sets the maximum X-coordinate of the bounding box for the prediction.
        /// </summary>
        [JsonProperty("x_max")]
        public int MaxX { get; set; }

        /// <summary>
        /// Gets or sets the maximum Y-coordinate of the bounding box for the prediction.
        /// </summary>
        [JsonProperty("y_max")]
        public int MaxY { get; set; }
    }
}
