using System.Collections.Generic;

namespace SynoAI.AIs.DeepStack
{
    /// <summary>
    /// An object representing a response from DeepStack.
    /// </summary>
    public class DeepStackResponse
    {
        public bool Success { get; set; }
        public IEnumerable<DeepStackPrediction> Predictions { get; set; }
    }
}
