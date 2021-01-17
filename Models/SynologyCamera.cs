using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SynoAI.Models
{
    public class SynologyCamera
    {
        public int Id { get; set; }
        [JsonProperty("Name")]
        public string NameOld { get; set; }
        [JsonProperty("newName")]
        public string NameNew { get; set; }

        public string GetName()
        {
            return string.IsNullOrWhiteSpace(NameNew) ? NameOld : NameNew;    
        }
    }
}
