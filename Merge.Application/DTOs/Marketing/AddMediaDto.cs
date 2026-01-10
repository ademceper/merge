using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Add Media DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record AddMediaDto
{
    [Required(ErrorMessage = "URL is required")]
    [Url(ErrorMessage = "Invalid URL format")]
    [StringLength(2048, ErrorMessage = "URL cannot exceed 2048 characters")]
    public string Url { get; init; } = string.Empty;

    [Required(ErrorMessage = "Media type is required")]
    [StringLength(50, ErrorMessage = "Media type cannot exceed 50 characters")]
    public string MediaType { get; init; } = "Photo";

    [Url(ErrorMessage = "Invalid thumbnail URL format")]
    [StringLength(2048, ErrorMessage = "Thumbnail URL cannot exceed 2048 characters")]
    public string? ThumbnailUrl { get; init; }
}
