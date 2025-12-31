using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Interfaces.Content;

public interface ILandingPageService
{
    Task<LandingPageDto> CreateLandingPageAsync(Guid? authorId, CreateLandingPageDto dto);
    Task<LandingPageDto?> GetLandingPageByIdAsync(Guid id);
    Task<LandingPageDto?> GetLandingPageBySlugAsync(string slug);
    Task<IEnumerable<LandingPageDto>> GetAllLandingPagesAsync(string? status = null, bool? isActive = null);
    Task<bool> UpdateLandingPageAsync(Guid id, CreateLandingPageDto dto);
    Task<bool> DeleteLandingPageAsync(Guid id);
    Task<bool> PublishLandingPageAsync(Guid id);
    Task<bool> TrackConversionAsync(Guid id);
    Task<LandingPageDto> CreateVariantAsync(Guid originalId, CreateLandingPageDto dto);
    Task<LandingPageAnalyticsDto> GetLandingPageAnalyticsAsync(Guid id, DateTime? startDate = null, DateTime? endDate = null);
}

