using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.LiveCommerce;

public class CreateLiveStreamDto
{
    [Required]
    public Guid SellerId { get; set; }
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    public DateTime? ScheduledStartTime { get; set; }
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? StreamUrl { get; set; }
    
    [StringLength(200)]
    public string? StreamKey { get; set; }
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? ThumbnailUrl { get; set; }
    
    [StringLength(100)]
    public string? Category { get; set; }
    
    [StringLength(500)]
    public string? Tags { get; set; }
}
