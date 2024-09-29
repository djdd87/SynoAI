namespace SynoAI.Core.Models;

/// <summary>
/// Represents a camera configuration.
/// </summary>
public class Camera
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required int Delay { get; set; }
    public required QualityProfile QualityProfile { get; set; }

    public ICollection<Zone> Zones { get; set; } = new List<Zone>();
}