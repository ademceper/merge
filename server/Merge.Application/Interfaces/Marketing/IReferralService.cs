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

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
public interface IReviewMediaService
{
    Task<ReviewMediaDto> AddMediaToReviewAsync(Guid reviewId, string url, string mediaType, string? thumbnailUrl = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReviewMediaDto>> GetReviewMediaAsync(Guid reviewId, CancellationToken cancellationToken = default);
    Task DeleteReviewMediaAsync(Guid mediaId, CancellationToken cancellationToken = default);
}

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
public interface ISharedWishlistService
{
    Task<SharedWishlistDto> CreateSharedWishlistAsync(
        Guid userId, 
        CreateSharedWishlistDto dto,
        CancellationToken cancellationToken = default);
    Task<SharedWishlistDto?> GetSharedWishlistByCodeAsync(
        string shareCode,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<SharedWishlistDto>> GetMySharedWishlistsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    Task DeleteSharedWishlistAsync(
        Guid wishlistId,
        CancellationToken cancellationToken = default);
    Task MarkItemAsPurchasedAsync(
        Guid itemId, 
        Guid purchasedBy,
        CancellationToken cancellationToken = default);
}
