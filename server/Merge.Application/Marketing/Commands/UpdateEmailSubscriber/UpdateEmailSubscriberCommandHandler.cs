using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Exceptions;
using System.Text.Json;
using EmailSubscriber = Merge.Domain.Modules.Marketing.EmailSubscriber;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.UpdateEmailSubscriber;

public class UpdateEmailSubscriberCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateEmailSubscriberCommandHandler> logger) : IRequestHandler<UpdateEmailSubscriberCommand, EmailSubscriberDto>
{
    public async Task<EmailSubscriberDto> Handle(UpdateEmailSubscriberCommand request, CancellationToken cancellationToken)
    {
        var subscriber = await context.Set<EmailSubscriber>()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subscriber == null)
        {
            throw new NotFoundException("Email abonesi", request.Id);
        }

        subscriber.UpdateDetails(
            firstName: request.FirstName,
            lastName: request.LastName,
            source: request.Source,
            tags: request.Tags != null ? JsonSerializer.Serialize(request.Tags) : null,
            customFields: request.CustomFields != null ? JsonSerializer.Serialize(request.CustomFields) : null);

        if (request.IsSubscribed.HasValue)
        {
            if (request.IsSubscribed.Value)
                subscriber.Subscribe();
            else
                subscriber.Unsubscribe();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedSubscriber = await context.Set<EmailSubscriber>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        logger.LogInformation(
            "Email abonesi güncellendi. SubscriberId: {SubscriberId}",
            request.Id);

        return mapper.Map<EmailSubscriberDto>(updatedSubscriber!);
    }
}
