namespace Merge.Application.DTOs.Order;

public class OrderExportDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }
    public Guid? UserId { get; set; }
    public bool IncludeOrderItems { get; set; } = true;
    public bool IncludeAddress { get; set; } = true;
}
