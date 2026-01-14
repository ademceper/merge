using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.ValueObjects;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Commands.CreateShipping;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class CreateShippingCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateShippingCommandHandler> logger) : IRequestHandler<CreateShippingCommand, ShippingDto>
{

    public async Task<ShippingDto> Handle(CreateShippingCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating shipping. OrderId: {OrderId}, Provider: {Provider}", request.OrderId, request.ShippingProvider);

        // ✅ PERFORMANCE: AsNoTracking - Check if order exists
        var order = await context.Set<OrderEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            logger.LogWarning("Order not found. OrderId: {OrderId}", request.OrderId);
            throw new NotFoundException("Sipariş", request.OrderId);
        }

        // ✅ PERFORMANCE: AsNoTracking - Check if shipping already exists
        var existingShipping = await context.Set<Shipping>()
            .AsNoTracking()
            .AnyAsync(s => s.OrderId == request.OrderId, cancellationToken);

        if (existingShipping)
        {
            logger.LogWarning("Shipping already exists for order. OrderId: {OrderId}", request.OrderId);
            throw new BusinessException("Bu sipariş için zaten bir kargo kaydı var.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var shippingCost = new Money(request.ShippingCost);
        var shipping = Shipping.Create(
            request.OrderId,
            request.ShippingProvider,
            shippingCost,
            null); // EstimatedDeliveryDate

        await context.Set<Shipping>().AddAsync(shipping, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking + Include ile tek query'de getir
        var createdShipping = await context.Set<Shipping>()
            .AsNoTracking()
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.Id == shipping.Id, cancellationToken);

        if (createdShipping == null)
        {
            logger.LogWarning("Shipping not found after creation. ShippingId: {ShippingId}", shipping.Id);
            throw new NotFoundException("Kargo", shipping.Id);
        }

        logger.LogInformation("Shipping created successfully. ShippingId: {ShippingId}, OrderId: {OrderId}", shipping.Id, request.OrderId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<ShippingDto>(createdShipping);
    }
}

