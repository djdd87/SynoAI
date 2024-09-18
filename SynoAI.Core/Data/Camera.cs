using SynoAI.Core.Models;

namespace SynoAI.Core.Data;

/// <summary>
/// Represents a camera configuration.
/// </summary>
public class Camera
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required CameraQuality QualityProfile { get; set; }

    public ICollection<Zone> Zones { get; set; } = new List<Zone>();
}