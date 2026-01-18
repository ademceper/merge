using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.CreatePurchaseOrder;

public record CreatePurchaseOrderCommand(
    Guid B2BUserId,
    CreatePurchaseOrderDto Dto
) : IRequest<PurchaseOrderDto>;

