using SynoAI.Models;

namespace SynoAI.AIs
{
    internal abstract class AI
    {
        public abstract Task<IEnumerable<AIPrediction>> Process(ILogger logger, Camera camera, byte[] image);
    }
}
