using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.User;

public class CreateActivityLogDto
{
    public Guid? UserId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string ActivityType { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string EntityType { get; set; } = string.Empty;
    
    public Guid? EntityId { get; set; }
    
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    public string? Metadata { get; set; }
    
    [Range(0, int.MaxValue)]
    public int DurationMs { get; set; } = 0;
    
    public bool WasSuccessful { get; set; } = true;
    
    [StringLength(1000)]
    public string? ErrorMessage { get; set; }
}
