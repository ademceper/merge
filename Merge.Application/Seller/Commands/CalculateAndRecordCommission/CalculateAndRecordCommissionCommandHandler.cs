using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using OrderEntity = Merge.Domain.Entities.Order;

namespace Merge.Application.Seller.Commands.CalculateAndRecordCommission;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CalculateAndRecordCommissionCommandHandler : IRequestHandler<CalculateAndRecordCommissionCommand, SellerCommissionDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IOptions<SellerSettings> _sellerSettings;
    private readonly ILogger<CalculateAndRecordCommissionCommandHandler> _logger;

    public CalculateAndRecordCommissionCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IOptions<SellerSettings> sellerSettings,
        ILogger<CalculateAndRecordCommissionCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _sellerSettings = sellerSettings;
        _logger = logger;
    }

    public async Task<SellerCommissionDto> Handle(CalculateAndRecordCommissionCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Calculating commission for OrderId: {OrderId}, OrderItemId: {OrderItemId}",
            request.OrderId, request.OrderItemId);

        var orderItem = await _context.Set<OrderItem>()
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .FirstOrDefaultAsync(oi => oi.Id == request.OrderItemId && oi.OrderId == request.OrderId, cancellationToken);

        if (orderItem == null)
        {
            _logger.LogWarning("Order item not found. OrderId: {OrderId}, OrderItemId: {OrderItemId}",
                request.OrderId, request.OrderItemId);
            throw new NotFoundException("Sipariş kalemi", request.OrderItemId);
        }

        if (!orderItem.Product.SellerId.HasValue)
        {
            _logger.LogWarning("Product has no seller assigned. ProductId: {ProductId}",
                orderItem.Product.Id);
            throw new BusinessException("Ürüne atanmış satıcı yok.");
        }

        var sellerId = orderItem.Product.SellerId.Value;

        // Check if commission already exists
        var existing = await _context.Set<SellerCommission>()
            .AsNoTracking()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem)
            .FirstOrDefaultAsync(sc => sc.OrderItemId == request.OrderItemId, cancellationToken);

        if (existing != null)
        {
            _logger.LogInformation("Commission already exists. CommissionId: {CommissionId}",
                existing.Id);
            return _mapper.Map<SellerCommissionDto>(existing);
        }

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
                // ✅ BOLUM 12.0: Magic number config'den - SellerSettings kullanımı
                commissionRate = _sellerSettings.Value.DefaultCommissionRateWhenNoTier;
                platformFeeRate = _sellerSettings.Value.DefaultPlatformFeeRate;
            }
        }

        var orderAmount = orderItem.TotalPrice;

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var commission = SellerCommission.Create(
            sellerId: sellerId,
            orderId: request.OrderId,
            orderItemId: request.OrderItemId,
            orderAmount: orderAmount,
            commissionRate: commissionRate,
            platformFee: orderAmount * (platformFeeRate / 100));

        await _context.Set<SellerCommission>().AddAsync(commission, cancellationToken);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        commission = await _context.Set<SellerCommission>()
            .AsNoTracking()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem)
            .FirstOrDefaultAsync(sc => sc.Id == commission.Id, cancellationToken);

        _logger.LogInformation("Commission calculated and recorded. CommissionId: {CommissionId}, SellerId: {SellerId}, Amount: {Amount}",
            commission!.Id, sellerId, commission.NetAmount);

        return _mapper.Map<SellerCommissionDto>(commission);
    }

    private async Task<CommissionTier?> GetTierForSalesAsync(decimal totalSales, CancellationToken cancellationToken)
    {
        var tier = await _context.Set<CommissionTier>()
            .AsNoTracking()
            .Where(t => t.IsActive && t.MinSales <= totalSales && t.MaxSales >= totalSales)
            .OrderBy(t => t.Priority)
            .FirstOrDefaultAsync(cancellationToken);

        return tier;
    }
}
