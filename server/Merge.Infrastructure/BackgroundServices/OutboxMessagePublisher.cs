using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using System.Text.Json;

namespace Merge.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that processes outbox messages for reliable event publishing.
/// Implements the Outbox Pattern to ensure at-least-once delivery of domain events.
/// </summary>
public class OutboxMessagePublisher(IServiceScopeFactory scopeFactory, ILogger<OutboxMessagePublisher> logger) : BackgroundService
{

    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);
    private readonly int _batchSize = 20;
    private readonly int _maxRetryCount = 3;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OutboxMessagePublisher started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        logger.LogInformation("OutboxMessagePublisher stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var messages = await dbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedOnUtc == null && m.RetryCount < _maxRetryCount)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(_batchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        logger.LogInformation("Processing {Count} outbox messages", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                var domainEvent = DeserializeDomainEvent(message);
                if (domainEvent != null)
                {
                    await mediator.Publish(domainEvent, cancellationToken);

                    message.ProcessedOnUtc = DateTime.UtcNow;
                    message.Error = null;

                    logger.LogDebug("Successfully processed outbox message {MessageId} of type {Type}",
                        message.Id, message.Type);
                }
                else
                {
                    message.Error = "Failed to deserialize domain event";
                    message.RetryCount++;

                    logger.LogWarning("Failed to deserialize outbox message {MessageId} of type {Type}",
                        message.Id, message.Type);
                }
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
                message.RetryCount++;

                logger.LogError(ex, "Error processing outbox message {MessageId} of type {Type}",
                    message.Id, message.Type);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private object? DeserializeDomainEvent(OutboxMessage message)
    {
        try
        {
            var type = Type.GetType(message.Type);
            if (type == null)
            {
                logger.LogWarning("Could not find type {Type} for outbox message {MessageId}",
                    message.Type, message.Id);
                return null;
            }

            return JsonSerializer.Deserialize(message.Content, type, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deserializing outbox message {MessageId}", message.Id);
            return null;
        }
    }
}
