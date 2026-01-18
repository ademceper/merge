using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Logistics;

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
