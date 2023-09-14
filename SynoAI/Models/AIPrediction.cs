namespace SynoAI.Models
{
    /// <summary>
/// Represents an AI prediction.
/// </summary>
    public class AIPrediction
    {
        /// <summary>
        /// Gets or sets the label of the prediction.
        /// </summary>
        public string Label { get; set; }
        /// <summary>
        /// Gets or sets the confidence of the prediction.
        /// </summary>
        public decimal Confidence { get; set; }
        /// <summary>
        /// Gets or sets the minimum X size of the prediction.
        /// </summary>
        public int MinX { get; set; }
        /// <summary>
        /// Gets or sets the minimum Y size of the prediction.
        /// </summary>
        public int MinY { get; set; }
        /// <summary>
        /// Gets or sets the maximum X size of the prediction.
        /// </summary>
        public int MaxX { get; set; }
        /// <summary>
        /// Gets or sets the maximum Y size of the prediction.
        /// </summary>
        public int MaxY { get; set; }
        /// <summary>
        /// Gets the results of X size.
        /// </summary>
        public int SizeX
        {
            get
            {
                return MaxX - MinX;
            }
        }
        /// <summary>
        /// Gets the results of Y size.
        /// </summary>
        public int SizeY
        {
            get
            {
                return MaxY - MinY;
            }
        }
    }
}
