using SynoAI.Models;

namespace SynoAI.Services
{
    public interface IAIService
    {
        Task<IEnumerable<AIPrediction>> ProcessAsync(Camera camera, byte[] image);
    }
}
