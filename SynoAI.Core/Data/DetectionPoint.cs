namespace SynoAI.Core.Data;

/// <summary>
/// Represents a point to generate a defined zone for a <see cref="DetectionArea"/>.
/// </summary>
public class DetectionPoint
{
    public required Guid Id { get; set; }
    public required int X { get; set; }
    public required int Y { get; set; }
    public required int Order { get; set; }
    public required Guid DetectionAreaId { get; set; }

    public DetectionArea? DetectionArea { get; set; }
}