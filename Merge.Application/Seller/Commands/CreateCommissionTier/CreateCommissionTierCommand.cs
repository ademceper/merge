using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Commands.CreateCommissionTier;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateCommissionTierCommand(
    string Name,
    decimal MinSales,
    decimal MaxSales,
    decimal CommissionRate,
    decimal PlatformFeeRate,
    int Priority
) : IRequest<CommissionTierDto>;
