using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SynoAI.Core.Data;

/// <summary>
/// Represents a time range that the detection area is valid within.
/// </summary>
public class DetectionTimeRange
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required TimeSpan Start { get; set; }
    public required TimeSpan End { get; set; }
    public required Guid DetectionAreaId { get; set; }

    public DetectionArea? DetectionArea { get; set; }
}