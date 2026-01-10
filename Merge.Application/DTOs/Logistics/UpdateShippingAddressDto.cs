using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
public record UpdateShippingAddressDto(
    [StringLength(50)]
    string? Label = null,
    
    [StringLength(100, MinimumLength = 2)]
    string? FirstName = null,
    
    [StringLength(100, MinimumLength = 2)]
    string? LastName = null,
    
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [StringLength(20)]
    string? Phone = null,
    
    [StringLength(200, MinimumLength = 5)]
    string? AddressLine1 = null,
    
    [StringLength(200)]
    string? AddressLine2 = null,
    
    [StringLength(100, MinimumLength = 2)]
    string? City = null,
    
    [StringLength(100)]
    string? State = null,
    
    [StringLength(20)]
    string? PostalCode = null,
    
    [StringLength(100)]
    string? Country = null,
    
    bool? IsDefault = null,
    
    bool? IsActive = null,
    
    [StringLength(500)]
    string? Instructions = null
);
