namespace Merge.Application.DTOs.International;

public class ExchangeRateHistoryDto
{
    public Guid Id { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public DateTime RecordedAt { get; set; }
    public string Source { get; set; } = string.Empty;
}
