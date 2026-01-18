using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserEntity = Merge.Domain.Modules.Identity.User;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Seller;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Application.DTOs.Seller;
using Merge.Application.Common;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Services.Seller;

public class SellerFinanceService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<SellerFinanceService> logger, IOptions<PaginationSettings> paginationSettings) : ISellerFinanceService
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<SellerBalanceDto> GetSellerBalanceAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        var seller = await context.Set<SellerProfile>()
            .AsNoTracking()
            .Include(sp => sp.User)
            .FirstOrDefaultAsync(sp => sp.UserId == sellerId, cancellationToken);

        if (seller is null)
        {
            throw new NotFoundException("Satıcı", sellerId);
        }

        // Calculate in-transit balance (payouts being processed)
        var inTransitBalance = await context.Set<CommissionPayout>()
            .AsNoTracking()
            .Where(p => p.SellerId == sellerId && 
                   (p.Status == PayoutStatus.Pending || p.Status == PayoutStatus.Processing))
            .SumAsync(p => p.TotalAmount, cancellationToken);

        // Calculate total payouts
        var totalPayouts = await context.Set<CommissionPayout>()
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

    public async Task<decimal> GetAvailableBalanceAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        var seller = await context.Set<SellerProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.UserId == sellerId, cancellationToken);

        return seller?.AvailableBalance ?? 0;
    }

    public async Task<decimal> GetPendingBalanceAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        var seller = await context.Set<SellerProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.UserId == sellerId, cancellationToken);

        return seller?.PendingBalance ?? 0;
    }

    public async Task<SellerTransactionDto> CreateTransactionAsync(Guid sellerId, SellerTransactionType transactionType, decimal amount, string description, Guid? relatedEntityId = null, string? relatedEntityType = null, CancellationToken cancellationToken = default)
    {
        var seller = await context.Set<SellerProfile>()
            .FirstOrDefaultAsync(sp => sp.UserId == sellerId, cancellationToken);

        if (seller is null)
        {
            throw new NotFoundException("Satıcı", sellerId);
        }

        var balanceBefore = seller.AvailableBalance;
        var balanceAfter = balanceBefore + amount;

        var transaction = SellerTransaction.Create(
            sellerId: sellerId,
            transactionType: transactionType,
            description: description,
            amount: amount,
            balanceBefore: balanceBefore,
            relatedEntityId: relatedEntityId,
            relatedEntityType: relatedEntityType);

        // Update seller balance using domain methods
        if (amount > 0)
        {
            seller.AddEarnings(amount);
        }
        else
        {
            seller.DeductFromAvailableBalance(Math.Abs(amount));
        }

        transaction.Complete();

        await context.Set<SellerTransaction>().AddAsync(transaction, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        transaction = await context.Set<SellerTransaction>()
            .AsNoTracking()
            .Include(t => t.Seller)
            .FirstOrDefaultAsync(t => t.Id == transaction.Id, cancellationToken);

        return mapper.Map<SellerTransactionDto>(transaction!);
    }

    public async Task<SellerTransactionDto?> GetTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await context.Set<SellerTransaction>()
            .AsNoTracking()
            .Include(t => t.Seller)
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken);

        if (transaction is null) return null;

        return mapper.Map<SellerTransactionDto>(transaction);
    }

    public async Task<PagedResult<SellerTransactionDto>> GetSellerTransactionsAsync(Guid sellerId, SellerTransactionType? transactionType = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > paginationConfig.MaxPageSize) pageSize = paginationConfig.MaxPageSize;
        if (page < 1) page = 1;

        IQueryable<SellerTransaction> query = context.Set<SellerTransaction>()
            .AsNoTracking()
            .Include(t => t.Seller)
            .Where(t => t.SellerId == sellerId);

        if (transactionType.HasValue)
        {
            query = query.Where(t => t.TransactionType == transactionType.Value);
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

        var transactionDtos = mapper.Map<IEnumerable<SellerTransactionDto>>(transactions).ToList();

        return new PagedResult<SellerTransactionDto>
        {
            Items = transactionDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<SellerInvoiceDto> GenerateInvoiceAsync(CreateSellerInvoiceDto dto, CancellationToken cancellationToken = default)
    {
        var seller = await context.Set<SellerProfile>()
            .Include(sp => sp.User)
            .FirstOrDefaultAsync(sp => sp.UserId == dto.SellerId, cancellationToken);

        if (seller is null)
        {
            throw new NotFoundException("Satıcı", dto.SellerId);
        }

        // Get commissions for the period
        var commissionStats = await context.Set<SellerCommission>()
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

        // Get payouts for the period
        var totalPayouts = await context.Set<CommissionPayout>()
            .AsNoTracking()
            .Where(p => p.SellerId == dto.SellerId &&
                  p.CreatedAt >= dto.PeriodStart &&
                  p.CreatedAt <= dto.PeriodEnd &&
                  p.Status == PayoutStatus.Completed)
            .SumAsync(p => p.NetAmount, cancellationToken);

        // Get orders for the period (for total earnings calculation)
        var totalEarnings = await (
            from o in context.Set<OrderEntity>().AsNoTracking()
            join oi in context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
            join p in context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
            where o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= dto.PeriodStart &&
                  o.CreatedAt <= dto.PeriodEnd &&
                  p.SellerId == dto.SellerId
            select oi.TotalPrice
        ).SumAsync(cancellationToken);

        var commissions = await context.Set<SellerCommission>()
            .AsNoTracking()
            .Include(c => c.Order)
            .Where(sc => sc.SellerId == dto.SellerId &&
                  sc.CreatedAt >= dto.PeriodStart &&
                  sc.CreatedAt <= dto.PeriodEnd)
            .ToListAsync(cancellationToken);

        var invoiceNumber = await GenerateInvoiceNumberAsync(dto.PeriodStart, cancellationToken);

        var invoice = SellerInvoice.Create(
            sellerId: dto.SellerId,
            invoiceNumber: invoiceNumber,
            periodStart: dto.PeriodStart,
            periodEnd: dto.PeriodEnd,
            totalEarnings: totalEarnings,
            totalCommissions: totalCommissions,
            totalPayouts: totalPayouts,
            platformFees: platformFees,
            netAmount: netCommissions - totalPayouts,
            notes: dto.Notes);

        List<InvoiceItemDto> invoiceItems = [];
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

        invoice.UpdateInvoiceData(JsonSerializer.Serialize(invoiceItems));

        await context.Set<SellerInvoice>().AddAsync(invoice, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        invoice = await context.Set<SellerInvoice>()
            .AsNoTracking()
            .Include(i => i.Seller)
            .FirstOrDefaultAsync(i => i.Id == invoice.Id, cancellationToken);

        return mapper.Map<SellerInvoiceDto>(invoice!);
    }

    public async Task<SellerInvoiceDto?> GetInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await context.Set<SellerInvoice>()
            .AsNoTracking()
            .Include(i => i.Seller)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);

        if (invoice is null) return null;

        return mapper.Map<SellerInvoiceDto>(invoice);
    }

    public async Task<PagedResult<SellerInvoiceDto>> GetSellerInvoicesAsync(Guid sellerId, SellerInvoiceStatus? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > paginationConfig.MaxPageSize) pageSize = paginationConfig.MaxPageSize;
        if (page < 1) page = 1;

        IQueryable<SellerInvoice> query = context.Set<SellerInvoice>()
            .AsNoTracking()
            .Include(i => i.Seller)
            .Where(i => i.SellerId == sellerId);

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var invoices = await query
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var invoiceDtos = mapper.Map<IEnumerable<SellerInvoiceDto>>(invoices).ToList();

        return new PagedResult<SellerInvoiceDto>
        {
            Items = invoiceDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> MarkInvoiceAsPaidAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await context.Set<SellerInvoice>()
            .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);

        if (invoice is null) return false;

        invoice.MarkAsPaid();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<SellerFinanceSummaryDto> GetSellerFinanceSummaryAsync(Guid sellerId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        startDate ??= DateTime.UtcNow.AddDays(-30);
        endDate ??= DateTime.UtcNow;

        var balance = await GetSellerBalanceAsync(sellerId, cancellationToken);

        // Recent transactions
        var transactions = await GetSellerTransactionsAsync(sellerId, null, startDate, endDate, 1, 10, cancellationToken);

        // Recent invoices
        var invoices = await GetSellerInvoicesAsync(sellerId, null, 1, 10, cancellationToken);

        // Earnings by month
        var earningsByMonth = await context.Set<SellerCommission>()
            .AsNoTracking()
            .Where(sc => sc.SellerId == sellerId &&
                  sc.CreatedAt >= startDate &&
                  sc.CreatedAt <= endDate)
            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
            .Select(g => new { Key = $"{g.Key.Year}-{g.Key.Month:D2}", Value = g.Sum(c => c.NetAmount) })
            .ToDictionaryAsync(x => x.Key, x => x.Value, cancellationToken);

        // Payouts by month
        var payoutsByMonth = await context.Set<CommissionPayout>()
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

    private async Task<string> GenerateInvoiceNumberAsync(DateTime periodStart, CancellationToken cancellationToken = default)
    {
        var yearMonth = periodStart.ToString("yyyyMM");
        var existingCount = await context.Set<SellerInvoice>()
            .AsNoTracking()
            .CountAsync(i => i.InvoiceNumber.StartsWith($"INV-{yearMonth}"), cancellationToken);

        return $"INV-{yearMonth}-{(existingCount + 1):D6}";
    }
}

