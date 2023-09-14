using SynoAI.Models;

namespace SynoAI.Services
{
    /// <summary>
    /// Represents the AI service interface
    /// </summary>
    public interface IAIService
    {
        /// <summary>
        /// Asynchronously processes an image from the specified camera.
        /// </summary>
        /// <param name="camera">The camera from which the image was captured.</param>
        /// <param name="image">The image data to be processed.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// The task result contains an enumerable collection of AI predictions.</returns>
        Task<IEnumerable<AIPrediction>> ProcessAsync(Camera camera, byte[] image);
    }
}
