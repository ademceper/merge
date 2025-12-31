namespace Merge.Application.DTOs.Logistics;

public class DeliveryTimeEstimateResultDto
{
    public int MinDays { get; set; }
    public int MaxDays { get; set; }
    public int AverageDays { get; set; }
    public DateTime EstimatedDeliveryDate { get; set; }
    public string? EstimationSource { get; set; } // Product, Category, Warehouse, Default
}
