using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Cart;

/// <summary>
/// Send Recovery Email DTO - BOLUM 4.1: Validation Attributes (ZORUNLU)
/// BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
/// </summary>
public class SendRecoveryEmailDto
{
    [Required(ErrorMessage = "Sepet ID zorunludur")]
    public Guid CartId { get; set; }
    
    // ✅ BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
    public AbandonedCartEmailType EmailType { get; set; } = AbandonedCartEmailType.First;
    
    public bool IncludeCoupon { get; set; } = false;
    
    [Range(0, 100, ErrorMessage = "Kupon indirim yüzdesi 0 ile 100 arasında olmalıdır.")]
    public decimal? CouponDiscountPercentage { get; set; }
}
