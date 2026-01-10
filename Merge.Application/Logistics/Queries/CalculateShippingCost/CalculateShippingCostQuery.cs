using MediatR;

namespace Merge.Application.Logistics.Queries.CalculateShippingCost;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CalculateShippingCostQuery(
    Guid OrderId,
    string ShippingProvider) : IRequest<decimal>;

