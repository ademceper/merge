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

public class UpdateShippingStatusCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateShippingStatusCommandHandler> logger) : IRequestHandler<UpdateShippingStatusCommand, ShippingDto>
{

    public async Task<ShippingDto> Handle(UpdateShippingStatusCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating shipping status. ShippingId: {ShippingId}, Status: {Status}", request.ShippingId, request.Status);

        var shipping = await context.Set<Shipping>()
            .FirstOrDefaultAsync(s => s.Id == request.ShippingId, cancellationToken);

        if (shipping == null)
        {
            logger.LogWarning("Shipping not found. ShippingId: {ShippingId}", request.ShippingId);
            throw new NotFoundException("Kargo kaydı", request.ShippingId);
        }

        shipping.TransitionTo(request.Status);

        if (request.Status == ShippingStatus.Delivered)
        {
            var order = await context.Set<OrderEntity>()
                .FirstOrDefaultAsync(o => o.Id == shipping.OrderId, cancellationToken);

            if (order != null)
            {
                order.Deliver();
            }

            logger.LogInformation("Shipping delivered for order {OrderId}", shipping.OrderId);
        }

        // Notification ve email gönderimi domain event handler'lar tarafından yapılacak
        await unitOfWork.SaveChangesAsync(cancellationToken);

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

        return mapper.Map<ShippingDto>(updatedShipping);
    }
}

