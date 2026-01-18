using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;


public class B2BUserCreatedEventHandler(
    ILogger<B2BUserCreatedEventHandler> logger) : INotificationHandler<B2BUserCreatedEvent>
{

    public async Task Handle(B2BUserCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "B2B user created event received. B2BUserId: {B2BUserId}, UserId: {UserId}, OrganizationId: {OrganizationId}, EmployeeId: {EmployeeId}, Department: {Department}, JobTitle: {JobTitle}, CreditLimit: {CreditLimit}",
            notification.B2BUserId, notification.UserId, notification.OrganizationId, notification.EmployeeId, notification.Department, notification.JobTitle, notification.CreditLimit);

        // TODO: İleride burada şunlar yapılabilir:
        // - Welcome email gönderimi (B2B kullanıcıya)
        // - Admin notification (yeni B2B kullanıcı kaydı)
        // - Analytics tracking
        // - Cache invalidation
        // - External system integration (CRM, ERP)
        // - Audit log

        await Task.CompletedTask;
    }
}
