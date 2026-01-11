using MediatR;

namespace Merge.Application.Seller.Commands.ApproveCommission;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ApproveCommissionCommand(
    Guid CommissionId
) : IRequest<bool>;
