using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Search;

namespace Merge.Application.Interfaces.Search;

public interface IElasticsearchService
{
    Task<bool> IndexProductAsync(ProductDto product);
    Task<bool> IndexProductsAsync(IEnumerable<ProductDto> products);
    Task<bool> DeleteProductAsync(Guid productId);
    Task<SearchResultDto> SearchAsync(SearchRequestDto request);
    Task<bool> ReindexAllProductsAsync();
    Task<bool> IsAvailableAsync();
}

