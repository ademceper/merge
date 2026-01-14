namespace Merge.Application.DTOs.B2B;

public class WholesalePriceDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public int MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
