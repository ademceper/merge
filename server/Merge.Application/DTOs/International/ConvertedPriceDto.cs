namespace Merge.Application.DTOs.International;

public record ConvertedPriceDto(
    decimal OriginalAmount,
    string FromCurrency,
    decimal ConvertedAmount,
    string ToCurrency,
    string FormattedPrice,
    decimal ExchangeRate);
