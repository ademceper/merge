using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Text.Json;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Microsoft.Extensions.Options;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Commands.CreateCustomerCommunication;

public class CreateCustomerCommunicationCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateCustomerCommunicationCommandHandler> logger, IOptions<SupportSettings> settings) : IRequestHandler<CreateCustomerCommunicationCommand, CustomerCommunicationDto>
{

    public async Task<CustomerCommunicationDto> Handle(CreateCustomerCommunicationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating customer communication. UserId: {UserId}, Type: {Type}, Channel: {Channel}, Direction: {Direction}",
            request.UserId, request.CommunicationType, request.Channel, request.Direction);

        var communication = CustomerCommunication.Create(
            request.UserId,
            request.CommunicationType,
            request.Channel,
            request.Subject,
            request.Content,
            request.Direction,
            request.RelatedEntityId,
            request.RelatedEntityType,
            request.SentByUserId,
            request.RecipientEmail,
            request.RecipientPhone,
            request.Metadata is not null ? JsonSerializer.Serialize(request.Metadata) : null);

        await context.Set<CustomerCommunication>().AddAsync(communication, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Customer communication {CommunicationId} created successfully. UserId: {UserId}, Type: {Type}",
            communication.Id, request.UserId, request.CommunicationType);

        communication = await context.Set<CustomerCommunication>()
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.SentBy)
            .FirstOrDefaultAsync(c => c.Id == communication.Id, cancellationToken);

        return mapper.Map<CustomerCommunicationDto>(communication!);
    }
}
