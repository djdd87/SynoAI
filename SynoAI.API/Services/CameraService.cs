using Microsoft.EntityFrameworkCore;
using SynoAI.API.Models.Data;

public interface ICameraService
{
    Task AddZoneToCameraAsync(Guid cameraId, Zone zone);

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
    Task<object> GetZoneByIdAsync(Guid zoneId);
    Task<object> GetZonesForCameraAsync(Guid cameraId);
}

public class CameraService : ICameraService
{
    private readonly IAppDbContext _context;
    private readonly ILogger<CameraService> _logger;

    public CameraService(IAppDbContext context, ILogger<CameraService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Camera>> GetListAsync()
    {
        _logger.LogInformation("Fetching cameras from database");
        return await _context.Cameras.ToListAsync();
    }

    public async Task<Camera?> GetAsync(Guid id)
    {
        _logger.LogInformation("Fetching camera '{Id}' from database", id);

        return await _context.Cameras.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Camera?> GetAsync(string name)
    {
        _logger.LogInformation("Fetching camera by name '{Name}' from database", name);

        return await _context.Cameras.FirstOrDefaultAsync(x => x.Name == name);
    }

    public async Task AddAsync(Camera camera)
    {
        _logger.LogInformation($"Adding camera to database: {camera.Name}");
        await _context.Cameras.AddAsync(camera);
        await _context.SaveChangesAsync();
    }
}