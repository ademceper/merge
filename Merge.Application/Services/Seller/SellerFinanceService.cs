using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserEntity = Merge.Domain.Entities.User;
using OrderEntity = Merge.Domain.Entities.Order;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Seller;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text.Json;
using Merge.Application.DTOs.Seller;
using Merge.Application.Common;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
namespace Merge.Application.Services.Seller;

public class SellerFinanceService : ISellerFinanceService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SellerFinanceService> _logger;

    public SellerFinanceService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SellerFinanceService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SellerBalanceDto> GetSellerBalanceAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !sp.IsDeleted (Global Query Filter)
        var seller = await _context.SellerProfiles
            .AsNoTracking()
            .Include(sp => sp.User)
            .FirstOrDefaultAsync(sp => sp.UserId == sellerId, cancellationToken);

        if (seller == null)
        {
            throw new NotFoundException("Satıcı", sellerId);
        }

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        // Calculate in-transit balance (payouts being processed)
        var inTransitBalance = await _context.Set<CommissionPayout>()
            .AsNoTracking()
            .Where(p => p.SellerId == sellerId && 
                   (p.Status == PayoutStatus.Pending || p.Status == PayoutStatus.Processing))
            .SumAsync(p => p.TotalAmount, cancellationToken);

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        // Calculate total payouts
        var totalPayouts = await _context.Set<CommissionPayout>()
            .AsNoTracking()
            .Where(p => p.SellerId == sellerId && 
                   p.Status == PayoutStatus.Completed)
            .SumAsync(p => p.NetAmount, cancellationToken);

        return new SellerBalanceDto
        {
            SellerId = sellerId,
            SellerName = seller.StoreName,
            TotalEarnings = seller.TotalEarnings,
            PendingBalance = seller.PendingBalance,
            AvailableBalance = seller.AvailableBalance,
            InTransitBalance = Math.Round(inTransitBalance, 2),
            TotalPayouts = Math.Round(totalPayouts, 2),
            NextPayoutDate = 0 // Would need payout schedule
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<decimal> GetAvailableBalanceAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !sp.IsDeleted (Global Query Filter)
        var seller = await _context.SellerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.UserId == sellerId, cancellationToken);

        return seller?.AvailableBalance ?? 0;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<decimal> GetPendingBalanceAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !sp.IsDeleted (Global Query Filter)
        var seller = await _context.SellerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.UserId == sellerId, cancellationToken);

        return seller?.PendingBalance ?? 0;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SellerTransactionDto> CreateTransactionAsync(Guid sellerId, string transactionType, decimal amount, string description, Guid? relatedEntityId = null, string? relatedEntityType = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !sp.IsDeleted (Global Query Filter)
        var seller = await _context.SellerProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == sellerId, cancellationToken);

        if (seller == null)
        {
            throw new NotFoundException("Satıcı", sellerId);
        }

        var balanceBefore = seller.AvailableBalance;
        var balanceAfter = balanceBefore + amount;

        // Update seller balance
        seller.AvailableBalance = balanceAfter;
        seller.TotalEarnings += amount > 0 ? amount : 0;
        seller.PendingBalance += amount < 0 ? Math.Abs(amount) : 0;

        var transaction = new SellerTransaction
        {
            SellerId = sellerId,
            TransactionType = transactionType,
            Description = description,
            Amount = amount,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            Status = FinanceTransactionStatus.Completed
        };

        await _context.Set<SellerTransaction>().AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        transaction = await _context.Set<SellerTransaction>()
            .AsNoTracking()
            .Include(t => t.Seller)
            .FirstOrDefaultAsync(t => t.Id == transaction.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<SellerTransactionDto>(transaction!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SellerTransactionDto?> GetTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var transaction = await _context.Set<SellerTransaction>()
            .AsNoTracking()
            .Include(t => t.Seller)
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return transaction != null ? _mapper.Map<SellerTransactionDto>(transaction) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<SellerTransactionDto>> GetSellerTransactionsAsync(Guid sellerId, string? transactionType = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        IQueryable<SellerTransaction> query = _context.Set<SellerTransaction>()
            .AsNoTracking()
            .Include(t => t.Seller)
            .Where(t => t.SellerId == sellerId);

        if (!string.IsNullOrEmpty(transactionType))
        {
            query = query.Where(t => t.TransactionType == transactionType);
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= endDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var transactions = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var transactionDtos = _mapper.Map<IEnumerable<SellerTransactionDto>>(transactions).ToList();

        return new PagedResult<SellerTransactionDto>
        {
            Items = transactionDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SellerInvoiceDto> GenerateInvoiceAsync(CreateSellerInvoiceDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !sp.IsDeleted (Global Query Filter)
        var seller = await _context.SellerProfiles
            .Include(sp => sp.User)
            .FirstOrDefaultAsync(sp => sp.UserId == dto.SellerId, cancellationToken);

        if (seller == null)
        {
            throw new NotFoundException("Satıcı", dto.SellerId);
        }

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // Get commissions for the period
        var commissionStats = await _context.Set<SellerCommission>()
            .AsNoTracking()
            .Where(sc => sc.SellerId == dto.SellerId &&
                  sc.CreatedAt >= dto.PeriodStart &&
                  sc.CreatedAt <= dto.PeriodEnd)
            .GroupBy(sc => 1)
            .Select(g => new
            {
                TotalCommissions = g.Sum(c => c.CommissionAmount),
                PlatformFees = g.Sum(c => c.PlatformFee),
                NetCommissions = g.Sum(c => c.NetAmount)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var totalCommissions = commissionStats?.TotalCommissions ?? 0;
        var platformFees = commissionStats?.PlatformFees ?? 0;
        var netCommissions = commissionStats?.NetCommissions ?? 0;

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // Get payouts for the period
        var totalPayouts = await _context.Set<CommissionPayout>()
            .AsNoTracking()
            .Where(p => p.SellerId == dto.SellerId &&
                  p.CreatedAt >= dto.PeriodStart &&
                  p.CreatedAt <= dto.PeriodEnd &&
                  p.Status == PayoutStatus.Completed)
            .SumAsync(p => p.NetAmount, cancellationToken);

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // Get orders for the period (for total earnings calculation)
        var totalEarnings = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= dto.PeriodStart &&
                  o.CreatedAt <= dto.PeriodEnd &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == dto.SellerId))
            .SelectMany(o => o.OrderItems.Where(oi => oi.Product.SellerId == dto.SellerId))
            .SumAsync(oi => oi.TotalPrice, cancellationToken);

        // ✅ PERFORMANCE: Batch load commissions for invoice items (N+1 fix)
        var commissions = await _context.Set<SellerCommission>()
            .AsNoTracking()
            .Include(c => c.Order)
            .Where(sc => sc.SellerId == dto.SellerId &&
                  sc.CreatedAt >= dto.PeriodStart &&
                  sc.CreatedAt <= dto.PeriodEnd)
            .ToListAsync(cancellationToken);

        var invoiceNumber = await GenerateInvoiceNumberAsync(dto.PeriodStart, cancellationToken);

        var invoice = new SellerInvoice
        {
            SellerId = dto.SellerId,
            InvoiceNumber = invoiceNumber,
            InvoiceDate = DateTime.UtcNow,
            PeriodStart = dto.PeriodStart,
            PeriodEnd = dto.PeriodEnd,
            TotalEarnings = totalEarnings,
            TotalCommissions = totalCommissions,
            TotalPayouts = totalPayouts,
            PlatformFees = platformFees,
            NetAmount = netCommissions - totalPayouts,
            Status = SellerInvoiceStatus.Draft,
            Notes = dto.Notes
        };

        // ✅ FIX: ToListAsync() sonrası Select().ToList() YASAK - foreach ile DTO oluştur
        var invoiceItems = new List<InvoiceItemDto>();
        foreach (var commission in commissions)
        {
            invoiceItems.Add(new InvoiceItemDto
            {
                Description = $"Commission for Order #{commission.Order.OrderNumber}",
                Quantity = 1,
                UnitPrice = commission.CommissionAmount,
                TotalPrice = commission.CommissionAmount
            });
        }

        invoice.InvoiceData = JsonSerializer.Serialize(invoiceItems);

        await _context.Set<SellerInvoice>().AddAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        invoice = await _context.Set<SellerInvoice>()
            .AsNoTracking()
            .Include(i => i.Seller)
            .FirstOrDefaultAsync(i => i.Id == invoice.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<SellerInvoiceDto>(invoice!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SellerInvoiceDto?> GetInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !i.IsDeleted (Global Query Filter)
        var invoice = await _context.Set<SellerInvoice>()
            .AsNoTracking()
            .Include(i => i.Seller)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return invoice != null ? _mapper.Map<SellerInvoiceDto>(invoice) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<SellerInvoiceDto>> GetSellerInvoicesAsync(Guid sellerId, string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !i.IsDeleted (Global Query Filter)
        IQueryable<SellerInvoice> query = _context.Set<SellerInvoice>()
            .AsNoTracking()
            .Include(i => i.Seller)
            .Where(i => i.SellerId == sellerId);

        if (!string.IsNullOrEmpty(status))
        {
            var statusEnum = Enum.Parse<SellerInvoiceStatus>(status);
            query = query.Where(i => i.Status == statusEnum);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var invoices = await query
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var invoiceDtos = _mapper.Map<IEnumerable<SellerInvoiceDto>>(invoices).ToList();

        return new PagedResult<SellerInvoiceDto>
        {
            Items = invoiceDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> MarkInvoiceAsPaidAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !i.IsDeleted (Global Query Filter)
        var invoice = await _context.Set<SellerInvoice>()
            .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);

        if (invoice == null) return false;

        invoice.Status = SellerInvoiceStatus.Paid;
        invoice.PaidAt = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SellerFinanceSummaryDto> GetSellerFinanceSummaryAsync(Guid sellerId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        startDate ??= DateTime.UtcNow.AddDays(-30);
        endDate ??= DateTime.UtcNow;

        var balance = await GetSellerBalanceAsync(sellerId, cancellationToken);

        // Recent transactions
        var transactions = await GetSellerTransactionsAsync(sellerId, null, startDate, endDate, 1, 10, cancellationToken);

        // Recent invoices
        var invoices = await GetSellerInvoicesAsync(sellerId, null, 1, 10, cancellationToken);

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // Earnings by month
        var earningsByMonth = await _context.Set<SellerCommission>()
            .AsNoTracking()
            .Where(sc => sc.SellerId == sellerId &&
                  sc.CreatedAt >= startDate &&
                  sc.CreatedAt <= endDate)
            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
            .Select(g => new { Key = $"{g.Key.Year}-{g.Key.Month:D2}", Value = g.Sum(c => c.NetAmount) })
            .ToDictionaryAsync(x => x.Key, x => x.Value, cancellationToken);

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // Payouts by month
        var payoutsByMonth = await _context.Set<CommissionPayout>()
            .AsNoTracking()
            .Where(p => p.SellerId == sellerId &&
                  p.CreatedAt >= startDate &&
                  p.CreatedAt <= endDate &&
                  p.Status == PayoutStatus.Completed)
            .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
            .Select(g => new { Key = $"{g.Key.Year}-{g.Key.Month:D2}", Value = g.Sum(p => p.NetAmount) })
            .ToDictionaryAsync(x => x.Key, x => x.Value, cancellationToken);

        return new SellerFinanceSummaryDto
        {
            SellerId = sellerId,
            Balance = balance,
            RecentTransactions = transactions.Items.ToList(),
            RecentInvoices = invoices.Items.ToList(),
            EarningsByMonth = earningsByMonth,
            PayoutsByMonth = payoutsByMonth
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task<string> GenerateInvoiceNumberAsync(DateTime periodStart, CancellationToken cancellationToken = default)
    {
        var yearMonth = periodStart.ToString("yyyyMM");
        // ✅ PERFORMANCE: Removed manual !i.IsDeleted (Global Query Filter)
        var existingCount = await _context.Set<SellerInvoice>()
            .AsNoTracking()
            .CountAsync(i => i.InvoiceNumber.StartsWith($"INV-{yearMonth}"), cancellationToken);

        return $"INV-{yearMonth}-{(existingCount + 1):D6}";
    }
}

