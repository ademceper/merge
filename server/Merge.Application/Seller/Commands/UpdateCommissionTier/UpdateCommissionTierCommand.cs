using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Commands.UpdateCommissionTier;

public record UpdateCommissionTierCommand(
    Guid TierId,
    string Name,
    decimal MinSales,
    decimal MaxSales,
    decimal CommissionRate,
    decimal PlatformFeeRate,
    int Priority
) : IRequest<bool>;
