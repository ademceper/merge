using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Seller;

public class UpdateStoreDto
{
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Mağaza adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string? StoreName { get; set; }
    
    [StringLength(2000)]
    public string? Description { get; set; }
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? LogoUrl { get; set; }
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? BannerUrl { get; set; }
    
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string? ContactEmail { get; set; }
    
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [StringLength(20)]
    public string? ContactPhone { get; set; }
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    [StringLength(100)]
    public string? City { get; set; }
    
    [StringLength(100)]
    public string? Country { get; set; }
    
    [StringLength(50)]
    public string? Status { get; set; }
    
    public bool? IsPrimary { get; set; }

    /// <summary>
    /// Magaza ayarlari - Typed DTO (Over-posting korumasi)
    /// </summary>
    public StoreSettingsDto? Settings { get; set; }
}
