using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReviewEntity = Merge.Domain.Entities.Review;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Review;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Entities.Order;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Application.DTOs.Review;


namespace Merge.Application.Services.Review;

public class TrustBadgeService : ITrustBadgeService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<TrustBadgeService> _logger;

    public TrustBadgeService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<TrustBadgeService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<TrustBadgeDto> CreateBadgeAsync(CreateTrustBadgeDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Trust badge oluşturuluyor. Name: {Name}, BadgeType: {BadgeType}",
            dto.Name, dto.BadgeType);

        var badge = new TrustBadge
        {
            Name = dto.Name,
            Description = dto.Description,
            IconUrl = dto.IconUrl,
            BadgeType = dto.BadgeType,
            Criteria = dto.Criteria != null ? JsonSerializer.Serialize(dto.Criteria) : string.Empty,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder,
            Color = dto.Color
        };

        await _context.Set<TrustBadge>().AddAsync(badge, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Trust badge oluşturuldu. BadgeId: {BadgeId}, Name: {Name}",
            badge.Id, badge.Name);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<TrustBadgeDto>(badge);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<TrustBadgeDto?> GetBadgeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !b.IsDeleted (Global Query Filter)
        var badge = await _context.Set<TrustBadge>()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return badge != null ? _mapper.Map<TrustBadgeDto>(badge) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<TrustBadgeDto>> GetBadgesAsync(string? badgeType = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !b.IsDeleted (Global Query Filter)
        var query = _context.Set<TrustBadge>()
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

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<TrustBadgeDto>>(badges);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<TrustBadgeDto> UpdateBadgeAsync(Guid id, UpdateTrustBadgeDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !b.IsDeleted (Global Query Filter)
        var badge = await _context.Set<TrustBadge>()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (badge == null)
        {
            throw new NotFoundException("Rozet", id);
        }

        if (!string.IsNullOrEmpty(dto.Name))
            badge.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.Description))
            badge.Description = dto.Description;
        if (!string.IsNullOrEmpty(dto.IconUrl))
            badge.IconUrl = dto.IconUrl;
        if (!string.IsNullOrEmpty(dto.BadgeType))
            badge.BadgeType = dto.BadgeType;
        if (dto.Criteria != null)
            badge.Criteria = JsonSerializer.Serialize(dto.Criteria);
        if (dto.IsActive.HasValue)
            badge.IsActive = dto.IsActive.Value;
        if (dto.DisplayOrder.HasValue)
            badge.DisplayOrder = dto.DisplayOrder.Value;
        if (dto.Color != null)
            badge.Color = dto.Color;

        badge.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<TrustBadgeDto>(badge);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteBadgeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !b.IsDeleted (Global Query Filter)
        var badge = await _context.Set<TrustBadge>()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (badge == null) return false;

        badge.IsDeleted = true;
        badge.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SellerTrustBadgeDto> AwardSellerBadgeAsync(Guid sellerId, AwardBadgeDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !stb.IsDeleted (Global Query Filter)
        var existing = await _context.Set<SellerTrustBadge>()
            .FirstOrDefaultAsync(stb => stb.SellerId == sellerId && stb.TrustBadgeId == dto.BadgeId, cancellationToken);

        if (existing != null)
        {
            existing.IsActive = true;
            existing.AwardedAt = DateTime.UtcNow;
            existing.ExpiresAt = dto.ExpiresAt;
            existing.AwardReason = dto.AwardReason;
        }
        else
        {
            var sellerBadge = new SellerTrustBadge
            {
                SellerId = sellerId,
                TrustBadgeId = dto.BadgeId,
                AwardedAt = DateTime.UtcNow,
                ExpiresAt = dto.ExpiresAt,
                IsActive = true,
                AwardReason = dto.AwardReason
            };

            await _context.Set<SellerTrustBadge>().AddAsync(sellerBadge, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetSellerBadgeDtoAsync(sellerId, dto.BadgeId, cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<SellerTrustBadgeDto>> GetSellerBadgesAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !stb.IsDeleted (Global Query Filter)
        var badges = await _context.Set<SellerTrustBadge>()
            .AsNoTracking()
            .Include(stb => stb.TrustBadge)
            .Include(stb => stb.Seller)
            .Where(stb => stb.SellerId == sellerId && stb.IsActive)
            .OrderBy(stb => stb.TrustBadge.DisplayOrder)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<SellerTrustBadgeDto>>(badges);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> RevokeSellerBadgeAsync(Guid sellerId, Guid badgeId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !stb.IsDeleted (Global Query Filter)
        var badge = await _context.Set<SellerTrustBadge>()
            .FirstOrDefaultAsync(stb => stb.SellerId == sellerId && stb.TrustBadgeId == badgeId, cancellationToken);

        if (badge == null) return false;

        badge.IsActive = false;
        badge.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ProductTrustBadgeDto> AwardProductBadgeAsync(Guid productId, AwardBadgeDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !ptb.IsDeleted (Global Query Filter)
        var existing = await _context.Set<ProductTrustBadge>()
            .FirstOrDefaultAsync(ptb => ptb.ProductId == productId && ptb.TrustBadgeId == dto.BadgeId, cancellationToken);

        if (existing != null)
        {
            existing.IsActive = true;
            existing.AwardedAt = DateTime.UtcNow;
            existing.ExpiresAt = dto.ExpiresAt;
            existing.AwardReason = dto.AwardReason;
        }
        else
        {
            var productBadge = new ProductTrustBadge
            {
                ProductId = productId,
                TrustBadgeId = dto.BadgeId,
                AwardedAt = DateTime.UtcNow,
                ExpiresAt = dto.ExpiresAt,
                IsActive = true,
                AwardReason = dto.AwardReason
            };

            await _context.Set<ProductTrustBadge>().AddAsync(productBadge, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetProductBadgeDtoAsync(productId, dto.BadgeId, cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ProductTrustBadgeDto>> GetProductBadgesAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !ptb.IsDeleted (Global Query Filter)
        var badges = await _context.Set<ProductTrustBadge>()
            .AsNoTracking()
            .Include(ptb => ptb.TrustBadge)
            .Include(ptb => ptb.Product)
            .Where(ptb => ptb.ProductId == productId && ptb.IsActive)
            .OrderBy(ptb => ptb.TrustBadge.DisplayOrder)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<ProductTrustBadgeDto>>(badges);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> RevokeProductBadgeAsync(Guid productId, Guid badgeId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !ptb.IsDeleted (Global Query Filter)
        var badge = await _context.Set<ProductTrustBadge>()
            .FirstOrDefaultAsync(ptb => ptb.ProductId == productId && ptb.TrustBadgeId == badgeId, cancellationToken);

        if (badge == null) return false;

        badge.IsActive = false;
        badge.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task EvaluateAndAwardBadgesAsync(Guid? sellerId = null, CancellationToken cancellationToken = default)
    {
        if (sellerId.HasValue)
        {
            await EvaluateSellerBadgesAsync(sellerId.Value, cancellationToken);
        }
        else
        {
            // ✅ PERFORMANCE: AsNoTracking + Removed manual !sp.IsDeleted (Global Query Filter)
            var sellers = await _context.Set<SellerProfile>()
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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task EvaluateSellerBadgesAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !b.IsDeleted (Global Query Filter)
        var badges = await _context.Set<TrustBadge>()
            .AsNoTracking()
            .Where(b => b.IsActive && b.BadgeType == "Seller")
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Removed manual !sp.IsDeleted (Global Query Filter)
        var seller = await _context.Set<SellerProfile>()
            .Include(sp => sp.User)
            .FirstOrDefaultAsync(sp => sp.UserId == sellerId, cancellationToken);

        if (seller == null) return;

        // ✅ PERFORMANCE: Removed manual !o.IsDeleted, !r.IsDeleted (Global Query Filter)
        // Get seller metrics
        var totalOrders = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .CountAsync(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId), cancellationToken);

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var totalRevenue = await _context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => oi.Product.SellerId == sellerId &&
                  oi.Order.PaymentStatus == PaymentStatus.Completed)
            .SumAsync(oi => oi.TotalPrice, cancellationToken);

        var averageRating = seller.AverageRating;
        var totalReviews = await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .CountAsync(r => r.IsApproved &&
                  r.Product.SellerId == sellerId, cancellationToken);

        var daysSinceJoined = (DateTime.UtcNow - seller.CreatedAt).Days;

        foreach (var badge in badges)
        {
            if (string.IsNullOrEmpty(badge.Criteria)) continue;

            var criteria = JsonSerializer.Deserialize<Dictionary<string, object>>(badge.Criteria);
            if (criteria == null) continue;

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
                // ✅ PERFORMANCE: Removed manual !stb.IsDeleted (Global Query Filter)
                var existing = await _context.Set<SellerTrustBadge>()
                    .FirstOrDefaultAsync(stb => stb.SellerId == sellerId && stb.TrustBadgeId == badge.Id, cancellationToken);

                if (existing == null)
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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task EvaluateProductBadgesAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !b.IsDeleted (Global Query Filter)
        var badges = await _context.Set<TrustBadge>()
            .AsNoTracking()
            .Where(b => b.IsActive && b.BadgeType == "Product")
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var product = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null) return;

        // ✅ PERFORMANCE: Removed manual !oi.Order.IsDeleted, !r.IsDeleted (Global Query Filter)
        // Get product metrics
        var totalSales = await _context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => oi.ProductId == productId && oi.Order.PaymentStatus == PaymentStatus.Completed)
            .SumAsync(oi => oi.Quantity, cancellationToken);

        var averageRating = product.Rating;
        var totalReviews = await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .CountAsync(r => r.ProductId == productId && r.IsApproved, cancellationToken);

        var daysSinceCreated = (DateTime.UtcNow - product.CreatedAt).Days;

        foreach (var badge in badges)
        {
            if (string.IsNullOrEmpty(badge.Criteria)) continue;

            var criteria = JsonSerializer.Deserialize<Dictionary<string, object>>(badge.Criteria);
            if (criteria == null) continue;

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
                // ✅ PERFORMANCE: Removed manual !ptb.IsDeleted (Global Query Filter)
                var existing = await _context.Set<ProductTrustBadge>()
                    .FirstOrDefaultAsync(ptb => ptb.ProductId == productId && ptb.TrustBadgeId == badge.Id, cancellationToken);

                if (existing == null)
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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task<SellerTrustBadgeDto> GetSellerBadgeDtoAsync(Guid sellerId, Guid badgeId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !stb.IsDeleted (Global Query Filter)
        var sellerBadge = await _context.Set<SellerTrustBadge>()
            .AsNoTracking()
            .Include(stb => stb.TrustBadge)
            .Include(stb => stb.Seller)
            .FirstOrDefaultAsync(stb => stb.SellerId == sellerId && stb.TrustBadgeId == badgeId, cancellationToken);

        if (sellerBadge == null)
            throw new NotFoundException("Satıcı rozeti", badgeId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<SellerTrustBadgeDto>(sellerBadge);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task<ProductTrustBadgeDto> GetProductBadgeDtoAsync(Guid productId, Guid badgeId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !ptb.IsDeleted (Global Query Filter)
        var productBadge = await _context.Set<ProductTrustBadge>()
            .AsNoTracking()
            .Include(ptb => ptb.TrustBadge)
            .Include(ptb => ptb.Product)
            .FirstOrDefaultAsync(ptb => ptb.ProductId == productId && ptb.TrustBadgeId == badgeId, cancellationToken);

        if (productBadge == null)
            throw new NotFoundException("Ürün rozeti", badgeId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ProductTrustBadgeDto>(productBadge);
    }
}

