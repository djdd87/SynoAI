using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SynoAI.Notifiers.Pushbullet
{
    public class PushbulletErrorResponse
    {
        public PushbulletError Error { get; set; }
        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }
    }
}
