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
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.CalculateAndRecordCommission;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CalculateAndRecordCommissionCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, IOptions<SellerSettings> sellerSettings, ILogger<CalculateAndRecordCommissionCommandHandler> logger) : IRequestHandler<CalculateAndRecordCommissionCommand, SellerCommissionDto>
{
    private readonly SellerSettings sellerConfig = sellerSettings.Value;

    public async Task<SellerCommissionDto> Handle(CalculateAndRecordCommissionCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Calculating commission for OrderId: {OrderId}, OrderItemId: {OrderItemId}",
            request.OrderId, request.OrderItemId);

        var orderItem = await context.Set<OrderItem>()
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .FirstOrDefaultAsync(oi => oi.Id == request.OrderItemId && oi.OrderId == request.OrderId, cancellationToken);

        if (orderItem == null)
        {
            logger.LogWarning("Order item not found. OrderId: {OrderId}, OrderItemId: {OrderItemId}",
                request.OrderId, request.OrderItemId);
            throw new NotFoundException("Sipariş kalemi", request.OrderItemId);
        }

        if (!orderItem.Product.SellerId.HasValue)
        {
            logger.LogWarning("Product has no seller assigned. ProductId: {ProductId}",
                orderItem.Product.Id);
            throw new BusinessException("Ürüne atanmış satıcı yok.");
        }

        var sellerId = orderItem.Product.SellerId.Value;

        var existing = await context.Set<SellerCommission>()
            .AsNoTracking()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem)
            .FirstOrDefaultAsync(sc => sc.OrderItemId == request.OrderItemId, cancellationToken);

        if (existing != null)
        {
            logger.LogInformation("Commission already exists. CommissionId: {CommissionId}",
                existing.Id);
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
                // ✅ BOLUM 12.0: Magic number config'den - SellerSettings kullanımı
                commissionRate = sellerConfig.DefaultCommissionRateWhenNoTier;
                platformFeeRate = sellerConfig.DefaultPlatformFeeRate;
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

        await context.Set<SellerCommission>().AddAsync(commission, cancellationToken);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await unitOfWork.SaveChangesAsync(cancellationToken);

        commission = await context.Set<SellerCommission>()
            .AsNoTracking()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem)
            .FirstOrDefaultAsync(sc => sc.Id == commission.Id, cancellationToken);

        logger.LogInformation("Commission calculated and recorded. CommissionId: {CommissionId}, SellerId: {SellerId}, Amount: {Amount}",
            commission!.Id, sellerId, commission.NetAmount);

        return mapper.Map<SellerCommissionDto>(commission);
    }

    private async Task<CommissionTier?> GetTierForSalesAsync(decimal totalSales, CancellationToken cancellationToken)
    {
        var tier = await context.Set<CommissionTier>()
            .AsNoTracking()
            .Where(t => t.IsActive && t.MinSales <= totalSales && t.MaxSales >= totalSales)
            .OrderBy(t => t.Priority)
            .FirstOrDefaultAsync(cancellationToken);

        return tier;
    }
}
