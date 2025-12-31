using Merge.Application.DTOs.LiveCommerce;
namespace Merge.Application.Interfaces.LiveCommerce;

public interface ILiveCommerceService
{
    Task<LiveStreamDto> CreateStreamAsync(CreateLiveStreamDto dto);
    Task<LiveStreamDto?> GetStreamAsync(Guid streamId);
    Task<IEnumerable<LiveStreamDto>> GetStreamsAsync(Guid? sellerId = null);
    Task<IEnumerable<LiveStreamDto>> GetActiveStreamsAsync();
    Task<IEnumerable<LiveStreamDto>> GetStreamsBySellerAsync(Guid sellerId);
    Task<LiveStreamDto> UpdateStreamAsync(Guid streamId, CreateLiveStreamDto dto);
    Task<bool> DeleteStreamAsync(Guid streamId);
    Task<bool> StartStreamAsync(Guid streamId);
    Task<bool> EndStreamAsync(Guid streamId);
    Task<LiveStreamDto> AddProductToStreamAsync(Guid streamId, Guid productId, AddProductToStreamDto? dto = null);
    Task<bool> ShowcaseProductAsync(Guid streamId, Guid productId);
    Task<bool> JoinStreamAsync(Guid streamId, Guid? userId, string? guestId = null);
    Task<bool> LeaveStreamAsync(Guid streamId, Guid? userId, string? guestId = null);
    Task<LiveStreamOrderDto> CreateOrderFromStreamAsync(Guid streamId, Guid orderId, Guid? productId = null);
    Task<LiveStreamStatsDto> GetStreamStatsAsync(Guid streamId);
}

