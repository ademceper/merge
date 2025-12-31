namespace Merge.Application.DTOs.Product;

public class SizeGuideEntryDto
{
    public Guid Id { get; set; }
    public string SizeLabel { get; set; } = string.Empty;
    public string? AlternativeLabel { get; set; }
    public decimal? Chest { get; set; }
    public decimal? Waist { get; set; }
    public decimal? Hips { get; set; }
    public decimal? Inseam { get; set; }
    public decimal? Shoulder { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public decimal? Weight { get; set; }
    public Dictionary<string, string>? AdditionalMeasurements { get; set; }
    public int DisplayOrder { get; set; }
}
