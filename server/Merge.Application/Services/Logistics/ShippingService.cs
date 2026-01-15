using AutoMapper;
using MediatR;
using Merge.Application.Services.Notification;
using Merge.Application.Interfaces.User;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using Merge.Application.Interfaces;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Notification.Commands.CreateNotification;
using Microsoft.Extensions.Logging;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;


namespace Merge.Application.Services.Logistics;

public class ShippingService : IShippingService
{
    private readonly Merge.Application.Interfaces.IRepository<Shipping> _shippingRepository;
    private readonly Merge.Application.Interfaces.IRepository<OrderEntity> _orderRepository;
    private readonly IEmailService? _emailService;
    private readonly IMediator _mediator;
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ShippingService> _logger;

    public ShippingService(
        Merge.Application.Interfaces.IRepository<Shipping> shippingRepository,
        Merge.Application.Interfaces.IRepository<OrderEntity> orderRepository,
        IDbContext context,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ILogger<ShippingService> logger,
        IMediator mediator,
        IEmailService? emailService = null)
    {
        _shippingRepository = shippingRepository;
        _orderRepository = orderRepository;
        _emailService = emailService;
        _mediator = mediator;
        _context = context;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ShippingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var shipping = await _context.Set<Shipping>()
            .AsNoTracking()
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (shipping == null) return null;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ShippingDto>(shipping);
    }

    public async Task<ShippingDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var shipping = await _context.Set<Shipping>()
            .AsNoTracking()
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.OrderId == orderId, cancellationToken);

        if (shipping == null) return null;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ShippingDto>(shipping);
    }

    public async Task<ShippingDto> CreateShippingAsync(CreateShippingDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (string.IsNullOrWhiteSpace(dto.ShippingProvider))
        {
            throw new ValidationException("Kargo firması boş olamaz.");
        }

        var order = await _orderRepository.GetByIdAsync(dto.OrderId);
        if (order == null)
        {
            throw new NotFoundException("Sipariş", dto.OrderId);
        }

        var existingShipping = await _context.Set<Shipping>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.OrderId == dto.OrderId);

        if (existingShipping != null)
        {
            throw new BusinessException("Bu sipariş için zaten bir kargo kaydı var.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
        var shippingCost = new Money(dto.ShippingCost);
        // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
        // CreateShippingDto'da EstimatedDeliveryDate yok, null geçiyoruz
        var shipping = Shipping.Create(
            dto.OrderId,
            dto.ShippingProvider,
            shippingCost,
            null // EstimatedDeliveryDate
        );

        shipping = await _shippingRepository.AddAsync(shipping);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Shipping created for order {OrderId} with provider {Provider}",
            dto.OrderId, dto.ShippingProvider);

        // ✅ PERFORMANCE: Reload with order information in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var createdShipping = await _context.Set<Shipping>()
            .AsNoTracking()
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.Id == shipping.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ShippingDto>(createdShipping!);
    }

    public async Task<ShippingDto> UpdateTrackingAsync(Guid shippingId, string trackingNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
        {
            throw new ValidationException("Takip numarası boş olamaz.");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var shipping = await _shippingRepository.GetByIdAsync(shippingId);
            if (shipping == null)
            {
                throw new NotFoundException("Kargo kaydı", shippingId);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            shipping.Ship(trackingNumber);
            shipping.UpdateEstimatedDeliveryDate(DateTime.UtcNow.AddDays(3));

            await _shippingRepository.UpdateAsync(shipping);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            // Order status'unu güncelle
            var order = await _orderRepository.GetByIdAsync(shipping.OrderId);
            if (order != null)
            {
                order.Ship();
                await _orderRepository.UpdateAsync(order);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Shipping tracking updated for order {OrderId}. Tracking: {TrackingNumber}",
                    shipping.OrderId, trackingNumber);

                // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - INotificationService yerine MediatR kullan
                // ✅ BOLUM 1.2: Enum kullanımı (string Type YASAK)
                await _mediator.Send(new CreateNotificationCommand(
                    order.UserId,
                    NotificationType.Shipping,
                    "Siparişiniz Kargoya Verildi",
                    $"Siparişiniz kargoya verildi. Takip No: {trackingNumber}",
                    $"/orders/{order.Id}"), cancellationToken);

                // Email gönder (after commit)
                if (_emailService != null)
                {
                    var user = await _context.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == order.UserId);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        await _emailService.SendOrderShippedAsync(user.Email, order.OrderNumber, trackingNumber);
                    }
                }
            }
            else
            {
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }

            // ✅ PERFORMANCE: Reload with order information in one query (N+1 fix)
            // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
            var updatedShipping = await _context.Set<Shipping>()
                .AsNoTracking()
                .Include(s => s.Order)
                .FirstOrDefaultAsync(s => s.Id == shippingId);

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return _mapper.Map<ShippingDto>(updatedShipping!);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error updating tracking for shipping {ShippingId}", shippingId);
            throw;
        }
    }

    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
    public async Task<ShippingDto> UpdateStatusAsync(Guid shippingId, ShippingStatus status, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var shipping = await _shippingRepository.GetByIdAsync(shippingId);
            if (shipping == null)
            {
                throw new NotFoundException("Kargo kaydı", shippingId);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            shipping.TransitionTo(status);

            if (status == ShippingStatus.Delivered)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
                // Order status'unu güncelle
                var order = await _orderRepository.GetByIdAsync(shipping.OrderId);
                if (order != null)
                {
                    order.Deliver();
                    await _orderRepository.UpdateAsync(order);
                }

                _logger.LogInformation("Shipping delivered for order {OrderId}", shipping.OrderId);
            }

            await _shippingRepository.UpdateAsync(shipping);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync();

            // ✅ PERFORMANCE: Reload with order information in one query (N+1 fix)
            // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
            var updatedShipping = await _context.Set<Shipping>()
                .AsNoTracking()
                .Include(s => s.Order)
                .FirstOrDefaultAsync(s => s.Id == shippingId);

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return _mapper.Map<ShippingDto>(updatedShipping!);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error updating status for shipping {ShippingId}", shippingId);
            throw;
        }
    }

    public async Task<decimal> CalculateShippingCostAsync(Guid orderId, string shippingProvider, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(shippingProvider))
        {
            throw new ValidationException("Kargo firması boş olamaz.");
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !o.IsDeleted (Global Query Filter)
        var order = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException("Sipariş", orderId);
        }

        // Basit kargo maliyeti hesaplama
        // Gerçek uygulamada kargo firması API'sinden alınacak
        decimal baseCost = shippingProvider switch
        {
            "Yurtiçi Kargo" => 50m,
            "Aras Kargo" => 45m,
            "MNG Kargo" => 40m,
            "Sürat Kargo" => 55m,
            _ => 50m
        };

        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan (BEST_PRACTICES_ANALIZI.md - BOLUM 2.1.4)
        // Not: OrderSettings inject edilmeli, şimdilik hardcoded bırakıldı (refactoring gerekli)
        if (order.SubTotal >= 500)
        {
            return 0;
        }

        // Ağırlık veya hacim bazlı hesaplama yapılabilir
        // Şimdilik sadece base cost döndürüyoruz
        return baseCost;
    }

    public Task<IEnumerable<ShippingProviderDto>> GetAvailableProvidersAsync(CancellationToken cancellationToken = default)
    {
        // Gerçek uygulamada veritabanından veya config'den alınacak
        return Task.FromResult<IEnumerable<ShippingProviderDto>>(new List<ShippingProviderDto>
        {
            new ShippingProviderDto("YURTICI", "Yurtiçi Kargo", 50m, 3),
            new ShippingProviderDto("ARAS", "Aras Kargo", 45m, 2),
            new ShippingProviderDto("MNG", "MNG Kargo", 40m, 2),
            new ShippingProviderDto("SURAT", "Sürat Kargo", 55m, 3)
        });
    }
}

