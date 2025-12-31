namespace Merge.Application.DTOs.Logistics;

public class CalculateShippingDto
{
    public Guid OrderId { get; set; }
    public string Provider { get; set; } = string.Empty;
}
