using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Support;

public class UpdateCommunicationStatusDto
{
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;
    
    public DateTime? DeliveredAt { get; set; }
    
    public DateTime? ReadAt { get; set; }
}
