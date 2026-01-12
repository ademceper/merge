using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Product Answer Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ProductAnswerCreatedEventHandler : INotificationHandler<ProductAnswerCreatedEvent>
{
    private readonly ILogger<ProductAnswerCreatedEventHandler> _logger;

    public ProductAnswerCreatedEventHandler(ILogger<ProductAnswerCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductAnswerCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Product answer created event received. AnswerId: {AnswerId}, QuestionId: {QuestionId}, UserId: {UserId}, IsSellerAnswer: {IsSellerAnswer}",
            notification.AnswerId, notification.QuestionId, notification.UserId, notification.IsSellerAnswer);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Email bildirimi gönderimi (question sahibine cevap bildirimi)
            // - Analytics tracking (answer creation metrics)
            // - Cache invalidation (product questions cache)
            // - Notification gönderimi (question sahibine)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ProductAnswerCreatedEvent. AnswerId: {AnswerId}, QuestionId: {QuestionId}",
                notification.AnswerId, notification.QuestionId);
            throw;
        }
    }
}
