using System.Collections.Generic;


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
        /// The number of milliseconds to wait until taking a snapshot after motion is detected.
        /// </summary>
        public int Wait { get; set; }
        /// <summary>
        /// The period of time in milliseconds (ms) that must occur between the last motion detection of camera and the next time it'll be processed.
        /// </summary>
        public int? Delay { get; set; }
        /// <summary>
        /// The period of time in milliseconds (ms) that must occur between the last successful motion detection of camera and the next time it'll be processed.
        /// </summary>
        public int? DelayAfterSuccess { get; set; }
        /// <summary>
        /// The minimum size the object must be horizontally to be considered as a valid result.
        /// </summary>
        public int? MinSizeX { get; set; }
        /// <summary>
        /// The minimum size the object must be vertically to be considered as a valid result.
        /// </summary>
        public int? MinSizeY { get; set; }
        /// <summary>
        /// The maximum size the object must be horizontally to be considered as a valid result.
        /// </summary>
        public int? MaxSizeX { get; set; }
        /// <summary>
        /// The maximum size the object must be vertically to be considered as a valid result.
        /// </summary>
        public int? MaxSizeY { get; set; }
        /// <summary>
        /// The number of degrees to rotate the captured image before processing.
        /// </summary>
        public float Rotate { get; set; }
        /// <summary>
        /// Upon movement, the maximum number of snapshots sequentially retrieved from SSS until finding an object of interest (i.e. 4 snapshots). If not specified, this will 
        /// use the value specified on the main config.
        /// </summary>
        public int? MaxSnapshots { get; set; } 
        /// <summary>
        /// The zones to exclude when checking for objects of interest.
        /// </summary>
        public List<Zone> Exclusions { get; set; } 

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

        /// <summary>
        /// Gets the maximum size the object must be horizontally to be considered as a valid result from either the current camera, or the main config default if not specified.
        /// </summary>
        public int GetMaxSizeX()
        {
            return MaxSizeX ?? Config.MaxSizeX;
        }

        /// <summary>
        /// Gets the maximum size the object must be vertically to be considered as a valid result from either the current camera, or the main config default if not specified.
        /// </summary>
        public int GetMaxSizeY()
        {
            return MaxSizeY ?? Config.MaxSizeY;
        }

        /// <summary>
        /// Gets the maximum number of snapshots to take for the current camera, or the main config default if not specified.
        /// </summary>
        public int GetMaxSnapshots()
        {
            return MaxSnapshots ?? Config.MaxSnapshots;
        }

        /// <summary>
        /// Gets the delay between the last camera motion detection and processing and the next.
        /// </summary>
        public int GetDelay()
        {
            return Delay ?? Config.Delay;
        }

        /// <summary>
        /// Gets the delay between the last camera motion detection and processing and the next after the successful detection. 
        /// </summary>
        public int GetDelayAfterSuccess()
        {
            return DelayAfterSuccess ?? Config.DelayAfterSuccess ?? GetDelay();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
