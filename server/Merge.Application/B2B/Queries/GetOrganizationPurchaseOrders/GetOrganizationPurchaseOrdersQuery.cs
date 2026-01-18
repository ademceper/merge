using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Queries.GetOrganizationPurchaseOrders;

public record GetOrganizationPurchaseOrdersQuery(
    Guid OrganizationId,
    string? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<PurchaseOrderDto>>;

