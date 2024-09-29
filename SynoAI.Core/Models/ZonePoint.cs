namespace SynoAI.Core.Models;

/// <summary>
/// Represents a point to generate a defined area for a <see cref="Zone"/>.
/// </summary>
public class ZonePoint
{
    public required Guid Id { get; set; }
    public required int X { get; set; }
    public required int Y { get; set; }
    public required int Order { get; set; }
    public required Guid ZoneId { get; set; }

    public Zone? Zone { get; set; }
}