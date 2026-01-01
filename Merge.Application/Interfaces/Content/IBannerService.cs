using Merge.Application.Common;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Interfaces.Content;

public interface IBannerService
{
    Task<BannerDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<BannerDto>> GetAllAsync();
    Task<PagedResult<BannerDto>> GetAllAsync(int page, int pageSize);
    Task<IEnumerable<BannerDto>> GetActiveBannersAsync(string? position = null);
    Task<PagedResult<BannerDto>> GetActiveBannersAsync(string? position, int page, int pageSize);
    Task<BannerDto> CreateAsync(CreateBannerDto dto);
    Task<BannerDto> UpdateAsync(Guid id, UpdateBannerDto dto);
    Task<bool> DeleteAsync(Guid id);
}

