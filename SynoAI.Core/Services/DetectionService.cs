using Microsoft.Extensions.Logging;
using SynoAI.Core.Interfaces;
using SynoAI.Core.Models.Results;

namespace SynoAI.Core.Services;

public class DetectionService : IDetectionService
{
    public DetectionService(ILogger<DetectionService> logger)
    {

    }

    public Task<DetectionResult> RunAsync(string cameraName)
    {
        throw new NotImplementedException();
    }
}