using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
public record CreateShippingAddressDto(
    [StringLength(50)]
    string Label,
    
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad en az 2, en fazla 100 karakter olmalıdır.")]
    string FirstName,
    
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Soyad en az 2, en fazla 100 karakter olmalıdır.")]
    string LastName,
    
    [Required]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [StringLength(20)]
    string Phone,
    
    [Required]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Adres satırı en az 5, en fazla 200 karakter olmalıdır.")]
    string AddressLine1,
    
    [StringLength(200)]
    string? AddressLine2 = null,
    
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Şehir en az 2, en fazla 100 karakter olmalıdır.")]
    string City,
    
    [StringLength(100)]
    string State,
    
    [StringLength(20)]
    string PostalCode,
    
    [Required]
    [StringLength(100)]
    string Country,
    
    bool IsDefault = false,
    
    [StringLength(500)]
    string? Instructions = null
);
