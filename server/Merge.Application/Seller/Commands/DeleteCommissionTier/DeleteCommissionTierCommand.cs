using MediatR;

namespace Merge.Application.Seller.Commands.DeleteCommissionTier;

public record DeleteCommissionTierCommand(
    Guid TierId
) : IRequest<bool>;
