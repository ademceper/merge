namespace Merge.Application.DTOs.Logistics;

public class DeliveryTimeEstimationDto
{
    public Guid Id { get; set; }
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public Guid? ShippingProviderId { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public int MinDays { get; set; }
    public int MaxDays { get; set; }
    public int AverageDays { get; set; }
    public bool IsActive { get; set; }
    /// Typed DTO (Over-posting korumasi)
    public DeliveryTimeSettingsDto? Conditions { get; set; }
    public DateTime CreatedAt { get; set; }
}
