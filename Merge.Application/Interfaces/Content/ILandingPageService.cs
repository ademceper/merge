using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;
using Merge.Application.Common;

namespace Merge.Application.Interfaces.Content;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
public interface ILandingPageService
{
    Task<LandingPageDto> CreateLandingPageAsync(Guid? authorId, CreateLandingPageDto dto, CancellationToken cancellationToken = default);
    Task<LandingPageDto?> GetLandingPageByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LandingPageDto?> GetLandingPageBySlugAsync(string slug, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    Task<PagedResult<LandingPageDto>> GetAllLandingPagesAsync(string? status = null, bool? isActive = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> UpdateLandingPageAsync(Guid id, CreateLandingPageDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteLandingPageAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> PublishLandingPageAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> TrackConversionAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LandingPageDto> CreateVariantAsync(Guid originalId, CreateLandingPageDto dto, CancellationToken cancellationToken = default);
    Task<LandingPageAnalyticsDto> GetLandingPageAnalyticsAsync(Guid id, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

