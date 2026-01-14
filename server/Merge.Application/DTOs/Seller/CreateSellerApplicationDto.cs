using System.ComponentModel.DataAnnotations;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
public record CreateSellerApplicationDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "İşletme adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string BusinessName { get; init; } = string.Empty;
    
    [Required]
    // ✅ ARCHITECTURE: Enum kullanımı (string BusinessType yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public BusinessType BusinessType { get; init; }
    
    [Required]
    [StringLength(50, MinimumLength = 10, ErrorMessage = "Vergi numarası en az 10, en fazla 50 karakter olmalıdır.")]
    public string TaxNumber { get; init; } = string.Empty;
    
    [Required]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Adres en az 5, en fazla 500 karakter olmalıdır.")]
    public string Address { get; init; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Şehir en az 2, en fazla 100 karakter olmalıdır.")]
    public string City { get; init; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Country { get; init; } = string.Empty;
    
    [StringLength(20)]
    public string PostalCode { get; init; } = string.Empty;
    
    [Required]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [StringLength(20)]
    public string PhoneNumber { get; init; } = string.Empty;
    
    [Required]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string Email { get; init; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string BankName { get; init; } = string.Empty;
    
    [Required]
    [StringLength(50, MinimumLength = 5, ErrorMessage = "Hesap numarası en az 5, en fazla 50 karakter olmalıdır.")]
    public string BankAccountNumber { get; init; } = string.Empty;
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Hesap sahibi adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string BankAccountHolderName { get; init; } = string.Empty;
    
    [Required]
    [StringLength(34, MinimumLength = 15, ErrorMessage = "IBAN en az 15, en fazla 34 karakter olmalıdır.")]
    public string IBAN { get; init; } = string.Empty;
    
    [StringLength(2000)]
    public string BusinessDescription { get; init; } = string.Empty;
    
    [StringLength(500)]
    public string ProductCategories { get; init; } = string.Empty;
    
    [Range(0, double.MaxValue, ErrorMessage = "Tahmini aylık gelir 0 veya daha büyük olmalıdır.")]
    public decimal EstimatedMonthlyRevenue { get; init; }
    
    [StringLength(500)]
    public string? IdentityDocumentUrl { get; init; }
    
    [StringLength(500)]
    public string? TaxCertificateUrl { get; init; }
    
    [StringLength(500)]
    public string? BankStatementUrl { get; init; }
    
    [StringLength(500)]
    public string? BusinessLicenseUrl { get; init; }
}
