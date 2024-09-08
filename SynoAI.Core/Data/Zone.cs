using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SynoAI.Core.Data;

public class Zone
{

    [Key]
    public required Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    public required Guid CameraId { get; set; }

    [ForeignKey(nameof(CameraId))]
    public required virtual Camera Camera { get; set; }
}