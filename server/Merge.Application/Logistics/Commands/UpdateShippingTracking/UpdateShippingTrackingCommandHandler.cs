using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Commands.UpdateShippingTracking;

public class UpdateShippingTrackingCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateShippingTrackingCommandHandler> logger,
    IOptions<ShippingSettings> shippingSettings) : IRequestHandler<UpdateShippingTrackingCommand, ShippingDto>
{
    private readonly ShippingSettings _shippingSettings = shippingSettings.Value;

    public async Task<ShippingDto> Handle(UpdateShippingTrackingCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating shipping tracking. ShippingId: {ShippingId}, TrackingNumber: {TrackingNumber}", request.ShippingId, request.TrackingNumber);

        var shipping = await context.Set<Shipping>()
            .FirstOrDefaultAsync(s => s.Id == request.ShippingId, cancellationToken);

        if (shipping == null)
        {
            logger.LogWarning("Shipping not found. ShippingId: {ShippingId}", request.ShippingId);
            throw new NotFoundException("Kargo kaydı", request.ShippingId);
        }

        shipping.Ship(request.TrackingNumber);
        
        // Varsayılan teslimat süresini configuration'dan al
        var estimatedDays = _shippingSettings.DefaultDeliveryTime.AverageDays;
        shipping.UpdateEstimatedDeliveryDate(DateTime.UtcNow.AddDays(estimatedDays));

        var order = await context.Set<OrderEntity>()
            .FirstOrDefaultAsync(o => o.Id == shipping.OrderId, cancellationToken);

        if (order != null)
        {
            order.Ship();
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

        logger.LogInformation("Shipping tracking updated successfully. ShippingId: {ShippingId}", request.ShippingId);

        return mapper.Map<ShippingDto>(updatedShipping);
    }
}

