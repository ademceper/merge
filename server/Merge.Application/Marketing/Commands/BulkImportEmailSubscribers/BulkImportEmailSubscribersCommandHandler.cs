using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.ValueObjects;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.SharedKernel;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.BulkImportEmailSubscribers;

public class BulkImportEmailSubscribersCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<BulkImportEmailSubscribersCommandHandler> logger) : IRequestHandler<BulkImportEmailSubscribersCommand, int>
{

    public async Task<int> Handle(BulkImportEmailSubscribersCommand request, CancellationToken cancellationToken)
    {
        if (request.Subscribers is null || !request.Subscribers.Any())
        {
            return 0;
        }

        var emails = request.Subscribers.Select(s => s.Email.ToLower()).Distinct().ToList();
        var existingSubscribers = await context.Set<EmailSubscriber>()
            .Where(s => emails.Contains(s.Email.ToLower()))
            .ToDictionaryAsync(s => s.Email.ToLower(), cancellationToken);

        List<EmailSubscriber> newSubscribers = [];
        List<EmailSubscriber> updatedSubscribers = [];

        foreach (var subscriberDto in request.Subscribers)
        {
            var email = subscriberDto.Email.ToLower();
            
            if (existingSubscribers.TryGetValue(email, out var existing))
            {
                // Update existing
                if (existing.IsDeleted)
                {
                    existing.Restore();
                }

                if (!existing.IsSubscribed)
                {
                    existing.Subscribe();
                }

                existing.UpdateDetails(
                    firstName: subscriberDto.FirstName,
                    lastName: subscriberDto.LastName,
                    source: subscriberDto.Source,
                    tags: subscriberDto.Tags is not null ? JsonSerializer.Serialize(subscriberDto.Tags) : null,
                    customFields: subscriberDto.CustomFields is not null ? JsonSerializer.Serialize(subscriberDto.CustomFields) : null);
                
                updatedSubscribers.Add(existing);
            }
            else
            {
                var emailValue = new Email(subscriberDto.Email);
                var subscriber = EmailSubscriber.Create(
                    email: emailValue,
                    firstName: subscriberDto.FirstName,
                    lastName: subscriberDto.LastName,
                    userId: null,
                    source: subscriberDto.Source,
                    tags: subscriberDto.Tags is not null ? JsonSerializer.Serialize(subscriberDto.Tags) : null,
                    customFields: subscriberDto.CustomFields is not null ? JsonSerializer.Serialize(subscriberDto.CustomFields) : null);
                
                subscriber.Subscribe();
                
                newSubscribers.Add(subscriber);
            }
        }

        if (newSubscribers.Count > 0)
        {
            await context.Set<EmailSubscriber>().AddRangeAsync(newSubscribers, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Toplu email abone import işlemi tamamlandı. Yeni: {NewCount}, Güncellenen: {UpdatedCount}",
            newSubscribers.Count, updatedSubscribers.Count);

        return newSubscribers.Count + updatedSubscribers.Count;
    }
}
