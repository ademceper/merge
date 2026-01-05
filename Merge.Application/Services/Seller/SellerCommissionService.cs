using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Services.Notification;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using UserEntity = Merge.Domain.Entities.User;
using OrderEntity = Merge.Domain.Entities.Order;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.Interfaces.Seller;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Application.DTOs.Seller;
using Merge.Application.Common;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
namespace Merge.Application.Services.Seller;

public class SellerCommissionService : ISellerCommissionService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly ILogger<SellerCommissionService> _logger;

    public SellerCommissionService(IDbContext context, IUnitOfWork unitOfWork, IEmailService emailService, IMapper mapper, ILogger<SellerCommissionService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SellerCommissionDto> CalculateAndRecordCommissionAsync(Guid orderId, Guid orderItemId, CancellationToken cancellationToken = default)
    {
        var orderItem = await _context.Set<OrderItem>()
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId && oi.OrderId == orderId, cancellationToken);

        if (orderItem == null)
        {
            throw new NotFoundException("Sipariş kalemi", orderItemId);
        }

        if (!orderItem.Product.SellerId.HasValue)
        {
            throw new BusinessException("Ürüne atanmış satıcı yok.");
        }

        var sellerId = orderItem.Product.SellerId.Value;

        // ✅ PERFORMANCE: Removed manual !sc.IsDeleted (Global Query Filter)
        // Check if commission already exists
        var existing = await _context.Set<SellerCommission>()
            .AsNoTracking()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem)
            .FirstOrDefaultAsync(sc => sc.OrderItemId == orderItemId, cancellationToken);

        if (existing != null)
        {
            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return _mapper.Map<SellerCommissionDto>(existing);
        }

        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        // Get seller settings
        var settings = await _context.Set<SellerCommissionSettings>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SellerId == sellerId, cancellationToken);

        decimal commissionRate;
        decimal platformFeeRate = 0;

        if (settings != null && settings.UseCustomRate)
        {
            commissionRate = settings.CustomCommissionRate;
        }
        else
        {
            // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
            // Calculate total sales for tier determination
            var totalSales = await _context.Set<OrderEntity>()
                .Where(o => o.OrderItems.Any(i => i.Product.SellerId == sellerId) && o.PaymentStatus == PaymentStatus.Completed)
                .SumAsync(o => o.TotalAmount, cancellationToken);

            var tier = await GetTierForSalesAsync(totalSales, cancellationToken);
            if (tier != null)
            {
                commissionRate = tier.CommissionRate;
                platformFeeRate = tier.PlatformFeeRate;
            }
            else
            {
                commissionRate = 10; // Default 10%
                platformFeeRate = 2; // Default 2%
            }
        }

        var orderAmount = orderItem.TotalPrice;
        var commissionAmount = orderAmount * (commissionRate / 100);
        var platformFee = orderAmount * (platformFeeRate / 100);
        var netAmount = commissionAmount - platformFee;

        var commission = new SellerCommission
        {
            SellerId = sellerId,
            OrderId = orderId,
            OrderItemId = orderItemId,
            OrderAmount = orderAmount,
            CommissionRate = commissionRate,
            CommissionAmount = commissionAmount,
            PlatformFee = platformFee,
            NetAmount = netAmount,
            Status = CommissionStatus.Pending
        };

        await _context.Set<SellerCommission>().AddAsync(commission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        commission = await _context.Set<SellerCommission>()
            .AsNoTracking()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem)
            .FirstOrDefaultAsync(sc => sc.Id == commission.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<SellerCommissionDto>(commission!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SellerCommissionDto?> GetCommissionAsync(Guid commissionId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sc.IsDeleted (Global Query Filter)
        var commission = await _context.Set<SellerCommission>()
            .AsNoTracking()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem)
            .FirstOrDefaultAsync(sc => sc.Id == commissionId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return commission != null ? _mapper.Map<SellerCommissionDto>(commission) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<SellerCommissionDto>> GetSellerCommissionsAsync(Guid sellerId, string? status = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sc.IsDeleted (Global Query Filter)
        IQueryable<SellerCommission> query = _context.Set<SellerCommission>()
            .AsNoTracking()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem)
            .Where(sc => sc.SellerId == sellerId);

        if (!string.IsNullOrEmpty(status))
        {
            var commissionStatus = Enum.Parse<CommissionStatus>(status, true);
            query = query.Where(sc => sc.Status == commissionStatus);
        }

        var commissions = await query
            .OrderByDescending(sc => sc.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<SellerCommissionDto>>(commissions);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<SellerCommissionDto>> GetAllCommissionsAsync(string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sc.IsDeleted (Global Query Filter)
        IQueryable<SellerCommission> query = _context.Set<SellerCommission>()
            .AsNoTracking()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem);

        if (!string.IsNullOrEmpty(status))
        {
            var commissionStatus = Enum.Parse<CommissionStatus>(status, true);
            query = query.Where(sc => sc.Status == commissionStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var commissions = await query
            .OrderByDescending(sc => sc.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var commissionDtos = _mapper.Map<IEnumerable<SellerCommissionDto>>(commissions).ToList();

        return new PagedResult<SellerCommissionDto>
        {
            Items = commissionDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> ApproveCommissionAsync(Guid commissionId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !sc.IsDeleted (Global Query Filter)
        var commission = await _context.Set<SellerCommission>()
            .FirstOrDefaultAsync(sc => sc.Id == commissionId, cancellationToken);

        if (commission == null) return false;

        commission.Status = CommissionStatus.Approved;
        commission.ApprovedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> CancelCommissionAsync(Guid commissionId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !sc.IsDeleted (Global Query Filter)
        var commission = await _context.Set<SellerCommission>()
            .FirstOrDefaultAsync(sc => sc.Id == commissionId, cancellationToken);

        if (commission == null) return false;

        commission.Status = CommissionStatus.Cancelled;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<CommissionTierDto> CreateTierAsync(CreateCommissionTierDto dto, CancellationToken cancellationToken = default)
    {
        var tier = new CommissionTier
        {
            Name = dto.Name,
            MinSales = dto.MinSales,
            MaxSales = dto.MaxSales,
            CommissionRate = dto.CommissionRate,
            PlatformFeeRate = dto.PlatformFeeRate,
            Priority = dto.Priority
        };

        await _context.Set<CommissionTier>().AddAsync(tier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CommissionTierDto
        {
            Id = tier.Id,
            Name = tier.Name,
            MinSales = tier.MinSales,
            MaxSales = tier.MaxSales,
            CommissionRate = tier.CommissionRate,
            PlatformFeeRate = tier.PlatformFeeRate,
            IsActive = tier.IsActive,
            Priority = tier.Priority
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<CommissionTierDto>> GetAllTiersAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var tiers = await _context.Set<CommissionTier>()
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Priority)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<CommissionTierDto>>(tiers);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<CommissionTierDto?> GetTierForSalesAsync(decimal totalSales, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var tier = await _context.Set<CommissionTier>()
            .AsNoTracking()
            .Where(t => t.IsActive && t.MinSales <= totalSales && t.MaxSales >= totalSales)
            .OrderBy(t => t.Priority)
            .FirstOrDefaultAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return tier != null ? _mapper.Map<CommissionTierDto>(tier) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> UpdateTierAsync(Guid tierId, CreateCommissionTierDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var tier = await _context.Set<CommissionTier>()
            .FirstOrDefaultAsync(t => t.Id == tierId, cancellationToken);

        if (tier == null) return false;

        tier.Name = dto.Name;
        tier.MinSales = dto.MinSales;
        tier.MaxSales = dto.MaxSales;
        tier.CommissionRate = dto.CommissionRate;
        tier.PlatformFeeRate = dto.PlatformFeeRate;
        tier.Priority = dto.Priority;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteTierAsync(Guid tierId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var tier = await _context.Set<CommissionTier>()
            .FirstOrDefaultAsync(t => t.Id == tierId, cancellationToken);

        if (tier == null) return false;

        tier.IsDeleted = true;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SellerCommissionSettingsDto?> GetSellerSettingsAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var settings = await _context.Set<SellerCommissionSettings>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SellerId == sellerId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return settings != null ? _mapper.Map<SellerCommissionSettingsDto>(settings) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SellerCommissionSettingsDto> UpdateSellerSettingsAsync(Guid sellerId, UpdateCommissionSettingsDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var settings = await _context.Set<SellerCommissionSettings>()
            .FirstOrDefaultAsync(s => s.SellerId == sellerId, cancellationToken);

        if (settings == null)
        {
            settings = new SellerCommissionSettings
            {
                SellerId = sellerId
            };
            await _context.Set<SellerCommissionSettings>().AddAsync(settings, cancellationToken);
        }

        if (dto.CustomCommissionRate.HasValue)
            settings.CustomCommissionRate = dto.CustomCommissionRate.Value;

        if (dto.UseCustomRate.HasValue)
            settings.UseCustomRate = dto.UseCustomRate.Value;

        if (dto.MinimumPayoutAmount.HasValue)
            settings.MinimumPayoutAmount = dto.MinimumPayoutAmount.Value;

        if (dto.PaymentMethod != null)
            settings.PaymentMethod = dto.PaymentMethod;

        if (dto.PaymentDetails != null)
            settings.PaymentDetails = dto.PaymentDetails;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<SellerCommissionSettingsDto>(settings);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<CommissionPayoutDto> RequestPayoutAsync(Guid sellerId, RequestPayoutDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !sc.IsDeleted (Global Query Filter)
        var commissions = await _context.Set<SellerCommission>()
            .AsNoTracking()
            .Where(sc => dto.CommissionIds.Contains(sc.Id) && sc.SellerId == sellerId)
            .Where(sc => sc.Status == CommissionStatus.Approved)
            .ToListAsync(cancellationToken);

        if (commissions.Count == 0)
        {
            throw new BusinessException("Onaylanmış komisyon bulunamadı.");
        }

        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var settings = await _context.Set<SellerCommissionSettings>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SellerId == sellerId, cancellationToken);

        // ✅ PERFORMANCE: Database'de sum yap (memory'de işlem YASAK)
        var totalAmount = await _context.Set<SellerCommission>()
            .AsNoTracking()
            .Where(sc => dto.CommissionIds.Contains(sc.Id) && sc.SellerId == sellerId && sc.Status == CommissionStatus.Approved)
            .SumAsync(c => c.NetAmount, cancellationToken);

        if (settings != null && totalAmount < settings.MinimumPayoutAmount)
        {
            throw new ValidationException($"Minimum ödeme tutarı {settings.MinimumPayoutAmount}.");
        }

        var transactionFee = totalAmount * 0.01m; // 1% transaction fee
        var netAmount = totalAmount - transactionFee;

        var payoutNumber = await GeneratePayoutNumberAsync(cancellationToken);

        var payout = new CommissionPayout
        {
            SellerId = sellerId,
            PayoutNumber = payoutNumber,
            TotalAmount = totalAmount,
            TransactionFee = transactionFee,
            NetAmount = netAmount,
            Status = PayoutStatus.Pending,
            PaymentMethod = settings?.PaymentMethod ?? "Bank Transfer",
            PaymentDetails = settings?.PaymentDetails
        };

        await _context.Set<CommissionPayout>().AddAsync(payout, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var commission in commissions)
        {
            var item = new CommissionPayoutItem
            {
                PayoutId = payout.Id,
                CommissionId = commission.Id
            };

            await _context.Set<CommissionPayoutItem>().AddAsync(item, cancellationToken);

            commission.Status = CommissionStatus.Paid;
            commission.PaidAt = DateTime.UtcNow;
            commission.PaymentReference = payoutNumber;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        payout = await _context.Set<CommissionPayout>()
            .AsNoTracking()
            .Include(p => p.Seller)
            .Include(p => p.Items)
                .ThenInclude(i => i.Commission)
                    .ThenInclude(c => c.Order)
            .FirstOrDefaultAsync(p => p.Id == payout.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<CommissionPayoutDto>(payout!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<CommissionPayoutDto?> GetPayoutAsync(Guid payoutId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var payout = await _context.Set<CommissionPayout>()
            .AsNoTracking()
            .Include(p => p.Seller)
            .Include(p => p.Items)
                .ThenInclude(i => i.Commission)
                    .ThenInclude(c => c.Order)
            .FirstOrDefaultAsync(p => p.Id == payoutId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return payout != null ? _mapper.Map<CommissionPayoutDto>(payout) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<CommissionPayoutDto>> GetSellerPayoutsAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var payouts = await _context.Set<CommissionPayout>()
            .AsNoTracking()
            .Include(p => p.Seller)
            .Include(p => p.Items)
                .ThenInclude(i => i.Commission)
                    .ThenInclude(c => c.Order)
            .Where(p => p.SellerId == sellerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<CommissionPayoutDto>>(payouts);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<CommissionPayoutDto>> GetAllPayoutsAsync(string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        IQueryable<CommissionPayout> query = _context.Set<CommissionPayout>()
            .AsNoTracking()
            .Include(p => p.Seller)
            .Include(p => p.Items)
                .ThenInclude(i => i.Commission)
                    .ThenInclude(c => c.Order);

        if (!string.IsNullOrEmpty(status))
        {
            var payoutStatus = Enum.Parse<PayoutStatus>(status, true);
            query = query.Where(p => p.Status == payoutStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var payouts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var payoutDtos = _mapper.Map<IEnumerable<CommissionPayoutDto>>(payouts).ToList();

        return new PagedResult<CommissionPayoutDto>
        {
            Items = payoutDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> ProcessPayoutAsync(Guid payoutId, string transactionReference, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var payout = await _context.Set<CommissionPayout>()
            .FirstOrDefaultAsync(p => p.Id == payoutId, cancellationToken);

        if (payout == null) return false;

        payout.Status = PayoutStatus.Processing;
        payout.TransactionReference = transactionReference;
        payout.ProcessedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> CompletePayoutAsync(Guid payoutId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var payout = await _context.Set<CommissionPayout>()
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == payoutId, cancellationToken);

        if (payout == null) return false;

        payout.Status = PayoutStatus.Completed;
        payout.CompletedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send confirmation email
        await _emailService.SendEmailAsync(
            payout.Seller?.Email ?? string.Empty,
            $"Payout Completed - {payout.PayoutNumber}",
            $"Your payout of {payout.NetAmount:C} has been completed. Transaction Reference: {payout.TransactionReference}",
            true,
            cancellationToken
        );

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> FailPayoutAsync(Guid payoutId, string reason, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var payout = await _context.Set<CommissionPayout>()
            .Include(p => p.Items)
                .ThenInclude(i => i.Commission)
            .FirstOrDefaultAsync(p => p.Id == payoutId, cancellationToken);

        if (payout == null) return false;

        payout.Status = PayoutStatus.Failed;
        payout.Notes = reason;

        // Revert commissions back to approved
        foreach (var item in payout.Items)
        {
            if (item.Commission != null)
            {
                item.Commission.Status = CommissionStatus.Approved;
                item.Commission.PaidAt = null;
                item.Commission.PaymentReference = null;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<CommissionStatsDto> GetCommissionStatsAsync(Guid? sellerId = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !sc.IsDeleted (Global Query Filter)
        IQueryable<SellerCommission> query = _context.Set<SellerCommission>()
            .AsNoTracking();

        if (sellerId.HasValue)
        {
            query = query.Where(sc => sc.SellerId == sellerId.Value);
        }

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var now = DateTime.UtcNow.AddMonths(-12);
        
        var totalCommissions = await query.CountAsync(cancellationToken);
        var totalEarnings = await query.SumAsync(c => c.CommissionAmount, cancellationToken);
        var pendingCommissions = await query.Where(c => c.Status == CommissionStatus.Pending).SumAsync(c => c.NetAmount, cancellationToken);
        var approvedCommissions = await query.Where(c => c.Status == CommissionStatus.Approved).SumAsync(c => c.NetAmount, cancellationToken);
        var paidCommissions = await query.Where(c => c.Status == CommissionStatus.Paid).SumAsync(c => c.NetAmount, cancellationToken);
        var averageCommissionRate = totalCommissions > 0 ? await query.AverageAsync(c => c.CommissionRate, cancellationToken) : 0;
        var totalPlatformFees = await query.SumAsync(c => c.PlatformFee, cancellationToken);

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        var commissionsByMonth = await query
            .Where(c => c.CreatedAt >= now)
            .GroupBy(c => c.CreatedAt.ToString("yyyy-MM"))
            .Select(g => new { Key = g.Key, Value = g.Sum(c => c.NetAmount) })
            .ToDictionaryAsync(x => x.Key, x => x.Value, cancellationToken);

        return new CommissionStatsDto
        {
            TotalCommissions = totalCommissions,
            TotalEarnings = totalEarnings,
            PendingCommissions = pendingCommissions,
            ApprovedCommissions = approvedCommissions,
            PaidCommissions = paidCommissions,
            AvailableForPayout = approvedCommissions,
            AverageCommissionRate = averageCommissionRate,
            TotalPlatformFees = totalPlatformFees,
            CommissionsByMonth = commissionsByMonth
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<decimal> GetAvailablePayoutAmountAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !sc.IsDeleted (Global Query Filter)
        return await _context.Set<SellerCommission>()
            .AsNoTracking()
            .Where(sc => sc.SellerId == sellerId && sc.Status == CommissionStatus.Approved)
            .SumAsync(sc => sc.NetAmount, cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task<string> GeneratePayoutNumberAsync(CancellationToken cancellationToken = default)
    {
        var lastPayout = await _context.Set<CommissionPayout>()
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        int nextNumber = 1;
        if (lastPayout != null && lastPayout.PayoutNumber.StartsWith("PAY-"))
        {
            var numberPart = lastPayout.PayoutNumber.Substring(4);
            if (int.TryParse(numberPart, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"PAY-{nextNumber:D6}";
    }
}
