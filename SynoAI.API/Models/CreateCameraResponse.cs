namespace SynoAI.API.Models
{
    /// <summary>
    /// Returned when a camera is created.
    /// </summary>
    public class CreateCameraResponse
    {
        /// <summary>
        /// The ID of the camera that was created.
        /// </summary>
        public Guid? Id { get; set; }
    }
}
