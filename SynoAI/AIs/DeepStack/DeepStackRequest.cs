using Newtonsoft.Json;

namespace SynoAI.AIs.DeepStack
{
    public class DeepStackRequest
    {
        [JsonProperty("min_confidence")]
        public decimal MinConfidence { get; set; }
    }
}
