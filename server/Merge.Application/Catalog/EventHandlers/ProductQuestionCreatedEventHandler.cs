using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Product Question Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ProductQuestionCreatedEventHandler : INotificationHandler<ProductQuestionCreatedEvent>
{
    private readonly ILogger<ProductQuestionCreatedEventHandler> _logger;

    public ProductQuestionCreatedEventHandler(ILogger<ProductQuestionCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductQuestionCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Product question created event received. QuestionId: {QuestionId}, ProductId: {ProductId}, UserId: {UserId}, Question: {Question}",
            notification.QuestionId, notification.ProductId, notification.UserId, notification.Question);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Email bildirimi gönderimi (seller'a yeni soru bildirimi)
            // - Analytics tracking (question creation metrics)
            // - Cache invalidation (product questions cache)
            // - Notification gönderimi (seller'a)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ProductQuestionCreatedEvent. QuestionId: {QuestionId}, ProductId: {ProductId}",
                notification.QuestionId, notification.ProductId);
            throw;
        }
    }
}
