using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;

namespace Merge.Application.B2B.EventHandlers;

/// <summary>
/// B2B User Approved Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class B2BUserApprovedEventHandler : INotificationHandler<B2BUserApprovedEvent>
{
    private readonly ILogger<B2BUserApprovedEventHandler> _logger;
    private readonly INotificationService? _notificationService;

    public B2BUserApprovedEventHandler(
        ILogger<B2BUserApprovedEventHandler> logger,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(B2BUserApprovedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "B2B user approved event received. B2BUserId: {B2BUserId}, UserId: {UserId}, OrganizationId: {OrganizationId}, ApprovedByUserId: {ApprovedByUserId}",
            notification.B2BUserId, notification.UserId, notification.OrganizationId, notification.ApprovedByUserId);

        try
        {
            // Email bildirimi gönder
            if (_notificationService != null)
            {
                var createDto = new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.System,
                    "B2B Hesabınız Onaylandı",
                    "B2B hesabınız başarıyla onaylandı. Artık toptan satış fiyatlarına ve özel indirimlere erişebilirsiniz.",
                    null,
                    null);
                await _notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // TODO: İleride burada şunlar yapılabilir:
            // - Welcome email gönderimi
            // - Analytics tracking
            // - Cache invalidation
            // - External system sync (CRM, ERP)
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling B2BUserApprovedEvent. B2BUserId: {B2BUserId}, UserId: {UserId}",
                notification.B2BUserId, notification.UserId);
            throw;
        }
    }
}
