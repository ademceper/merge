using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.LiveCommerce.Commands.CreateOrderFromStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class CreateOrderFromStreamCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateOrderFromStreamCommandHandler> logger) : IRequestHandler<CreateOrderFromStreamCommand, LiveStreamOrderDto>
{
    public async Task<LiveStreamOrderDto> Handle(CreateOrderFromStreamCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating order from stream. StreamId: {StreamId}, OrderId: {OrderId}, ProductId: {ProductId}", 
            request.StreamId, request.OrderId, request.ProductId);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var order = await context.Set<OrderEntity>()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            logger.LogWarning("Order not found. OrderId: {OrderId}", request.OrderId);
            throw new NotFoundException("Sipariş", request.OrderId);
        }

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var stream = await context.Set<LiveStream>()
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream == null)
        {
            logger.LogWarning("Stream not found. StreamId: {StreamId}", request.StreamId);
            throw new NotFoundException("Canlı yayın", request.StreamId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var streamOrder = LiveStreamOrder.Create(
            request.StreamId,
            request.OrderId,
            order.TotalAmount,
            request.ProductId);

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        // Aggregate root üzerinden order ekleme (encapsulation)
        // AddOrder method'u içinde OrderCount++ ve Revenue güncellemesi yapılıyor
        stream.AddOrder(streamOrder);

        // Update product stats if productId provided
        if (request.ProductId.HasValue)
        {
            var streamProduct = await context.Set<LiveStreamProduct>()
                .FirstOrDefaultAsync(p => p.LiveStreamId == request.StreamId && p.ProductId == request.ProductId.Value, cancellationToken);

            if (streamProduct != null)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                streamProduct.IncrementOrderCount();
            }
        }

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Order created from stream successfully. StreamId: {StreamId}, OrderId: {OrderId}", 
            request.StreamId, request.OrderId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<LiveStreamOrderDto>(streamOrder);
    }
}

