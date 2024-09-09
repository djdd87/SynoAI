using Microsoft.Extensions.Logging;
using SynoAI.Core.Interfaces;
using SynoAI.Core.Models;
using SynoAI.Core.Notifiers;

namespace SynoAI.Core.Services;

public class DetectionService : IDetectionService
{
    public DetectionService(ILogger<DetectionService> logger)
    {

    }

    public Task<RunDetectionResponse> RunAsync(string cameraName)
    {
        throw new NotImplementedException();
    }
}