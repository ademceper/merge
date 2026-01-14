using MediatR;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.SendRecoveryEmail;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
public record SendRecoveryEmailCommand(
    Guid CartId,
    AbandonedCartEmailType EmailType = AbandonedCartEmailType.First,
    bool IncludeCoupon = false,
    decimal? CouponDiscountPercentage = null
) : IRequest;

