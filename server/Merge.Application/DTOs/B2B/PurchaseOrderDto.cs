namespace Merge.Application.DTOs.B2B;

public class PurchaseOrderDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public Guid? B2BUserId { get; set; }
    public string? B2BUserName { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public Guid? CreditTermId { get; set; }
    public string? CreditTermName { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
