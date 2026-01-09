using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Entities.Order;

namespace Merge.Application.LiveCommerce.Commands.CreateOrderFromStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class CreateOrderFromStreamCommandHandler : IRequestHandler<CreateOrderFromStreamCommand, LiveStreamOrderDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateOrderFromStreamCommandHandler> _logger;

    public CreateOrderFromStreamCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateOrderFromStreamCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LiveStreamOrderDto> Handle(CreateOrderFromStreamCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating order from stream. StreamId: {StreamId}, OrderId: {OrderId}, ProductId: {ProductId}", 
            request.StreamId, request.OrderId, request.ProductId);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var order = await _context.Set<OrderEntity>()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order not found. OrderId: {OrderId}", request.OrderId);
            throw new NotFoundException("Sipariş", request.OrderId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var streamOrder = LiveStreamOrder.Create(
            request.StreamId,
            request.OrderId,
            order.TotalAmount,
            request.ProductId);

        await _context.Set<LiveStreamOrder>().AddAsync(streamOrder, cancellationToken);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var stream = await _context.Set<LiveStream>()
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            stream.AddOrder(order.TotalAmount);
        }

        // Update product stats if productId provided
        if (request.ProductId.HasValue)
        {
            var streamProduct = await _context.Set<LiveStreamProduct>()
                .FirstOrDefaultAsync(p => p.LiveStreamId == request.StreamId && p.ProductId == request.ProductId.Value, cancellationToken);

            if (streamProduct != null)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                streamProduct.IncrementOrderCount();
            }
        }

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order created from stream successfully. StreamId: {StreamId}, OrderId: {OrderId}", 
            request.StreamId, request.OrderId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<LiveStreamOrderDto>(streamOrder);
    }
}

