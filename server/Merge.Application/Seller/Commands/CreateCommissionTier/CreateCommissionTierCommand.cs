using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Commands.CreateCommissionTier;

public record CreateCommissionTierCommand(
    string Name,
    decimal MinSales,
    decimal MaxSales,
    decimal CommissionRate,
    decimal PlatformFeeRate,
    int Priority
) : IRequest<CommissionTierDto>;
