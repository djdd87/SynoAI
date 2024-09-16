using SynoAI.Core.Processors;

namespace SynoAI.Core.Data;

/// <summary>
/// Represents a detection area within a camera feed.
/// </summary>
public class DetectionArea
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required Guid CameraId { get; set; }
    public required ProcessorType ProcessorType { get; set; }
    public required bool Enabled { get; set; }

    public Camera? Camera { get; set; }

    public ICollection<DetectionPoint> DetectionPoints { get; set; } = new List<DetectionPoint>();
    public ICollection<DetectionTimeRange> DetectionTimeRanges { get; set; } = new List<DetectionTimeRange>();
}