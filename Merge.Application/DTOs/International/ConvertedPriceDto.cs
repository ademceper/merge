namespace Merge.Application.DTOs.International;

public class ConvertedPriceDto
{
    public decimal OriginalAmount { get; set; }
    public string FromCurrency { get; set; } = string.Empty;
    public decimal ConvertedAmount { get; set; }
    public string ToCurrency { get; set; } = string.Empty;
    public string FormattedPrice { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
}
