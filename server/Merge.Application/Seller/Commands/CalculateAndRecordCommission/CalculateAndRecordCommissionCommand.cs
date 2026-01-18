using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Commands.CalculateAndRecordCommission;

public record CalculateAndRecordCommissionCommand(
    Guid OrderId,
    Guid OrderItemId
) : IRequest<SellerCommissionDto>;
