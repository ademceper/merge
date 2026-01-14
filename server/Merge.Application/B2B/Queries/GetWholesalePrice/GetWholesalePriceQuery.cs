using MediatR;

namespace Merge.Application.B2B.Queries.GetWholesalePrice;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetWholesalePriceQuery(
    Guid ProductId,
    int Quantity,
    Guid? OrganizationId = null
) : IRequest<decimal?>;

