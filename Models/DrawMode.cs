namespace SynoAI.Models
{
    /// <summary>
    /// The mode to define how processed emails should be drawn.
    /// </summary>
    public enum DrawMode
    {
        /// <summary>
        /// The image will not have any boundary boxes drawn on it.
        /// </summary>
        Off,
        /// <summary>
        /// The image will have boundary boxes drawn on it only for labels that match the types the camera is looking for.
        /// </summary>
        Matches,
        /// <summary>
        /// The image will have boundary boxes drawn on it for all predictions received from the AI.
        /// </summary>
        All
    }
}