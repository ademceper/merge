using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.ValueObjects;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.SubscribeEmail;

public class SubscribeEmailCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<SubscribeEmailCommandHandler> logger) : IRequestHandler<SubscribeEmailCommand, EmailSubscriberDto>
{
    public async Task<EmailSubscriberDto> Handle(SubscribeEmailCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email aboneliği oluşturuluyor. Email: {Email}, Source: {Source}",
            request.Email, request.Source);

        var existing = await context.Set<EmailSubscriber>()
            .FirstOrDefaultAsync(s => EF.Functions.ILike(s.Email, request.Email), cancellationToken);

        if (existing is not null)
        {
            if (existing.IsDeleted)
            {
                existing.Restore();
            }

            existing.Subscribe();
            existing.UpdateDetails(
                firstName: request.FirstName,
                lastName: request.LastName,
                source: request.Source,
                tags: request.Tags is not null ? JsonSerializer.Serialize(request.Tags) : null,
                customFields: request.CustomFields is not null ? JsonSerializer.Serialize(request.CustomFields) : null);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            var reloadedExisting = await context.Set<EmailSubscriber>()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == existing.Id, cancellationToken);

            logger.LogInformation(
                "Email aboneliği güncellendi (mevcut kullanıcı). SubscriberId: {SubscriberId}, Email: {Email}",
                existing.Id, request.Email);

            return mapper.Map<EmailSubscriberDto>(reloadedExisting!);
        }

        var emailValue = new Email(request.Email);
        var subscriber = EmailSubscriber.Create(
            email: emailValue,
            firstName: request.FirstName,
            lastName: request.LastName,
            source: request.Source,
            tags: request.Tags is not null ? JsonSerializer.Serialize(request.Tags) : null,
            customFields: request.CustomFields is not null ? JsonSerializer.Serialize(request.CustomFields) : null);

        await context.Set<EmailSubscriber>().AddAsync(subscriber, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var createdSubscriber = await context.Set<EmailSubscriber>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == subscriber.Id, cancellationToken);

        logger.LogInformation(
            "Email aboneliği oluşturuldu. SubscriberId: {SubscriberId}, Email: {Email}",
            subscriber.Id, request.Email);

        return mapper.Map<EmailSubscriberDto>(createdSubscriber!);
    }
}
