namespace Merge.Domain.Entities;

public class Banner : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string Position { get; set; } = "Homepage"; // Homepage, Category, Product, Cart, etc.
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ProductId { get; set; }
    
    // Navigation properties
    public Category? Category { get; set; }
    public Product? Product { get; set; }
}

