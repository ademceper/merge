namespace Merge.Application.DTOs.Logistics;

public class ShippingProviderDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal BaseCost { get; set; }
    public int EstimatedDays { get; set; }
}
