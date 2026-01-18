using MediatR;

namespace Merge.Application.Logistics.Queries.CalculateShippingCost;

public record CalculateShippingCostQuery(
    Guid OrderId,
    string ShippingProvider) : IRequest<decimal>;

