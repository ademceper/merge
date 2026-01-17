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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateCustomerCommunicationCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateCustomerCommunicationCommandHandler> logger, IOptions<SupportSettings> settings) : IRequestHandler<CreateCustomerCommunicationCommand, CustomerCommunicationDto>
{

    public async Task<CustomerCommunicationDto> Handle(CreateCustomerCommunicationCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Creating customer communication. UserId: {UserId}, Type: {Type}, Channel: {Channel}, Direction: {Direction}",
            request.UserId, request.CommunicationType, request.Channel, request.Direction);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
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
            request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null);

        await context.Set<CustomerCommunication>().AddAsync(communication, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Customer communication {CommunicationId} created successfully. UserId: {UserId}, Type: {Type}",
            communication.Id, request.UserId, request.CommunicationType);

        // ✅ PERFORMANCE: Reload with includes for mapping
        communication = await context.Set<CustomerCommunication>()
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.SentBy)
            .FirstOrDefaultAsync(c => c.Id == communication.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return mapper.Map<CustomerCommunicationDto>(communication!);
    }
}
