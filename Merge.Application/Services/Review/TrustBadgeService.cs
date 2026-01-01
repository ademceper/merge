using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReviewEntity = Merge.Domain.Entities.Review;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.Interfaces.Review;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text.Json;
using Merge.Application.DTOs.Review;


namespace Merge.Application.Services.Review;

public class TrustBadgeService : ITrustBadgeService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<TrustBadgeService> _logger;

    public TrustBadgeService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<TrustBadgeService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TrustBadgeDto> CreateBadgeAsync(CreateTrustBadgeDto dto)
    {
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

        await _context.Set<TrustBadge>().AddAsync(badge);
        await _unitOfWork.SaveChangesAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<TrustBadgeDto>(badge);
    }

    public async Task<TrustBadgeDto?> GetBadgeAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !b.IsDeleted (Global Query Filter)
        var badge = await _context.Set<TrustBadge>()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return badge != null ? _mapper.Map<TrustBadgeDto>(badge) : null;
    }

    public async Task<IEnumerable<TrustBadgeDto>> GetBadgesAsync(string? badgeType = null)
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
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<TrustBadgeDto>>(badges);
    }

    public async Task<TrustBadgeDto> UpdateBadgeAsync(Guid id, UpdateTrustBadgeDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !b.IsDeleted (Global Query Filter)
        var badge = await _context.Set<TrustBadge>()
            .FirstOrDefaultAsync(b => b.Id == id);

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
        await _unitOfWork.SaveChangesAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<TrustBadgeDto>(badge);
    }

    public async Task<bool> DeleteBadgeAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !b.IsDeleted (Global Query Filter)
        var badge = await _context.Set<TrustBadge>()
            .FirstOrDefaultAsync(b => b.Id == id);

        if (badge == null) return false;

        badge.IsDeleted = true;
        badge.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<SellerTrustBadgeDto> AwardSellerBadgeAsync(Guid sellerId, AwardBadgeDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !stb.IsDeleted (Global Query Filter)
        var existing = await _context.Set<SellerTrustBadge>()
            .FirstOrDefaultAsync(stb => stb.SellerId == sellerId && stb.TrustBadgeId == dto.BadgeId);

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

            await _context.Set<SellerTrustBadge>().AddAsync(sellerBadge);
        }

        await _unitOfWork.SaveChangesAsync();

        return await GetSellerBadgeDtoAsync(sellerId, dto.BadgeId);
    }

    public async Task<IEnumerable<SellerTrustBadgeDto>> GetSellerBadgesAsync(Guid sellerId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !stb.IsDeleted (Global Query Filter)
        var badges = await _context.Set<SellerTrustBadge>()
            .AsNoTracking()
            .Include(stb => stb.TrustBadge)
            .Include(stb => stb.Seller)
            .Where(stb => stb.SellerId == sellerId && stb.IsActive)
            .OrderBy(stb => stb.TrustBadge.DisplayOrder)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<SellerTrustBadgeDto>>(badges);
    }

    public async Task<bool> RevokeSellerBadgeAsync(Guid sellerId, Guid badgeId)
    {
        // ✅ PERFORMANCE: Removed manual !stb.IsDeleted (Global Query Filter)
        var badge = await _context.Set<SellerTrustBadge>()
            .FirstOrDefaultAsync(stb => stb.SellerId == sellerId && stb.TrustBadgeId == badgeId);

        if (badge == null) return false;

        badge.IsActive = false;
        badge.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<ProductTrustBadgeDto> AwardProductBadgeAsync(Guid productId, AwardBadgeDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !ptb.IsDeleted (Global Query Filter)
        var existing = await _context.Set<ProductTrustBadge>()
            .FirstOrDefaultAsync(ptb => ptb.ProductId == productId && ptb.TrustBadgeId == dto.BadgeId);

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

            await _context.Set<ProductTrustBadge>().AddAsync(productBadge);
        }

        await _unitOfWork.SaveChangesAsync();

        return await GetProductBadgeDtoAsync(productId, dto.BadgeId);
    }

    public async Task<IEnumerable<ProductTrustBadgeDto>> GetProductBadgesAsync(Guid productId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !ptb.IsDeleted (Global Query Filter)
        var badges = await _context.Set<ProductTrustBadge>()
            .AsNoTracking()
            .Include(ptb => ptb.TrustBadge)
            .Include(ptb => ptb.Product)
            .Where(ptb => ptb.ProductId == productId && ptb.IsActive)
            .OrderBy(ptb => ptb.TrustBadge.DisplayOrder)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<ProductTrustBadgeDto>>(badges);
    }

    public async Task<bool> RevokeProductBadgeAsync(Guid productId, Guid badgeId)
    {
        // ✅ PERFORMANCE: Removed manual !ptb.IsDeleted (Global Query Filter)
        var badge = await _context.Set<ProductTrustBadge>()
            .FirstOrDefaultAsync(ptb => ptb.ProductId == productId && ptb.TrustBadgeId == badgeId);

        if (badge == null) return false;

        badge.IsActive = false;
        badge.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task EvaluateAndAwardBadgesAsync(Guid? sellerId = null)
    {
        if (sellerId.HasValue)
        {
            await EvaluateSellerBadgesAsync(sellerId.Value);
        }
        else
        {
            // ✅ PERFORMANCE: AsNoTracking + Removed manual !sp.IsDeleted (Global Query Filter)
            var sellers = await _context.SellerProfiles
                .AsNoTracking()
                .Where(sp => sp.Status == "Approved")
                .Select(sp => sp.UserId)
                .ToListAsync();

            foreach (var seller in sellers)
            {
                await EvaluateSellerBadgesAsync(seller);
            }
        }
    }

    public async Task EvaluateSellerBadgesAsync(Guid sellerId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !b.IsDeleted (Global Query Filter)
        var badges = await _context.Set<TrustBadge>()
            .AsNoTracking()
            .Where(b => b.IsActive && b.BadgeType == "Seller")
            .ToListAsync();

        // ✅ PERFORMANCE: Removed manual !sp.IsDeleted (Global Query Filter)
        var seller = await _context.SellerProfiles
            .Include(sp => sp.User)
            .FirstOrDefaultAsync(sp => sp.UserId == sellerId);

        if (seller == null) return;

        // ✅ PERFORMANCE: Removed manual !o.IsDeleted, !r.IsDeleted (Global Query Filter)
        // Get seller metrics
        var totalOrders = await _context.Orders
            .AsNoTracking()
            .CountAsync(o => o.PaymentStatus == "Paid" &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId));

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var totalRevenue = await _context.OrderItems
            .AsNoTracking()
            .Where(oi => oi.Product.SellerId == sellerId &&
                  oi.Order.PaymentStatus == "Paid")
            .SumAsync(oi => oi.TotalPrice);

        var averageRating = seller.AverageRating;
        var totalReviews = await _context.Reviews
            .AsNoTracking()
            .CountAsync(r => r.IsApproved &&
                  r.Product.SellerId == sellerId);

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
                    .FirstOrDefaultAsync(stb => stb.SellerId == sellerId && stb.TrustBadgeId == badge.Id);

                if (existing == null)
                {
                    await AwardSellerBadgeAsync(sellerId, new AwardBadgeDto
                    {
                        BadgeId = badge.Id,
                        AwardReason = "Automatically awarded based on performance criteria"
                    });
                }
            }
        }
    }

    public async Task EvaluateProductBadgesAsync(Guid productId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !b.IsDeleted (Global Query Filter)
        var badges = await _context.Set<TrustBadge>()
            .AsNoTracking()
            .Where(b => b.IsActive && b.BadgeType == "Product")
            .ToListAsync();

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null) return;

        // ✅ PERFORMANCE: Removed manual !oi.Order.IsDeleted, !r.IsDeleted (Global Query Filter)
        // Get product metrics
        var totalSales = await _context.OrderItems
            .AsNoTracking()
            .Where(oi => oi.ProductId == productId && oi.Order.PaymentStatus == "Paid")
            .SumAsync(oi => oi.Quantity);

        var averageRating = product.Rating;
        var totalReviews = await _context.Reviews
            .AsNoTracking()
            .CountAsync(r => r.ProductId == productId && r.IsApproved);

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
                    .FirstOrDefaultAsync(ptb => ptb.ProductId == productId && ptb.TrustBadgeId == badge.Id);

                if (existing == null)
                {
                    await AwardProductBadgeAsync(productId, new AwardBadgeDto
                    {
                        BadgeId = badge.Id,
                        AwardReason = "Automatically awarded based on product performance"
                    });
                }
            }
        }
    }


    private async Task<SellerTrustBadgeDto> GetSellerBadgeDtoAsync(Guid sellerId, Guid badgeId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !stb.IsDeleted (Global Query Filter)
        var sellerBadge = await _context.Set<SellerTrustBadge>()
            .AsNoTracking()
            .Include(stb => stb.TrustBadge)
            .Include(stb => stb.Seller)
            .FirstOrDefaultAsync(stb => stb.SellerId == sellerId && stb.TrustBadgeId == badgeId);

        if (sellerBadge == null)
            throw new NotFoundException("Satıcı rozeti", badgeId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<SellerTrustBadgeDto>(sellerBadge);
    }

    private async Task<ProductTrustBadgeDto> GetProductBadgeDtoAsync(Guid productId, Guid badgeId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !ptb.IsDeleted (Global Query Filter)
        var productBadge = await _context.Set<ProductTrustBadge>()
            .AsNoTracking()
            .Include(ptb => ptb.TrustBadge)
            .Include(ptb => ptb.Product)
            .FirstOrDefaultAsync(ptb => ptb.ProductId == productId && ptb.TrustBadgeId == badgeId);

        if (productBadge == null)
            throw new NotFoundException("Ürün rozeti", badgeId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ProductTrustBadgeDto>(productBadge);
    }
}

