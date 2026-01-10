using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.ValueObjects;
using System.Text.Json;

namespace Merge.Application.Marketing.Commands.BulkImportEmailSubscribers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class BulkImportEmailSubscribersCommandHandler : IRequestHandler<BulkImportEmailSubscribersCommand, int>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BulkImportEmailSubscribersCommandHandler> _logger;

    public BulkImportEmailSubscribersCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<BulkImportEmailSubscribersCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<int> Handle(BulkImportEmailSubscribersCommand request, CancellationToken cancellationToken)
    {
        if (request.Subscribers == null || !request.Subscribers.Any())
        {
            return 0;
        }

        // ✅ PERFORMANCE: Batch load existing subscribers (N+1 fix)
        var emails = request.Subscribers.Select(s => s.Email.ToLower()).Distinct().ToList();
        var existingSubscribers = await _context.Set<EmailSubscriber>()
            .Where(s => emails.Contains(s.Email.ToLower()))
            .ToDictionaryAsync(s => s.Email.ToLower(), cancellationToken);

        var newSubscribers = new List<EmailSubscriber>();
        var updatedSubscribers = new List<EmailSubscriber>();

        foreach (var subscriberDto in request.Subscribers)
        {
            var email = subscriberDto.Email.ToLower();
            
            if (existingSubscribers.TryGetValue(email, out var existing))
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                // Update existing
                if (existing.IsDeleted)
                {
                    // Soft delete'den geri almak için reflection gerekebilir (BaseEntity'de IsDeleted private değil)
                    // Ancak bu durumda direkt set kabul edilebilir çünkü BaseEntity'de private değil
                    existing.IsDeleted = false;
                }

                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                if (!existing.IsSubscribed)
                {
                    existing.Subscribe();
                }

                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                existing.UpdateDetails(
                    firstName: subscriberDto.FirstName,
                    lastName: subscriberDto.LastName,
                    source: subscriberDto.Source,
                    tags: subscriberDto.Tags != null ? JsonSerializer.Serialize(subscriberDto.Tags) : null,
                    customFields: subscriberDto.CustomFields != null ? JsonSerializer.Serialize(subscriberDto.CustomFields) : null);
                
                updatedSubscribers.Add(existing);
            }
            else
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
                // ✅ BOLUM 1.3: Value Objects - Email Value Object kullanımı
                var emailValue = new Email(subscriberDto.Email);
                var subscriber = EmailSubscriber.Create(
                    email: emailValue,
                    firstName: subscriberDto.FirstName,
                    lastName: subscriberDto.LastName,
                    userId: null,
                    source: subscriberDto.Source,
                    tags: subscriberDto.Tags != null ? JsonSerializer.Serialize(subscriberDto.Tags) : null,
                    customFields: subscriberDto.CustomFields != null ? JsonSerializer.Serialize(subscriberDto.CustomFields) : null);
                
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                subscriber.Subscribe();
                
                newSubscribers.Add(subscriber);
            }
        }

        if (newSubscribers.Count > 0)
        {
            await _context.Set<EmailSubscriber>().AddRangeAsync(newSubscribers, cancellationToken);
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Toplu email abone import işlemi tamamlandı. Yeni: {NewCount}, Güncellenen: {UpdatedCount}",
            newSubscribers.Count, updatedSubscribers.Count);

        return newSubscribers.Count + updatedSubscribers.Count;
    }
}
