using SynoAI.Core.Models;
using SynoAI.Core.Models.Contracts;
using SynoAI.Core.Models.Results;

namespace SynoAI.Core.Interfaces;

public interface IZoneService
{
    /// <summary>
    /// Fetches the details for the specified zone.
    /// </summary>
    /// <param name="zoneId">The ID of the zone to fetch.</param>
    /// <returns>A <see cref="Zone"/>.</returns>
    Task<Zone?> GetAsync(Guid zoneId);

    /// <summary>
    /// Gets all the zones.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{Zone}"/></returns>
    Task<IEnumerable<Zone>> GetListAsync();

    /// <summary>
    /// Gets all the cameras for the specified camera.
    /// </summary>
    /// <param name="cameraId">The ID of the camera to get the zones for.</param>
    /// <returns>An <see cref="IEnumerable{Zone}"/></returns>
    Task<IEnumerable<Zone>> GetListByCameraAsync(Guid cameraId);

    /// <summary>
    /// Deletes a camera zone.
    /// </summary>
    /// <param name="zoneId">The ID of the zone to delete.</param>
    /// <returns>True if the zone was deleted, false if the zone was not found.</returns>
    Task<bool> DeleteAsync(Guid zoneId);

    Task<CreateResult<Zone>> CreateAsync(CreateZone contract);

    /// <summary>
    /// Updates a zone.
    /// </summary>
    /// <param name="zoneId">The ID of the zone to update.</param>
    /// <param name="contract">The update details.</param>
    /// <returns>A <see cref="UpdateResult<Zone>"/></returns>
    Task<UpdateResult<Zone>> UpdateAsync(Guid zoneId, UpdateZone contract);
}
