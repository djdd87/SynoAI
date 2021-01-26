using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SynoAI.Notifiers.Pushbullet
{
    public class PushbulletError
    {
        public string Code { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public string Cat { get; set; }

        public override string ToString()
        {
            return $"{Code} - {Message}";
        }
    }
}
