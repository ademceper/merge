using MediatR;

namespace Merge.Application.B2B.Queries.CalculateVolumeDiscount;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CalculateVolumeDiscountQuery(
    Guid ProductId,
    int Quantity,
    Guid? OrganizationId = null
) : IRequest<decimal>;

