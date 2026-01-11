using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
public record UpdateStoreDto
{
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Mağaza adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string? StoreName { get; init; }
    
    [StringLength(2000)]
    public string? Description { get; init; }
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? LogoUrl { get; init; }
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? BannerUrl { get; init; }
    
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string? ContactEmail { get; init; }
    
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [StringLength(20)]
    public string? ContactPhone { get; init; }
    
    [StringLength(500)]
    public string? Address { get; init; }
    
    [StringLength(100)]
    public string? City { get; init; }
    
    [StringLength(100)]
    public string? Country { get; init; }
    
    [StringLength(20)]
    public string? PostalCode { get; init; }
    
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public EntityStatus? Status { get; init; }
    
    public bool? IsPrimary { get; init; }

    /// <summary>
    /// Magaza ayarlari - Typed DTO (Over-posting korumasi)
    /// </summary>
    public StoreSettingsDto? Settings { get; init; }
}
