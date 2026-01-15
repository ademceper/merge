using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Answer Helpfulness Marked Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class AnswerHelpfulnessMarkedEventHandler(
    ILogger<AnswerHelpfulnessMarkedEventHandler> logger) : INotificationHandler<AnswerHelpfulnessMarkedEvent>
{

    public async Task Handle(AnswerHelpfulnessMarkedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex,
                "Error handling AnswerHelpfulnessMarkedEvent. AnswerId: {AnswerId}, UserId: {UserId}",
                notification.AnswerId, notification.UserId);
            throw;
        }
    }
}
