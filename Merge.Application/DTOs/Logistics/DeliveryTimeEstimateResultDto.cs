namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
public record DeliveryTimeEstimateResultDto(
    int MinDays,
    int MaxDays,
    int AverageDays,
    DateTime EstimatedDeliveryDate,
    string? EstimationSource // Product, Category, Warehouse, Default
);
