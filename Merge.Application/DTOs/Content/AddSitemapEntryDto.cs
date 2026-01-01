using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Content;

public class AddSitemapEntryDto
{
    [Required(ErrorMessage = "URL is required")]
    [Url(ErrorMessage = "Invalid URL format")]
    [StringLength(2048, ErrorMessage = "URL cannot exceed 2048 characters")]
    public string Url { get; set; } = string.Empty;

    [Required(ErrorMessage = "Page type is required")]
    [StringLength(50, ErrorMessage = "Page type cannot exceed 50 characters")]
    public string PageType { get; set; } = string.Empty;

    public Guid? EntityId { get; set; }

    [StringLength(20, ErrorMessage = "Change frequency cannot exceed 20 characters")]
    [RegularExpression(@"^(always|hourly|daily|weekly|monthly|yearly|never)$", ErrorMessage = "Invalid change frequency value")]
    public string? ChangeFrequency { get; set; }

    [Range(0.0, 1.0, ErrorMessage = "Priority must be between 0.0 and 1.0")]
    public decimal? Priority { get; set; }
}
