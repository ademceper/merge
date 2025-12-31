namespace Merge.Application.DTOs.International;

public class LanguageCoverageDto
{
    public string LanguageCode { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;
    public int ProductsTranslated { get; set; }
    public int TotalProducts { get; set; }
    public decimal CoveragePercentage { get; set; }
}
