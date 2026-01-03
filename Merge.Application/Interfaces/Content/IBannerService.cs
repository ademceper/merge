using Merge.Application.Common;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Interfaces.Content;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
public interface IBannerService
{
    Task<BannerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<BannerDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<BannerDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<BannerDto>> GetActiveBannersAsync(string? position = null, CancellationToken cancellationToken = default);
    Task<PagedResult<BannerDto>> GetActiveBannersAsync(string? position, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<BannerDto> CreateAsync(CreateBannerDto dto, CancellationToken cancellationToken = default);
    Task<BannerDto> UpdateAsync(Guid id, UpdateBannerDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

