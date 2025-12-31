namespace Merge.Domain.Entities;

public class SearchHistory : BaseEntity
{
    public Guid? UserId { get; set; } // Nullable for anonymous users
    public string SearchTerm { get; set; } = string.Empty;
    public int ResultCount { get; set; }
    public bool ClickedResult { get; set; } = false;
    public Guid? ClickedProductId { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public Product? ClickedProduct { get; set; }
}

public class PopularSearch : BaseEntity
{
    public string SearchTerm { get; set; } = string.Empty;
    public int SearchCount { get; set; }
    public int ClickThroughCount { get; set; }
    public decimal ClickThroughRate { get; set; }
    public DateTime LastSearchedAt { get; set; }
}
