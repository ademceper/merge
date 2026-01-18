using MediatR;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.SendRecoveryEmail;

public record SendRecoveryEmailCommand(
    Guid CartId,
    AbandonedCartEmailType EmailType = AbandonedCartEmailType.First,
    bool IncludeCoupon = false,
    decimal? CouponDiscountPercentage = null
) : IRequest;

