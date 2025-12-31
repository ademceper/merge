using Merge.Application.DTOs.Product;

namespace Merge.Application.Interfaces.Product;

public interface IProductTemplateService
{
    Task<ProductTemplateDto> CreateTemplateAsync(CreateProductTemplateDto dto);
    Task<ProductTemplateDto?> GetTemplateByIdAsync(Guid templateId);
    Task<IEnumerable<ProductTemplateDto>> GetAllTemplatesAsync(Guid? categoryId = null, bool? isActive = null);
    Task<bool> UpdateTemplateAsync(Guid templateId, UpdateProductTemplateDto dto);
    Task<bool> DeleteTemplateAsync(Guid templateId);
    Task<ProductDto> CreateProductFromTemplateAsync(CreateProductFromTemplateDto dto);
    Task<IEnumerable<ProductTemplateDto>> GetPopularTemplatesAsync(int limit = 10);
}

