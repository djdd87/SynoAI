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
    }
}
