using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SynoAI.AIs.DeepStack
{
    public class DeepStackPrediction
    {
        public decimal Confidence { get; set; }
        public string Label { get; set; }

        [JsonProperty("x_min")]
        public int MinX { get; set; }
        [JsonProperty("y_min")]
        public int MinY { get; set; }
        [JsonProperty("x_max")]
        public int MaxX { get; set; }
        [JsonProperty("y_max")]
        public int MaxY { get; set; }
    }
}
