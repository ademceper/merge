namespace Merge.Application.DTOs.International;

public class ConvertPriceDto
{
    public decimal Amount { get; set; }
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
}
