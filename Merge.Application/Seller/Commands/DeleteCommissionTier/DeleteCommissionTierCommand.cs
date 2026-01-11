using MediatR;

namespace Merge.Application.Seller.Commands.DeleteCommissionTier;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteCommissionTierCommand(
    Guid TierId
) : IRequest<bool>;
