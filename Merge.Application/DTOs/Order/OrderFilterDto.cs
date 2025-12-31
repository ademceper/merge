namespace Merge.Application.DTOs.Order;

public class OrderFilterDto
{
    public Guid? UserId { get; set; }
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? OrderNumber { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } // Date, Amount, Status
    public bool SortDescending { get; set; } = true;
}
