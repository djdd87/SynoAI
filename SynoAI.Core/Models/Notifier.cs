using SynoAI.Core.Notifiers;

namespace SynoAI.Core.Models;

public class Notifier
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required NotifierType NotifierType { get; set; }
    public string? MessageTemplate { get; set; }
}