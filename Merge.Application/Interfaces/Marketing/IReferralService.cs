using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Marketing;

public interface IReferralService
{
    Task<ReferralCodeDto> GetMyReferralCodeAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ReferralCodeDto> CreateReferralCodeAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReferralDto>> GetMyReferralsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PagedResult<ReferralDto>> GetMyReferralsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> ApplyReferralCodeAsync(Guid newUserId, string code, CancellationToken cancellationToken = default);
    Task ProcessReferralRewardAsync(Guid referredUserId, Guid orderId, CancellationToken cancellationToken = default);
    Task<ReferralStatsDto> GetReferralStatsAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IReviewMediaService
{
    Task<ReviewMediaDto> AddMediaToReviewAsync(Guid reviewId, string url, string mediaType, string? thumbnailUrl = null);
    Task<IEnumerable<ReviewMediaDto>> GetReviewMediaAsync(Guid reviewId);
    Task DeleteReviewMediaAsync(Guid mediaId);
}

public interface ISharedWishlistService
{
    Task<SharedWishlistDto> CreateSharedWishlistAsync(Guid userId, CreateSharedWishlistDto dto);
    Task<SharedWishlistDto?> GetSharedWishlistByCodeAsync(string shareCode);
    Task<IEnumerable<SharedWishlistDto>> GetMySharedWishlistsAsync(Guid userId);
    Task DeleteSharedWishlistAsync(Guid wishlistId);
    Task MarkItemAsPurchasedAsync(Guid itemId, Guid purchasedBy);
}
