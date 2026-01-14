namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
public record DeliveryTimeEstimationDto(
    Guid Id,
    Guid? ProductId,
    string? ProductName,
    Guid? CategoryId,
    string? CategoryName,
    Guid? WarehouseId,
    string? WarehouseName,
    Guid? ShippingProviderId,
    string? City,
    string? Country,
    int MinDays,
    int MaxDays,
    int AverageDays,
    bool IsActive,
    DeliveryTimeSettingsDto? Conditions, // Typed DTO (Over-posting korumasi)
    DateTime CreatedAt
);
