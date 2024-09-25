using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SynoAI.Core.Models;
using SynoAI.Core.Interfaces;
using SynoAI.Core.Models.Requests;
using SynoAI.Core.Models.Results;

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
        _logger.LogInformation("Fetching cameras");
        return await _context.Cameras.ToListAsync();
    }

    public async Task<Camera?> GetAsync(Guid cameraId)
    {
        _logger.LogInformation("Fetching camera '{cameraId}'", cameraId);

        return await _context.Cameras.FirstOrDefaultAsync(x => x.Id == cameraId);
    }

    public async Task<Camera?> GetAsync(string name)
    {
        _logger.LogInformation("Fetching camera by name '{name}'", name);

        return await _context.Cameras.FirstOrDefaultAsync(x => x.Name == name);
    }

    public async Task<CreateResult<Camera>> CreateAsync(CreateCamera create)
    {
        // Validate the create data
        if (string.IsNullOrWhiteSpace(create.Name))
        {
            _logger.LogWarning("No camera name specified.");
            return CreateResult<Camera>.Failure("Camera name is required.");
        }

        // Ensure the camera doesn't already exist
        _logger.LogInformation("Checking if camera '{name}' exists.", create.Name);

        Camera? existing = await _context.Cameras.FirstOrDefaultAsync(x=> x.Name == create.Name);
        if (existing is not null)
        {
            _logger.LogWarning("A camera with name '{name}' already exists.", create.Name);
            return CreateResult<Camera>.Failure($"Camera with the name '{create.Name}' already exists.");
        }

        // Create the new camera
        _logger.LogInformation("Saving new camera '{name}'.", create.Name);

        var camera = new Camera
        {
            Id = Guid.NewGuid(),
            Name = create.Name,
            QualityProfile = create.QualityProfile
        };

        _context.Cameras.Add(camera);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Camera name '{name}' saved with ID {cameraId}.", create.Name, camera.Id);
        return CreateResult<Camera>.Success(camera);
    }

    public async Task<bool> DeleteAsync(Guid cameraId)
    {
        _logger.LogInformation("Checking if camera with ID '{cameraId}' exists.", cameraId);

        Camera? camera = await _context.Cameras.FirstOrDefaultAsync(x => x.Id == cameraId);
        if (camera is null)
        {
            _logger.LogWarning("A camera with ID '{cameraId}' was not found.", cameraId);
            return false;
        }

        _logger.LogInformation("Deleting camera '{cameraId}'.", cameraId);

        _context.Cameras.Remove(camera!);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Camera with ID '{cameraId}' deleted.", cameraId);
        return true;
    }
}