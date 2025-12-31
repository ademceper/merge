namespace Merge.Application.DTOs.Search;

public class SearchSuggestionDto
{
    public string Term { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Product, Category, Brand
    public int Frequency { get; set; }
    public Guid? ReferenceId { get; set; }
}
