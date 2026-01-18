using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Inventory;
namespace Merge.Application.DTOs.Logistics;

public record DeliveryTimeEstimateResultDto(
    int MinDays,
    int MaxDays,
    int AverageDays,
    DateTime EstimatedDeliveryDate,
    string? EstimationSource // Product, Category, Warehouse, Default
);
