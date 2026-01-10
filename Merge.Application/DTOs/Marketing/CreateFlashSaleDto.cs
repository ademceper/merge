using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Create Flash Sale DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record CreateFlashSaleDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    public string Title { get; init; } = string.Empty;
    
    [StringLength(2000)]
    public string Description { get; init; } = string.Empty;
    
    [Required]
    public DateTime StartDate { get; init; }
    
    [Required]
    public DateTime EndDate { get; init; }
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? BannerImageUrl { get; init; }
}
