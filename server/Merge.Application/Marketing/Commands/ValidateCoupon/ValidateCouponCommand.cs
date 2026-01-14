using MediatR;

namespace Merge.Application.Marketing.Commands.ValidateCoupon;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ValidateCouponCommand(
    string Code,
    decimal OrderAmount,
    Guid? UserId,
    List<Guid>? ProductIds) : IRequest<decimal>;
