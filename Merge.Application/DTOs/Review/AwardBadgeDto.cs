using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.Review;

public class AwardBadgeDto
{
    [Required]
    public Guid BadgeId { get; set; }
    
    [StringLength(500)]
    public string? AwardReason { get; set; }
    
    public DateTime? ExpiresAt { get; set; }
}
