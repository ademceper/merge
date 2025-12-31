using Merge.Application.DTOs.Content;

namespace Merge.Application.Interfaces.Content;

public interface IPageBuilderService
{
    Task<PageBuilderDto> CreatePageAsync(CreatePageBuilderDto dto);
    Task<PageBuilderDto?> GetPageAsync(Guid id);
    Task<PageBuilderDto?> GetPageBySlugAsync(string slug);
    Task<IEnumerable<PageBuilderDto>> GetAllPagesAsync(string? status = null, int page = 1, int pageSize = 20);
    Task<bool> UpdatePageAsync(Guid id, UpdatePageBuilderDto dto);
    Task<bool> DeletePageAsync(Guid id);
    Task<bool> PublishPageAsync(Guid id);
    Task<bool> UnpublishPageAsync(Guid id);
}

