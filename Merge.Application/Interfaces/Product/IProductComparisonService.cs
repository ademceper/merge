using Merge.Application.DTOs.Product;
using Merge.Application.Common;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
namespace Merge.Application.Interfaces.Product;

public interface IProductComparisonService
{
    Task<ProductComparisonDto> CreateComparisonAsync(Guid userId, CreateComparisonDto dto, CancellationToken cancellationToken = default);
    Task<ProductComparisonDto?> GetComparisonAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductComparisonDto?> GetUserComparisonAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductComparisonDto>> GetUserComparisonsAsync(Guid userId, bool savedOnly = false, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ProductComparisonDto?> GetComparisonByShareCodeAsync(string shareCode, CancellationToken cancellationToken = default);
    Task<ProductComparisonDto> AddProductToComparisonAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
    Task<bool> RemoveProductFromComparisonAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
    Task<bool> SaveComparisonAsync(Guid userId, string name, CancellationToken cancellationToken = default);
    Task<string> GenerateShareCodeAsync(Guid comparisonId, CancellationToken cancellationToken = default);
    Task<bool> ClearComparisonAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteComparisonAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<ComparisonMatrixDto> GetComparisonMatrixAsync(Guid comparisonId, CancellationToken cancellationToken = default);
}
