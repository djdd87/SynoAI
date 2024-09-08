using System.ComponentModel.DataAnnotations;

namespace SynoAI.Core.Data;

public class Notifier
{
    [Key]
    public required Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public required string Name { get; set; }
}