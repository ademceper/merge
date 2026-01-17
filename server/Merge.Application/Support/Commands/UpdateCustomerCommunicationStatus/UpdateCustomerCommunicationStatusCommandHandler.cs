using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Commands.UpdateCustomerCommunicationStatus;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UpdateCustomerCommunicationStatusCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<UpdateCustomerCommunicationStatusCommandHandler> logger) : IRequestHandler<UpdateCustomerCommunicationStatusCommand, bool>
{

    public async Task<bool> Handle(UpdateCustomerCommunicationStatusCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Updating customer communication status. CommunicationId: {CommunicationId}, NewStatus: {Status}",
            request.CommunicationId, request.Status);

        var communication = await context.Set<CustomerCommunication>()
            .FirstOrDefaultAsync(c => c.Id == request.CommunicationId, cancellationToken);

        if (communication == null)
        {
            logger.LogWarning("Customer communication {CommunicationId} not found for status update", request.CommunicationId);
            throw new NotFoundException("Müşteri iletişimi", request.CommunicationId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        var newStatus = Enum.Parse<CommunicationStatus>(request.Status, true);
        communication.UpdateStatus(newStatus, request.DeliveredAt, request.ReadAt);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Customer communication {CommunicationId} status updated to {Status} successfully",
            request.CommunicationId, request.Status);

        return true;
    }
}
