using Microsoft.Extensions.Logging;
using SynoAI.AIs;
using SynoAI.AIs.DeepStack;
using SynoAI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SynoAI.Services
{
    public class AIService : IAIService
    {
        private readonly ILogger<AIService> _logger;

        public AIService(ILogger<AIService> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<AIPrediction>> ProcessAsync(Camera camera, byte[] image)
        {
            AI ai = GetAI();
            return await ai.Process(_logger, camera, image);
        }

        private AI GetAI()
        {
            switch (Config.AI)
            {
                case AIType.DeepStack:
                case AIType.CodeProjectAIServer: // Works the same as DeepStack
                    return new DeepStackAI();
                default:
                    throw new NotImplementedException(Config.AI.ToString());
            }
        }
    }
}
