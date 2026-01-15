using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Ordering;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.GenerateInvoice;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GenerateInvoiceCommandHandler : IRequestHandler<GenerateInvoiceCommand, SellerInvoiceDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GenerateInvoiceCommandHandler> _logger;

    public GenerateInvoiceCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GenerateInvoiceCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SellerInvoiceDto> Handle(GenerateInvoiceCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Generating invoice. SellerId: {SellerId}, Period: {PeriodStart} - {PeriodEnd}",
            request.Dto.SellerId, request.Dto.PeriodStart, request.Dto.PeriodEnd);

        // ✅ PERFORMANCE: Removed manual !sp.IsDeleted (Global Query Filter)
        var seller = await _context.Set<SellerProfile>()
            .Include(sp => sp.User)
            .FirstOrDefaultAsync(sp => sp.UserId == request.Dto.SellerId, cancellationToken);

        if (seller == null)
        {
            _logger.LogWarning("Seller not found. SellerId: {SellerId}", request.Dto.SellerId);
            throw new NotFoundException("Satıcı", request.Dto.SellerId);
        }

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // Get commissions for the period
        var commissionStats = await _context.Set<SellerCommission>()
            .AsNoTracking()
            .Where(sc => sc.SellerId == request.Dto.SellerId &&
                  sc.CreatedAt >= request.Dto.PeriodStart &&
                  sc.CreatedAt <= request.Dto.PeriodEnd)
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
            .Where(p => p.SellerId == request.Dto.SellerId &&
                  p.CreatedAt >= request.Dto.PeriodStart &&
                  p.CreatedAt <= request.Dto.PeriodEnd &&
                  p.Status == PayoutStatus.Completed)
            .SumAsync(p => p.NetAmount, cancellationToken);

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Explicit Join yaklaşımı - tek sorgu (N+1 fix)
        // Get orders for the period (for total earnings calculation)
        var totalEarnings = await (
            from o in _context.Set<OrderEntity>().AsNoTracking()
            join oi in _context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
            join p in _context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
            where o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= request.Dto.PeriodStart &&
                  o.CreatedAt <= request.Dto.PeriodEnd &&
                  p.SellerId == request.Dto.SellerId
            select oi.TotalPrice
        ).SumAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load commissions for invoice items (N+1 fix)
        var commissions = await _context.Set<SellerCommission>()
            .AsNoTracking()
            .Include(c => c.Order)
            .Where(sc => sc.SellerId == request.Dto.SellerId &&
                  sc.CreatedAt >= request.Dto.PeriodStart &&
                  sc.CreatedAt <= request.Dto.PeriodEnd)
            .ToListAsync(cancellationToken);

        var invoiceNumber = await GenerateInvoiceNumberAsync(request.Dto.PeriodStart, cancellationToken);

        // ✅ FIX: ToListAsync() sonrası Select().ToList() YASAK - foreach ile DTO oluştur
        var invoiceItems = new List<InvoiceItemDto>();
        foreach (var commission in commissions)
        {
            invoiceItems.Add(new InvoiceItemDto
            {
                CommissionId = commission.Id,
                OrderId = commission.OrderId,
                OrderNumber = commission.Order?.OrderNumber ?? string.Empty,
                CommissionAmount = commission.CommissionAmount,
                PlatformFee = commission.PlatformFee,
                NetAmount = commission.NetAmount,
                CreatedAt = commission.CreatedAt
            });
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var invoice = SellerInvoice.Create(
            sellerId: request.Dto.SellerId,
            invoiceNumber: invoiceNumber,
            periodStart: request.Dto.PeriodStart,
            periodEnd: request.Dto.PeriodEnd,
            totalEarnings: totalEarnings,
            totalCommissions: totalCommissions,
            totalPayouts: totalPayouts,
            platformFees: platformFees,
            netAmount: netCommissions - totalPayouts,
            notes: request.Dto.Notes,
            invoiceData: JsonSerializer.Serialize(invoiceItems));

        await _context.Set<SellerInvoice>().AddAsync(invoice, cancellationToken);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        invoice = await _context.Set<SellerInvoice>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(i => i.Seller)
            .FirstOrDefaultAsync(i => i.Id == invoice.Id, cancellationToken);

        _logger.LogInformation("Invoice generated. InvoiceId: {InvoiceId}, InvoiceNumber: {InvoiceNumber}",
            invoice!.Id, invoiceNumber);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<SellerInvoiceDto>(invoice);
    }

    private async Task<string> GenerateInvoiceNumberAsync(DateTime periodStart, CancellationToken cancellationToken)
    {
        var year = periodStart.Year;
        var month = periodStart.Month;
        var prefix = $"INV-{year}{month:D2}";

        var lastInvoice = await _context.Set<SellerInvoice>()
            .AsNoTracking()
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync(cancellationToken);

        int nextNumber = 1;
        if (lastInvoice != null)
        {
            var numberPart = lastInvoice.InvoiceNumber.Substring(prefix.Length + 1);
            if (int.TryParse(numberPart, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}-{nextNumber:D4}";
    }
}
