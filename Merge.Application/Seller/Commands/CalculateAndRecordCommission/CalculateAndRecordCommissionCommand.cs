using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Commands.CalculateAndRecordCommission;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CalculateAndRecordCommissionCommand(
    Guid OrderId,
    Guid OrderItemId
) : IRequest<SellerCommissionDto>;
