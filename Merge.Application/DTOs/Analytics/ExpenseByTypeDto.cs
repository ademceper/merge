namespace Merge.Application.DTOs.Analytics;

public class ExpenseByTypeDto
{
    public string ExpenseType { get; set; } = string.Empty; // Shipping, Commission, Refund, Discount, etc.
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}
