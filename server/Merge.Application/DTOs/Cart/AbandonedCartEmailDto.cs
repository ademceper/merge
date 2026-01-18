using Merge.Domain.Enums;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Cart;


public record AbandonedCartEmailDto(
    Guid Id,
    Guid CartId,
    Guid UserId,
    AbandonedCartEmailType EmailType,
    DateTime SentAt,
    bool WasOpened,
    bool WasClicked,
    bool ResultedInPurchase,
    Guid? CouponId,
    string? CouponCode
);
