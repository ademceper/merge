using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Interfaces.Product;

public interface IProductTemplateService
{
    Task<ProductTemplateDto> CreateTemplateAsync(CreateProductTemplateDto dto, CancellationToken cancellationToken = default);
    Task<ProductTemplateDto?> GetTemplateByIdAsync(Guid templateId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductTemplateDto>> GetAllTemplatesAsync(Guid? categoryId = null, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateTemplateAsync(Guid templateId, UpdateProductTemplateDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteTemplateAsync(Guid templateId, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateProductFromTemplateAsync(CreateProductFromTemplateDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductTemplateDto>> GetPopularTemplatesAsync(int limit = 10, CancellationToken cancellationToken = default);
}

