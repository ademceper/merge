namespace Merge.Application.DTOs.Logistics;

public record EstimateDeliveryTimeDto(
    Guid? ProductId = null,
    Guid? CategoryId = null,
    Guid? WarehouseId = null,
    Guid? ShippingProviderId = null,
    string? City = null,
    string? Country = null,
    DateTime OrderDate = default
);
