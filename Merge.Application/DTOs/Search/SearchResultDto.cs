using Merge.Application.DTOs.Product;

namespace Merge.Application.DTOs.Search;

public class SearchResultDto
{
    public List<ProductDto> Products { get; set; } = new List<ProductDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public List<string> AvailableBrands { get; set; } = new List<string>();
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
}
