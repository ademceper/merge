namespace Merge.Application.DTOs.International;

public record CurrencyStatsDto(
    int TotalCurrencies,
    int ActiveCurrencies,
    string BaseCurrency,
    DateTime LastRateUpdate,
    IReadOnlyList<CurrencyUsageDto> MostUsedCurrencies);
