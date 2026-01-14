namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
public record EstimateDeliveryTimeDto(
    Guid? ProductId = null,
    Guid? CategoryId = null,
    Guid? WarehouseId = null,
    Guid? ShippingProviderId = null,
    string? City = null,
    string? Country = null,
    DateTime OrderDate = default
);
