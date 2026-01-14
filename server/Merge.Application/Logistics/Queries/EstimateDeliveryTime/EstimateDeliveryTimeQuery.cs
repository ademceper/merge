using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.EstimateDeliveryTime;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record EstimateDeliveryTimeQuery(
    Guid? ProductId,
    Guid? CategoryId,
    Guid? WarehouseId,
    Guid? ShippingProviderId,
    string? City,
    string? Country,
    DateTime OrderDate) : IRequest<DeliveryTimeEstimateResultDto>;

