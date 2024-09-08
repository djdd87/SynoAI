using System.ComponentModel.DataAnnotations;

namespace SynoAI.Core.Data;
public class Camera
{
    [Key]
    public required Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    public virtual ICollection<Zone> Zones { get; set; } = new HashSet<Zone>();
}