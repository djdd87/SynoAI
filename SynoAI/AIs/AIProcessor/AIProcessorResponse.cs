namespace SynoAI.AIs.AIProcessor
{
    /// <summary>
    /// An object representing a response from DeepStack.
    /// </summary>
    public class AIProcessorResponse
    {
        /// <summary>
        /// Gets or sets the succes
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// Gets or sets the collection of predictions made by DeepStack AI.
        /// </summary>
        public IEnumerable<AIProcessorPrediction> Predictions { get; set; }
    }
}
