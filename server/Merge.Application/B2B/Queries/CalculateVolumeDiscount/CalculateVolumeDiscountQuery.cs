using MediatR;

namespace Merge.Application.B2B.Queries.CalculateVolumeDiscount;

public record CalculateVolumeDiscountQuery(
    Guid ProductId,
    int Quantity,
    Guid? OrganizationId = null
) : IRequest<decimal>;

