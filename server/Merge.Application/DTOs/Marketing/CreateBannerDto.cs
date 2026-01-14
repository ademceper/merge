using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Content;

namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Create Banner DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record CreateBannerDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    public string Title { get; init; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; init; }
    
    [Required]
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string ImageUrl { get; init; } = string.Empty;
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? LinkUrl { get; init; }
    
    [StringLength(50)]
    public string Position { get; init; } = "Homepage";
    
    [Range(0, int.MaxValue)]
    public int SortOrder { get; init; } = 0;
    
    public DateTime? StartDate { get; init; }
    
    public DateTime? EndDate { get; init; }
    
    public Guid? CategoryId { get; init; }
    
    public Guid? ProductId { get; init; }
}
