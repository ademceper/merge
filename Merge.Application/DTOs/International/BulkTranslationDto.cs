namespace Merge.Application.DTOs.International;

public class BulkTranslationDto
{
    public string LanguageCode { get; set; } = string.Empty;
    public Dictionary<string, string> Translations { get; set; } = new();
}
