using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;


public record UpdateFlashSaleDto
{
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    public string Title { get; init; } = string.Empty;
    
    [StringLength(2000)]
    public string Description { get; init; } = string.Empty;
    
    public DateTime StartDate { get; init; }
    
    public DateTime EndDate { get; init; }
    
    public bool IsActive { get; init; }
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? BannerImageUrl { get; init; }
}
