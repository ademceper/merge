namespace Merge.Application.DTOs.Analytics;

public class LandingPageVariantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Views { get; set; }
    public int Conversions { get; set; }
    public decimal ConversionRate { get; set; }
    public int TrafficSplit { get; set; }
}
