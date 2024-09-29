using SynoAI.Core.Processors;

namespace SynoAI.Core.Models;

/// <summary>
/// Represents a detection area within a camera feed.
/// </summary>
public class Zone
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required Guid CameraId { get; set; }
    public required ZoneType ZoneType { get; set; }
    public required ProcessorType ProcessorType { get; set; }
    public required bool Enabled { get; set; }

    public Camera? Camera { get; set; }

    public ICollection<ZonePoint> ZonePoints { get; set; } = new List<ZonePoint>();
    public ICollection<ZoneTarget> ZoneTargets { get; set; } = new List<ZoneTarget>();
    public ICollection<ZoneTimeRange> ZoneTimeRanges { get; set; } = new List<ZoneTimeRange>();
}