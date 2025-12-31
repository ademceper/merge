using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Interfaces.Marketing;

public interface IReferralService
{
    Task<ReferralCodeDto> GetMyReferralCodeAsync(Guid userId);
    Task<ReferralCodeDto> CreateReferralCodeAsync(Guid userId);
    Task<IEnumerable<ReferralDto>> GetMyReferralsAsync(Guid userId);
    Task<bool> ApplyReferralCodeAsync(Guid newUserId, string code);
    Task ProcessReferralRewardAsync(Guid referredUserId, Guid orderId);
    Task<ReferralStatsDto> GetReferralStatsAsync(Guid userId);
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
