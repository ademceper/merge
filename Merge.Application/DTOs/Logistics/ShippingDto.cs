namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
public record ShippingDto(
    Guid Id,
    Guid OrderId,
    string OrderNumber,
    string ShippingProvider,
    string TrackingNumber,
    string Status,
    DateTime? ShippedDate,
    DateTime? EstimatedDeliveryDate,
    DateTime? DeliveredDate,
    decimal ShippingCost,
    string? ShippingLabelUrl,
    DateTime CreatedAt
);
