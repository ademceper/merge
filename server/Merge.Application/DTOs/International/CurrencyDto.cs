namespace Merge.Application.DTOs.International;

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
