using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Analytics;

public record DashboardMetricDto(
    string Name,
    string Key,
    string Category,
    decimal Value,
    string ValueFormatted,
    decimal? ChangePercentage,
    DateTime CalculatedAt
);
