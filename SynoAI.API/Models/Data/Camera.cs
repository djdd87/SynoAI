namespace SynoAI.API.Models.Data;

public record Camera
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public ICollection<Zone> Zones { get; init; } = new List<Zone>();

    public Camera(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}