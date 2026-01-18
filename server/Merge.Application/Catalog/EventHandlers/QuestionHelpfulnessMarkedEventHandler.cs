using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;


public class QuestionHelpfulnessMarkedEventHandler(
    ILogger<QuestionHelpfulnessMarkedEventHandler> logger) : INotificationHandler<QuestionHelpfulnessMarkedEvent>
{

    public async Task Handle(QuestionHelpfulnessMarkedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Question helpfulness marked event received. QuestionId: {QuestionId}, UserId: {UserId}",
            notification.QuestionId, notification.UserId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (product questions cache)
            // - Analytics tracking (question helpfulness metrics)
            // - Notification gönderimi (question owner'a helpfulness bildirimi)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling QuestionHelpfulnessMarkedEvent. QuestionId: {QuestionId}, UserId: {UserId}",
                notification.QuestionId, notification.UserId);
            throw;
        }
    }
}
