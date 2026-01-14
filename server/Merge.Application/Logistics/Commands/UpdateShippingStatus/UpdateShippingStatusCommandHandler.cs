using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Commands.UpdateShippingStatus;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class UpdateShippingStatusCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateShippingStatusCommandHandler> logger) : IRequestHandler<UpdateShippingStatusCommand, ShippingDto>
{

    public async Task<ShippingDto> Handle(UpdateShippingStatusCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating shipping status. ShippingId: {ShippingId}, Status: {Status}", request.ShippingId, request.Status);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var shipping = await context.Set<Shipping>()
            .FirstOrDefaultAsync(s => s.Id == request.ShippingId, cancellationToken);

        if (shipping == null)
        {
            logger.LogWarning("Shipping not found. ShippingId: {ShippingId}", request.ShippingId);
            throw new NotFoundException("Kargo kaydı", request.ShippingId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        shipping.TransitionTo(request.Status);

        // ✅ BOLUM 7.1.6: Pattern Matching - Switch expression kullanımı
        if (request.Status == ShippingStatus.Delivered)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Order status'unu güncelle
            var order = await context.Set<OrderEntity>()
                .FirstOrDefaultAsync(o => o.Id == shipping.OrderId, cancellationToken);

            if (order != null)
            {
                order.Deliver();
            }

            logger.LogInformation("Shipping delivered for order {OrderId}", shipping.OrderId);
        }

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        // Notification ve email gönderimi domain event handler'lar tarafından yapılacak
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking + Include ile tek query'de getir
        var updatedShipping = await context.Set<Shipping>()
            .AsNoTracking()
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.Id == request.ShippingId, cancellationToken);

        if (updatedShipping == null)
        {
            logger.LogWarning("Shipping not found after update. ShippingId: {ShippingId}", request.ShippingId);
            throw new NotFoundException("Kargo", request.ShippingId);
        }

        logger.LogInformation("Shipping status updated successfully. ShippingId: {ShippingId}, Status: {Status}", request.ShippingId, request.Status);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<ShippingDto>(updatedShipping);
    }
}

