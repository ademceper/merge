using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Services.Notification;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Configuration;
using UserEntity = Merge.Domain.Modules.Identity.User;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Application.Interfaces.Seller;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Application.DTOs.Seller;
using Merge.Application.Common;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Services.Seller;

public class SellerCommissionService(IDbContext context, IUnitOfWork unitOfWork, IEmailService emailService, IMapper mapper, ILogger<SellerCommissionService> logger, IOptions<SellerSettings> sellerSettings, IOptions<PaginationSettings> paginationSettings) : ISellerCommissionService
{
    private readonly SellerSettings sellerConfig = sellerSettings.Value;
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<SellerCommissionDto> CalculateAndRecordCommissionAsync(Guid orderId, Guid orderItemId, CancellationToken cancellationToken = default)
    {
        var orderItem = await context.Set<OrderItem>()
            .AsNoTracking()
            .AsSplitQuery()
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

        var existing = await context.Set<SellerCommission>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem)
            .FirstOrDefaultAsync(sc => sc.OrderItemId == orderItemId, cancellationToken);

        if (existing != null)
        {
            return mapper.Map<SellerCommissionDto>(existing);
        }

        // Get seller settings
        var settings = await context.Set<SellerCommissionSettings>()
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
            // Calculate total sales for tier determination
            var totalSales = await context.Set<OrderEntity>()
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
                commissionRate = sellerConfig.DefaultCommissionRateWhenNoTier;
                platformFeeRate = sellerConfig.DefaultPlatformFeeRate;
            }
        }

        var orderAmount = orderItem.TotalPrice;
        var commissionAmount = orderAmount * (commissionRate / 100);
        var platformFee = orderAmount * (platformFeeRate / 100);
        var netAmount = commissionAmount - platformFee;

        var commission = SellerCommission.Create(
            sellerId: sellerId,
            orderId: orderId,
            orderItemId: orderItemId,
            orderAmount: orderAmount,
            commissionRate: commissionRate,
            platformFee: platformFee);

        await context.Set<SellerCommission>().AddAsync(commission, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        commission = await context.Set<SellerCommission>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem)
            .FirstOrDefaultAsync(sc => sc.Id == commission.Id, cancellationToken);

        return mapper.Map<SellerCommissionDto>(commission!);
    }

    public async Task<SellerCommissionDto?> GetCommissionAsync(Guid commissionId, CancellationToken cancellationToken = default)
    {
        var commission = await context.Set<SellerCommission>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem)
            .FirstOrDefaultAsync(sc => sc.Id == commissionId, cancellationToken);

        return commission != null ? mapper.Map<SellerCommissionDto>(commission) : null;
    }

    public async Task<IEnumerable<SellerCommissionDto>> GetSellerCommissionsAsync(Guid sellerId, CommissionStatus? status = null, CancellationToken cancellationToken = default)
    {
        IQueryable<SellerCommission> query = context.Set<SellerCommission>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem)
            .Where(sc => sc.SellerId == sellerId);

        if (status.HasValue)
        {
            query = query.Where(sc => sc.Status == status.Value);
        }

        var commissions = await query
            .OrderByDescending(sc => sc.CreatedAt)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<SellerCommissionDto>>(commissions);
    }

    public async Task<PagedResult<SellerCommissionDto>> GetAllCommissionsAsync(CommissionStatus? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > paginationConfig.MaxPageSize) pageSize = paginationConfig.MaxPageSize;
        if (page < 1) page = 1;

        IQueryable<SellerCommission> query = context.Set<SellerCommission>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem);

        if (status.HasValue)
        {
            query = query.Where(sc => sc.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var commissions = await query
            .OrderByDescending(sc => sc.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var commissionDtos = mapper.Map<IEnumerable<SellerCommissionDto>>(commissions).ToList();

        return new PagedResult<SellerCommissionDto>
        {
            Items = commissionDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> ApproveCommissionAsync(Guid commissionId, CancellationToken cancellationToken = default)
    {
        var commission = await context.Set<SellerCommission>()
            .FirstOrDefaultAsync(sc => sc.Id == commissionId, cancellationToken);

        if (commission == null) return false;

        commission.Approve();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CancelCommissionAsync(Guid commissionId, CancellationToken cancellationToken = default)
    {
        var commission = await context.Set<SellerCommission>()
            .FirstOrDefaultAsync(sc => sc.Id == commissionId, cancellationToken);

        if (commission == null) return false;

        commission.Cancel();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<CommissionTierDto> CreateTierAsync(CreateCommissionTierDto dto, CancellationToken cancellationToken = default)
    {
        var tier = CommissionTier.Create(
            name: dto.Name,
            minSales: dto.MinSales,
            maxSales: dto.MaxSales,
            commissionRate: dto.CommissionRate,
            platformFeeRate: dto.PlatformFeeRate,
            priority: dto.Priority);

        await context.Set<CommissionTier>().AddAsync(tier, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<CommissionTierDto>(tier);
    }

    public async Task<IEnumerable<CommissionTierDto>> GetAllTiersAsync(CancellationToken cancellationToken = default)
    {
        var tiers = await context.Set<CommissionTier>()
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Priority)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<CommissionTierDto>>(tiers);
    }

    public async Task<CommissionTierDto?> GetTierForSalesAsync(decimal totalSales, CancellationToken cancellationToken = default)
    {
        var tier = await context.Set<CommissionTier>()
            .AsNoTracking()
            .Where(t => t.IsActive && t.MinSales <= totalSales && t.MaxSales >= totalSales)
            .OrderBy(t => t.Priority)
            .FirstOrDefaultAsync(cancellationToken);

        return tier != null ? mapper.Map<CommissionTierDto>(tier) : null;
    }

    public async Task<bool> UpdateTierAsync(Guid tierId, CreateCommissionTierDto dto, CancellationToken cancellationToken = default)
    {
        var tier = await context.Set<CommissionTier>()
            .FirstOrDefaultAsync(t => t.Id == tierId, cancellationToken);

        if (tier == null) return false;

        tier.UpdateDetails(
            name: dto.Name,
            minSales: dto.MinSales,
            maxSales: dto.MaxSales,
            commissionRate: dto.CommissionRate,
            platformFeeRate: dto.PlatformFeeRate,
            priority: dto.Priority);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteTierAsync(Guid tierId, CancellationToken cancellationToken = default)
    {
        var tier = await context.Set<CommissionTier>()
            .FirstOrDefaultAsync(t => t.Id == tierId, cancellationToken);

        if (tier == null) return false;

        tier.Delete();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<SellerCommissionSettingsDto?> GetSellerSettingsAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        var settings = await context.Set<SellerCommissionSettings>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SellerId == sellerId, cancellationToken);

        return settings != null ? mapper.Map<SellerCommissionSettingsDto>(settings) : null;
    }

    public async Task<SellerCommissionSettingsDto> UpdateSellerSettingsAsync(Guid sellerId, UpdateCommissionSettingsDto dto, CancellationToken cancellationToken = default)
    {
        var settings = await context.Set<SellerCommissionSettings>()
            .FirstOrDefaultAsync(s => s.SellerId == sellerId, cancellationToken);

        if (settings == null)
        {
            settings = SellerCommissionSettings.Create(
                sellerId: sellerId,
                minimumPayoutAmount: dto.MinimumPayoutAmount ?? sellerConfig.DefaultMinimumPayoutAmount);
            await context.Set<SellerCommissionSettings>().AddAsync(settings, cancellationToken);
        }

        if (dto.CustomCommissionRate.HasValue || dto.UseCustomRate.HasValue)
        {
            settings.UpdateCustomCommissionRate(
                commissionRate: dto.CustomCommissionRate ?? settings.CustomCommissionRate,
                useCustomRate: dto.UseCustomRate ?? settings.UseCustomRate);
        }

        if (dto.MinimumPayoutAmount.HasValue)
        {
            settings.UpdateMinimumPayoutAmount(dto.MinimumPayoutAmount.Value);
        }

        if (dto.PaymentMethod != null || dto.PaymentDetails != null)
        {
            settings.UpdatePaymentMethod(
                paymentMethod: dto.PaymentMethod,
                paymentDetails: dto.PaymentDetails);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<SellerCommissionSettingsDto>(settings);
    }

    public async Task<CommissionPayoutDto> RequestPayoutAsync(Guid sellerId, RequestPayoutDto dto, CancellationToken cancellationToken = default)
    {
        var commissions = await context.Set<SellerCommission>()
            .AsNoTracking()
            .Where(sc => dto.CommissionIds.Contains(sc.Id) && sc.SellerId == sellerId)
            .Where(sc => sc.Status == CommissionStatus.Approved)
            .ToListAsync(cancellationToken);

        if (commissions.Count == 0)
        {
            throw new BusinessException("Onaylanmış komisyon bulunamadı.");
        }

        var settings = await context.Set<SellerCommissionSettings>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SellerId == sellerId, cancellationToken);

        var totalAmount = await context.Set<SellerCommission>()
            .AsNoTracking()
            .Where(sc => dto.CommissionIds.Contains(sc.Id) && sc.SellerId == sellerId && sc.Status == CommissionStatus.Approved)
            .SumAsync(c => c.NetAmount, cancellationToken);

        if (settings != null && totalAmount < settings.MinimumPayoutAmount)
        {
            throw new ValidationException($"Minimum ödeme tutarı {settings.MinimumPayoutAmount}.");
        }

        var transactionFee = totalAmount * (sellerConfig.PayoutTransactionFeeRate / 100);
        var netAmount = totalAmount - transactionFee;

        var payoutNumber = await GeneratePayoutNumberAsync(cancellationToken);

        var payout = CommissionPayout.Create(
            sellerId: sellerId,
            payoutNumber: payoutNumber,
            totalAmount: totalAmount,
            transactionFee: transactionFee,
            paymentMethod: settings?.PaymentMethod ?? sellerConfig.DefaultPaymentMethod,
            paymentDetails: settings?.PaymentDetails);

        await context.Set<CommissionPayout>().AddAsync(payout, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var commission in commissions)
        {
            payout.AddItem(commission.Id);

            commission.MarkAsPaid(payoutNumber);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        payout = await context.Set<CommissionPayout>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Seller)
            .Include(p => p.Items)
                .ThenInclude(i => i.Commission)
                    .ThenInclude(c => c.Order)
            .FirstOrDefaultAsync(p => p.Id == payout.Id, cancellationToken);

        return mapper.Map<CommissionPayoutDto>(payout!);
    }

    public async Task<CommissionPayoutDto?> GetPayoutAsync(Guid payoutId, CancellationToken cancellationToken = default)
    {
        var payout = await context.Set<CommissionPayout>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Seller)
            .Include(p => p.Items)
                .ThenInclude(i => i.Commission)
                    .ThenInclude(c => c.Order)
            .FirstOrDefaultAsync(p => p.Id == payoutId, cancellationToken);

        return payout != null ? mapper.Map<CommissionPayoutDto>(payout) : null;
    }

    public async Task<IEnumerable<CommissionPayoutDto>> GetSellerPayoutsAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        var payouts = await context.Set<CommissionPayout>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Seller)
            .Include(p => p.Items)
                .ThenInclude(i => i.Commission)
                    .ThenInclude(c => c.Order)
            .Where(p => p.SellerId == sellerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<CommissionPayoutDto>>(payouts);
    }

    public async Task<PagedResult<CommissionPayoutDto>> GetAllPayoutsAsync(PayoutStatus? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > paginationConfig.MaxPageSize) pageSize = paginationConfig.MaxPageSize;
        if (page < 1) page = 1;

        IQueryable<CommissionPayout> query = context.Set<CommissionPayout>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Seller)
            .Include(p => p.Items)
                .ThenInclude(i => i.Commission)
                    .ThenInclude(c => c.Order);

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var payouts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var payoutDtos = mapper.Map<IEnumerable<CommissionPayoutDto>>(payouts).ToList();

        return new PagedResult<CommissionPayoutDto>
        {
            Items = payoutDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> ProcessPayoutAsync(Guid payoutId, string transactionReference, CancellationToken cancellationToken = default)
    {
        var payout = await context.Set<CommissionPayout>()
            .FirstOrDefaultAsync(p => p.Id == payoutId, cancellationToken);

        if (payout == null) return false;

        payout.Process(transactionReference);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CompletePayoutAsync(Guid payoutId, CancellationToken cancellationToken = default)
    {
        var payout = await context.Set<CommissionPayout>()
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == payoutId, cancellationToken);

        if (payout == null) return false;

        payout.Complete();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Send confirmation email
        await emailService.SendEmailAsync(
            payout.Seller?.Email ?? string.Empty,
            $"Payout Completed - {payout.PayoutNumber}",
            $"Your payout of {payout.NetAmount:C} has been completed. Transaction Reference: {payout.TransactionReference}",
            true,
            cancellationToken
        );

        return true;
    }

    public async Task<bool> FailPayoutAsync(Guid payoutId, string reason, CancellationToken cancellationToken = default)
    {
        var payout = await context.Set<CommissionPayout>()
            .AsSplitQuery()
            .Include(p => p.Items)
                .ThenInclude(i => i.Commission)
            .FirstOrDefaultAsync(p => p.Id == payoutId, cancellationToken);

        if (payout == null) return false;

        payout.Fail(reason);

        // Revert commissions back to approved
        foreach (var item in payout.Items)
        {
            if (item.Commission != null)
            {
                item.Commission.RevertToApproved();
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<CommissionStatsDto> GetCommissionStatsAsync(Guid? sellerId = null, CancellationToken cancellationToken = default)
    {
        IQueryable<SellerCommission> query = context.Set<SellerCommission>()
            .AsNoTracking();

        if (sellerId.HasValue)
        {
            query = query.Where(sc => sc.SellerId == sellerId.Value);
        }

        var now = DateTime.UtcNow.AddMonths(-12);
        
        var totalCommissions = await query.CountAsync(cancellationToken);
        var totalEarnings = await query.SumAsync(c => c.CommissionAmount, cancellationToken);
        var pendingCommissions = await query.Where(c => c.Status == CommissionStatus.Pending).SumAsync(c => c.NetAmount, cancellationToken);
        var approvedCommissions = await query.Where(c => c.Status == CommissionStatus.Approved).SumAsync(c => c.NetAmount, cancellationToken);
        var paidCommissions = await query.Where(c => c.Status == CommissionStatus.Paid).SumAsync(c => c.NetAmount, cancellationToken);
        var averageCommissionRate = totalCommissions > 0 ? await query.AverageAsync(c => c.CommissionRate, cancellationToken) : 0;
        var totalPlatformFees = await query.SumAsync(c => c.PlatformFee, cancellationToken);

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

    public async Task<decimal> GetAvailablePayoutAmountAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        return await context.Set<SellerCommission>()
            .AsNoTracking()
            .Where(sc => sc.SellerId == sellerId && sc.Status == CommissionStatus.Approved)
            .SumAsync(sc => sc.NetAmount, cancellationToken);
    }

    private async Task<string> GeneratePayoutNumberAsync(CancellationToken cancellationToken = default)
    {
        var lastPayout = await context.Set<CommissionPayout>()
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
