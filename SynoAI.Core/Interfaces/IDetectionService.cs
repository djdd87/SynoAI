using SynoAI.Core.Models.Results;

namespace SynoAI.Core.Interfaces;

public interface IDetectionService
{
    /// <summary>
    /// Runs a detection check for the specified camera name.
    /// </summary>
    /// <param name="cameraName">The name of the camera to check.</param>
    /// <returns>A <see cref="DetectionResult"/>.</returns>
    Task<DetectionResult> RunAsync(string cameraName);
}