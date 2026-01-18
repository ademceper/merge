using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Review;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Application.DTOs.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Services.Review;

public class TrustBadgeService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<TrustBadgeService> logger) : ITrustBadgeService
{

    public async Task<TrustBadgeDto> CreateBadgeAsync(CreateTrustBadgeDto dto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Trust badge oluşturuluyor. Name: {Name}, BadgeType: {BadgeType}",
            dto.Name, dto.BadgeType);

        var badge = TrustBadge.Create(
            dto.Name,
            dto.Description ?? string.Empty,
            dto.IconUrl ?? string.Empty,
            dto.BadgeType,
            dto.Criteria is not null ? JsonSerializer.Serialize(dto.Criteria) : string.Empty,
            dto.DisplayOrder,
            dto.Color);

        await context.Set<TrustBadge>().AddAsync(badge, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Trust badge oluşturuldu. BadgeId: {BadgeId}, Name: {Name}",
            badge.Id, badge.Name);

        return mapper.Map<TrustBadgeDto>(badge);
    }

    public async Task<TrustBadgeDto?> GetBadgeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var badge = await context.Set<TrustBadge>()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        return badge is not null ? mapper.Map<TrustBadgeDto>(badge) : null;
    }

    public async Task<IEnumerable<TrustBadgeDto>> GetBadgesAsync(string? badgeType = null, CancellationToken cancellationToken = default)
    {
        var query = context.Set<TrustBadge>()
            .AsNoTracking()
            .Where(b => b.IsActive);

        if (!string.IsNullOrEmpty(badgeType))
        {
            query = query.Where(b => b.BadgeType == badgeType);
        }

        var badges = await query
            .OrderBy(b => b.DisplayOrder)
            .ThenBy(b => b.Name)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<TrustBadgeDto>>(badges);
    }

    public async Task<TrustBadgeDto> UpdateBadgeAsync(Guid id, UpdateTrustBadgeDto dto, CancellationToken cancellationToken = default)
    {
        var badge = await context.Set<TrustBadge>()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (badge is null)
        {
            throw new NotFoundException("Rozet", id);
        }

        if (!string.IsNullOrEmpty(dto.Name))
            badge.UpdateName(dto.Name);
        if (!string.IsNullOrEmpty(dto.Description))
            badge.UpdateDescription(dto.Description);
        if (!string.IsNullOrEmpty(dto.IconUrl))
            badge.UpdateIconUrl(dto.IconUrl);
        if (!string.IsNullOrEmpty(dto.BadgeType))
            badge.UpdateBadgeType(dto.BadgeType);
        if (dto.Criteria is not null)
            badge.UpdateCriteria(JsonSerializer.Serialize(dto.Criteria));
        if (dto.IsActive.HasValue)
        {
            if (dto.IsActive.Value)
                badge.Activate();
            else
                badge.Deactivate();
        }
        if (dto.DisplayOrder.HasValue)
            badge.UpdateDisplayOrder(dto.DisplayOrder.Value);
        if (dto.Color is not null)
            badge.UpdateColor(dto.Color);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<TrustBadgeDto>(badge);
    }

    public async Task<bool> DeleteBadgeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var badge = await context.Set<TrustBadge>()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (badge is null) return false;

        badge.MarkAsDeleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<SellerTrustBadgeDto> AwardSellerBadgeAsync(Guid sellerId, AwardBadgeDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await context.Set<SellerTrustBadge>()
            .FirstOrDefaultAsync(stb => stb.SellerId == sellerId && stb.TrustBadgeId == dto.BadgeId, cancellationToken);

        if (existing is not null)
        {
            existing.Activate();
            existing.UpdateExpiryDate(dto.ExpiresAt);
            existing.UpdateAwardReason(dto.AwardReason);
        }
        else
        {
            var sellerBadge = SellerTrustBadge.Create(
                sellerId,
                dto.BadgeId,
                DateTime.UtcNow,
                dto.ExpiresAt,
                dto.AwardReason);

            await context.Set<SellerTrustBadge>().AddAsync(sellerBadge, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetSellerBadgeDtoAsync(sellerId, dto.BadgeId, cancellationToken);
    }

    public async Task<IEnumerable<SellerTrustBadgeDto>> GetSellerBadgesAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        var badges = await context.Set<SellerTrustBadge>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(stb => stb.TrustBadge)
            .Include(stb => stb.Seller)
            .Where(stb => stb.SellerId == sellerId && stb.IsActive)
            .OrderBy(stb => stb.TrustBadge.DisplayOrder)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<SellerTrustBadgeDto>>(badges);
    }

    public async Task<bool> RevokeSellerBadgeAsync(Guid sellerId, Guid badgeId, CancellationToken cancellationToken = default)
    {
        var badge = await context.Set<SellerTrustBadge>()
            .FirstOrDefaultAsync(stb => stb.SellerId == sellerId && stb.TrustBadgeId == badgeId, cancellationToken);

        if (badge is null) return false;

        badge.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<ProductTrustBadgeDto> AwardProductBadgeAsync(Guid productId, AwardBadgeDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await context.Set<ProductTrustBadge>()
            .FirstOrDefaultAsync(ptb => ptb.ProductId == productId && ptb.TrustBadgeId == dto.BadgeId, cancellationToken);

        if (existing is not null)
        {
            existing.Activate();
            existing.UpdateExpiryDate(dto.ExpiresAt);
            existing.UpdateAwardReason(dto.AwardReason);
        }
        else
        {
            var productBadge = ProductTrustBadge.Create(
                productId,
                dto.BadgeId,
                DateTime.UtcNow,
                dto.ExpiresAt,
                dto.AwardReason);

            await context.Set<ProductTrustBadge>().AddAsync(productBadge, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetProductBadgeDtoAsync(productId, dto.BadgeId, cancellationToken);
    }

    public async Task<IEnumerable<ProductTrustBadgeDto>> GetProductBadgesAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var badges = await context.Set<ProductTrustBadge>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(ptb => ptb.TrustBadge)
            .Include(ptb => ptb.Product)
            .Where(ptb => ptb.ProductId == productId && ptb.IsActive)
            .OrderBy(ptb => ptb.TrustBadge.DisplayOrder)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<ProductTrustBadgeDto>>(badges);
    }

    public async Task<bool> RevokeProductBadgeAsync(Guid productId, Guid badgeId, CancellationToken cancellationToken = default)
    {
        var badge = await context.Set<ProductTrustBadge>()
            .FirstOrDefaultAsync(ptb => ptb.ProductId == productId && ptb.TrustBadgeId == badgeId, cancellationToken);

        if (badge is null) return false;

        badge.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task EvaluateAndAwardBadgesAsync(Guid? sellerId = null, CancellationToken cancellationToken = default)
    {
        if (sellerId.HasValue)
        {
            await EvaluateSellerBadgesAsync(sellerId.Value, cancellationToken);
        }
        else
        {
            var sellers = await context.Set<SellerProfile>()
                .AsNoTracking()
                .Where(sp => sp.Status == SellerStatus.Approved)
                .Select(sp => sp.UserId)
                .ToListAsync(cancellationToken);

            foreach (var seller in sellers)
            {
                await EvaluateSellerBadgesAsync(seller, cancellationToken);
            }
        }
    }

    public async Task EvaluateSellerBadgesAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        var badges = await context.Set<TrustBadge>()
            .AsNoTracking()
            .Where(b => b.IsActive && b.BadgeType == "Seller")
            .ToListAsync(cancellationToken);

        var seller = await context.Set<SellerProfile>()
            .Include(sp => sp.User)
            .FirstOrDefaultAsync(sp => sp.UserId == sellerId, cancellationToken);

        if (seller is null) return;

        // Get seller metrics
        var totalOrders = await context.Set<OrderEntity>()
            .AsNoTracking()
            .CountAsync(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId), cancellationToken);

        var totalRevenue = await context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => oi.Product.SellerId == sellerId &&
                  oi.Order.PaymentStatus == PaymentStatus.Completed)
            .SumAsync(oi => oi.TotalPrice, cancellationToken);

        var averageRating = seller.AverageRating;
        var totalReviews = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .CountAsync(r => r.IsApproved &&
                  r.Product.SellerId == sellerId, cancellationToken);

        var daysSinceJoined = (DateTime.UtcNow - seller.CreatedAt).Days;

        foreach (var badge in badges)
        {
            if (string.IsNullOrEmpty(badge.Criteria)) continue;

            var criteria = JsonSerializer.Deserialize<Dictionary<string, object>>(badge.Criteria);
            if (criteria is null) continue;

            bool qualifies = true;

            // Check criteria
            if (criteria.ContainsKey("minOrders") && totalOrders < Convert.ToInt32(criteria["minOrders"]))
                qualifies = false;
            if (criteria.ContainsKey("minRevenue") && totalRevenue < Convert.ToDecimal(criteria["minRevenue"]))
                qualifies = false;
            if (criteria.ContainsKey("minRating") && averageRating < Convert.ToDecimal(criteria["minRating"]))
                qualifies = false;
            if (criteria.ContainsKey("minReviews") && totalReviews < Convert.ToInt32(criteria["minReviews"]))
                qualifies = false;
            if (criteria.ContainsKey("minDaysActive") && daysSinceJoined < Convert.ToInt32(criteria["minDaysActive"]))
                qualifies = false;

            if (qualifies)
            {
                var existing = await context.Set<SellerTrustBadge>()
                    .FirstOrDefaultAsync(stb => stb.SellerId == sellerId && stb.TrustBadgeId == badge.Id, cancellationToken);

                if (existing is null)
                {
                    await AwardSellerBadgeAsync(sellerId, new AwardBadgeDto
                    {
                        BadgeId = badge.Id,
                        AwardReason = "Automatically awarded based on performance criteria"
                    }, cancellationToken);
                }
            }
        }
    }

    public async Task EvaluateProductBadgesAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var badges = await context.Set<TrustBadge>()
            .AsNoTracking()
            .Where(b => b.IsActive && b.BadgeType == "Product")
            .ToListAsync(cancellationToken);

        var product = await context.Set<ProductEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product is null) return;

        // Get product metrics
        var totalSales = await context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => oi.ProductId == productId && oi.Order.PaymentStatus == PaymentStatus.Completed)
            .SumAsync(oi => oi.Quantity, cancellationToken);

        var averageRating = product.Rating;
        var totalReviews = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .CountAsync(r => r.ProductId == productId && r.IsApproved, cancellationToken);

        var daysSinceCreated = (DateTime.UtcNow - product.CreatedAt).Days;

        foreach (var badge in badges)
        {
            if (string.IsNullOrEmpty(badge.Criteria)) continue;

            var criteria = JsonSerializer.Deserialize<Dictionary<string, object>>(badge.Criteria);
            if (criteria is null) continue;

            bool qualifies = true;

            // Check criteria
            if (criteria.ContainsKey("minSales") && totalSales < Convert.ToInt32(criteria["minSales"]))
                qualifies = false;
            if (criteria.ContainsKey("minRating") && averageRating < Convert.ToDecimal(criteria["minRating"]))
                qualifies = false;
            if (criteria.ContainsKey("minReviews") && totalReviews < Convert.ToInt32(criteria["minReviews"]))
                qualifies = false;
            if (criteria.ContainsKey("minDaysActive") && daysSinceCreated < Convert.ToInt32(criteria["minDaysActive"]))
                qualifies = false;
            if (criteria.ContainsKey("minStock") && product.StockQuantity < Convert.ToInt32(criteria["minStock"]))
                qualifies = false;

            if (qualifies)
            {
                var existing = await context.Set<ProductTrustBadge>()
                    .FirstOrDefaultAsync(ptb => ptb.ProductId == productId && ptb.TrustBadgeId == badge.Id, cancellationToken);

                if (existing is null)
                {
                    await AwardProductBadgeAsync(productId, new AwardBadgeDto
                    {
                        BadgeId = badge.Id,
                        AwardReason = "Automatically awarded based on product performance"
                    }, cancellationToken);
                }
            }
        }
    }

    private async Task<SellerTrustBadgeDto> GetSellerBadgeDtoAsync(Guid sellerId, Guid badgeId, CancellationToken cancellationToken = default)
    {
        var sellerBadge = await context.Set<SellerTrustBadge>()
            .AsNoTracking()
            .Include(stb => stb.TrustBadge)
            .Include(stb => stb.Seller)
            .FirstOrDefaultAsync(stb => stb.SellerId == sellerId && stb.TrustBadgeId == badgeId, cancellationToken);

        if (sellerBadge is null)
            throw new NotFoundException("Satıcı rozeti", badgeId);

        return mapper.Map<SellerTrustBadgeDto>(sellerBadge);
    }

    private async Task<ProductTrustBadgeDto> GetProductBadgeDtoAsync(Guid productId, Guid badgeId, CancellationToken cancellationToken = default)
    {
        var productBadge = await context.Set<ProductTrustBadge>()
            .AsNoTracking()
            .Include(ptb => ptb.TrustBadge)
            .Include(ptb => ptb.Product)
            .FirstOrDefaultAsync(ptb => ptb.ProductId == productId && ptb.TrustBadgeId == badgeId, cancellationToken);

        if (productBadge is null)
            throw new NotFoundException("Ürün rozeti", badgeId);

        return mapper.Map<ProductTrustBadgeDto>(productBadge);
    }
}

