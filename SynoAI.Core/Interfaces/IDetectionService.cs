using SynoAI.Core.Models;

namespace SynoAI.Core.Interfaces;

public interface IDetectionService
{
    /// <summary>
    /// Runs a detection check for the specified camera name.
    /// </summary>
    /// <param name="cameraName">The name of the camera to check.</param>
    /// <returns></returns>
    Task<RunDetectionResponse> RunAsync(string cameraName);
}