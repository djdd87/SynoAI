using SynoAI.Models;

namespace SynoAI.AIs
{
    /// <summary>
    /// Represents the base class for all AI implementations.
    /// </summary>
    public abstract class AI
    {
        /// <summary>
        /// Gets the type of the AI being used.
        /// </summary>
        /// <value>The type of the AI.</value>
        public abstract AIType AIType { get; }

        /// <summary>
        /// Processes the given image using the AI and returns the predictions.
        /// </summary>
        /// <param name="logger">The logger to use for logging.</param>
        /// <param name="camera">The camera from which the image was captured.</param>
        /// <param name="image">The image to be processed.</param>
        /// <returns>A list of predictions made by the AI.</returns>
        public abstract Task<IEnumerable<AIPrediction>> Process(ILogger logger, Camera camera, byte[] image);
    }
}
