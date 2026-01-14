using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Search;
using Merge.Domain.Modules.Catalog;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Search;

public interface IElasticsearchService
{
    Task<bool> IndexProductAsync(ProductDto product, CancellationToken cancellationToken = default);
    Task<bool> IndexProductsAsync(IEnumerable<ProductDto> products, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<SearchResultDto> SearchAsync(SearchRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> ReindexAllProductsAsync(CancellationToken cancellationToken = default);
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}

