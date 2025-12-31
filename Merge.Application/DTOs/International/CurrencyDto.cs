namespace Merge.Application.DTOs.International;

public class CurrencyDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public bool IsBaseCurrency { get; set; }
    public bool IsActive { get; set; }
    public DateTime LastUpdated { get; set; }
    public int DecimalPlaces { get; set; }
    public string Format { get; set; } = string.Empty;
}
