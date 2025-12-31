namespace Merge.Application.DTOs.Product;

public class SizeGuideDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string Type { get; set; } = string.Empty;
    public string MeasurementUnit { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<SizeGuideEntryDto> Entries { get; set; } = new();
}
