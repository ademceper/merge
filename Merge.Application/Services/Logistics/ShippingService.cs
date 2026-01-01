using AutoMapper;
using Merge.Application.Services.Notification;
using Merge.Application.Interfaces.User;
using OrderEntity = Merge.Domain.Entities.Order;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.Interfaces.Notification;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Logistics;
using Merge.Application.DTOs.Notification;
using Microsoft.Extensions.Logging;


namespace Merge.Application.Services.Logistics;

public class ShippingService : IShippingService
{
    private readonly IRepository<Shipping> _shippingRepository;
    private readonly IRepository<OrderEntity> _orderRepository;
    private readonly IEmailService? _emailService;
    private readonly INotificationService? _notificationService;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ShippingService> _logger;

    public ShippingService(
        IRepository<Shipping> shippingRepository,
        IRepository<OrderEntity> orderRepository,
        ApplicationDbContext context,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ILogger<ShippingService> logger,
        IEmailService? emailService = null,
        INotificationService? notificationService = null)
    {
        _shippingRepository = shippingRepository;
        _orderRepository = orderRepository;
        _emailService = emailService;
        _notificationService = notificationService;
        _context = context;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ShippingDto?> GetByIdAsync(Guid id)
    {
        var shipping = await _context.Shippings
            .AsNoTracking()
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (shipping == null) return null;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ShippingDto>(shipping);
    }

    public async Task<ShippingDto?> GetByOrderIdAsync(Guid orderId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var shipping = await _context.Shippings
            .AsNoTracking()
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.OrderId == orderId);

        if (shipping == null) return null;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ShippingDto>(shipping);
    }

    public async Task<ShippingDto> CreateShippingAsync(CreateShippingDto dto)
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

        var existingShipping = await _context.Shippings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.OrderId == dto.OrderId);

        if (existingShipping != null)
        {
            throw new BusinessException("Bu sipariş için zaten bir kargo kaydı var.");
        }

        var shipping = new Shipping
        {
            OrderId = dto.OrderId,
            ShippingProvider = dto.ShippingProvider,
            ShippingCost = dto.ShippingCost,
            Status = ShippingStatus.Preparing
        };

        shipping = await _shippingRepository.AddAsync(shipping);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Shipping created for order {OrderId} with provider {Provider}",
            dto.OrderId, dto.ShippingProvider);

        // ✅ PERFORMANCE: Reload with order information in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var createdShipping = await _context.Shippings
            .AsNoTracking()
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.Id == shipping.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ShippingDto>(createdShipping!);
    }

    public async Task<ShippingDto> UpdateTrackingAsync(Guid shippingId, string trackingNumber)
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

            shipping.TrackingNumber = trackingNumber;
            shipping.Status = ShippingStatus.Shipped;
            shipping.ShippedDate = DateTime.UtcNow;
            shipping.EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3);

            await _shippingRepository.UpdateAsync(shipping);

            // Order status'unu güncelle
            var order = await _orderRepository.GetByIdAsync(shipping.OrderId);
            if (order != null)
            {
                order.Status = OrderStatus.Shipped;
                order.ShippedDate = shipping.ShippedDate;
                await _orderRepository.UpdateAsync(order);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Shipping tracking updated for order {OrderId}. Tracking: {TrackingNumber}",
                    shipping.OrderId, trackingNumber);

                // Bildirim gönder (after commit)
                if (_notificationService != null)
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = order.UserId,
                        Type = "Shipping",
                        Title = "Siparişiniz Kargoya Verildi",
                        Message = $"Siparişiniz kargoya verildi. Takip No: {trackingNumber}",
                        Link = $"/orders/{order.Id}"
                    });
                }

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
            var updatedShipping = await _context.Shippings
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

    public async Task<ShippingDto> UpdateStatusAsync(Guid shippingId, string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ValidationException("Durum boş olamaz.");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var shipping = await _shippingRepository.GetByIdAsync(shippingId);
            if (shipping == null)
            {
                throw new NotFoundException("Kargo kaydı", shippingId);
            }

            shipping.Status = Enum.Parse<ShippingStatus>(status);

            if (status == "Delivered")
            {
                shipping.DeliveredDate = DateTime.UtcNow;

                // Order status'unu güncelle
                var order = await _orderRepository.GetByIdAsync(shipping.OrderId);
                if (order != null)
                {
                    order.Status = OrderStatus.Delivered;
                    order.DeliveredDate = shipping.DeliveredDate;
                    await _orderRepository.UpdateAsync(order);
                }

                _logger.LogInformation("Shipping delivered for order {OrderId}", shipping.OrderId);
            }

            await _shippingRepository.UpdateAsync(shipping);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            // ✅ PERFORMANCE: Reload with order information in one query (N+1 fix)
            // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
            var updatedShipping = await _context.Shippings
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

    public async Task<decimal> CalculateShippingCostAsync(Guid orderId, string shippingProvider)
    {
        if (string.IsNullOrWhiteSpace(shippingProvider))
        {
            throw new ValidationException("Kargo firması boş olamaz.");
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !o.IsDeleted (Global Query Filter)
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);

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

        // Ücretsiz kargo eşiği: 500 TL
        if (order.SubTotal >= 500)
        {
            return 0;
        }

        // Ağırlık veya hacim bazlı hesaplama yapılabilir
        // Şimdilik sadece base cost döndürüyoruz
        return baseCost;
    }

    public Task<IEnumerable<ShippingProviderDto>> GetAvailableProvidersAsync()
    {
        // Gerçek uygulamada veritabanından veya config'den alınacak
        return Task.FromResult<IEnumerable<ShippingProviderDto>>(new List<ShippingProviderDto>
        {
            new ShippingProviderDto { Code = "YURTICI", Name = "Yurtiçi Kargo", BaseCost = 50m, EstimatedDays = 3 },
            new ShippingProviderDto { Code = "ARAS", Name = "Aras Kargo", BaseCost = 45m, EstimatedDays = 2 },
            new ShippingProviderDto { Code = "MNG", Name = "MNG Kargo", BaseCost = 40m, EstimatedDays = 2 },
            new ShippingProviderDto { Code = "SURAT", Name = "Sürat Kargo", BaseCost = 55m, EstimatedDays = 3 }
        });
    }
}

