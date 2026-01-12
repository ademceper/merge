using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Support;

/// <summary>
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
public record UpdateCommunicationStatusDto
{
    [Required]
    [StringLength(50)]
    public string Status { get; init; } = string.Empty;
    
    public DateTime? DeliveredAt { get; init; }
    
    public DateTime? ReadAt { get; init; }
}
