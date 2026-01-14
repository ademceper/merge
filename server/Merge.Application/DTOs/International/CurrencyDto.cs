namespace Merge.Application.DTOs.International;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record CurrencyDto(
    Guid Id,
    string Code,
    string Name,
    string Symbol,
    decimal ExchangeRate,
    bool IsBaseCurrency,
    bool IsActive,
    DateTime LastUpdated,
    int DecimalPlaces,
    string Format);
