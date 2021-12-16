namespace SynoAI.Models
{
    public enum SaveSnapshotMode
    {
        /// <summary>
        /// The snapshots are never saved.
        /// </summary>
        Off,
        /// <summary>
        /// The snapshots are always saved regardless of whether there are any valid detections.
        /// </summary>
        Always,
        /// <summary>
        /// The snapshots are saved only if there are predictions.
        /// </summary>
        WithPredictions,
        /// <summary>
        /// The snapshots are saved only if there are valid predictions.
        /// </summary>
        WithValidPredictions
    }
}