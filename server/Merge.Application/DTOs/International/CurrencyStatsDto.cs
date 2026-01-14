namespace Merge.Application.DTOs.International;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record CurrencyStatsDto(
    int TotalCurrencies,
    int ActiveCurrencies,
    string BaseCurrency,
    DateTime LastRateUpdate,
    IReadOnlyList<CurrencyUsageDto> MostUsedCurrencies);
