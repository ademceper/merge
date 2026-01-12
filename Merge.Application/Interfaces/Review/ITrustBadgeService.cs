using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Review;

public interface ITrustBadgeService
{
    // Badge Management
    Task<TrustBadgeDto> CreateBadgeAsync(CreateTrustBadgeDto dto, CancellationToken cancellationToken = default);
    Task<TrustBadgeDto?> GetBadgeAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TrustBadgeDto>> GetBadgesAsync(string? badgeType = null, CancellationToken cancellationToken = default);
    Task<TrustBadgeDto> UpdateBadgeAsync(Guid id, UpdateTrustBadgeDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteBadgeAsync(Guid id, CancellationToken cancellationToken = default);

    // Seller Badges
    Task<SellerTrustBadgeDto> AwardSellerBadgeAsync(Guid sellerId, AwardBadgeDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<SellerTrustBadgeDto>> GetSellerBadgesAsync(Guid sellerId, CancellationToken cancellationToken = default);
    Task<bool> RevokeSellerBadgeAsync(Guid sellerId, Guid badgeId, CancellationToken cancellationToken = default);

    // Product Badges
    Task<ProductTrustBadgeDto> AwardProductBadgeAsync(Guid productId, AwardBadgeDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductTrustBadgeDto>> GetProductBadgesAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<bool> RevokeProductBadgeAsync(Guid productId, Guid badgeId, CancellationToken cancellationToken = default);

    // Auto Award
    Task EvaluateAndAwardBadgesAsync(Guid? sellerId = null, CancellationToken cancellationToken = default);
    Task EvaluateSellerBadgesAsync(Guid sellerId, CancellationToken cancellationToken = default);
    Task EvaluateProductBadgesAsync(Guid productId, CancellationToken cancellationToken = default);
}

