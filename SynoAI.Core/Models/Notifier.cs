using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SynoAI.Core.Notifiers;
using System.ComponentModel.DataAnnotations;

namespace SynoAI.Core.Data;

public class Notifier
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required NotifierType NotifierType { get; set; }
    public string? MessageTemplate { get; set; }
}