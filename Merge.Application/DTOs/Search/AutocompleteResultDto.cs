namespace Merge.Application.DTOs.Search;

public class AutocompleteResultDto
{
    public List<ProductSuggestionDto> Products { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public List<string> Brands { get; set; } = new();
    public List<string> PopularSearches { get; set; } = new();
}
