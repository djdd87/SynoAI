namespace SynoAI.Core.Models.Contracts;

public record CreateCamera
{
    public required string Name { get; init; }
    public required QualityProfile QualityProfile { get; init; }
}
