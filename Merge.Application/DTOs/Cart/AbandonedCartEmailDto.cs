using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Cart;

/// <summary>
/// Abandoned Cart Email DTO - BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
/// </summary>
public class AbandonedCartEmailDto
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Guid UserId { get; set; }
    // ✅ BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
    public AbandonedCartEmailType EmailType { get; set; }
    public DateTime SentAt { get; set; }
    public bool WasOpened { get; set; }
    public bool WasClicked { get; set; }
    public bool ResultedInPurchase { get; set; }
    public Guid? CouponId { get; set; }
    public string? CouponCode { get; set; }
}
