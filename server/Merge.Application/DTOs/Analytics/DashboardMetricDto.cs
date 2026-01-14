using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record DashboardMetricDto(
    string Name,
    string Key,
    string Category,
    decimal Value,
    string ValueFormatted,
    decimal? ChangePercentage,
    DateTime CalculatedAt
);
