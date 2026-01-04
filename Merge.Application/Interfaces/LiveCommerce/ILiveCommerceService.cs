using Merge.Application.DTOs.LiveCommerce;
using Merge.Application.Common;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.LiveCommerce;

public interface ILiveCommerceService
{
    Task<LiveStreamDto> CreateStreamAsync(CreateLiveStreamDto dto, CancellationToken cancellationToken = default);
    Task<LiveStreamDto?> GetStreamAsync(Guid streamId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LiveStreamDto>> GetStreamsAsync(Guid? sellerId = null, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    Task<PagedResult<LiveStreamDto>> GetActiveStreamsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    Task<PagedResult<LiveStreamDto>> GetStreamsBySellerAsync(Guid sellerId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<LiveStreamDto> UpdateStreamAsync(Guid streamId, CreateLiveStreamDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteStreamAsync(Guid streamId, CancellationToken cancellationToken = default);
    Task<bool> StartStreamAsync(Guid streamId, CancellationToken cancellationToken = default);
    Task<bool> EndStreamAsync(Guid streamId, CancellationToken cancellationToken = default);
    Task<LiveStreamDto?> AddProductToStreamAsync(Guid streamId, Guid productId, AddProductToStreamDto? dto = null, CancellationToken cancellationToken = default);
    Task<bool> ShowcaseProductAsync(Guid streamId, Guid productId, CancellationToken cancellationToken = default);
    Task<bool> JoinStreamAsync(Guid streamId, Guid? userId, string? guestId = null, CancellationToken cancellationToken = default);
    Task<bool> LeaveStreamAsync(Guid streamId, Guid? userId, string? guestId = null, CancellationToken cancellationToken = default);
    Task<LiveStreamOrderDto> CreateOrderFromStreamAsync(Guid streamId, Guid orderId, Guid? productId = null, CancellationToken cancellationToken = default);
    Task<LiveStreamStatsDto> GetStreamStatsAsync(Guid streamId, CancellationToken cancellationToken = default);
}

