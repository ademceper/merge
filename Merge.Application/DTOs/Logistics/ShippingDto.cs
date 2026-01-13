using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
// ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
public record ShippingDto(
    Guid Id,
    Guid OrderId,
    string OrderNumber,
    string ShippingProvider,
    string TrackingNumber,
    ShippingStatus Status,
    DateTime? ShippedDate,
    DateTime? EstimatedDeliveryDate,
    DateTime? DeliveredDate,
    decimal ShippingCost,
    string? ShippingLabelUrl,
    DateTime CreatedAt
);
