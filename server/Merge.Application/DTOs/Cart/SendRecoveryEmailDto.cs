using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Cart;


public record SendRecoveryEmailDto(
    [Required(ErrorMessage = "Sepet ID zorunludur")]
    Guid CartId,
    
    AbandonedCartEmailType EmailType,
    
    bool IncludeCoupon,
    
    [Range(0, 100, ErrorMessage = "Kupon indirim yüzdesi 0 ile 100 arasında olmalıdır.")]
    decimal? CouponDiscountPercentage
);
