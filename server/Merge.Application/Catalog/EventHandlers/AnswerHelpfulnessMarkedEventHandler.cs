using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;


public class AnswerHelpfulnessMarkedEventHandler(
    ILogger<AnswerHelpfulnessMarkedEventHandler> logger) : INotificationHandler<AnswerHelpfulnessMarkedEvent>
{

    public async Task Handle(AnswerHelpfulnessMarkedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Answer helpfulness marked event received. AnswerId: {AnswerId}, UserId: {UserId}",
            notification.AnswerId, notification.UserId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (product answers cache)
            // - Analytics tracking (answer helpfulness metrics)
            // - Notification gönderimi (answer owner'a helpfulness bildirimi)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling AnswerHelpfulnessMarkedEvent. AnswerId: {AnswerId}, UserId: {UserId}",
                notification.AnswerId, notification.UserId);
            throw;
        }
    }
}
