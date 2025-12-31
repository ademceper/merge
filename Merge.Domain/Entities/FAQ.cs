namespace Merge.Domain.Entities;

public class FAQ : BaseEntity
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string Category { get; set; } = "General"; // General, Orders, Payments, Shipping, Returns, etc.
    public int SortOrder { get; set; } = 0;
    public int ViewCount { get; set; } = 0;
    public bool IsPublished { get; set; } = true;
}

