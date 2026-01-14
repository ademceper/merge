using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Question Helpfulness Marked Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class QuestionHelpfulnessMarkedEventHandler : INotificationHandler<QuestionHelpfulnessMarkedEvent>
{
    private readonly ILogger<QuestionHelpfulnessMarkedEventHandler> _logger;

    public QuestionHelpfulnessMarkedEventHandler(ILogger<QuestionHelpfulnessMarkedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(QuestionHelpfulnessMarkedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling QuestionHelpfulnessMarkedEvent. QuestionId: {QuestionId}, UserId: {UserId}",
                notification.QuestionId, notification.UserId);
            throw;
        }
    }
}
