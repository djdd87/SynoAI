using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SynoAI.Core.Data;

/// <summary>
/// Represents a time range that the <see cref="Zone"> will operate within.
/// </summary>
public class ZoneTimeRange
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required TimeSpan Start { get; set; }
    public required TimeSpan End { get; set; }
    public required Guid ZoneId { get; set; }

    public Zone? Zone { get; set; }
}