namespace Merge.Application.DTOs.International;

public class TranslationStatsDto
{
    public int TotalLanguages { get; set; }
    public int ActiveLanguages { get; set; }
    public string DefaultLanguage { get; set; } = string.Empty;
    public List<LanguageCoverageDto> LanguageCoverage { get; set; } = new();
}
