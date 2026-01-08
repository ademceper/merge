using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Cart;

/// <summary>
/// Abandoned Cart Email DTO - BOLUM 7.1.5: Records (ZORUNLU)
/// BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
/// </summary>
public record AbandonedCartEmailDto(
    Guid Id,
    Guid CartId,
    Guid UserId,
    // ✅ BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
    AbandonedCartEmailType EmailType,
    DateTime SentAt,
    bool WasOpened,
    bool WasClicked,
    bool ResultedInPurchase,
    Guid? CouponId,
    string? CouponCode
);
