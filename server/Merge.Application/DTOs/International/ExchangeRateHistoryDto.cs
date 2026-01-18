namespace Merge.Application.DTOs.International;

public record ExchangeRateHistoryDto(
    Guid Id,
    string CurrencyCode,
    decimal ExchangeRate,
    DateTime RecordedAt,
    string Source);
