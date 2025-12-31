using Merge.Application.DTOs.Review;

namespace Merge.Application.Interfaces.Review;

public interface ITrustBadgeService
{
    // Badge Management
    Task<TrustBadgeDto> CreateBadgeAsync(CreateTrustBadgeDto dto);
    Task<TrustBadgeDto?> GetBadgeAsync(Guid id);
    Task<IEnumerable<TrustBadgeDto>> GetBadgesAsync(string? badgeType = null);
    Task<TrustBadgeDto> UpdateBadgeAsync(Guid id, UpdateTrustBadgeDto dto);
    Task<bool> DeleteBadgeAsync(Guid id);

    // Seller Badges
    Task<SellerTrustBadgeDto> AwardSellerBadgeAsync(Guid sellerId, AwardBadgeDto dto);
    Task<IEnumerable<SellerTrustBadgeDto>> GetSellerBadgesAsync(Guid sellerId);
    Task<bool> RevokeSellerBadgeAsync(Guid sellerId, Guid badgeId);

    // Product Badges
    Task<ProductTrustBadgeDto> AwardProductBadgeAsync(Guid productId, AwardBadgeDto dto);
    Task<IEnumerable<ProductTrustBadgeDto>> GetProductBadgesAsync(Guid productId);
    Task<bool> RevokeProductBadgeAsync(Guid productId, Guid badgeId);

    // Auto Award
    Task EvaluateAndAwardBadgesAsync(Guid? sellerId = null);
    Task EvaluateSellerBadgesAsync(Guid sellerId);
    Task EvaluateProductBadgesAsync(Guid productId);
}

