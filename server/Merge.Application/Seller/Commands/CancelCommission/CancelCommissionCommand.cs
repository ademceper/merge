using MediatR;

namespace Merge.Application.Seller.Commands.CancelCommission;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CancelCommissionCommand(
    Guid CommissionId
) : IRequest<bool>;
