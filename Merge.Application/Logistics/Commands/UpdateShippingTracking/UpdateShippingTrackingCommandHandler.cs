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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class UpdateShippingTrackingCommandHandler : IRequestHandler<UpdateShippingTrackingCommand, ShippingDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateShippingTrackingCommandHandler> _logger;
    private readonly ShippingSettings _shippingSettings;

    public UpdateShippingTrackingCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateShippingTrackingCommandHandler> logger,
        IOptions<ShippingSettings> shippingSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _shippingSettings = shippingSettings.Value;
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
        
        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan (BEST_PRACTICES_ANALIZI.md - BOLUM 2.1.4)
        // Varsayılan teslimat süresini configuration'dan al
        var estimatedDays = _shippingSettings.DefaultDeliveryTime.AverageDays;
        shipping.UpdateEstimatedDeliveryDate(DateTime.UtcNow.AddDays(estimatedDays));

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

