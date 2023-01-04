using SynoAI.AIs;
using SynoAI.AIs.DeepStack;
using SynoAI.Models;

namespace SynoAI.Services
{
    internal class AIService : IAIService
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

        private static AI GetAI()
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
