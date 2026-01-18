using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Queries.GetPurchaseOrderById;

public record GetPurchaseOrderByIdQuery(Guid Id) : IRequest<PurchaseOrderDto?>;

