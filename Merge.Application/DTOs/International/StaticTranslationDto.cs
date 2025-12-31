namespace Merge.Application.DTOs.International;

public class StaticTranslationDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
