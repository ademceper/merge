using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
public record CreateShippingDto(
    [Required]
    Guid OrderId,
    
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Kargo sağlayıcısı adı en az 2, en fazla 100 karakter olmalıdır.")]
    string ShippingProvider,
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Kargo maliyeti 0 veya daha büyük olmalıdır.")]
    decimal ShippingCost
);
