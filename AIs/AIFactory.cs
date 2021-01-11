using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SynoAI.AIs.DeepStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SynoAI.AIs
{
    /// <summary>
    /// Handles the constructor of the AI.
    /// </summary>
    public abstract class AIFactory
    {
        public abstract IAI Create(ILogger logger, IConfigurationSection section);

        public static IAI Create(AIType type, ILogger logger, IConfigurationSection section)
        {
            AIFactory factory;
            switch (type)
            {
                case AIType.DeepStack:
                    factory = new DeepStackFactory();
                    break;
                default:
                    throw new NotImplementedException(type.ToString());
            }

            return factory.Create(logger, section);
        }
    }
}
