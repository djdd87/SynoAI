using SynoAI.Core.Models;
using SynoAI.Core.Models.Requests;
using SynoAI.Core.Models.Results;

namespace SynoAI.Core.Interfaces;

public interface ICameraService
{
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

    /// <summary>
    /// Creates a new camera.
    /// </summary>
    /// <param name="create">The data of the camera to create.</param>
    /// <returns>A <see cref="CreateResult<Camera>"/> result.</returns>
    Task<CreateResult<Camera>> CreateAsync(CreateCamera create);

    /// <summary>
    /// Deletes a camera.
    /// </summary>
    /// <param name="cameraId">The data of the camera to create.</param>
    /// <returns>True if the camera was deleted, false if there was no camera to delete.</returns>
    Task<bool> DeleteAsync(Guid id);
}
