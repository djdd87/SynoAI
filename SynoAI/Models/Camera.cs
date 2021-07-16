using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SynoAI.Models
{
    /// <summary>
    /// Represents a camera object.
    /// </summary>
    public class Camera
    {
        /// <summary>
        /// The name of the camera.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The list of types that this camera is looking for.
        /// </summary>
        public IEnumerable<string> Types { get; set; }
        /// <summary>
        /// The Threshold to apply to the target types.
        /// </summary>
        public decimal Threshold { get; set; }
        /// <summary>
        /// The minimum size the object must be horizontally to be considered as a valid result.
        /// </summary>
        public int? MinSizeX { get; set; }
        /// <summary>
        /// The minimum size the object must be vertically to be considered as a valid result.
        /// </summary>
        public int? MinSizeY { get; set; }
        /// <summary>
        /// The number of degrees to rotate the captured image before processing.
        /// </summary>
        public float Rotate { get; set; }

        /// <summary>
        /// Gets the minimum size the object must be horizontally to be considered as a valid result from either the current camera, or the main config default if not specified.
        /// </summary>
        public int GetMinSizeX()
        {
            return MinSizeX ?? Config.MinSizeX;
        }

        /// <summary>
        /// Gets the minimum size the object must be vertically to be considered as a valid result from either the current camera, or the main config default if not specified.
        /// </summary>
        public int GetMinSizeY()
        {
            return MinSizeY ?? Config.MinSizeY;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
