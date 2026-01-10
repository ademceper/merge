using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using OrderEntity = Merge.Domain.Entities.Order;

namespace Merge.Application.Logistics.Commands.UpdateShippingStatus;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class UpdateShippingStatusCommandHandler : IRequestHandler<UpdateShippingStatusCommand, ShippingDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateShippingStatusCommandHandler> _logger;

    public UpdateShippingStatusCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateShippingStatusCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ShippingDto> Handle(UpdateShippingStatusCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating shipping status. ShippingId: {ShippingId}, Status: {Status}", request.ShippingId, request.Status);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var shipping = await _context.Set<Shipping>()
            .FirstOrDefaultAsync(s => s.Id == request.ShippingId, cancellationToken);

        if (shipping == null)
        {
            _logger.LogWarning("Shipping not found. ShippingId: {ShippingId}", request.ShippingId);
            throw new NotFoundException("Kargo kaydı", request.ShippingId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        shipping.TransitionTo(request.Status);

        if (request.Status == ShippingStatus.Delivered)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Order status'unu güncelle
            var order = await _context.Set<OrderEntity>()
                .FirstOrDefaultAsync(o => o.Id == shipping.OrderId, cancellationToken);

            if (order != null)
            {
                order.Deliver();
            }

            _logger.LogInformation("Shipping delivered for order {OrderId}", shipping.OrderId);
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

        _logger.LogInformation("Shipping status updated successfully. ShippingId: {ShippingId}, Status: {Status}", request.ShippingId, request.Status);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ShippingDto>(updatedShipping);
    }
}

