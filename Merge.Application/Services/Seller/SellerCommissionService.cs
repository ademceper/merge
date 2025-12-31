using Microsoft.EntityFrameworkCore;
using Merge.Application.Services.Notification;
using Merge.Application.Interfaces.User;
using UserEntity = Merge.Domain.Entities.User;
using OrderEntity = Merge.Domain.Entities.Order;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.Interfaces.Seller;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Seller;


namespace Merge.Application.Services.Seller;

public class SellerCommissionService : ISellerCommissionService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public SellerCommissionService(ApplicationDbContext context, IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<SellerCommissionDto> CalculateAndRecordCommissionAsync(Guid orderId, Guid orderItemId)
    {
        var orderItem = await _context.Set<OrderItem>()
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId && oi.OrderId == orderId);

        if (orderItem == null)
        {
            throw new NotFoundException("Sipariş kalemi", orderItemId);
        }

        if (!orderItem.Product.SellerId.HasValue)
        {
            throw new BusinessException("Ürüne atanmış satıcı yok.");
        }

        var sellerId = orderItem.Product.SellerId.Value;

        // Check if commission already exists
        var existing = await _context.Set<SellerCommission>()
            .FirstOrDefaultAsync(sc => sc.OrderItemId == orderItemId && !sc.IsDeleted);

        if (existing != null)
        {
            return await MapToDto(existing);
        }

        // Get seller settings
        var settings = await _context.Set<SellerCommissionSettings>()
            .FirstOrDefaultAsync(s => s.SellerId == sellerId && !s.IsDeleted);

        decimal commissionRate;
        decimal platformFeeRate = 0;

        if (settings != null && settings.UseCustomRate)
        {
            commissionRate = settings.CustomCommissionRate;
        }
        else
        {
            // Calculate total sales for tier determination
            var totalSales = await _context.Set<OrderEntity>()
                .Where(o => o.OrderItems.Any(i => i.Product.SellerId == sellerId) && o.PaymentStatus == "Paid" && !o.IsDeleted)
                .SumAsync(o => o.TotalAmount);

            var tier = await GetTierForSalesAsync(totalSales);
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

        await _context.Set<SellerCommission>().AddAsync(commission);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDto(commission);
    }

    public async Task<SellerCommissionDto?> GetCommissionAsync(Guid commissionId)
    {
        var commission = await _context.Set<SellerCommission>()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem)
            .FirstOrDefaultAsync(sc => sc.Id == commissionId && !sc.IsDeleted);

        return commission != null ? await MapToDto(commission) : null;
    }

    public async Task<IEnumerable<SellerCommissionDto>> GetSellerCommissionsAsync(Guid sellerId, string? status = null)
    {
        var query = _context.Set<SellerCommission>()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem)
            .Where(sc => sc.SellerId == sellerId && !sc.IsDeleted);

        if (!string.IsNullOrEmpty(status))
        {
            var commissionStatus = Enum.Parse<CommissionStatus>(status, true);
            query = query.Where(sc => sc.Status == commissionStatus);
        }

        var commissions = await query
            .OrderByDescending(sc => sc.CreatedAt)
            .ToListAsync();

        var dtos = new List<SellerCommissionDto>();
        foreach (var commission in commissions)
        {
            dtos.Add(await MapToDto(commission));
        }

        return dtos;
    }

    public async Task<IEnumerable<SellerCommissionDto>> GetAllCommissionsAsync(string? status = null, int page = 1, int pageSize = 20)
    {
        var query = _context.Set<SellerCommission>()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem)
            .Where(sc => !sc.IsDeleted);

        if (!string.IsNullOrEmpty(status))
        {
            var commissionStatus = Enum.Parse<CommissionStatus>(status, true);
            query = query.Where(sc => sc.Status == commissionStatus);
        }

        var commissions = await query
            .OrderByDescending(sc => sc.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = new List<SellerCommissionDto>();
        foreach (var commission in commissions)
        {
            dtos.Add(await MapToDto(commission));
        }

        return dtos;
    }

    public async Task<bool> ApproveCommissionAsync(Guid commissionId)
    {
        var commission = await _context.Set<SellerCommission>()
            .FirstOrDefaultAsync(sc => sc.Id == commissionId && !sc.IsDeleted);

        if (commission == null) return false;

        commission.Status = CommissionStatus.Approved;
        commission.ApprovedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CancelCommissionAsync(Guid commissionId)
    {
        var commission = await _context.Set<SellerCommission>()
            .FirstOrDefaultAsync(sc => sc.Id == commissionId && !sc.IsDeleted);

        if (commission == null) return false;

        commission.Status = CommissionStatus.Cancelled;

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<CommissionTierDto> CreateTierAsync(CreateCommissionTierDto dto)
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

        await _context.Set<CommissionTier>().AddAsync(tier);
        await _unitOfWork.SaveChangesAsync();

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

    public async Task<IEnumerable<CommissionTierDto>> GetAllTiersAsync()
    {
        var tiers = await _context.Set<CommissionTier>()
            .Where(t => !t.IsDeleted && t.IsActive)
            .OrderBy(t => t.Priority)
            .ToListAsync();

        return tiers.Select(t => new CommissionTierDto
        {
            Id = t.Id,
            Name = t.Name,
            MinSales = t.MinSales,
            MaxSales = t.MaxSales,
            CommissionRate = t.CommissionRate,
            PlatformFeeRate = t.PlatformFeeRate,
            IsActive = t.IsActive,
            Priority = t.Priority
        });
    }

    public async Task<CommissionTierDto?> GetTierForSalesAsync(decimal totalSales)
    {
        var tier = await _context.Set<CommissionTier>()
            .Where(t => !t.IsDeleted && t.IsActive && t.MinSales <= totalSales && t.MaxSales >= totalSales)
            .OrderBy(t => t.Priority)
            .FirstOrDefaultAsync();

        if (tier == null) return null;

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

    public async Task<bool> UpdateTierAsync(Guid tierId, CreateCommissionTierDto dto)
    {
        var tier = await _context.Set<CommissionTier>()
            .FirstOrDefaultAsync(t => t.Id == tierId && !t.IsDeleted);

        if (tier == null) return false;

        tier.Name = dto.Name;
        tier.MinSales = dto.MinSales;
        tier.MaxSales = dto.MaxSales;
        tier.CommissionRate = dto.CommissionRate;
        tier.PlatformFeeRate = dto.PlatformFeeRate;
        tier.Priority = dto.Priority;

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteTierAsync(Guid tierId)
    {
        var tier = await _context.Set<CommissionTier>()
            .FirstOrDefaultAsync(t => t.Id == tierId && !t.IsDeleted);

        if (tier == null) return false;

        tier.IsDeleted = true;

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<SellerCommissionSettingsDto?> GetSellerSettingsAsync(Guid sellerId)
    {
        var settings = await _context.Set<SellerCommissionSettings>()
            .FirstOrDefaultAsync(s => s.SellerId == sellerId && !s.IsDeleted);

        if (settings == null) return null;

        return new SellerCommissionSettingsDto
        {
            SellerId = settings.SellerId,
            CustomCommissionRate = settings.CustomCommissionRate,
            UseCustomRate = settings.UseCustomRate,
            MinimumPayoutAmount = settings.MinimumPayoutAmount,
            PaymentMethod = settings.PaymentMethod,
            PaymentDetails = settings.PaymentDetails
        };
    }

    public async Task<SellerCommissionSettingsDto> UpdateSellerSettingsAsync(Guid sellerId, UpdateCommissionSettingsDto dto)
    {
        var settings = await _context.Set<SellerCommissionSettings>()
            .FirstOrDefaultAsync(s => s.SellerId == sellerId && !s.IsDeleted);

        if (settings == null)
        {
            settings = new SellerCommissionSettings
            {
                SellerId = sellerId
            };
            await _context.Set<SellerCommissionSettings>().AddAsync(settings);
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

        await _unitOfWork.SaveChangesAsync();

        return new SellerCommissionSettingsDto
        {
            SellerId = settings.SellerId,
            CustomCommissionRate = settings.CustomCommissionRate,
            UseCustomRate = settings.UseCustomRate,
            MinimumPayoutAmount = settings.MinimumPayoutAmount,
            PaymentMethod = settings.PaymentMethod,
            PaymentDetails = settings.PaymentDetails
        };
    }

    public async Task<CommissionPayoutDto> RequestPayoutAsync(Guid sellerId, RequestPayoutDto dto)
    {
        var commissions = await _context.Set<SellerCommission>()
            .Where(sc => dto.CommissionIds.Contains(sc.Id) && sc.SellerId == sellerId && !sc.IsDeleted)
            .Where(sc => sc.Status == CommissionStatus.Approved)
            .ToListAsync();

        if (!commissions.Any())
        {
            throw new BusinessException("Onaylanmış komisyon bulunamadı.");
        }

        var settings = await _context.Set<SellerCommissionSettings>()
            .FirstOrDefaultAsync(s => s.SellerId == sellerId && !s.IsDeleted);

        var totalAmount = commissions.Sum(c => c.NetAmount);

        if (settings != null && totalAmount < settings.MinimumPayoutAmount)
        {
            throw new ValidationException($"Minimum ödeme tutarı {settings.MinimumPayoutAmount}.");
        }

        var transactionFee = totalAmount * 0.01m; // 1% transaction fee
        var netAmount = totalAmount - transactionFee;

        var payoutNumber = await GeneratePayoutNumberAsync();

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

        await _context.Set<CommissionPayout>().AddAsync(payout);
        await _unitOfWork.SaveChangesAsync();

        foreach (var commission in commissions)
        {
            var item = new CommissionPayoutItem
            {
                PayoutId = payout.Id,
                CommissionId = commission.Id
            };

            await _context.Set<CommissionPayoutItem>().AddAsync(item);

            commission.Status = CommissionStatus.Paid;
            commission.PaidAt = DateTime.UtcNow;
            commission.PaymentReference = payoutNumber;
        }

        await _unitOfWork.SaveChangesAsync();

        return await MapPayoutToDto(payout);
    }

    public async Task<CommissionPayoutDto?> GetPayoutAsync(Guid payoutId)
    {
        var payout = await _context.Set<CommissionPayout>()
            .Include(p => p.Seller)
            .Include(p => p.Items)
                .ThenInclude(i => i.Commission)
                    .ThenInclude(c => c.Order)
            .FirstOrDefaultAsync(p => p.Id == payoutId && !p.IsDeleted);

        return payout != null ? await MapPayoutToDto(payout) : null;
    }

    public async Task<IEnumerable<CommissionPayoutDto>> GetSellerPayoutsAsync(Guid sellerId)
    {
        var payouts = await _context.Set<CommissionPayout>()
            .Include(p => p.Seller)
            .Include(p => p.Items)
                .ThenInclude(i => i.Commission)
            .Where(p => p.SellerId == sellerId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var dtos = new List<CommissionPayoutDto>();
        foreach (var payout in payouts)
        {
            dtos.Add(await MapPayoutToDto(payout));
        }

        return dtos;
    }

    public async Task<IEnumerable<CommissionPayoutDto>> GetAllPayoutsAsync(string? status = null, int page = 1, int pageSize = 20)
    {
        var query = _context.Set<CommissionPayout>()
            .Include(p => p.Seller)
            .Include(p => p.Items)
                .ThenInclude(i => i.Commission)
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrEmpty(status))
        {
            var payoutStatus = Enum.Parse<PayoutStatus>(status, true);
            query = query.Where(p => p.Status == payoutStatus);
        }

        var payouts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = new List<CommissionPayoutDto>();
        foreach (var payout in payouts)
        {
            dtos.Add(await MapPayoutToDto(payout));
        }

        return dtos;
    }

    public async Task<bool> ProcessPayoutAsync(Guid payoutId, string transactionReference)
    {
        var payout = await _context.Set<CommissionPayout>()
            .FirstOrDefaultAsync(p => p.Id == payoutId && !p.IsDeleted);

        if (payout == null) return false;

        payout.Status = PayoutStatus.Processing;
        payout.TransactionReference = transactionReference;
        payout.ProcessedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CompletePayoutAsync(Guid payoutId)
    {
        var payout = await _context.Set<CommissionPayout>()
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == payoutId && !p.IsDeleted);

        if (payout == null) return false;

        payout.Status = PayoutStatus.Completed;
        payout.CompletedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        // Send confirmation email
        await _emailService.SendEmailAsync(
            payout.Seller?.Email ?? string.Empty,
            $"Payout Completed - {payout.PayoutNumber}",
            $"Your payout of {payout.NetAmount:C} has been completed. Transaction Reference: {payout.TransactionReference}"
        );

        return true;
    }

    public async Task<bool> FailPayoutAsync(Guid payoutId, string reason)
    {
        var payout = await _context.Set<CommissionPayout>()
            .Include(p => p.Items)
                .ThenInclude(i => i.Commission)
            .FirstOrDefaultAsync(p => p.Id == payoutId && !p.IsDeleted);

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

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<CommissionStatsDto> GetCommissionStatsAsync(Guid? sellerId = null)
    {
        var query = _context.Set<SellerCommission>()
            .Where(sc => !sc.IsDeleted);

        if (sellerId.HasValue)
        {
            query = query.Where(sc => sc.SellerId == sellerId.Value);
        }

        var commissions = await query.ToListAsync();

        var now = DateTime.UtcNow;
        var commissionsByMonth = commissions
            .Where(c => c.CreatedAt >= now.AddMonths(-12))
            .GroupBy(c => c.CreatedAt.ToString("yyyy-MM"))
            .ToDictionary(g => g.Key, g => g.Sum(c => c.NetAmount));

        return new CommissionStatsDto
        {
            TotalCommissions = commissions.Count,
            TotalEarnings = commissions.Sum(c => c.CommissionAmount),
            PendingCommissions = commissions.Where(c => c.Status == CommissionStatus.Pending).Sum(c => c.NetAmount),
            ApprovedCommissions = commissions.Where(c => c.Status == CommissionStatus.Approved).Sum(c => c.NetAmount),
            PaidCommissions = commissions.Where(c => c.Status == CommissionStatus.Paid).Sum(c => c.NetAmount),
            AvailableForPayout = commissions.Where(c => c.Status == CommissionStatus.Approved).Sum(c => c.NetAmount),
            AverageCommissionRate = commissions.Any() ? commissions.Average(c => c.CommissionRate) : 0,
            TotalPlatformFees = commissions.Sum(c => c.PlatformFee),
            CommissionsByMonth = commissionsByMonth
        };
    }

    public async Task<decimal> GetAvailablePayoutAmountAsync(Guid sellerId)
    {
        return await _context.Set<SellerCommission>()
            .Where(sc => sc.SellerId == sellerId && !sc.IsDeleted && sc.Status == CommissionStatus.Approved)
            .SumAsync(sc => sc.NetAmount);
    }

    private async Task<string> GeneratePayoutNumberAsync()
    {
        var lastPayout = await _context.Set<CommissionPayout>()
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

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

    private async Task<SellerCommissionDto> MapToDto(SellerCommission commission)
    {
        var seller = commission.Seller ?? await _context.Set<UserEntity>()
            .FirstOrDefaultAsync(u => u.Id == commission.SellerId);

        var order = commission.Order ?? await _context.Set<OrderEntity>()
            .FirstOrDefaultAsync(o => o.Id == commission.OrderId);

        return new SellerCommissionDto
        {
            Id = commission.Id,
            SellerId = commission.SellerId,
            SellerName = seller != null ? $"{seller.FirstName} {seller.LastName}" : "Unknown",
            OrderId = commission.OrderId,
            OrderNumber = order?.OrderNumber ?? "Unknown",
            OrderAmount = commission.OrderAmount,
            CommissionRate = commission.CommissionRate,
            CommissionAmount = commission.CommissionAmount,
            PlatformFee = commission.PlatformFee,
            NetAmount = commission.NetAmount,
            Status = commission.Status.ToString(),
            ApprovedAt = commission.ApprovedAt,
            PaidAt = commission.PaidAt,
            PaymentReference = commission.PaymentReference,
            CreatedAt = commission.CreatedAt
        };
    }

    private async Task<CommissionPayoutDto> MapPayoutToDto(CommissionPayout payout)
    {
        var seller = payout.Seller ?? await _context.Set<UserEntity>()
            .FirstOrDefaultAsync(u => u.Id == payout.SellerId);

        var commissions = new List<SellerCommissionDto>();
        if (payout.Items != null)
        {
            foreach (var item in payout.Items.Where(i => !i.IsDeleted))
            {
                if (item.Commission != null)
                {
                    commissions.Add(await MapToDto(item.Commission));
                }
            }
        }

        return new CommissionPayoutDto
        {
            Id = payout.Id,
            SellerId = payout.SellerId,
            SellerName = seller != null ? $"{seller.FirstName} {seller.LastName}" : "Unknown",
            PayoutNumber = payout.PayoutNumber,
            TotalAmount = payout.TotalAmount,
            TransactionFee = payout.TransactionFee,
            NetAmount = payout.NetAmount,
            Status = payout.Status.ToString(),
            PaymentMethod = payout.PaymentMethod,
            TransactionReference = payout.TransactionReference,
            ProcessedAt = payout.ProcessedAt,
            CompletedAt = payout.CompletedAt,
            Notes = payout.Notes,
            CreatedAt = payout.CreatedAt,
            Commissions = commissions
        };
    }
}
