using SynoAI.Core.Data;

namespace SynoAI.Core.Interfaces;

public interface ICameraService
{
    Task AddZoneToCameraAsync(Guid cameraId, DetectionArea zone);

    /// <summary>
    /// Gets a specific camera record by it's ID.
    /// </summary>
    /// <param name="id">The unique ID of the camera.</param>
    /// <returns>The specified <see cref="Camera"/>.</returns>
    Task<Camera?> GetAsync(Guid id);

    /// <summary>
    /// Gets a specific camera record by it's name.
    /// </summary>
    /// <param name="name">The unique name of the camera.</param>
    /// <returns>The specified <see cref="Camera"/>.</returns>
    Task<Camera?> GetAsync(string name);

    /// <summary>
    /// Gets the list of cameras stored in the system.
    /// </summary>
    /// <returns>A list of <see cref="Camera"/>.</returns>
    Task<IEnumerable<Camera>> GetListAsync();
    Task<DetectionArea> GetZoneByIdAsync(Guid zoneId);
    Task<IEnumerable<DetectionArea>> GetZonesForCameraAsync(Guid cameraId);
    Task<object> DeleteZoneAsync(Guid zoneId);
}
