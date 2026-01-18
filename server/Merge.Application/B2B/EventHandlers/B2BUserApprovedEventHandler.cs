using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;

namespace Merge.Application.B2B.EventHandlers;


public class B2BUserApprovedEventHandler(
    ILogger<B2BUserApprovedEventHandler> logger,
    INotificationService? notificationService = null) : INotificationHandler<B2BUserApprovedEvent>
{

    public async Task Handle(B2BUserApprovedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "B2B user approved event received. B2BUserId: {B2BUserId}, UserId: {UserId}, OrganizationId: {OrganizationId}, ApprovedByUserId: {ApprovedByUserId}",
            notification.B2BUserId, notification.UserId, notification.OrganizationId, notification.ApprovedByUserId);

        try
        {
            // Email bildirimi gönder
            if (notificationService != null)
            {
                var createDto = new CreateNotificationDto(
                    notification.UserId,
                    NotificationType.System,
                    "B2B Hesabınız Onaylandı",
                    "B2B hesabınız başarıyla onaylandı. Artık toptan satış fiyatlarına ve özel indirimlere erişebilirsiniz.",
                    null,
                    null);
                await notificationService.CreateNotificationAsync(createDto, cancellationToken);
            }

            // TODO: İleride burada şunlar yapılabilir:
            // - Welcome email gönderimi
            // - Analytics tracking
            // - Cache invalidation
            // - External system sync (CRM, ERP)
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling B2BUserApprovedEvent. B2BUserId: {B2BUserId}, UserId: {UserId}",
                notification.B2BUserId, notification.UserId);
            throw;
        }
    }
}
