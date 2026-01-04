using Merge.Application.DTOs.Content;
using Merge.Application.Common;

namespace Merge.Application.Interfaces.Content;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
public interface IPageBuilderService
{
    Task<PageBuilderDto> CreatePageAsync(CreatePageBuilderDto dto, CancellationToken cancellationToken = default);
    Task<PageBuilderDto?> GetPageAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PageBuilderDto?> GetPageBySlugAsync(string slug, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    Task<PagedResult<PageBuilderDto>> GetAllPagesAsync(string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> UpdatePageAsync(Guid id, UpdatePageBuilderDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeletePageAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> PublishPageAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UnpublishPageAsync(Guid id, CancellationToken cancellationToken = default);
}

