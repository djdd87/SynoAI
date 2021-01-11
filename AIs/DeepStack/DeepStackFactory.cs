using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SynoAI.AIs.DeepStack
{
    public class DeepStackFactory : AIFactory
    {
        public override IAI Create(ILogger logger, IConfigurationSection section)
        {
            return new DeepStack();
        }
    }
}
