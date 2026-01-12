using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;

/// <summary>
/// Purchase Order Submitted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class PurchaseOrderSubmittedEventHandler : INotificationHandler<PurchaseOrderSubmittedEvent>
{
    private readonly ILogger<PurchaseOrderSubmittedEventHandler> _logger;
    private readonly INotificationService? _notificationService;

    public PurchaseOrderSubmittedEventHandler(
        ILogger<PurchaseOrderSubmittedEventHandler> logger,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(PurchaseOrderSubmittedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Purchase order submitted event received. PurchaseOrderId: {PurchaseOrderId}, OrganizationId: {OrganizationId}, PONumber: {PONumber}, TotalAmount: {TotalAmount}",
            notification.PurchaseOrderId, notification.OrganizationId, notification.PONumber, notification.TotalAmount);

        try
        {
            // Email bildirimi gönder (admin'lere)
            if (_notificationService != null)
            {
                // TODO: Admin user ID'lerini al ve notification gönder
                // var adminUsers = await _userService.GetAdminUsersAsync(cancellationToken);
                // foreach (var admin in adminUsers)
                // {
                //     var createDto = new CreateNotificationDto(
                //         admin.Id,
                //         NotificationType.Order,
                //         "Yeni Satın Alma Siparişi",
                //         $"Yeni bir satın alma siparişi gönderildi. PO No: {notification.PONumber}",
                //         null,
                //         null);
                //     await _notificationService.CreateNotificationAsync(createDto, cancellationToken);
                // }
            }

            // TODO: İleride burada şunlar yapılabilir:
            // - Email notification (admin'lere)
            // - Analytics tracking
            // - Audit log
            // - External system sync (ERP)
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling PurchaseOrderSubmittedEvent. PurchaseOrderId: {PurchaseOrderId}, PONumber: {PONumber}",
                notification.PurchaseOrderId, notification.PONumber);
            throw;
        }
    }
}
