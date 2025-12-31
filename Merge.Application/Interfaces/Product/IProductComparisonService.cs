using Merge.Application.DTOs.Product;

namespace Merge.Application.Interfaces.Product;

public interface IProductComparisonService
{
    Task<ProductComparisonDto> CreateComparisonAsync(Guid userId, CreateComparisonDto dto);
    Task<ProductComparisonDto?> GetComparisonAsync(Guid id);
    Task<ProductComparisonDto?> GetUserComparisonAsync(Guid userId);
    Task<IEnumerable<ProductComparisonDto>> GetUserComparisonsAsync(Guid userId, bool savedOnly = false);
    Task<ProductComparisonDto?> GetComparisonByShareCodeAsync(string shareCode);
    Task<ProductComparisonDto> AddProductToComparisonAsync(Guid userId, Guid productId);
    Task<bool> RemoveProductFromComparisonAsync(Guid userId, Guid productId);
    Task<bool> SaveComparisonAsync(Guid userId, string name);
    Task<string> GenerateShareCodeAsync(Guid comparisonId);
    Task<bool> ClearComparisonAsync(Guid userId);
    Task<bool> DeleteComparisonAsync(Guid id, Guid userId);
    Task<ComparisonMatrixDto> GetComparisonMatrixAsync(Guid comparisonId);
}
