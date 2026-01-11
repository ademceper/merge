using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Commands.UpdateCommissionTier;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateCommissionTierCommand(
    Guid TierId,
    string Name,
    decimal MinSales,
    decimal MaxSales,
    decimal CommissionRate,
    decimal PlatformFeeRate,
    int Priority
) : IRequest<bool>;
