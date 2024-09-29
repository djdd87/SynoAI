using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SynoAI.Core.Models;
using SynoAI.Core.Interfaces;
using SynoAI.Core.Models.Contracts;
using SynoAI.Core.Models.Results;

namespace SynoAI.Core.Services;

public class ZoneService : IZoneService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ZoneService> _logger;

    public ZoneService(ILogger<ZoneService> logger, AppDbContext context)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Zone?> GetAsync(Guid zoneId)
    {
        _logger.LogInformation("Fetching zone '{Id}'", zoneId);

        return await _context.Zones.FirstOrDefaultAsync(x => x.Id == zoneId);
    }

    public async Task<IEnumerable<Zone>> GetListAsync()
    {
        _logger.LogInformation("Fetching zones");
        return await _context.Zones.ToListAsync();
    }

    public async Task<IEnumerable<Zone>> GetListByCameraAsync(Guid cameraId)
    {
        _logger.LogInformation("Fetching zones for camera '{cameraId}'", cameraId);
        return await _context.Zones.Where(x => x.CameraId == cameraId).ToListAsync();
    }

    public async Task<bool> DeleteAsync(Guid zoneId)
    {
        _logger.LogInformation("Checking if zone with ID '{zoneId}' exists.", zoneId);

        Zone? zone = await _context.Zones.FirstOrDefaultAsync(x => x.Id == zoneId);
        if (zone is null)
        {
            _logger.LogWarning("A zone with ID '{zoneId}' was not found.", zoneId);
            return false;
        }

        _logger.LogInformation("Deleting zone '{zoneId}'.", zoneId);

        _context.Zones.Remove(zone!);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Zone with ID '{zoneId}' deleted.", zoneId);
        return true;
    }

    public Task<CreateResult<Zone>> CreateAsync(CreateZone contract)
    {
        throw new NotImplementedException();
    }

    public Task<UpdateResult<Zone>> UpdateAsync(Guid zoneId, UpdateZone contract)
    {
        throw new NotImplementedException();
    }
}
