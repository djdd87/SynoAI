using System;


namespace SynoAI.Models
{
    /// <summary>
    /// Holds data extracted from filenames inside a captures/camera folder
    /// </summary>

    public class GraphData
    {
        /// <summary>
        /// The hour of the day
        /// </summary>
        public int Hour { get; set; }

        /// <summary>
        /// The amount of valid predictions
        /// </summary>
        public int Predictions { get; set; }

        /// <summary>
        /// The amount of valid objects detected
        /// </summary>
        public int Objects { get; set; }
    }

}