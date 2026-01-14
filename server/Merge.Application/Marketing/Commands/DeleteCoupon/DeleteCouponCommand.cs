using MediatR;

namespace Merge.Application.Marketing.Commands.DeleteCoupon;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteCouponCommand(
    Guid Id) : IRequest<bool>;
