namespace Merge.Application.DTOs.Logistics;

public class EstimateDeliveryTimeDto
{
    public Guid? ProductId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? WarehouseId { get; set; }
    public Guid? ShippingProviderId { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
}
