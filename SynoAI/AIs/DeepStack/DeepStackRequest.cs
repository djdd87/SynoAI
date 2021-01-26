using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SynoAI.AIs.DeepStack
{
    public class DeepStackRequest
    {
        [JsonProperty("min_confidence")]
        public decimal MinConfidence { get; set; }
    }
}
