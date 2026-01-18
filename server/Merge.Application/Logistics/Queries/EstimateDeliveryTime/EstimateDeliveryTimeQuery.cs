using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.EstimateDeliveryTime;

public record EstimateDeliveryTimeQuery(
    Guid? ProductId,
    Guid? CategoryId,
    Guid? WarehouseId,
    Guid? ShippingProviderId,
    string? City,
    string? Country,
    DateTime OrderDate) : IRequest<DeliveryTimeEstimateResultDto>;

