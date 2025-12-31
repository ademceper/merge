namespace Merge.Application.DTOs.International;

public class CurrencyUsageDto
{
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencyName { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public decimal Percentage { get; set; }
}
