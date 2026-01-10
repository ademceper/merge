using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Entities.Order;

namespace Merge.Application.Logistics.Commands.UpdateShippingTracking;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class UpdateShippingTrackingCommandHandler : IRequestHandler<UpdateShippingTrackingCommand, ShippingDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateShippingTrackingCommandHandler> _logger;

    public UpdateShippingTrackingCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateShippingTrackingCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ShippingDto> Handle(UpdateShippingTrackingCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating shipping tracking. ShippingId: {ShippingId}, TrackingNumber: {TrackingNumber}", request.ShippingId, request.TrackingNumber);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var shipping = await _context.Set<Shipping>()
            .FirstOrDefaultAsync(s => s.Id == request.ShippingId, cancellationToken);

        if (shipping == null)
        {
            _logger.LogWarning("Shipping not found. ShippingId: {ShippingId}", request.ShippingId);
            throw new NotFoundException("Kargo kaydı", request.ShippingId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        shipping.Ship(request.TrackingNumber);
        shipping.UpdateEstimatedDeliveryDate(DateTime.UtcNow.AddDays(3));

        // ✅ BOLUM 1.1: Rich Domain Model - Order status'unu güncelle
        var order = await _context.Set<OrderEntity>()
            .FirstOrDefaultAsync(o => o.Id == shipping.OrderId, cancellationToken);

        if (order != null)
        {
            order.Ship();
        }

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        // Notification ve email gönderimi domain event handler'lar tarafından yapılacak
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking + Include ile tek query'de getir
        var updatedShipping = await _context.Set<Shipping>()
            .AsNoTracking()
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.Id == request.ShippingId, cancellationToken);

        if (updatedShipping == null)
        {
            _logger.LogWarning("Shipping not found after update. ShippingId: {ShippingId}", request.ShippingId);
            throw new NotFoundException("Kargo", request.ShippingId);
        }

        _logger.LogInformation("Shipping tracking updated successfully. ShippingId: {ShippingId}", request.ShippingId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ShippingDto>(updatedShipping);
    }
}

