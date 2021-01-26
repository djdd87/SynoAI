using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
