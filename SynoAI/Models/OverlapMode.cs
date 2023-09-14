namespace SynoAI.Models
{
    /// <summary>
    /// Defines the overlapMode for camera images
    /// </summary>
    public enum OverlapMode
    {   
        /// <summary>
        /// The boundary box around the prediction must be entirely contained within the exclusion zone for it to be ignored.
        /// </summary>
        Contains,
        /// <summary>
        /// The boundary box around the prediction must intersect the exlusion zone anywhere for it to be ignored.
        /// </summary>
        Intersect
    }
}