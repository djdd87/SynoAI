
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SynoAI.Core.Data;
using SynoAI.Core.Interfaces;

namespace SynoAI.Core.Services;

public class CameraService : ICameraService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CameraService> _logger;

    public CameraService(ILogger<CameraService> logger, AppDbContext context)
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

    public Task AddZoneToCameraAsync(Guid cameraId, Zone zone)
    {
        throw new NotImplementedException();
    }

    public Task<Zone> GetZoneByIdAsync(Guid zoneId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Zone>> GetZonesForCameraAsync(Guid cameraId)
    {
        throw new NotImplementedException();
    }

    public Task<object> DeleteZoneAsync(Guid zoneId)
    {
        throw new NotImplementedException();
    }
}