using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Cart;

/// <summary>
/// Send Recovery Email DTO - BOLUM 7.1.5: Records (ZORUNLU)
/// BOLUM 4.1: Validation Attributes (ZORUNLU)
/// BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
/// </summary>
public record SendRecoveryEmailDto(
    [Required(ErrorMessage = "Sepet ID zorunludur")]
    Guid CartId,
    
    // ✅ BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
    AbandonedCartEmailType EmailType,
    
    bool IncludeCoupon,
    
    [Range(0, 100, ErrorMessage = "Kupon indirim yüzdesi 0 ile 100 arasında olmalıdır.")]
    decimal? CouponDiscountPercentage
);
