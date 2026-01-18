using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Support;


public record UpdateCommunicationStatusDto
{
    [Required]
    [StringLength(50)]
    public string Status { get; init; } = string.Empty;
    
    public DateTime? DeliveredAt { get; init; }
    
    public DateTime? ReadAt { get; init; }
}
