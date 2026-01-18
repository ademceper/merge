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
using CreateNotificationCommand = Merge.Application.Notification.Commands.CreateNotification.CreateNotificationCommand;
using Microsoft.Extensions.Logging;
using Merge.Application.Common;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IShippingRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Ordering.Shipping>;
using IOrderRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Ordering.Order>;

namespace Merge.Application.Services.Logistics;

public class ShippingService(
    IShippingRepository shippingRepository,
    IOrderRepository orderRepository,
    IDbContext context,
    IMapper mapper,
    IUnitOfWork unitOfWork,
    ILogger<ShippingService> logger,
    IMediator mediator,
    IEmailService? emailService) : IShippingService
{

    public async Task<ShippingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var shipping = await context.Set<Shipping>()
            .AsNoTracking()
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (shipping is null) return null;

        return mapper.Map<ShippingDto>(shipping);
    }

    public async Task<ShippingDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var shipping = await context.Set<Shipping>()
            .AsNoTracking()
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.OrderId == orderId, cancellationToken);

        if (shipping is null) return null;

        return mapper.Map<ShippingDto>(shipping);
    }

    public async Task<ShippingDto> CreateShippingAsync(CreateShippingDto dto, CancellationToken cancellationToken = default)
    {
        if (dto is null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (string.IsNullOrWhiteSpace(dto.ShippingProvider))
        {
            throw new ValidationException("Kargo firması boş olamaz.");
        }

        var order = await orderRepository.GetByIdAsync(dto.OrderId);
        if (order is null)
        {
            throw new NotFoundException("Sipariş", dto.OrderId);
        }

        var existingShipping = await context.Set<Shipping>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.OrderId == dto.OrderId);

        if (existingShipping is not null)
        {
            throw new BusinessException("Bu sipariş için zaten bir kargo kaydı var.");
        }

        var shippingCost = new Money(dto.ShippingCost);
        // CreateShippingDto'da EstimatedDeliveryDate yok, null geçiyoruz
        var shipping = Shipping.Create(
            dto.OrderId,
            dto.ShippingProvider,
            shippingCost,
            null // EstimatedDeliveryDate
        );

        shipping = await shippingRepository.AddAsync(shipping);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Shipping created for order {OrderId} with provider {Provider}",
            dto.OrderId, dto.ShippingProvider);

        var createdShipping = await context.Set<Shipping>()
            .AsNoTracking()
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.Id == shipping.Id);

        return mapper.Map<ShippingDto>(createdShipping!);
    }

    public async Task<ShippingDto> UpdateTrackingAsync(Guid shippingId, string trackingNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
        {
            throw new ValidationException("Takip numarası boş olamaz.");
        }

        await unitOfWork.BeginTransactionAsync();
        try
        {
            var shipping = await shippingRepository.GetByIdAsync(shippingId);
            if (shipping is null)
            {
                throw new NotFoundException("Kargo kaydı", shippingId);
            }

            shipping.Ship(trackingNumber);
            shipping.UpdateEstimatedDeliveryDate(DateTime.UtcNow.AddDays(3));

            await shippingRepository.UpdateAsync(shipping);

            // Order status'unu güncelle
            var order = await orderRepository.GetByIdAsync(shipping.OrderId);
            if (order is not null)
            {
                order.Ship();
                await orderRepository.UpdateAsync(order);

                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitTransactionAsync();

                logger.LogInformation("Shipping tracking updated for order {OrderId}. Tracking: {TrackingNumber}",
                    shipping.OrderId, LogMasking.MaskTrackingNumber(trackingNumber));

                await mediator.Send(new CreateNotificationCommand(
                    order.UserId,
                    NotificationType.Shipping,
                    "Siparişiniz Kargoya Verildi",
                    $"Siparişiniz kargoya verildi. Takip No: {trackingNumber}",
                    $"/orders/{order.Id}"), cancellationToken);

                // Email gönder (after commit)
                if (emailService is not null)
                {
                    var user = await context.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == order.UserId);
                    if (user is not null && !string.IsNullOrEmpty(user.Email))
                    {
                        await emailService.SendOrderShippedAsync(user.Email, order.OrderNumber, trackingNumber);
                    }
                }
            }
            else
            {
                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitTransactionAsync();
            }

            var updatedShipping = await context.Set<Shipping>()
                .AsNoTracking()
                .Include(s => s.Order)
                .FirstOrDefaultAsync(s => s.Id == shippingId);

            return mapper.Map<ShippingDto>(updatedShipping!);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, "Error updating tracking for shipping {ShippingId}", shippingId);
            throw;
        }
    }

    public async Task<ShippingDto> UpdateStatusAsync(Guid shippingId, ShippingStatus status, CancellationToken cancellationToken = default)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var shipping = await shippingRepository.GetByIdAsync(shippingId);
            if (shipping is null)
            {
                throw new NotFoundException("Kargo kaydı", shippingId);
            }

            shipping.TransitionTo(status);

            if (status == ShippingStatus.Delivered)
            {
                // Order status'unu güncelle
                var order = await orderRepository.GetByIdAsync(shipping.OrderId);
                if (order is not null)
                {
                    order.Deliver();
                    await orderRepository.UpdateAsync(order);
                }

                logger.LogInformation("Shipping delivered for order {OrderId}", shipping.OrderId);
            }

            await shippingRepository.UpdateAsync(shipping);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync();

            var updatedShipping = await context.Set<Shipping>()
                .AsNoTracking()
                .Include(s => s.Order)
                .FirstOrDefaultAsync(s => s.Id == shippingId);

            return mapper.Map<ShippingDto>(updatedShipping!);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, "Error updating status for shipping {ShippingId}", shippingId);
            throw;
        }
    }

    public async Task<decimal> CalculateShippingCostAsync(Guid orderId, string shippingProvider, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(shippingProvider))
        {
            throw new ValidationException("Kargo firması boş olamaz.");
        }

        var order = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
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

