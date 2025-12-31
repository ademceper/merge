namespace Merge.Application.DTOs.Order;

public class ReturnRequestDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public decimal RefundAmount { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<Guid> OrderItemIds { get; set; } = new List<Guid>();
    public DateTime CreatedAt { get; set; }
}
