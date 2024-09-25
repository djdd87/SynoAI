namespace SynoAI.Core.Models;

/// <summary>
/// Represents the target type that the image processors will alert for, e.g. "Person".
/// </summary>
public class ZoneTarget
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required Guid ZoneId { get; set; }

    public Zone? Zone { get; set; }
}
