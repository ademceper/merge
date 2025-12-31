namespace Merge.Application.DTOs.International;

public class CategoryTranslationDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
