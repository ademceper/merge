using Microsoft.EntityFrameworkCore;
using UserEntity = Merge.Domain.Entities.User;
using OrderEntity = Merge.Domain.Entities.Order;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Seller;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text.Json;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Services.Seller;

public class SellerFinanceService : ISellerFinanceService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public SellerFinanceService(ApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<SellerBalanceDto> GetSellerBalanceAsync(Guid sellerId)
    {
        var seller = await _context.SellerProfiles
            .Include(sp => sp.User)
            .FirstOrDefaultAsync(sp => sp.UserId == sellerId && !sp.IsDeleted);

        if (seller == null)
        {
            throw new NotFoundException("Satıcı", sellerId);
        }

        // Calculate in-transit balance (payouts being processed)
        var inTransitBalance = await _context.Set<CommissionPayout>()
            .Where(p => p.SellerId == sellerId && 
                   (p.Status == PayoutStatus.Pending || p.Status == PayoutStatus.Processing) &&
                   !p.IsDeleted)
            .SumAsync(p => p.TotalAmount);

        // Calculate total payouts
        var totalPayouts = await _context.Set<CommissionPayout>()
            .Where(p => p.SellerId == sellerId && 
                   p.Status == PayoutStatus.Completed &&
                   !p.IsDeleted)
            .SumAsync(p => p.NetAmount);

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

    public async Task<decimal> GetAvailableBalanceAsync(Guid sellerId)
    {
        var seller = await _context.SellerProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == sellerId && !sp.IsDeleted);

        return seller?.AvailableBalance ?? 0;
    }

    public async Task<decimal> GetPendingBalanceAsync(Guid sellerId)
    {
        var seller = await _context.SellerProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == sellerId && !sp.IsDeleted);

        return seller?.PendingBalance ?? 0;
    }

    public async Task<SellerTransactionDto> CreateTransactionAsync(Guid sellerId, string transactionType, decimal amount, string description, Guid? relatedEntityId = null, string? relatedEntityType = null)
    {
        var seller = await _context.SellerProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == sellerId && !sp.IsDeleted);

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
            Status = "Completed"
        };

        await _context.Set<SellerTransaction>().AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        return await MapToTransactionDto(transaction);
    }

    public async Task<SellerTransactionDto?> GetTransactionAsync(Guid transactionId)
    {
        var transaction = await _context.Set<SellerTransaction>()
            .Include(t => t.Seller)
            .FirstOrDefaultAsync(t => t.Id == transactionId && !t.IsDeleted);

        return transaction != null ? await MapToTransactionDto(transaction) : null;
    }

    public async Task<IEnumerable<SellerTransactionDto>> GetSellerTransactionsAsync(Guid sellerId, string? transactionType = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
    {
        var query = _context.Set<SellerTransaction>()
            .Include(t => t.Seller)
            .Where(t => t.SellerId == sellerId && !t.IsDeleted);

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

        var transactions = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new List<SellerTransactionDto>();
        foreach (var transaction in transactions)
        {
            result.Add(await MapToTransactionDto(transaction));
        }
        return result;
    }

    public async Task<SellerInvoiceDto> GenerateInvoiceAsync(CreateSellerInvoiceDto dto)
    {
        var seller = await _context.SellerProfiles
            .Include(sp => sp.User)
            .FirstOrDefaultAsync(sp => sp.UserId == dto.SellerId && !sp.IsDeleted);

        if (seller == null)
        {
            throw new NotFoundException("Satıcı", dto.SellerId);
        }

        // Get commissions for the period
        var commissions = await _context.Set<SellerCommission>()
            .Where(sc => sc.SellerId == dto.SellerId &&
                  sc.CreatedAt >= dto.PeriodStart &&
                  sc.CreatedAt <= dto.PeriodEnd &&
                  !sc.IsDeleted)
            .ToListAsync();

        var totalCommissions = commissions.Sum(c => c.CommissionAmount);
        var platformFees = commissions.Sum(c => c.PlatformFee);
        var netCommissions = commissions.Sum(c => c.NetAmount);

        // Get payouts for the period
        var payouts = await _context.Set<CommissionPayout>()
            .Where(p => p.SellerId == dto.SellerId &&
                  p.CreatedAt >= dto.PeriodStart &&
                  p.CreatedAt <= dto.PeriodEnd &&
                  !p.IsDeleted)
            .ToListAsync();

        var totalPayouts = payouts.Where(p => p.Status == PayoutStatus.Completed).Sum(p => p.NetAmount);

        // Get orders for the period (for total earnings calculation)
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => !o.IsDeleted &&
                  o.PaymentStatus == "Paid" &&
                  o.CreatedAt >= dto.PeriodStart &&
                  o.CreatedAt <= dto.PeriodEnd &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == dto.SellerId))
            .ToListAsync();

        var totalEarnings = orders.Sum(o => o.OrderItems
            .Where(oi => oi.Product.SellerId == dto.SellerId)
            .Sum(oi => oi.TotalPrice));

        var invoiceNumber = await GenerateInvoiceNumberAsync(dto.PeriodStart);

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
            Status = "Draft",
            Notes = dto.Notes
        };

        // Create invoice items
        var invoiceItems = commissions.Select(c => new InvoiceItemDto
        {
            Description = $"Commission for Order #{c.Order.OrderNumber}",
            Quantity = 1,
            UnitPrice = c.CommissionAmount,
            TotalPrice = c.CommissionAmount
        }).ToList();

        invoice.InvoiceData = JsonSerializer.Serialize(invoiceItems);

        await _context.Set<SellerInvoice>().AddAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        return await MapToInvoiceDto(invoice);
    }

    public async Task<SellerInvoiceDto?> GetInvoiceAsync(Guid invoiceId)
    {
        var invoice = await _context.Set<SellerInvoice>()
            .Include(i => i.Seller)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

        return invoice != null ? await MapToInvoiceDto(invoice) : null;
    }

    public async Task<IEnumerable<SellerInvoiceDto>> GetSellerInvoicesAsync(Guid sellerId, string? status = null, int page = 1, int pageSize = 20)
    {
        var query = _context.Set<SellerInvoice>()
            .Include(i => i.Seller)
            .Where(i => i.SellerId == sellerId && !i.IsDeleted);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(i => i.Status == status);
        }

        var invoices = await query
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new List<SellerInvoiceDto>();
        foreach (var invoice in invoices)
        {
            result.Add(await MapToInvoiceDto(invoice));
        }
        return result;
    }

    public async Task<bool> MarkInvoiceAsPaidAsync(Guid invoiceId)
    {
        var invoice = await _context.Set<SellerInvoice>()
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

        if (invoice == null) return false;

        invoice.Status = "Paid";
        invoice.PaidAt = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<SellerFinanceSummaryDto> GetSellerFinanceSummaryAsync(Guid sellerId, DateTime? startDate = null, DateTime? endDate = null)
    {
        startDate ??= DateTime.UtcNow.AddDays(-30);
        endDate ??= DateTime.UtcNow;

        var balance = await GetSellerBalanceAsync(sellerId);

        // Recent transactions
        var transactions = await GetSellerTransactionsAsync(sellerId, null, startDate, endDate, 1, 10);

        // Recent invoices
        var invoices = await GetSellerInvoicesAsync(sellerId, null, 1, 10);

        // Earnings by month
        var commissions = await _context.Set<SellerCommission>()
            .Where(sc => sc.SellerId == sellerId &&
                  sc.CreatedAt >= startDate &&
                  sc.CreatedAt <= endDate &&
                  !sc.IsDeleted)
            .ToListAsync();

        var earningsByMonth = commissions
            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
            .ToDictionary(
                g => $"{g.Key.Year}-{g.Key.Month:D2}",
                g => g.Sum(c => c.NetAmount)
            );

        // Payouts by month
        var payouts = await _context.Set<CommissionPayout>()
            .Where(p => p.SellerId == sellerId &&
                  p.CreatedAt >= startDate &&
                  p.CreatedAt <= endDate &&
                  p.Status == PayoutStatus.Completed &&
                  !p.IsDeleted)
            .ToListAsync();

        var payoutsByMonth = payouts
            .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
            .ToDictionary(
                g => $"{g.Key.Year}-{g.Key.Month:D2}",
                g => g.Sum(p => p.NetAmount)
            );

        return new SellerFinanceSummaryDto
        {
            SellerId = sellerId,
            Balance = balance,
            RecentTransactions = transactions.ToList(),
            RecentInvoices = invoices.ToList(),
            EarningsByMonth = earningsByMonth,
            PayoutsByMonth = payoutsByMonth
        };
    }

    private async Task<string> GenerateInvoiceNumberAsync(DateTime periodStart)
    {
        var yearMonth = periodStart.ToString("yyyyMM");
        var existingCount = await _context.Set<SellerInvoice>()
            .CountAsync(i => i.InvoiceNumber.StartsWith($"INV-{yearMonth}") && !i.IsDeleted);

        return $"INV-{yearMonth}-{(existingCount + 1):D6}";
    }

    private async Task<SellerTransactionDto> MapToTransactionDto(SellerTransaction transaction)
    {
        await _context.Entry(transaction)
            .Reference(t => t.Seller)
            .LoadAsync();

        return new SellerTransactionDto
        {
            Id = transaction.Id,
            SellerId = transaction.SellerId,
            TransactionType = transaction.TransactionType,
            Description = transaction.Description,
            Amount = transaction.Amount,
            BalanceBefore = transaction.BalanceBefore,
            BalanceAfter = transaction.BalanceAfter,
            RelatedEntityId = transaction.RelatedEntityId,
            RelatedEntityType = transaction.RelatedEntityType,
            Status = transaction.Status,
            CreatedAt = transaction.CreatedAt
        };
    }

    private async Task<SellerInvoiceDto> MapToInvoiceDto(SellerInvoice invoice)
    {
        await _context.Entry(invoice)
            .Reference(i => i.Seller)
            .LoadAsync();

        var sellerProfile = await _context.SellerProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == invoice.SellerId && !sp.IsDeleted);

        var items = !string.IsNullOrEmpty(invoice.InvoiceData)
            ? JsonSerializer.Deserialize<List<InvoiceItemDto>>(invoice.InvoiceData) ?? new List<InvoiceItemDto>()
            : new List<InvoiceItemDto>();

        return new SellerInvoiceDto
        {
            Id = invoice.Id,
            SellerId = invoice.SellerId,
            SellerName = sellerProfile?.StoreName ?? string.Empty,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDate = invoice.InvoiceDate,
            PeriodStart = invoice.PeriodStart,
            PeriodEnd = invoice.PeriodEnd,
            TotalEarnings = invoice.TotalEarnings,
            TotalCommissions = invoice.TotalCommissions,
            TotalPayouts = invoice.TotalPayouts,
            PlatformFees = invoice.PlatformFees,
            NetAmount = invoice.NetAmount,
            Status = invoice.Status,
            PaidAt = invoice.PaidAt,
            Items = items,
            CreatedAt = invoice.CreatedAt
        };
    }
}

