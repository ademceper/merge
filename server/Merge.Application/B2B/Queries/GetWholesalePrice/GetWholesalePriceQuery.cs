using MediatR;

namespace Merge.Application.B2B.Queries.GetWholesalePrice;

public record GetWholesalePriceQuery(
    Guid ProductId,
    int Quantity,
    Guid? OrganizationId = null
) : IRequest<decimal?>;

