using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Support;

/// <summary>
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
public record UpdateSupportTicketDto
{
    [StringLength(200)]
    public string? Subject { get; init; }
    
    [StringLength(5000)]
    public string? Description { get; init; }
    
    [StringLength(50)]
    public string? Category { get; init; }
    
    [StringLength(20)]
    public string? Priority { get; init; }
    
    [StringLength(50)]
    public string? Status { get; init; }
    
    public Guid? AssignedToId { get; init; }
}
